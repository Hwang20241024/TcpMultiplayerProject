using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobbyChatButton : MonoBehaviour
{
    public InputField inputField;  // 인풋 필드를 연결할 변수

    // Start is called before the first frame update
    void Start()
    {
        // 인풋 필드가 자동으로 할당되지 않으면 연결을 확인할 수 있습니다.
        if (inputField == null)
        {
            inputField = GetComponentInChildren<InputField>(); // 인풋 필드가 자식에 있을 경우 자동으로 찾아줍니다.
        }
    }

    public async void OnButtonClick()
    {
        bool isActive = GameObjectManager.Instance.GetGameObject("ErrorPanel").activeSelf;
        if (!isActive)
        {
            // 인풋박스에서 텍스트 가져오기
            string text = inputField.text;

            // 텍스트가 비어있지 않다면 엔터를 눌렀을 때와 동일한 동작을 수행
            if (!string.IsNullOrEmpty(text))
            {
                Debug.Log("버튼 클릭 시 입력된 텍스트: " + text);
                // 텍스트 처리 로직 추가
                string deviceId = PlayerManager.Instance.MainPlayer.UserInfo.UserId;
                await NetworkManager.Instance.SendLobbyChatPacketAsync(deviceId, text);

                // 텍스트 처리 후 인풋 박스 내용 지우기 (원하는 경우)
                inputField.text = string.Empty;
                inputField.ActivateInputField(); // 타겟팅 다시 설정.
            }
        }            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
