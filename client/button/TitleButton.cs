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
    int portNumber;

    public async void OnButtonClick()
    {
        bool isActive = GameObjectManager.Instance.GetGameObject("ErrorPanel").activeSelf;

        if (!isActive)
        {
            // InputField ������Ʈ �������� 
            InputField inputDeviceId = GameObjectManager.Instance.GetInputField("InputDeviceId");
            InputField inputIp = GameObjectManager.Instance.GetInputField("InputIp");
            InputField inputPort = GameObjectManager.Instance.GetInputField("InputPort");


            // InputField�� �ؽ�Ʈ �б�
            string inputDeviceIdText = inputDeviceId.text;
            string inputIpText = inputIp.text;
            string inputPortText = inputPort.text;

            Debug.Log("�Էµ� �ؽ�Ʈ: " + inputDeviceIdText);
            Debug.Log("�Էµ� �ؽ�Ʈ: " + inputIpText);
            Debug.Log("�Էµ� �ؽ�Ʈ: " + inputPortText);

            //// 1. ������ ����.
            // 1-1.IPv4 ���Խ�
            string pattern = @"^((25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)$";
            bool isValid = Regex.IsMatch(inputIpText, pattern);

            //// 2. ��Ʈ ����.
            // �Է°��� ���ڷθ� �����Ǿ� �ִ��� Ȯ���ϰ� ���� ����
            bool isNumeric = int.TryParse(inputPortText, out int port); // �������� Ȯ��
            bool isInRange = isNumeric && port >= 0 && port <= 65535; // ���� Ȯ��

            if (!isValid)
            {
                GameObjectManager.Instance.AlertText("�����Ǹ� IPv4 �������� �Է����ּ���.");

                inputDeviceId.text = "";
                inputIp.text = "";
                inputPort.text = "";
                return;
            }
            else if (!isInRange)
            {
                GameObjectManager.Instance.AlertText("��Ʈ�� ����� �Է��ϼ���. (0 ~ 65535)");
                inputDeviceId.text = "";
                inputIp.text = "";
                inputPort.text = "";
                return;
            }


            //// 3. ���� ���� ����.
            int.TryParse(inputPortText, out portNumber);
            await NetworkManager.Instance.ConnectToServer(inputIpText, portNumber);
            //await tcpClientExample.SendPacket();

            if (!NetworkManager.Instance.IsConnected)
            {
                GameObjectManager.Instance.AlertText("������ ������ �� �����ϴ�.");
                return;
            }
            else
            {
                await NetworkManager.Instance.SendTitlePacketAsync(inputDeviceIdText);
            }
        }
       
    }

}
