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
    // 인스턴스 관련 변수
    private static NetworkManager _instance; // 인스턴스 저장 변수.
    public static NetworkManager Instance => _instance; // 인스턴스 읽기 전용.

    // 통신 관련 변수
    private TcpClient client;
    private NetworkStream stream;

    public bool IsConnected { get; private set; }

    //// 싱글턴으로 구현.
    // Awake : Unity에서 MonoBehaviour를 상속받은 클래스의 생명 주기 이벤트로, "객체가 처음 생성될 때 호출"됩니다.
    // 이 메서드에서 싱글턴 인스턴스를 설정합니다.
    private void Awake()
    {
        // 기존에 인스턴스가 존재하거나 생성된 인스턴스가 다른 경우. 
        // (씬이 다를 경우 에러 발생할수 있으니 나중에 꼭기억하자.)
        if (_instance != null && _instance != this)
        {
            // 이미 다른 인스턴스가 있으므로, 새로 생성된 GameObject를 삭제하여 싱글턴 패턴을 유지합니다.
            Destroy(this.gameObject);
            return;
        }
        // 여기 왔다면 최초 생성하는 경우이다.
        _instance = this;
        // Unity에서 기본적으로는 Scene 전환 시 현재 Scene의 모든 객체가 제거됩니다.
        // DontDestroyOnLoad를 "호출하면 이 객체가 Scene 전환에도 제거되지 않도록 설정"됩니다.
        // 이로 인해 NetworkManager는 모든 Scene에서 동일한 인스턴스를 유지합니다.
        DontDestroyOnLoad(this.gameObject);
    }

    // 서버 연결 함수
    public async Task<bool> ConnectToServer(string serverIP, int serverPort)
    {
        try
        {
            client = new TcpClient();
            await client.ConnectAsync(serverIP, serverPort);
            stream = client.GetStream();

            IsConnected = true;
            Debug.Log("서버 연결 성공");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("서버 연결 실패: " + e.Message);
            IsConnected = false;
            return false;
        }
    }

    // 서버 연결 해제 함수
    public void Disconnect()
    {
        if (stream != null)
        {
            stream.Close();  // 스트림을 닫아줍니다.
            stream = null;   // 참조를 null로 설정
        }

        if (client != null)
        {
            client.Close();  // 클라이언트를 닫습니다.
            client = null;   // 참조를 null로 설정
        }

        IsConnected = false;  // 연결 상태를 false로 변경
    }


    // 애플리케이션 종료 시 연결 해제
    void OnApplicationQuit()
    {
        // 애플리케이션 종료 시 자동 호출
        Disconnect();
    }

    public async Task<bool> SendTitlePacketAsync(string deviceId)
    {
        if (!IsConnected || stream == null) return false;
        try
        {
            // 데이터 생성
            var packet = new InitialPacket
            {
                DeviceId = deviceId,
            };

            // 직렬화 (바이트 배열로 변환)
            byte[] serializedData;
            using (var stream = new MemoryStream())
            {
                packet.WriteTo(stream);
                serializedData = stream.ToArray();
            }

            // 패킷 길이 정보를 포함한 버퍼 생성
            const int TOTAL_LENGTH = 4; // 전체 길이를 나타내는 4바이트
            const int PACKET_TYPE_LENGTH = 1; // 패킷 타입을 나타내는 1바이트

            byte[] packetLength = new byte[TOTAL_LENGTH];
            WriteUInt32BE(packetLength, (uint)(serializedData.Length + TOTAL_LENGTH + PACKET_TYPE_LENGTH));

            // 패킷 타입 정보를 포함한 버퍼 생성
            byte[] packetType = new byte[1];
            packetType[0] = 1; // 패킷 타입 값 예시: 0

            // 길이 정보와 메시지를 함께 전송
            // Array.Copy는 깊은 복사를 위해서 사용.(독립적인 데이터 유지)
            // 패킷 전송 시 데이터 보호 (원본 데이터가 변경될 가능성이 있기때문)
            byte[] packetWithLength = new byte[packetLength.Length + packetType.Length + serializedData.Length];
            Array.Copy(packetLength, 0, packetWithLength, 0, packetLength.Length);
            Array.Copy(packetType, 0, packetWithLength, packetLength.Length, packetType.Length);
            Array.Copy(serializedData, 0, packetWithLength, packetLength.Length + packetType.Length, serializedData.Length);


            await stream.WriteAsync(packetWithLength, 0, packetWithLength.Length);

            // 최초 한 번 데이터 받기
            await ReceiveTitlePacket(); // 데이터를 한 번만 받음

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("패킷 전송 실패: " + e.Message);
            return false;
        }
    }

    // Big-Endian 방식으로 UInt32 값을 버퍼에 쓰는 함수
    static void WriteUInt32BE(byte[] buffer, uint value)
    {
        buffer[0] = (byte)((value >> 24) & 0xFF);
        buffer[1] = (byte)((value >> 16) & 0xFF);
        buffer[2] = (byte)((value >> 8) & 0xFF);
        buffer[3] = (byte)(value & 0xFF);
    }

    // Big-Endian 방식으로 UInt32 값을 읽는 함수
    static uint ReadUInt32BE(byte[] buffer, int offset)
    {
        return (uint)(
            (buffer[offset] << 24) |
            (buffer[offset + 1] << 16) |
            (buffer[offset + 2] << 8) |
            buffer[offset + 3]
        );
    }



    // 서버에서 패킷을 받는 함수
    public async Task ReceiveTitlePacket()
    {
        try
        {
            // 데이터를 받을 버퍼 설정
            byte[] buffer = new byte[1024]; // 패킷 크기 조정 가능
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);  // 비동기적으로 데이터 읽기

            if (bytesRead > 0)
            {

                Debug.Log("데이터를 받았습니다.");


            }
            else
            {
                Debug.LogWarning("받은 데이터가 없습니다.");
                await Task.Delay(100);  // 잠시 대기 후 다시 시도
            }
        }
        catch (Exception e)
        {
            Debug.LogError("데이터 받기 오류: " + e.Message);
        }
    }



    //public async Task ReceivePacket()
    //{
    //    try
    //    {
    //        while(IsConnected)
    //        {
    //            // 데이터를 받을 버퍼 설정
    //            byte[] buffer = new byte[1024]; // 패킷 크기 조정 가능
    //            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);  // 비동기적으로 데이터 읽기

    //            if (bytesRead > 0)
    //            {

    //                Debug.Log("데이터를 받았습니다.");

    //                //// 받은 데이터에서 길이 정보와 패킷 타입 정보 추출
    //                //uint totalLength = ReadUInt32BE(buffer, 0); // 첫 4바이트에서 전체 패킷 길이 읽기
    //                //byte packetType = buffer[4]; // 5번째 바이트에서 패킷 타입 읽기

    //                //// 실제 데이터를 추출 (길이 정보 이후의 데이터)
    //                //byte[] receivedData = new byte[totalLength - 5];  // 전체 길이에서 길이 정보(4바이트)와 타입 정보(1바이트) 제외
    //                //Array.Copy(buffer, 5, receivedData, 0, receivedData.Length);

    //                //// 받은 데이터를 프로토콜에 맞게 역직렬화
    //                //InitialPacket receivedPacket;
    //                //using (var ms = new MemoryStream(receivedData))
    //                //{
    //                //    receivedPacket = InitialPacket.Parser.ParseFrom(ms);  // 프로토콜 파싱
    //                //}

    //            }
    //            else
    //            {
    //                Debug.LogWarning("받은 데이터가 없습니다.");
    //                await Task.Delay(100);  // 잠시 대기 후 다시 시도
    //            }

    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError("데이터 받기 오류: " + e.Message);
    //    }
    //}



    // Update is called once per frame
    void Update()
    {
        
    }
}
