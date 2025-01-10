using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using Google.Protobuf;
using Initial;
using Google.Protobuf.WellKnownTypes;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
// using Main;
// using Common;

public class NetworkManager : MonoBehaviour
{
    // �ν��Ͻ� ���� ����
    private static NetworkManager _instance; // �ν��Ͻ� ���� ����.
    public static NetworkManager Instance => _instance; // �ν��Ͻ� �б� ����.

    // ��� ���� ����
    private TcpClient client;
    private NetworkStream stream;

    public bool IsConnected { get; private set; }

    //// �̱������� ����.
    // Awake : Unity���� MonoBehaviour�� ��ӹ��� Ŭ������ ���� �ֱ� �̺�Ʈ��, "��ü�� ó�� ������ �� ȣ��"�˴ϴ�.
    // �� �޼��忡�� �̱��� �ν��Ͻ��� �����մϴ�.
    private void Awake()
    {
        // ������ �ν��Ͻ��� �����ϰų� ������ �ν��Ͻ��� �ٸ� ���. 
        // (���� �ٸ� ��� ���� �߻��Ҽ� ������ ���߿� ���������.)
        if (_instance != null && _instance != this)
        {
            // �̹� �ٸ� �ν��Ͻ��� �����Ƿ�, ���� ������ GameObject�� �����Ͽ� �̱��� ������ �����մϴ�.
            Destroy(this.gameObject);
            return;
        }
        // ���� �Դٸ� ���� �����ϴ� ����̴�.
        _instance = this;
        // Unity���� �⺻�����δ� Scene ��ȯ �� ���� Scene�� ��� ��ü�� ���ŵ˴ϴ�.
        // DontDestroyOnLoad�� "ȣ���ϸ� �� ��ü�� Scene ��ȯ���� ���ŵ��� �ʵ��� ����"�˴ϴ�.
        // �̷� ���� NetworkManager�� ��� Scene���� ������ �ν��Ͻ��� �����մϴ�.
        DontDestroyOnLoad(this.gameObject);
    }

    // ���� ���� �Լ�
    public async Task<bool> ConnectToServer(string serverIP, int serverPort)
    {
        try
        {
            client = new TcpClient();
            await client.ConnectAsync(serverIP, serverPort);
            stream = client.GetStream();

            IsConnected = true;
            Debug.Log("���� ���� ����");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("���� ���� ����: " + e.Message);
            IsConnected = false;
            return false;
        }
    }

    // ���� ���� ���� �Լ�
    public void Disconnect()
    {
        if (stream != null)
        {
            stream.Close();  // ��Ʈ���� �ݾ��ݴϴ�.
            stream = null;   // ������ null�� ����
        }

        if (client != null)
        {
            client.Close();  // Ŭ���̾�Ʈ�� �ݽ��ϴ�.
            client = null;   // ������ null�� ����
        }

        IsConnected = false;  // ���� ���¸� false�� ����
    }


    // ���ø����̼� ���� �� ���� ����
    void OnApplicationQuit()
    {
        // ���ø����̼� ���� �� �ڵ� ȣ��
        Disconnect();
    }

    public async Task<bool> SendTitlePacketAsync(string deviceId)
    {
        if (!IsConnected || stream == null) return false;
        try
        {
            // ������ ����
            var packet = new InitialPacket
            {
                DeviceId = deviceId,
            };

            // ����ȭ (����Ʈ �迭�� ��ȯ)
            byte[] serializedData;
            using (var stream = new MemoryStream())
            {
                packet.WriteTo(stream);
                serializedData = stream.ToArray();
            }

            // ��Ŷ ���� ������ ������ ���� ����
            const int TOTAL_LENGTH = 4; // ��ü ���̸� ��Ÿ���� 4����Ʈ
            const int PACKET_TYPE_LENGTH = 1; // ��Ŷ Ÿ���� ��Ÿ���� 1����Ʈ

            byte[] packetLength = new byte[TOTAL_LENGTH];
            WriteUInt32BE(packetLength, (uint)(serializedData.Length + TOTAL_LENGTH + PACKET_TYPE_LENGTH));

            // ��Ŷ Ÿ�� ������ ������ ���� ����
            byte[] packetType = new byte[1];
            packetType[0] = 1; // ��Ŷ Ÿ�� �� ����: 0

            // ���� ������ �޽����� �Բ� ����
            // Array.Copy�� ���� ���縦 ���ؼ� ���.(�������� ������ ����)
            // ��Ŷ ���� �� ������ ��ȣ (���� �����Ͱ� ����� ���ɼ��� �ֱ⶧��)
            byte[] packetWithLength = new byte[packetLength.Length + packetType.Length + serializedData.Length];
            Array.Copy(packetLength, 0, packetWithLength, 0, packetLength.Length);
            Array.Copy(packetType, 0, packetWithLength, packetLength.Length, packetType.Length);
            Array.Copy(serializedData, 0, packetWithLength, packetLength.Length + packetType.Length, serializedData.Length);


            await stream.WriteAsync(packetWithLength, 0, packetWithLength.Length);

            // ���� �� �� ������ �ޱ�
            await ReceiveTitlePacket(); // �����͸� �� ���� ����

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("��Ŷ ���� ����: " + e.Message);
            return false;
        }
    }

