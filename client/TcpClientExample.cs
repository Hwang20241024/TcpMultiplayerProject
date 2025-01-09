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
    // 변수 초기화
    private TcpClient client;
    private NetworkStream stream;
    private StreamReader reader;
    private StreamWriter writer;

    public string serverIP = "127.0.0.1";  // 서버 IP 주소
    public int serverPort = 5555;  // 서버 포트 번호

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
            client = new TcpClient(serverIP, serverPort);  // 서버에 연결
            stream = client.GetStream();  // 네트워크 스트림 얻기
            reader = new StreamReader(stream);  // 서버로부터 읽기 위한 스트림
            writer = new StreamWriter(stream);  // 서버로 데이터를 보내기 위한 스트림
            writer.AutoFlush = true;  // 자동으로 플러시하여 데이터를 즉시 보낼 수 있게 설정

            Debug.Log("서버에 연결됨!");

            byte[] packet = SendPacket();  // 패킷을 생성해서 받음

            // 서버에 패킷 전송
            stream.Write(packet, 0, packet.Length);

            Debug.Log("서버로 패킷을 전송했습니다.");

        }
        catch (Exception e)
        {
            Debug.LogError("서버에 연결 실패: " + e.Message);
        }
    }


    public byte[] SendPacket()
    {
        // 데이터 생성
        long unixTimeMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var packet = new FirstConnectionCheck
        {
            Timestamp = unixTimeMilliseconds
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
        byte[] packetWithLength = new byte[packetLength.Length + packetType.Length + serializedData.Length];
        Array.Copy(packetLength, 0, packetWithLength, 0, packetLength.Length);
        Array.Copy(packetType, 0, packetWithLength, packetLength.Length, packetType.Length);
        Array.Copy(serializedData, 0, packetWithLength, packetLength.Length + packetType.Length, serializedData.Length);

        stream.Write(packetWithLength, 0, packetWithLength.Length);
        return packetWithLength;

    }

    // Big-Endian 방식으로 UInt32 값을 버퍼에 쓰는 함수
    static void WriteUInt32BE(byte[] buffer, uint value)
    {
        buffer[0] = (byte)((value >> 24) & 0xFF);
        buffer[1] = (byte)((value >> 16) & 0xFF);
        buffer[2] = (byte)((value >> 8) & 0xFF);
        buffer[3] = (byte)(value & 0xFF);
    }

    // 서버에서 패킷을 받는 함수
    public void ReceivePacket()
    {
        try
        {
            // 데이터를 받을 버퍼 설정
            byte[] buffer = new byte[1024]; // 패킷 크기 조정 가능
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            if (bytesRead > 0)
            {
                // 받은 데이터에서 길이 정보와 패킷 타입 정보 추출
                uint totalLength = ReadUInt32BE(buffer, 0); // 첫 4바이트에서 전체 패킷 길이 읽기
                byte packetType = buffer[4]; // 5번째 바이트에서 패킷 타입 읽기

                // 실제 데이터를 추출 (길이 정보 이후의 데이터)
                byte[] receivedData = new byte[totalLength - 5];  // 전체 길이에서 길이 정보(4바이트)와 타입 정보(1바이트) 제외
                Array.Copy(buffer, 5, receivedData, 0, receivedData.Length);

                // 받은 데이터를 프로토콜에 맞게 역직렬화
                FirstConnectionCheck receivedPacket;
                using (var ms = new MemoryStream(receivedData))
                {
                    receivedPacket = FirstConnectionCheck.Parser.ParseFrom(ms);  // 프로토콜 파싱
                }

                // 받은 데이터 처리
                Debug.Log($"Received Timestamp: {receivedPacket.Timestamp}");
            }
            else
            {
                Debug.LogWarning("받은 데이터가 없습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("데이터 받기 오류: " + e.Message);
        }
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



    // Update is called once per frame
    void Update()
    {
        
    }
}
