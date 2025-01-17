using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static GameHub.KeyInput.Types;

public class GameManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    private static GameManager _instance;

    // ���� �߰�
    private string userId;
    private string ip;
    private uint port;

    // ����Ű ���¸� �����ϴ� ����
    private bool isLeftPressed = false;
    private bool isRightPressed = false;
    private bool isJump = false;

    // �̱��� �ν��Ͻ� ���ٿ�
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // �ν��Ͻ��� ������ ���� ������Ʈ���� �߰��ϰ�, �̱��� ��ü�� ����
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

    // Awake()���� �̱��� �ʱ�ȭ
    private void Awake()
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

    // ����� ������ �ʱ�ȭ�ϴ� �Լ�
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
            Debug.LogError("PlayerManager or MainPlayer �� �������� �ʽ��ϴ�.");
        }
    }

    // ���� �ð��� Unix Ÿ�ӽ������� ��ȯ�ϴ� �Լ�
    private long GetTimestamp()
    {
        DateTime now = DateTime.Now;
        long timestamp = new DateTimeOffset(now).ToUnixTimeMilliseconds(); // �и��� ����
        return timestamp;
    }

    // Update is called once per frame
    async void Update()
    {
        // ����Ű �� �� ���� ����
        if (Input.GetKeyDown(KeyCode.LeftArrow) && !isRightPressed)
        {
            isLeftPressed = true;
            await NetworkManager.Instance.SendKeyInputPacketAsync(userId, "LeftArrow", InputAction.Down, GetTimestamp());
            Debug.Log("���� ����Ű�� �������ϴ�!");
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            isRightPressed = true;
            await NetworkManager.Instance.SendKeyInputPacketAsync(userId, "RightArrow", InputAction.Down, GetTimestamp());
            Debug.Log("������ ����Ű�� �������ϴ�!");
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && !isLeftPressed)
        {
            
            Debug.Log("���� ����Ű�� �������ϴ�!");
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            
            Debug.Log("�Ʒ��� ����Ű�� �������ϴ�!");
        }

        // ���� ��Ʈ Ű ���� ����
        if (Input.GetKeyDown(KeyCode.LeftAlt) && !isJump)
        {
            isJump = true;
            await NetworkManager.Instance.SendKeyInputPacketAsync(userId, "Jump", InputAction.Down, GetTimestamp());
            Debug.Log("���� Alt Ű�� �������ϴ�!");
        }

        // ����Ű�� ������ �� ����
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            isLeftPressed = false;
            await NetworkManager.Instance.SendKeyInputPacketAsync(userId, "NULL", InputAction.Up, GetTimestamp());
            Debug.Log("���� ����Ű�� �ý��ϴ�!");
        }

        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            isRightPressed = false;
            await NetworkManager.Instance.SendKeyInputPacketAsync(userId, "NULL", InputAction.Up, GetTimestamp());
            Debug.Log("������ ����Ű�� �ý��ϴ�!");
        }

        if (Input.GetKeyUp(KeyCode.UpArrow))
        {  
            Debug.Log("���� ����Ű�� �ý��ϴ�!");
        }

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            Debug.Log("�Ʒ��� ����Ű�� �ý��ϴ�!");
        }

        // ���� ��Ʈ Ű ������ ����
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            isJump = false;
            await NetworkManager.Instance.SendKeyInputPacketAsync(userId, "NULL", InputAction.Up, GetTimestamp());
            Debug.Log("���� Alt Ű�� �ý��ϴ�!");
        }
    }
}