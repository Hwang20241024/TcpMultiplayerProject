using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using Google.Protobuf;
using MainHub;
using Google.Protobuf.WellKnownTypes;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine.UIElements;


/*
 export const HANDLER_IDS = {
  INITIAL_USER: 0,
  FIRST_CONNECTION_CHECK:1,
  CREATE_GAME: 4,
  JOIN_GAME: 5,
  UPDATE_LOCATION: 6,
};
 */


public class NetworkManager : MonoBehaviour
{
   
    // 인스턴스 관련 변수
    private static NetworkManager _instance; // 인스턴스 저장 변수.
    public static NetworkManager Instance => _instance; // 인스턴스 읽기 전용.

    // 통신 관련 변수
    private TcpClient client;
    private NetworkStream stream;

    // 메세지 생성 관련 변수
    private string contentStr;
    private string viewportStr;
    private string dataStr;

    //RobbyCreateRoomButton
    // 상수
    public enum PacketType
    {
        PING = 0,
        INITIAL_USER = 1,
        CONNECTED_USERS = 2,
        LOBBY_CHAT = 3,
        CREATE_ROOM = 4,
        ROOM_JOIN = 5,
        START_ACK = 6,
        GAME_START = 55,
        LOCATION = 56,
        NORMAL = 57,
    }

    public bool IsConnected { get; private set; }

    //// 싱글턴으로 구현.
    // Awake : Unity에서 MonoBehaviour를 상속받은 클래스의 생명 주기 이벤트로, "객체가 처음 생성될 때 호출"됩니다.
    // 이 메서드에서 싱글턴 인스턴스를 설정합니다.
    private void Awake()
    {

        // 백그라운드에서도 실행되도록 설정
        Application.runInBackground = true;

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

        // 매니저 초기화.
        PlayerManager.Instance.RemoveAllPlayers();
        //GameObjectManager.Instance.RemoveAllGameObjects();
    }


    // 애플리케이션 종료 시 연결 해제
    void OnApplicationQuit()
    {
        // 애플리케이션 종료 시 자동 호출
        Disconnect();
    }

    private byte[] CreateResponse(IMessage packet, byte num)
    {
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
        packetType[0] = num; // 패킷 타입 값 예시: 0

        // 길이 정보와 메시지를 함께 전송
        // Array.Copy는 깊은 복사를 위해서 사용.(독립적인 데이터 유지)
        // 패킷 전송 시 데이터 보호 (원본 데이터가 변경될 가능성이 있기때문)
        byte[] packetWithLength = new byte[packetLength.Length + packetType.Length + serializedData.Length];
        Array.Copy(packetLength, 0, packetWithLength, 0, packetLength.Length);
        Array.Copy(packetType, 0, packetWithLength, packetLength.Length, packetType.Length);
        Array.Copy(serializedData, 0, packetWithLength, packetLength.Length + packetType.Length, serializedData.Length);

        return packetWithLength;
    }

