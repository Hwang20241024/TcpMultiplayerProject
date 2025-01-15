using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using System.Threading.Tasks;  // 비동기 작업을 위한 Task

public class TitleHandler : MonoBehaviour
{
   
    // 변수 
    int portNumber;

    public async void OnButtonClick()
    {
        bool isActive = GameObjectManager.Instance.GetGameObject("ErrorPanel").activeSelf;

        if (!isActive)
        {
            // InputField 컴포넌트 가져오기 
            InputField inputDeviceId = GameObjectManager.Instance.GetInputField("InputDeviceId");
            InputField inputIp = GameObjectManager.Instance.GetInputField("InputIp");
            InputField inputPort = GameObjectManager.Instance.GetInputField("InputPort");


            // InputField의 텍스트 읽기
            string inputDeviceIdText = inputDeviceId.text;
            string inputIpText = inputIp.text;
            string inputPortText = inputPort.text;

            Debug.Log("입력된 텍스트: " + inputDeviceIdText);
            Debug.Log("입력된 텍스트: " + inputIpText);
            Debug.Log("입력된 텍스트: " + inputPortText);

            //// 1. 아이피 검증.
            // 1-1.IPv4 정규식
            string pattern = @"^((25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)$";
            bool isValid = Regex.IsMatch(inputIpText, pattern);

            //// 2. 포트 검증.
            // 입력값이 숫자로만 구성되어 있는지 확인하고 범위 검증
            bool isNumeric = int.TryParse(inputPortText, out int port); // 숫자인지 확인
            bool isInRange = isNumeric && port >= 0 && port <= 65535; // 범위 확인

            if (!isValid)
            {
                GameObjectManager.Instance.AlertText("아이피를 IPv4 형식으로 입력해주세요.");

                inputDeviceId.text = "";
                inputIp.text = "";
                inputPort.text = "";
                return;
            }
            else if (!isInRange)
            {
                GameObjectManager.Instance.AlertText("포트를 제대로 입력하세요. (0 ~ 65535)");
                inputDeviceId.text = "";
                inputIp.text = "";
                inputPort.text = "";
                return;
            }


            //// 3. 서버 접속 검증.
            int.TryParse(inputPortText, out portNumber);
            await NetworkManager.Instance.ConnectToServer(inputIpText, portNumber);
            //await tcpClientExample.SendPacket();

            if (!NetworkManager.Instance.IsConnected)
            {
                GameObjectManager.Instance.AlertText("서버에 접속할 수 없습니다.");
                return;
            }
            else
            {
                await NetworkManager.Instance.SendTitlePacketAsync(inputDeviceIdText);
            }
        }
       
    }

}
