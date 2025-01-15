using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyInputBox : MonoBehaviour
{
    public InputField inputField;  // InputField�� ������ ����

    void Start()
    {
        // Start���� InputField�� onEndEdit �̺�Ʈ�� �޼��� �߰�
        inputField.onEndEdit.AddListener(OnEndEdit);
    }

    // "Enter" Ű�� ������ �� ȣ��� �Լ�
    public async void OnEndEdit(string text)
    {
        bool isActive = GameObjectManager.Instance.GetGameObject("ErrorPanel").activeSelf;
        if (!isActive)
        {
            if (Input.GetKeyDown(KeyCode.Return))  // Return�� Enter Ű�� �ش�
            {
                Debug.Log("Enter Ű�� ���Ƚ��ϴ�. �Էµ� �ؽ�Ʈ: " + text);
                // ���⿡ �ʿ��� ������ �߰�
                string deviceId = PlayerManager.Instance.MainPlayer.UserInfo.UserId;
                await NetworkManager.Instance.SendLobbyChatPacketAsync(deviceId, text);

                // ============================
                inputField.text = ""; // �Է� �ʵ��� �ؽ�Ʈ�� ���
                inputField.ActivateInputField(); // Ÿ���� �ٽ� ����.
            }
        }
    }

   

    // Update is called once per frame
    void Update()
    {
        
    }
}
