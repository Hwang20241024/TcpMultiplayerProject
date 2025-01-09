using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleHandler : MonoBehaviour
{

    public TcpClientExample tcpClientExample;
    public InputField inputDeviceId;  // InputField ����
    public InputField inputIp;  // InputField ����
    public InputField inputPort;  // InputField ����

    public GameObject alertPanel; // �г� ����
    private Text alertText;
    //alertPanel.SetActive(false); �����
    public void OnButtonClick()
    {
        if (tcpClientExample != null)
        {
            // InputField�� �ؽ�Ʈ �б�
            string inputDeviceIdText = inputDeviceId.text;
            string inputIpText = inputIp.text;
            string inputPorttText = inputPort.text;

            Debug.Log("�Էµ� �ؽ�Ʈ: " + inputDeviceIdText);
            Debug.Log("�Էµ� �ؽ�Ʈ: " + inputIpText);
            Debug.Log("�Էµ� �ؽ�Ʈ: " + inputPorttText);

            if(inputDeviceIdText == "")
            {
                AlertText("�ؽ�Ʈ�� �Է��ϼ���");
            }

            tcpClientExample.SendPacket(); // TcpClientExample�� �޼��� ȣ��
        }
        else
        {
            Debug.LogError("TcpClientExample ��ũ��Ʈ�� ������� �ʾҽ��ϴ�!");
        }
    }

    public void AlertText(string str)
    {
        alertText = alertPanel.GetComponentInChildren<Text>(); // �г� �ڽĿ��� Text ������Ʈ�� ã��
        alertText.text = str;
        alertPanel.SetActive(true);
    }
}
