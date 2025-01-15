using MainHub;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public GameObject roomButtonPrefab; // ������ ����
    public Transform contentTransform;   // Scroll View�� Content ��ü (���⼭ ��ư���� �߰��˴ϴ�.)


    // �̱��� �ν��Ͻ�
    private static LobbyManager _instance;

    public static LobbyManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // �ν��Ͻ��� ������ ���� ������Ʈ���� �߰��ϰ�, �̱��� ��ü�� ����
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

    }



    private void Start()
    {
       
    }

    // �� ��ư�� �����ϴ� �Լ�
    public void CreateRoomButton(string roomId, string roomName, string host, int currentPlayers, int maxPlayers)
    {

        

        Room room = new Room(roomId, roomName, host, currentPlayers, maxPlayers);

        // �������� �ν��Ͻ�ȭ�Ͽ� ��ư ����
        GameObject roomButton = Instantiate(roomButtonPrefab, contentTransform);

        // ��ư�� �� �̸��� �ο� �� ����
        Text buttonText = roomButton.GetComponentInChildren<Text>();
        buttonText.text = $"{room.RoomName} (Host: {room.Host}, Players: {room.CurrentPlayers}/{room.MaxPlayers})";

        // ��ư Ŭ�� ������ �߰�
        Button button = roomButton.GetComponent<Button>();
        button.onClick.AddListener(() => OnRoomButtonClick(room));


        // ��ư �ȿ� �ִ� ��� ã��
        Toggle toggle = roomButton.GetComponentInChildren<Toggle>();

        if (toggle != null)
        {
            // ��� ��Ȱ��ȭ (����ڰ� ������ �� ���� ����)
            toggle.interactable = false;

            toggle.isOn = true;
        }

        if(room.CurrentPlayers == room.MaxPlayers)
        {
            toggle.isOn = false;
            button.interactable = false;
        }

    }

    // �� ��ü ����.
    public void DeleteAllRoomButtons()
    {
        // contentTransform�� null�� �ƴϸ� ���� ����
        if (contentTransform != null)
        {
            // contentTransform�� �ڽ��� �ִ� ���� �ݺ�
            foreach (Transform child in contentTransform)
            {
                // �� �ڽ��� ����
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("contentTransform�� ���������ʽ��ϴ�.");
        }
    }


    // �� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    async void OnRoomButtonClick(Room room)
    {
        Debug.Log($"Entered room: {room.RoomName}");
        // �ش� �濡 �����ϴ� ������ �߰� (���� ����, �� ��ȯ ��)
        await NetworkManager.Instance.SendRoomJoinPacketAsync(room.RoomId);
    }
}

// �� Ŭ���� ����
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