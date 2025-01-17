using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using Google.Protobuf;
using MainHub;
using GameHub;
using Google.Protobuf.WellKnownTypes;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using static NetworkManager;
using static GameHub.KeyInput.Types;
using TMPro;



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
   
    // �ν��Ͻ� ���� ����
    private static NetworkManager _instance; // �ν��Ͻ� ���� ����.
    public static NetworkManager Instance => _instance; // �ν��Ͻ� �б� ����.

    // ��� ���� ����
    private TcpClient client;
    private NetworkStream stream;

    // �޼��� ���� ���� ����
    private string contentStr;
    private string viewportStr;
    private string dataStr;

    //RobbyCreateRoomButton
    // ���
    public enum PacketType
    {
        PING = 100,
        INITIAL_USER = 1,
        CONNECTED_USERS = 2,
        LOBBY_CHAT = 3,
        CREATE_ROOM = 4,
        ROOM_JOIN = 5,
        START_ACK = 6,
        SPAWN_USER = 20,
        KEY_INPUT = 21,
        ANIMATION_STATE = 22,
        COLLISION = 23,
        GAME_EXIT = 24,
        USER_START = 30,
        UPDATE_POSITION = 31,
        UPDATE_ANIMATION = 32,
        INITIAL_ENTITY = 33,
        DELETE_ENTITY = 34,
        DELETE_USER = 35,
    }


    public bool IsConnected { get; private set; }

    //// �̱������� ����.
    // Awake : Unity���� MonoBehaviour�� ��ӹ��� Ŭ������ ���� �ֱ� �̺�Ʈ��, "��ü�� ó�� ������ �� ȣ��"�˴ϴ�.
    // �� �޼��忡�� �̱��� �ν��Ͻ��� �����մϴ�.
    private void Awake()
    {

        // ��׶��忡���� ����ǵ��� ����
        Application.runInBackground = true;

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

        // �Ŵ��� �ʱ�ȭ.
        PlayerManager.Instance.RemoveAllPlayers();
        //GameObjectManager.Instance.RemoveAllGameObjects();
    }


    // ���ø����̼� ���� �� ���� ����
    void OnApplicationQuit()
    {
        // ���ø����̼� ���� �� �ڵ� ȣ��
        Disconnect();
    }

    private byte[] CreateResponse(IMessage packet, byte num)
    {
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
        packetType[0] = num; // ��Ŷ Ÿ�� �� ����: 0

        // ���� ������ �޽����� �Բ� ����
        // Array.Copy�� ���� ���縦 ���ؼ� ���.(�������� ������ ����)
        // ��Ŷ ���� �� ������ ��ȣ (���� �����Ͱ� ����� ���ɼ��� �ֱ⶧��)
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
            Debug.LogError($"��Ŷ ���� ���� (��Ŷ Ÿ��: {packetType}): " + e.Message);
            return false;
        }
    }

    public async Task<bool> SendTitlePacketAsync(string deviceId)
    {
        if (!IsConnected || stream == null) return false;
        try
        {
            // ������ ����
            var packet = new MainHub.InitialUserPacket
            {
                DeviceId = deviceId,
            };

            byte[] packetWithLength = CreateResponse(packet, (byte)PacketType.INITIAL_USER);
            await stream.WriteAsync(packetWithLength, 0, packetWithLength.Length);

            // ��Ŷ �ޱ�.
            await ReceiveTitlePacket();

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("��Ŷ ���� ����: " + e.Message);
            return false;
        }
    }

    // �κ� ä��
    public async Task<bool> SendLobbyChatPacketAsync(string deviceId, string chatData)
    {
        if (!IsConnected || stream == null) return false;

        try
        {
            // ������ ����
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
            Debug.LogError("��Ŷ ���� ����: " + e.Message);
            return false;
        }
    }

    // �����
    public async Task<bool> SendInitialRoomPacketAsync()
    {
        if (!IsConnected || stream == null) return false;

        try
        {
            // ������ ����
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
            Debug.LogError("��Ŷ ���� ����: " + e.Message);
            return false;
        }

    }

    // ������.  RoomJoinPacket
    public async Task<bool> SendRoomJoinPacketAsync(string str)
    {
        if (!IsConnected || stream == null) return false;

        try
        {
            // ������ ����
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
            Debug.LogError("��Ŷ ���� ����: " + e.Message);
            return false;
        }

    }

    // ���� PING
    public async Task<bool> SendPingPacketAsync(long timestamp)
    {
        if (!IsConnected || stream == null) return false;

        try
        {
            // ������ ����
            var packet = new GameHub.Ping
            {
                Timestamp = timestamp,
            };
            Debug.Log(timestamp);

            byte[] packetWithLength = CreateResponse(packet, (byte)PacketType.PING);
            await stream.WriteAsync(packetWithLength, 0, packetWithLength.Length);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("��Ŷ ���� ����: " + e.Message);
            return false;
        }

    }


    // ���� ���� SPAWN_USER
    public async Task<bool> SendSpawnUserPacketAsync(string userId)
    {
        if (!IsConnected || stream == null) return false;

        try
        {
            // ������ ����
            var packet = new GameHub.SpawnUserRequest
            {
                UserId = userId,
            };

            byte[] packetWithLength = CreateResponse(packet, (byte)PacketType.SPAWN_USER);
            await stream.WriteAsync(packetWithLength, 0, packetWithLength.Length);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("��Ŷ ���� ����: " + e.Message);
            return false;
        }

    }

    // Ű�Է� ���� KeyInput
    public async Task<bool> SendKeyInputPacketAsync(string userId, string keyName, InputAction action, long timestamp)
    {
        if (!IsConnected || stream == null) return false;

        try
        {
            // ������ ����
            var packet = new GameHub.KeyInput
            {
                UserId = userId,
                KeyName = keyName,
                Action = action,
                Timestamp = timestamp,
            };

            byte[] packetWithLength = CreateResponse(packet, (byte)PacketType.KEY_INPUT);
            await stream.WriteAsync(packetWithLength, 0, packetWithLength.Length);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("��Ŷ ���� ����: " + e.Message);
            return false;
        }

    }

    // �ִϸ��̼� AnimationState
    public async Task<bool> SendAnimationStatePacketAsync(string userId, string animationState)
    {
        if (!IsConnected || stream == null) return false;

        try
        {
            // ������ ����
            var packet = new GameHub.AnimationState
            {
                UserId = userId,
                AnimationState_ = animationState,
            };

            byte[] packetWithLength = CreateResponse(packet, (byte)PacketType.ANIMATION_STATE);
            await stream.WriteAsync(packetWithLength, 0, packetWithLength.Length);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("��Ŷ ���� ����: " + e.Message);
            return false;
        }

    }

    // �浹 Collision
    public async Task<bool> SendCollisionPacketAsync(string roomId, string sourceId, string targetId, float sourceX, float sourceY, float targetX, float targetY, EntityType entityType)
    {
        if (!IsConnected || stream == null) return false;

        try
        {
            // ������ ����
            var packet = new GameHub.Collision
            {
                RoomId = roomId,
                SourceId = sourceId,
                TargetId = targetId,
                TargetX = targetX,
                TargetY = targetY, 
                SourceX = sourceX,
                SourceY = sourceY,
                EntityType = entityType,
            };

            byte[] packetWithLength = CreateResponse(packet, (byte)PacketType.ANIMATION_STATE);
            await stream.WriteAsync(packetWithLength, 0, packetWithLength.Length);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("��Ŷ ���� ����: " + e.Message);
            return false;
        }

    }

    // �������� GameExit
    public async Task<bool> SendGameExitPacketAsync(string userId)
    {
        if (!IsConnected || stream == null) return false;

        try
        {
            // ������ ����
            var packet = new GameHub.GameExit
            {
                UserId = userId,
            };

            byte[] packetWithLength = CreateResponse(packet, (byte)PacketType.GAME_EXIT);
            await stream.WriteAsync(packetWithLength, 0, packetWithLength.Length);

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
            while(IsConnected)
            {
                // �����͸� ���� ���� ����
                byte[] buffer = new byte[1024]; // ��Ŷ ũ�� ���� ����
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);  // �񵿱������� ������ �б�

                if (bytesRead > 0)
                {
                    //// ���� �����Ϳ��� ���� ������ ��Ŷ Ÿ�� ���� ����
                    uint totalLength = ReadUInt32BE(buffer, 0); // ù 4����Ʈ���� ��ü ��Ŷ ���� �б�
                    byte packetType = buffer[4]; // 5��° ����Ʈ���� ��Ŷ Ÿ�� �б�

                    //// ���� �����͸� ���� (���� ���� ������ ������)
                    byte[] receivedData = new byte[totalLength - 5];
                    Array.Copy(buffer, 5, receivedData, 0, receivedData.Length);


                    Debug.Log((PacketType)packetType);
                    Debug.Log("���ڱ� �ȵǳ�1");
                    switch ((PacketType)packetType)
                    {
                        case PacketType.INITIAL_USER:
                            Debug.Log("INITIAL_USER");
                             //// ���� �����͸� ��Ŷ�� �°� ������ȭ
                            var data = MainHub.ResponseInitialUserPacket.Parser.ParseFrom(receivedData);
                            var userData = MainHub.UserData.Parser.ParseFrom(data.UserData);
                            if (data.ResponseCode == 0)
                            {
                                GameObjectManager.Instance.AlertText("�α��� �ϼ̽��ϴ�.");
                                
                                Debug.Log(userData.UserId);
                                PlayerManager.Instance.CreateMainPlayer(userData);

                                GameObjectManager.Instance.UpdateGameObjectState("TitleCanvas", false);
                                GameObjectManager.Instance.UpdateGameObjectState("LobbyCanvas", true);
                            }
                            else
                            {
                                GameObjectManager.Instance.AlertText("�̹� ���� �ϰ� �ִ� �����Դϴ�.");
                                
                            }
                            break;
                        case PacketType.CONNECTED_USERS:
                            Debug.Log("CONNECTED_USERS");
                            var data2 = MainHub.ResponseConnectedUserPacket.Parser.ParseFrom(receivedData);
                            var connectedUsers = MainHub.ConnectedUsersData.Parser.ParseFrom(data2.ConnectedUsersData);

                            GameObjectManager.Instance.ClearMessages("LobbyPanel02ScrollViewContent");

                            foreach (var user in connectedUsers.Users)
                            {
                              
                                // �̰� �����ؾ���. �����ʹ� �߿� . 
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
                            // �����.
                            break;

                        case PacketType.START_ACK:
                            Debug.Log("START_ACK");

                            SceneFadeTransition.Instance.FadeToScene("GameScene");

                            // ����ȯ �־���� .������
                            // ����...
                            // �̤� 
                            break;

                        default:
                            Console.WriteLine("Other");
                            await ReceiveGamePacket((PacketType)packetType, receivedData);
                            break;
                    }

                    //Debug.Log("�����͸� �޾ҽ��ϴ�.");
                    //Debug.Log(data.ResponseCode);

                    //Debug.Log(userData.Uuid);
                    //Debug.Log(userData.UserId);
                    //Debug.Log(userData.SocketId);
                    //Debug.Log(userData.SocketPort);


                    
                }
                else
                {
                    Debug.LogWarning("���� �����Ͱ� �����ϴ�.");
                    await Task.Delay(100);  // ��� ��� �� �ٽ� �õ�
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("������ �ޱ� ����: " + e.Message);
        }
    }


    public async Task ReceiveGamePacket(PacketType type, byte[] receivedData)
    {
        switch (type)
        {
            case PacketType.PING:
                Debug.Log("PING");
                var data = GameHub.Ping.Parser.ParseFrom(receivedData);
                await SendPingPacketAsync(data.Timestamp);
                break;
            case PacketType.USER_START:
                Debug.Log("USER_START");
                //// ���� �����͸� ��Ŷ�� �°� ������ȭ
                var userStartResponse = GameHub.UserStartResponse.Parser.ParseFrom(receivedData);
                var userData = MainHub.UserData.Parser.ParseFrom(userStartResponse.UserData);

                // �������� �ƴ��� Ȯ��.
                string mainPlayerName = PlayerManager.Instance.MainPlayer.UserInfo.UserId;
                if(mainPlayerName == userData.UserId)
                {
                    PlayerManager.Instance.MainPlayer.CreateAndAddPlayerPrefab(mainPlayerName);
                }
                else
                {
                    PlayerManager.Instance.CreateOtherPlayers(userData);
                }

                break;
            case PacketType.UPDATE_POSITION:
                Debug.Log("UPDATE_POSITION");

                var updatePositionResponse = GameHub.UpdatePositionResponse.Parser.ParseFrom(receivedData);
                var updatePosition = GameHub.UpdatePosition.Parser.ParseFrom(updatePositionResponse.UpdatePosition);

                Debug.Log($"x : {updatePosition.SourceX}, y : {updatePosition.SourceY}");

                // �÷��̾� ��ü ã��
                GameObject player = PlayerManager.Instance.FindPlayerObject(updatePosition.SourceId);

                if (player != null)
                {
                    EntityController controller = player.GetComponent<EntityController>();
                    if (controller != null)
                    {
                        controller.UpdateTargetPosition(updatePosition.SourceX, updatePosition.SourceY);
                    }
                }


                //if (player != null)
                //{
                //    Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                //    Vector3 targetPosition = new Vector3(updatePosition.SourceX, updatePosition.SourceY, 0f);

                //    if (rb != null)
                //    {
                //        // Rigidbody2D�� ����Ͽ� �ε巴�� �̵�
                //        rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, 5f * Time.deltaTime));
                //    }
                //    else
                //    {
                //        // Rigidbody2D�� ������ ���� �̵�
                //        player.transform.position = Vector3.Lerp(player.transform.position, targetPosition, 100f * Time.deltaTime);
                //    }

                //    // ���ŷ ���� �ٸ����� ���� ����.
                //}
                break;
            case PacketType.UPDATE_ANIMATION:
                Debug.Log("UPDATE_ANIMATION");
               

                break;
            case PacketType.INITIAL_ENTITY:
                Debug.Log("INITIAL_ENTITY");
               
                break;

            case PacketType.DELETE_ENTITY:
                Debug.Log("DELETE_ENTITY");

   
                break;

            case PacketType.DELETE_USER:
                Debug.Log("DELETE_USER");


                break;

            default:
                Console.WriteLine("Other");
                break;
        }
    }

}
