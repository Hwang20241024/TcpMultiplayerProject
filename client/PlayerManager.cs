using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    private static PlayerManager _instance;

    // �÷��̾�
    public Player MainPlayer { get; private set; } // ����
    public List<Player> OtherPlayers { get; private set; } // �ٸ� �÷��̾��

    public static PlayerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // �ν��Ͻ��� ������ ���� ������Ʈ���� �߰��ϰ�, �̱��� ��ü�� ����
                _instance = FindObjectOfType<PlayerManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("PlayerManager");
                    _instance = obj.AddComponent<PlayerManager>();
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        // ������ �ν��Ͻ��� ���ٸ� �����ϰ�, ������ �̹� �����ϴ� �ν��Ͻ��� ���
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // ���� ����Ǿ ���� ������Ʈ�� �����ǵ��� ����
        }
        else
        {
            Destroy(gameObject); // �ߺ� ���� ����
        }

        // ����Ʈ �ʱ�ȭ
        OtherPlayers = new List<Player>();
    }

    public void RemoveAllPlayers()
    {
        // ���� �÷��̾� ����
        if (MainPlayer != null)
        {
            Destroy(MainPlayer.gameObject);
            MainPlayer = null;
        }

        // �ٸ� �÷��̾�� ����
        foreach (Player player in OtherPlayers)
        {
            if (player != null)
            {
                Destroy(player.gameObject);
            }
        }
        OtherPlayers.Clear(); // ����Ʈ ����
    }

    public void CreateMainPlayer(MainHub.UserData userData)
    {
        Debug.Log(userData.UserId);

        if (MainPlayer == null)
        {
            // ���� ������Ʈ�� Player ������Ʈ �߰�
            GameObject playerObject = new GameObject("MainPlayer");
            MainPlayer = playerObject.AddComponent<Player>();

            // �÷��̾� ������ ����
            MainPlayer.UserInfo = CreateUserInfo(userData);
            MainPlayer.Stats = CreatePlayerStats(userData);
            MainPlayer.Position = CreatePlayerPosition(userData);
            MainPlayer.RoomId = userData.RoomId;

            //Debug.Log(userData.UserId);
            //Debug.Log(MainPlayer.UserInfo.Uuid);
            //Debug.Log(MainPlayer.UserInfo.UserId);
            //Debug.Log(MainPlayer.UserInfo.SocketId);
            //Debug.Log(MainPlayer.UserInfo.SocketPort);

            //Debug.Log(MainPlayer.Stats.Sequence);
            //Debug.Log(MainPlayer.Stats.IsGame);
            //Debug.Log(MainPlayer.Stats.Score);
            //Debug.Log(MainPlayer.Stats.BestScore);

            //Debug.Log(MainPlayer.Position.X);
            //Debug.Log(MainPlayer.Position.Y);

            //Debug.Log(MainPlayer.RoomId);
        }
    }

    public UserInfo CreateUserInfo(MainHub.UserData userData)
    {
        return new UserInfo(userData.Uuid, userData.UserId, userData.SocketId, userData.SocketPort);
    }

    public PlayerStats CreatePlayerStats(MainHub.UserData userData)
    {
        return new PlayerStats(userData.Sequence, userData.IsGame, userData.Score, userData.BestScore);
    }

    public PlayerPosition CreatePlayerPosition(MainHub.UserData userData)
    {
        return new PlayerPosition(userData.X, userData.Y);
    }

}


public class Player : MonoBehaviour
{
    // �Ӽ� (Fields)
    public UserInfo UserInfo { get; set; }
    public PlayerStats Stats { get; set; }
    public PlayerPosition Position { get; set; }
    public string RoomId { get; set; }

    // ������
    public Player(UserInfo userInfo, PlayerStats stats, PlayerPosition position, string roomId)
    {
        UserInfo = userInfo;
        Stats = stats;
        Position = position;
        RoomId = roomId;
    }

}


public class UserInfo
{
    public string Uuid { get; set; }
    public string UserId { get; set; }
    public string SocketId { get; set; }
    public uint SocketPort { get; set; }


    // ������.
    public UserInfo(string uuid, string userId, string socketId, uint socketPort)
    {
        Uuid = uuid;
        UserId = userId;
        SocketId = socketId;
        SocketPort = socketPort;
    }
}

public class PlayerStats
{
    public uint Sequence { get; set; }
    public bool IsGame { get; set; }
    public uint Score { get; set; }
    public uint BestScore { get; set; }

    // ������
    public PlayerStats(uint sequence, bool isGame, uint score, uint bestScore)
    {
        Sequence = sequence;
        IsGame = isGame;
        Score = score;
        BestScore = bestScore;
    }
}

public class PlayerPosition
{
    public float X { get; set; }
    public float Y { get; set; }

    // ������
    public PlayerPosition(float x, float y)
    {
        X = x;
        Y = y;
    }
}
