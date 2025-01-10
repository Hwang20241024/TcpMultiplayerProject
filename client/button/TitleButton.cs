using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using System.Threading.Tasks;  // 비동기 작업을 위한 Task

public class TitleHandler : MonoBehaviour
{
    // 연결
    
    public InputField inputDeviceId;  // InputField 연결
    public InputField inputIp;  // InputField 연결
    public InputField inputPort;  // InputField 연결

    public GameObject alertPanel; // alert 패널 연결
    private Text alertText; // alert 패널안에 있는 텍스트

    // 변수 
    int portNumber;

    public async void OnButtonClick()
    {
        // InputField의 텍스트 읽기
        string inputDeviceIdText = inputDeviceId.text;
        string inputIpText = inputIp.text;
        string inputPorttText = inputPort.text;

        Debug.Log("입력된 텍스트: " + inputDeviceIdText);
        Debug.Log("입력된 텍스트: " + inputIpText);
        Debug.Log("입력된 텍스트: " + inputPorttText);

        //// 1. 아이피 검증.
        // 1-1.IPv4 정규식
        string pattern = @"^((25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)$";
        bool isValid = Regex.IsMatch(inputIpText, pattern);

        //// 2. 포트 검증.
        // 입력값이 숫자로만 구성되어 있는지 확인하고 범위 검증
        bool isNumeric = int.TryParse(inputPorttText, out int port); // 숫자인지 확인
        bool isInRange = isNumeric && port >= 0 && port <= 65535; // 범위 확인

        if (!isValid)
        {
            AlertText("아이피를 IPv4 형식으로 입력해주세요.");
            inputDeviceId.text = "";
            inputIp.text = "";
            inputPort.text = "";

        }
        else if (!isInRange)
        {
            AlertText("포트를 제대로 입력하세요. (0 ~ 65535)");
            inputDeviceId.text = "";
            inputIp.text = "";
            inputPort.text = "";
        }


        //// 3. 서버 접속 검증.
        int.TryParse(inputPorttText, out portNumber);
        await NetworkManager.Instance.ConnectToServer(inputIpText, portNumber);
        //await tcpClientExample.SendPacket();

        if (!NetworkManager.Instance.IsConnected)
        {
            AlertText("서버에 접속할 수 없습니다.");
        } else
        {
            await NetworkManager.Instance.SendTitlePacketAsync(inputDeviceIdText);
        }
    }

    public void AlertText(string str)
    {
        alertText = alertPanel.GetComponentInChildren<Text>(); // 패널 자식에서 Text 컴포넌트를 찾음
        alertText.text = str;
        alertPanel.SetActive(true);
    }
}
