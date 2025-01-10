using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using System.Threading.Tasks;  // �񵿱� �۾��� ���� Task

public class TitleHandler : MonoBehaviour
{
    // ����
    
    public InputField inputDeviceId;  // InputField ����
    public InputField inputIp;  // InputField ����
    public InputField inputPort;  // InputField ����

    public GameObject alertPanel; // alert �г� ����
    private Text alertText; // alert �гξȿ� �ִ� �ؽ�Ʈ

    // ���� 
    int portNumber;

    public async void OnButtonClick()
    {
        // InputField�� �ؽ�Ʈ �б�
        string inputDeviceIdText = inputDeviceId.text;
        string inputIpText = inputIp.text;
        string inputPorttText = inputPort.text;

        Debug.Log("�Էµ� �ؽ�Ʈ: " + inputDeviceIdText);
        Debug.Log("�Էµ� �ؽ�Ʈ: " + inputIpText);
        Debug.Log("�Էµ� �ؽ�Ʈ: " + inputPorttText);

        //// 1. ������ ����.
        // 1-1.IPv4 ���Խ�
        string pattern = @"^((25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)$";
        bool isValid = Regex.IsMatch(inputIpText, pattern);

        //// 2. ��Ʈ ����.
        // �Է°��� ���ڷθ� �����Ǿ� �ִ��� Ȯ���ϰ� ���� ����
        bool isNumeric = int.TryParse(inputPorttText, out int port); // �������� Ȯ��
        bool isInRange = isNumeric && port >= 0 && port <= 65535; // ���� Ȯ��

        if (!isValid)
        {
            AlertText("�����Ǹ� IPv4 �������� �Է����ּ���.");
            inputDeviceId.text = "";
            inputIp.text = "";
            inputPort.text = "";

        }
        else if (!isInRange)
        {
            AlertText("��Ʈ�� ����� �Է��ϼ���. (0 ~ 65535)");
            inputDeviceId.text = "";
            inputIp.text = "";
            inputPort.text = "";
        }


        //// 3. ���� ���� ����.
        int.TryParse(inputPorttText, out portNumber);
        await NetworkManager.Instance.ConnectToServer(inputIpText, portNumber);
        //await tcpClientExample.SendPacket();

        if (!NetworkManager.Instance.IsConnected)
        {
            AlertText("������ ������ �� �����ϴ�.");
        } else
        {
            await NetworkManager.Instance.SendTitlePacketAsync(inputDeviceIdText);
        }
    }

    public void AlertText(string str)
    {
        alertText = alertPanel.GetComponentInChildren<Text>(); // �г� �ڽĿ��� Text ������Ʈ�� ã��
        alertText.text = str;
        alertPanel.SetActive(true);
    }
}
