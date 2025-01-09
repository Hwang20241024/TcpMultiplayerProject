using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using Google.Protobuf;
using Initial;
using Main;

public class TcpClientExample : MonoBehaviour
{
    // ���� �ʱ�ȭ
    private TcpClient client;
    private NetworkStream stream;
    private StreamReader reader;
    private StreamWriter writer;

    public string serverIP = "127.0.0.1";  // ���� IP �ּ�
    public int serverPort = 5555;  // ���� ��Ʈ ��ȣ

    // Start is called before the first frame update
    void Start()
    {
        ConnectToServer();
        ReceivePacket();
    }

    private void ConnectToServer()
    {
        try
        {
            client = new TcpClient(serverIP, serverPort);  // ������ ����
            stream = client.GetStream();  // ��Ʈ��ũ ��Ʈ�� ���
            reader = new StreamReader(stream);  // �����κ��� �б� ���� ��Ʈ��
            writer = new StreamWriter(stream);  // ������ �����͸� ������ ���� ��Ʈ��
            writer.AutoFlush = true;  // �ڵ����� �÷����Ͽ� �����͸� ��� ���� �� �ְ� ����

            Debug.Log("������ �����!");

            byte[] packet = SendPacket();  // ��Ŷ�� �����ؼ� ����

            // ������ ��Ŷ ����
            stream.Write(packet, 0, packet.Length);

            Debug.Log("������ ��Ŷ�� �����߽��ϴ�.");

        }
        catch (Exception e)
        {
            Debug.LogError("������ ���� ����: " + e.Message);
        }
    }


    public byte[] SendPacket()
    {
        // ������ ����
        long unixTimeMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var packet = new FirstConnectionCheck
        {
            Timestamp = unixTimeMilliseconds
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
        byte[] packetWithLength = new byte[packetLength.Length + packetType.Length + serializedData.Length];
        Array.Copy(packetLength, 0, packetWithLength, 0, packetLength.Length);
        Array.Copy(packetType, 0, packetWithLength, packetLength.Length, packetType.Length);
        Array.Copy(serializedData, 0, packetWithLength, packetLength.Length + packetType.Length, serializedData.Length);

        stream.Write(packetWithLength, 0, packetWithLength.Length);
        return packetWithLength;

    }

    // Big-Endian ������� UInt32 ���� ���ۿ� ���� �Լ�
    static void WriteUInt32BE(byte[] buffer, uint value)
    {
        buffer[0] = (byte)((value >> 24) & 0xFF);
        buffer[1] = (byte)((value >> 16) & 0xFF);
        buffer[2] = (byte)((value >> 8) & 0xFF);
        buffer[3] = (byte)(value & 0xFF);
    }

    // �������� ��Ŷ�� �޴� �Լ�
    public void ReceivePacket()
    {
        try
        {
            // �����͸� ���� ���� ����
            byte[] buffer = new byte[1024]; // ��Ŷ ũ�� ���� ����
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            if (bytesRead > 0)
            {
                // ���� �����Ϳ��� ���� ������ ��Ŷ Ÿ�� ���� ����
                uint totalLength = ReadUInt32BE(buffer, 0); // ù 4����Ʈ���� ��ü ��Ŷ ���� �б�
                byte packetType = buffer[4]; // 5��° ����Ʈ���� ��Ŷ Ÿ�� �б�

                // ���� �����͸� ���� (���� ���� ������ ������)
                byte[] receivedData = new byte[totalLength - 5];  // ��ü ���̿��� ���� ����(4����Ʈ)�� Ÿ�� ����(1����Ʈ) ����
                Array.Copy(buffer, 5, receivedData, 0, receivedData.Length);

                // ���� �����͸� �������ݿ� �°� ������ȭ
                FirstConnectionCheck receivedPacket;
                using (var ms = new MemoryStream(receivedData))
                {
                    receivedPacket = FirstConnectionCheck.Parser.ParseFrom(ms);  // �������� �Ľ�
                }

                // ���� ������ ó��
                Debug.Log($"Received Timestamp: {receivedPacket.Timestamp}");
            }
            else
            {
                Debug.LogWarning("���� �����Ͱ� �����ϴ�.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("������ �ޱ� ����: " + e.Message);
        }
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



    // Update is called once per frame
    void Update()
    {
        
    }
}
