using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyInputBox : MonoBehaviour
{
    public InputField inputField;  // InputField를 연결할 변수

    void Start()
    {
        // Start에서 InputField의 onEndEdit 이벤트에 메서드 추가
        inputField.onEndEdit.AddListener(OnEndEdit);
    }

    // "Enter" 키가 눌렸을 때 호출될 함수
    public async void OnEndEdit(string text)
    {
        bool isActive = GameObjectManager.Instance.GetGameObject("ErrorPanel").activeSelf;
        if (!isActive)
        {
            if (Input.GetKeyDown(KeyCode.Return))  // Return은 Enter 키에 해당
            {
                Debug.Log("Enter 키가 눌렸습니다. 입력된 텍스트: " + text);
                // 여기에 필요한 동작을 추가
                string deviceId = PlayerManager.Instance.MainPlayer.UserInfo.UserId;
                await NetworkManager.Instance.SendLobbyChatPacketAsync(deviceId, text);

                // ============================
                inputField.text = ""; // 입력 필드의 텍스트를 비움
                inputField.ActivateInputField(); // 타겟팅 다시 설정.
            }
        }
    }

   

    // Update is called once per frame
    void Update()
    {
        
    }
}
