using MainHub;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public GameObject roomButtonPrefab; // 프리팹 변수
    public Transform contentTransform;   // Scroll View의 Content 객체 (여기서 버튼들이 추가됩니다.)


    // 싱글턴 인스턴스
    private static LobbyManager _instance;

    public static LobbyManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 인스턴스가 없으면 게임 오브젝트에서 추가하고, 싱글턴 객체를 설정
                _instance = FindObjectOfType<LobbyManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("GameObjectManager");
                    _instance = obj.AddComponent<LobbyManager>();
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

    }



    private void Start()
    {
       
    }

    // 방 버튼을 생성하는 함수
    public void CreateRoomButton(string roomId, string roomName, string host, int currentPlayers, int maxPlayers)
    {

        

        Room room = new Room(roomId, roomName, host, currentPlayers, maxPlayers);

        // 프리팹을 인스턴스화하여 버튼 생성
        GameObject roomButton = Instantiate(roomButtonPrefab, contentTransform);

        // 버튼에 방 이름과 인원 수 설정
        Text buttonText = roomButton.GetComponentInChildren<Text>();
        buttonText.text = $"{room.RoomName} (Host: {room.Host}, Players: {room.CurrentPlayers}/{room.MaxPlayers})";

        // 버튼 클릭 리스너 추가
        Button button = roomButton.GetComponent<Button>();
        button.onClick.AddListener(() => OnRoomButtonClick(room));


        // 버튼 안에 있는 토글 찾기
        Toggle toggle = roomButton.GetComponentInChildren<Toggle>();

        if (toggle != null)
        {
            // 토글 비활성화 (사용자가 조작할 수 없게 만듬)
            toggle.interactable = false;

            toggle.isOn = true;
        }

        if(room.CurrentPlayers == room.MaxPlayers)
        {
            toggle.isOn = false;
            button.interactable = false;
        }

    }

    // 방 전체 삭제.
    public void DeleteAllRoomButtons()
    {
        // contentTransform이 null이 아니면 삭제 진행
        if (contentTransform != null)
        {
            // contentTransform에 자식이 있는 동안 반복
            foreach (Transform child in contentTransform)
            {
                // 각 자식을 삭제
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("contentTransform이 존재하지않습니다.");
        }
    }


    // 방 버튼 클릭 시 호출되는 함수
    async void OnRoomButtonClick(Room room)
    {
        Debug.Log($"Entered room: {room.RoomName}");
        // 해당 방에 입장하는 로직을 추가 (서버 연결, 씬 전환 등)
        await NetworkManager.Instance.SendRoomJoinPacketAsync(room.RoomId);
    }
}

// 방 클래스 예시
public class Room
{
    public string RoomId { get; set; }
    public string RoomName { get; set; }
    public string Host { get; set; }
    public int CurrentPlayers { get; set; }
    public int MaxPlayers { get; set; }

    public Room(string roomId, string roomName, string host, int currentPlayers, int maxPlayers)
    {
        RoomId = roomId;
        RoomName = roomName;
        Host = host;
        CurrentPlayers = currentPlayers;
        MaxPlayers = maxPlayers;
    }
}