    // Big-Endian ������� UInt32 ���� ���ۿ� ���� �Լ�
    static void WriteUInt32BE(byte[] buffer, uint value)
    {
        buffer[0] = (byte)((value >> 24) & 0xFF);
        buffer[1] = (byte)((value >> 16) & 0xFF);
        buffer[2] = (byte)((value >> 8) & 0xFF);
        buffer[3] = (byte)(value & 0xFF);
    }

    // Big-Endian ������� UInt32 ���� �д� �Լ�
    static uint ReadUInt32BE(byte[] buffer, int offset)
    {
        return (uint)(
            (buffer[offset] << 24) |
            (buffer[offset + 1] << 16) |
            (buffer[offset + 2] << 8) |
            buffer[offset + 3]
        );
    }



    // �������� ��Ŷ�� �޴� �Լ�
    public async Task ReceiveTitlePacket()
    {
        try
        {
            // �����͸� ���� ���� ����
            byte[] buffer = new byte[1024]; // ��Ŷ ũ�� ���� ����
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);  // �񵿱������� ������ �б�

            if (bytesRead > 0)
            {

                Debug.Log("�����͸� �޾ҽ��ϴ�.");


            }
            else
            {
                Debug.LogWarning("���� �����Ͱ� �����ϴ�.");
                await Task.Delay(100);  // ��� ��� �� �ٽ� �õ�
            }
        }
        catch (Exception e)
        {
            Debug.LogError("������ �ޱ� ����: " + e.Message);
        }
    }



    //public async Task ReceivePacket()
    //{
    //    try
    //    {
    //        while(IsConnected)
    //        {
    //            // �����͸� ���� ���� ����
    //            byte[] buffer = new byte[1024]; // ��Ŷ ũ�� ���� ����
    //            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);  // �񵿱������� ������ �б�

    //            if (bytesRead > 0)
    //            {

    //                Debug.Log("�����͸� �޾ҽ��ϴ�.");

    //                //// ���� �����Ϳ��� ���� ������ ��Ŷ Ÿ�� ���� ����
    //                //uint totalLength = ReadUInt32BE(buffer, 0); // ù 4����Ʈ���� ��ü ��Ŷ ���� �б�
    //                //byte packetType = buffer[4]; // 5��° ����Ʈ���� ��Ŷ Ÿ�� �б�

    //                //// ���� �����͸� ���� (���� ���� ������ ������)
    //                //byte[] receivedData = new byte[totalLength - 5];  // ��ü ���̿��� ���� ����(4����Ʈ)�� Ÿ�� ����(1����Ʈ) ����
    //                //Array.Copy(buffer, 5, receivedData, 0, receivedData.Length);

    //                //// ���� �����͸� �������ݿ� �°� ������ȭ
    //                //InitialPacket receivedPacket;
    //                //using (var ms = new MemoryStream(receivedData))
    //                //{
    //                //    receivedPacket = InitialPacket.Parser.ParseFrom(ms);  // �������� �Ľ�
    //                //}

    //            }
    //            else
    //            {
    //                Debug.LogWarning("���� �����Ͱ� �����ϴ�.");
    //                await Task.Delay(100);  // ��� ��� �� �ٽ� �õ�
    //            }

    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError("������ �ޱ� ����: " + e.Message);
    //    }
    //}



    // Update is called once per frame
    void Update()
    {
        
    }
}
