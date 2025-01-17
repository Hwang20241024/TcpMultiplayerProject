using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // 싱글턴 인스턴스
    private static PlayerManager _instance;

    // 플레이어
    public Player MainPlayer { get; private set; } // 본인
    public List<Player> OtherPlayers { get; private set; } // 다른 플레이어들

    public static PlayerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 인스턴스가 없으면 게임 오브젝트에서 추가하고, 싱글턴 객체를 설정
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
        // 기존에 인스턴스가 없다면 설정하고, 있으면 이미 존재하는 인스턴스를 사용
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 변경되어도 게임 오브젝트가 유지되도록 설정
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지
        }

        // 리스트 초기화
        OtherPlayers = new List<Player>();
    }

    // 본인 스폰 요청.
    public async void SpawnMainPlayer()
    {
        string userId = $"user:{MainPlayer.UserInfo.SocketId}:{MainPlayer.UserInfo.SocketPort}";
        await NetworkManager.Instance.SendSpawnUserPacketAsync(userId);
    }

    // 다른 플레이어 추가 .
    public void CreateOtherPlayers(MainHub.UserData userData)
    {
        // 새로 추가할 플레이어 객체 생성
        Player otherPlayer = new Player(CreateUserInfo(userData), CreatePlayerStats(userData), CreatePlayerPosition(userData), userData.RoomId);

        // 'UserInfo.Uuid' 기준으로 리스트에 동일한 플레이어가 있는지 확인
        var existingPlayer = OtherPlayers.Find(p => p.UserInfo.Uuid == otherPlayer.UserInfo.Uuid);

        if (existingPlayer == null)  // 리스트가 비어 있거나, 같은 Uuid를 가진 플레이어가 없는 경우
        {
            OtherPlayers.Add(otherPlayer);  // 새 플레이어 추가

            var player = OtherPlayers.Find(p => p.UserInfo.Uuid == otherPlayer.UserInfo.Uuid);
            player.CreateAndAddPlayerPrefab(player.UserInfo.UserId);
        }
    }

    public GameObject FindPlayerObject(string uuid)
    {
        if (MainPlayer.UserInfo.Uuid == uuid)
        {
            GameObject targetObject = GameObject.Find(MainPlayer.UserInfo.UserId);
            return targetObject;
        }

        var existingPlayer = OtherPlayers.Find(p => p.UserInfo.Uuid == uuid);

        if (existingPlayer != null) // 리스트가 비어 있거나, 같은 Uuid를 가진 플레이어가 없는 경우
        {
            GameObject targetObject = GameObject.Find(existingPlayer.UserInfo.UserId);
            return targetObject;
        }

        // 어떤 조건에도 해당하지 않을 경우
        return null;
    }

    public void RemoveAllPlayers()
    {
        // 메인 플레이어 삭제
        if (MainPlayer != null)
        {
            Destroy(MainPlayer.gameObject);
            MainPlayer = null;
        }

        // 다른 플레이어들 삭제
        foreach (Player player in OtherPlayers)
        {
            if (player != null)
            {
                Destroy(player.gameObject);
            }
        }
        OtherPlayers.Clear(); // 리스트 비우기
    }

    public void CreateMainPlayer(MainHub.UserData userData)
    {
        Debug.Log(userData.UserId);

        if (MainPlayer == null)
        {
            // 게임 오브젝트에 Player 컴포넌트 추가
            GameObject playerObject = new GameObject("MainPlayer");
            MainPlayer = playerObject.AddComponent<Player>();

            // 플레이어 데이터 설정
            MainPlayer.UserInfo = CreateUserInfo(userData);
            MainPlayer.Stats = CreatePlayerStats(userData);
            MainPlayer.Position = CreatePlayerPosition(userData);
            MainPlayer.RoomId = userData.RoomId;

            GameManager.Instance.InitializeUserInfo();

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
        return new PlayerStats(userData.Sequence, userData.IsGame, userData.Score, userData.BestScore, userData.InputKey, userData.KeyPressedTimestamp, userData.IsJump);
    }

    public PlayerPosition CreatePlayerPosition(MainHub.UserData userData)
    {
        return new PlayerPosition(userData.X, userData.Y);
    }

}


public class Player : MonoBehaviour
{
    // 속성 (Fields)
    public UserInfo UserInfo { get; set; }
    public PlayerStats Stats { get; set; }
    public PlayerPosition Position { get; set; }
    public string RoomId { get; set; }

    // 생성자
    public Player(UserInfo userInfo, PlayerStats stats, PlayerPosition position, string roomId)
    {
        UserInfo = userInfo;
        Stats = stats;
        Position = position;
        RoomId = roomId;
    }

    public void CreateAndAddPlayerPrefab(string name)
    {

        // "Assets/Prefab/Player" 경로의 프리팹을 로드
        GameObject playerPrefab = Resources.Load<GameObject>("Prefab/Player");

        if (playerPrefab != null)
        {
            // 프리팹을 씬에 인스턴스화
            GameObject playerObject = Object.Instantiate(playerPrefab);

            // 이름 설정 (선택 사항)
            playerObject.name = name;

            // 추가 설정 가능 (예: 태그, 레이어 등)
            // playerObject.tag = "Player";
            // playerObject.layer = LayerMask.NameToLayer("PlayerLayer");

            // 씬에 추가된 후의 추가 설정을 하려면 여기서부터
        }
        else
        {
            Debug.LogError("Player Prefab을 찾을 수 없습니다.");
        }


        // fff
        // 특정 태그나 레이어 등을 설정할 수 있다
        // emptyObject.tag = "PlayerSpawnPoint";
        // emptyObject.layer = LayerMask.NameToLayer("UI");

    }

}


public class UserInfo
{
    public string Uuid { get; set; }
    public string UserId { get; set; }
    public string SocketId { get; set; }
    public uint SocketPort { get; set; }


    // 생성자.
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
    public string InputKey { get; set; }
    public uint KeyPressedTimestamp { get; set; }
    public bool IsJump { get; set; }

    // 생성자
    public PlayerStats(uint sequence, bool isGame, uint score, uint bestScore, string inputKey, uint keyPressedTimestamp, bool isJump)
    {
        Sequence = sequence;
        IsGame = isGame;
        Score = score;
        BestScore = bestScore;
        InputKey = inputKey;
        KeyPressedTimestamp = keyPressedTimestamp;
        IsJump = isJump;

    }
}

public class PlayerPosition
{
    public float X { get; set; }
    public float Y { get; set; }

    // 생성자
    public PlayerPosition(float x, float y)
    {
        X = x;
        Y = y;
    }
}
