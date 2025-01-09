using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleHandler : MonoBehaviour
{

    public TcpClientExample tcpClientExample;
    public InputField inputDeviceId;  // InputField 연결
    public InputField inputIp;  // InputField 연결
    public InputField inputPort;  // InputField 연결

    public GameObject alertPanel; // 패널 연결
    private Text alertText;
    //alertPanel.SetActive(false); 숨기기
    public void OnButtonClick()
    {
        if (tcpClientExample != null)
        {
            // InputField의 텍스트 읽기
            string inputDeviceIdText = inputDeviceId.text;
            string inputIpText = inputIp.text;
            string inputPorttText = inputPort.text;

            Debug.Log("입력된 텍스트: " + inputDeviceIdText);
            Debug.Log("입력된 텍스트: " + inputIpText);
            Debug.Log("입력된 텍스트: " + inputPorttText);

            if(inputDeviceIdText == "")
            {
                AlertText("텍스트를 입력하세요");
            }

            tcpClientExample.SendPacket(); // TcpClientExample의 메서드 호출
        }
        else
        {
            Debug.LogError("TcpClientExample 스크립트가 연결되지 않았습니다!");
        }
    }

    public void AlertText(string str)
    {
        alertText = alertPanel.GetComponentInChildren<Text>(); // 패널 자식에서 Text 컴포넌트를 찾음
        alertText.text = str;
        alertPanel.SetActive(true);
    }
}
