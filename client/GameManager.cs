using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static GameHub.KeyInput.Types;

public class GameManager : MonoBehaviour
{
    // 싱글턴 인스턴스
    private static GameManager _instance;

    // 변수 추가
    private string userId;
    private string ip;
    private uint port;

    // 방향키 상태를 추적하는 변수
    private bool isLeftPressed = false;
    private bool isRightPressed = false;
    private bool isJump = false;

    // 싱글턴 인스턴스 접근용
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 인스턴스가 없으면 게임 오브젝트에서 추가하고, 싱글턴 객체를 설정
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    _instance = obj.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }

    // Awake()에서 싱글턴 초기화
    private void Awake()
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

    // 사용자 정보를 초기화하는 함수
    public void InitializeUserInfo()
    {
        if (PlayerManager.Instance != null && PlayerManager.Instance.MainPlayer != null)
        {
            ip = PlayerManager.Instance.MainPlayer.UserInfo.SocketId;
            port = PlayerManager.Instance.MainPlayer.UserInfo.SocketPort;
            userId = $"user:{ip}:{port}";
        }
        else
        {
            Debug.LogError("PlayerManager or MainPlayer 가 존재하지 않습니다.");
        }
    }

    // 현재 시간의 Unix 타임스탬프를 반환하는 함수
    private long GetTimestamp()
    {
        DateTime now = DateTime.Now;
        long timestamp = new DateTimeOffset(now).ToUnixTimeMilliseconds(); // 밀리초 단위
        return timestamp;
    }

    // Update is called once per frame
    async void Update()
    {
        // 방향키 한 번 눌림 감지
        if (Input.GetKeyDown(KeyCode.LeftArrow) && !isRightPressed)
        {
            isLeftPressed = true;
            await NetworkManager.Instance.SendKeyInputPacketAsync(userId, "LeftArrow", InputAction.Down, GetTimestamp());
            Debug.Log("왼쪽 방향키를 눌렀습니다!");
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            isRightPressed = true;
            await NetworkManager.Instance.SendKeyInputPacketAsync(userId, "RightArrow", InputAction.Down, GetTimestamp());
            Debug.Log("오른쪽 방향키를 눌렀습니다!");
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && !isLeftPressed)
        {
            
            Debug.Log("위쪽 방향키를 눌렀습니다!");
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            
            Debug.Log("아래쪽 방향키를 눌렀습니다!");
        }

        // 왼쪽 알트 키 눌림 감지
        if (Input.GetKeyDown(KeyCode.LeftAlt) && !isJump)
        {
            isJump = true;
            await NetworkManager.Instance.SendKeyInputPacketAsync(userId, "Jump", InputAction.Down, GetTimestamp());
            Debug.Log("왼쪽 Alt 키를 눌렀습니다!");
        }

        // 방향키가 떼어질 때 감지
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            isLeftPressed = false;
            await NetworkManager.Instance.SendKeyInputPacketAsync(userId, "NULL", InputAction.Up, GetTimestamp());
            Debug.Log("왼쪽 방향키를 뗐습니다!");
        }

        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            isRightPressed = false;
            await NetworkManager.Instance.SendKeyInputPacketAsync(userId, "NULL", InputAction.Up, GetTimestamp());
            Debug.Log("오른쪽 방향키를 뗐습니다!");
        }

        if (Input.GetKeyUp(KeyCode.UpArrow))
        {  
            Debug.Log("위쪽 방향키를 뗐습니다!");
        }

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            Debug.Log("아래쪽 방향키를 뗐습니다!");
        }

        // 왼쪽 알트 키 떼어짐 감지
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            isJump = false;
            await NetworkManager.Instance.SendKeyInputPacketAsync(userId, "NULL", InputAction.Up, GetTimestamp());
            Debug.Log("왼쪽 Alt 키를 뗐습니다!");
        }
    }
}