    public async Task<bool> SendPacketAsync(IMessage packet, byte packetType)
    {
        if (!IsConnected || stream == null) return false;

        try
        {
            byte[] packetWithLength = CreateResponse(packet, packetType);
            await stream.WriteAsync(packetWithLength, 0, packetWithLength.Length);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"패킷 전송 실패 (패킷 타입: {packetType}): " + e.Message);
            return false;
        }
    }

    public async Task<bool> SendTitlePacketAsync(string deviceId)
    {
        if (!IsConnected || stream == null) return false;
        try
        {
            // 데이터 생성
            var packet = new MainHub.InitialUserPacket
            {
                DeviceId = deviceId,
            };

            byte[] packetWithLength = CreateResponse(packet, (byte)PacketType.INITIAL_USER);
            await stream.WriteAsync(packetWithLength, 0, packetWithLength.Length);

            // 패킷 받기.
            await ReceiveTitlePacket();

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("패킷 전송 실패: " + e.Message);
            return false;
        }
    }

    // 로비 채팅
    public async Task<bool> SendLobbyChatPacketAsync(string deviceId, string chatData)
    {
        if (!IsConnected || stream == null) return false;

        try
        {
            // 데이터 생성
            var packet = new MainHub.LobbyChatPacket
            {
                DeviceId = deviceId,
                ChatData = chatData
            };

            byte[] packetWithLength = CreateResponse(packet, (byte)PacketType.LOBBY_CHAT);
            await stream.WriteAsync(packetWithLength, 0, packetWithLength.Length);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("패킷 전송 실패: " + e.Message);
            return false;
        }
    }

    // 방생성
    public async Task<bool> SendInitialRoomPacketAsync()
    {
        if (!IsConnected || stream == null) return false;

        try
        {
            // 데이터 생성
            var packet = new MainHub.InitialRoomPacket
            {
                DeviceId = PlayerManager.Instance.MainPlayer.UserInfo.UserId,
                Sequence = PlayerManager.Instance.MainPlayer.Stats.Sequence
            };

            byte[] packetWithLength = CreateResponse(packet, (byte)PacketType.CREATE_ROOM);
            await stream.WriteAsync(packetWithLength, 0, packetWithLength.Length);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("패킷 전송 실패: " + e.Message);
            return false;
        }

    }

    // 방입장.  RoomJoinPacket
    public async Task<bool> SendRoomJoinPacketAsync(string str)
    {
        if (!IsConnected || stream == null) return false;

        try
        {
            // 데이터 생성
            var packet = new MainHub.RoomJoinPacket
            {
                RoomId = str,
                DeviceId = PlayerManager.Instance.MainPlayer.UserInfo.UserId,
                Sequence = PlayerManager.Instance.MainPlayer.Stats.Sequence
            };

            byte[] packetWithLength = CreateResponse(packet, (byte)PacketType.ROOM_JOIN);
            await stream.WriteAsync(packetWithLength, 0, packetWithLength.Length);

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
            while(IsConnected)
            {
                // 데이터를 받을 버퍼 설정
                byte[] buffer = new byte[1024]; // 패킷 크기 조정 가능
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);  // 비동기적으로 데이터 읽기

                if (bytesRead > 0)
                {
                    //// 받은 데이터에서 길이 정보와 패킷 타입 정보 추출
                    uint totalLength = ReadUInt32BE(buffer, 0); // 첫 4바이트에서 전체 패킷 길이 읽기
                    byte packetType = buffer[4]; // 5번째 바이트에서 패킷 타입 읽기

                    //// 실제 데이터를 추출 (길이 정보 이후의 데이터)
                    byte[] receivedData = new byte[totalLength - 5];
                    Array.Copy(buffer, 5, receivedData, 0, receivedData.Length);


                    Debug.Log((PacketType)packetType);
                    Debug.Log("갑자기 안되네1");
                    switch ((PacketType)packetType)
                    {
                        case PacketType.INITIAL_USER:
                            Debug.Log("INITIAL_USER");
                             //// 받은 데이터를 패킷에 맞게 역직렬화
                            var data = MainHub.ResponseInitialUserPacket.Parser.ParseFrom(receivedData);
                            var userData = MainHub.UserData.Parser.ParseFrom(data.UserData);
                            Debug.Log("갑자기 안되네2");
                            if (data.ResponseCode == 0)
                            {
                                GameObjectManager.Instance.AlertText("로그인 하셨습니다.");
                                
                                Debug.Log(userData.UserId);
                                PlayerManager.Instance.CreateMainPlayer(userData);

                                GameObjectManager.Instance.UpdateGameObjectState("TitleCanvas", false);
                                GameObjectManager.Instance.UpdateGameObjectState("LobbyCanvas", true);
                            }
                            else
                            {
                                GameObjectManager.Instance.AlertText("이미 접속 하고 있는 계정입니다.");
                                
                            }
                            break;
                        case PacketType.CONNECTED_USERS:
                            Debug.Log("CONNECTED_USERS");
                            var data2 = MainHub.ResponseConnectedUserPacket.Parser.ParseFrom(receivedData);
                            var connectedUsers = MainHub.ConnectedUsersData.Parser.ParseFrom(data2.ConnectedUsersData);

                            GameObjectManager.Instance.ClearMessages("LobbyPanel02ScrollViewContent");

                            foreach (var user in connectedUsers.Users)
                            {
                              
                                // 이거 수정해야함. 데이터는 잘옴 . 
                                contentStr = "LobbyPanel02ScrollViewContent";
                                viewportStr = "LobbyPanel02ScrollView";
                                dataStr = $"Device ID: {user.DeviceId}, Score: {user.Score}";
                                GameObjectManager.Instance.AddMessage(contentStr, viewportStr, dataStr);


                                Debug.Log($"Device ID: {user.DeviceId}, Score: {user.Score}");
                            }
                            break;
                        case PacketType.LOBBY_CHAT:
                            Debug.Log("LOBBY_CHAT");
                            var data3 = MainHub.ResponseLobbyChatPacket.Parser.ParseFrom(receivedData);
                            contentStr = "LobbyPanel01ScrollViewChatContent";
                            viewportStr = "LobbyPanel01ScrollViewChat";
                            dataStr = data3.ChatData;
                            GameObjectManager.Instance.AddMessage(contentStr, viewportStr, dataStr);

                            break;
                        case PacketType.CREATE_ROOM:
                            Debug.Log("CREATE_ROOM");
                            var data4 = MainHub.ResponseRoomInfoPacket.Parser.ParseFrom(receivedData);
                            var roomData = MainHub.RoomsData.Parser.ParseFrom(data4.RoomsData);

                            LobbyManager.Instance.DeleteAllRoomButtons();
                            foreach (var room in roomData.Rooms)
                            {
                                LobbyManager.Instance.CreateRoomButton(room.RoomId, room.RoomName, room.Host, (int)room.CurrentPlayers, (int)room.MaxPlayers);

                            }
                            // 방생성.
                            break;

                        case PacketType.START_ACK:
                            Debug.Log("START_ACK");

                            SceneFadeTransition.Instance.FadeToScene("GameScene");

                            // 씬전환 넣어야함 .ㅋㅋㅋ
                            // 제발...
                            // ㅜㅜ 
                            break;

                        default:
                            Console.WriteLine("Other");
                            break;
                    }

                    //Debug.Log("데이터를 받았습니다.");
                    //Debug.Log(data.ResponseCode);

                    //Debug.Log(userData.Uuid);
                    //Debug.Log(userData.UserId);
                    //Debug.Log(userData.SocketId);
                    //Debug.Log(userData.SocketPort);


                    
                }
                else
                {
                    Debug.LogWarning("받은 데이터가 없습니다.");
                    await Task.Delay(100);  // 잠시 대기 후 다시 시도
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("데이터 받기 오류: " + e.Message);
        }
    }



    


    // 메세지를 받는 시점을 바꾸자. 
    // 1. 로비로 진행한 순간에 해야함..


    // Update is called once per frame
    void Update()
    {
        
    }
}
