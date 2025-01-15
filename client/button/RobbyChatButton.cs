using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobbyChatButton : MonoBehaviour
{
    public InputField inputField;  // ��ǲ �ʵ带 ������ ����

    // Start is called before the first frame update
    void Start()
    {
        // ��ǲ �ʵ尡 �ڵ����� �Ҵ���� ������ ������ Ȯ���� �� �ֽ��ϴ�.
        if (inputField == null)
        {
            inputField = GetComponentInChildren<InputField>(); // ��ǲ �ʵ尡 �ڽĿ� ���� ��� �ڵ����� ã���ݴϴ�.
        }
    }

    public async void OnButtonClick()
    {
        bool isActive = GameObjectManager.Instance.GetGameObject("ErrorPanel").activeSelf;
        if (!isActive)
        {
            // ��ǲ�ڽ����� �ؽ�Ʈ ��������
            string text = inputField.text;

            // �ؽ�Ʈ�� ������� �ʴٸ� ���͸� ������ ���� ������ ������ ����
            if (!string.IsNullOrEmpty(text))
            {
                Debug.Log("��ư Ŭ�� �� �Էµ� �ؽ�Ʈ: " + text);
                // �ؽ�Ʈ ó�� ���� �߰�
                string deviceId = PlayerManager.Instance.MainPlayer.UserInfo.UserId;
                await NetworkManager.Instance.SendLobbyChatPacketAsync(deviceId, text);

                // �ؽ�Ʈ ó�� �� ��ǲ �ڽ� ���� ����� (���ϴ� ���)
                inputField.text = string.Empty;
                inputField.ActivateInputField(); // Ÿ���� �ٽ� ����.
            }
        }            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
