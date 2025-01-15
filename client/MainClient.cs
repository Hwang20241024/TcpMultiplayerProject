using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainClient : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 이곳에 게임 오브젝트를 연결하자.
        TitleCanvas(); // 타이틀 캔버스 관련된 오브젝트.
        LobbyCanvas(); // 로비 캔버스 관련된 오브젝트.
    }

    void TitleCanvas()
    {
        // 게임 오브젝트 연결.
        GameObject titleCanvas = GameObject.Find("TitleCanvas");
        GameObjectManager.Instance.AddGameObject("TitleCanvas", titleCanvas);

        GameObject titlePanel = GameObject.Find("TitlePanel");
        GameObjectManager.Instance.AddGameObject("TitlePanel", titlePanel);

        GameObject inputDeviceId = GameObject.Find("InputDeviceId");
        GameObjectManager.Instance.AddGameObject("InputDeviceId", inputDeviceId);

        GameObject inputIp = GameObject.Find("InputIp");
        GameObjectManager.Instance.AddGameObject("InputIp", inputIp);

        GameObject inputPort = GameObject.Find("InputPort");
        GameObjectManager.Instance.AddGameObject("InputPort", inputPort);

        GameObject characterSelection = GameObject.Find("CharacterSelection");
        GameObjectManager.Instance.AddGameObject("CharacterSelection", characterSelection);

        GameObject errorPanel = GameObject.Find("ErrorPanel");
        GameObjectManager.Instance.AddGameObject("ErrorPanel", errorPanel);

        GameObject errorPanelText = GameObject.Find("ErrorPanelText");
        GameObjectManager.Instance.AddGameObject("ErrorPanelText", errorPanelText);

        errorPanel.SetActive(false);


    }

    void LobbyCanvas()
    {
        // 게임 오브젝트 연결.
        GameObject lobbyCanvas = GameObject.Find("LobbyCanvas");
        GameObjectManager.Instance.AddGameObject("LobbyCanvas", lobbyCanvas);

        GameObject lobbyPanel01 = GameObject.Find("LobbyPanel01");
        GameObjectManager.Instance.AddGameObject("LobbyPanel01", lobbyPanel01);

        GameObject lobbyPanel02 = GameObject.Find("LobbyPanel02");
        GameObjectManager.Instance.AddGameObject("LobbyPanel02", lobbyPanel02);

        GameObject lobbyPanel01ScrollView = GameObject.Find("LobbyPanel01ScrollView");
        GameObjectManager.Instance.AddGameObject("LobbyPanel01ScrollView", lobbyPanel01ScrollView);
    
        GameObject lobbyPanel01ScrollViewChat = GameObject.Find("LobbyPanel01ScrollViewChat");
        GameObjectManager.Instance.AddGameObject("LobbyPanel01ScrollViewChat", lobbyPanel01ScrollViewChat);

        GameObject lobbyPanel01ScrollViewContent = GameObject.Find("LobbyPanel01ScrollViewContent");
        GameObjectManager.Instance.AddGameObject("LobbyPanel01ScrollViewContent", lobbyPanel01ScrollViewContent);

        //LobbyPanel01ScrollViewChatContent
        GameObject lobbyPanel01ScrollViewChatContent = GameObject.Find("LobbyPanel01ScrollViewChatContent");
        GameObjectManager.Instance.AddGameObject("LobbyPanel01ScrollViewChatContent", lobbyPanel01ScrollViewChatContent);

        GameObject lobbyPanel02ScrollView = GameObject.Find("LobbyPanel02ScrollView");
        GameObjectManager.Instance.AddGameObject("LobbyPanel02ScrollView", lobbyPanel02ScrollView);

        GameObject lobbyPanel02ScrollViewContent = GameObject.Find("LobbyPanel02ScrollViewContent");
        GameObjectManager.Instance.AddGameObject("LobbyPanel02ScrollViewContent", lobbyPanel02ScrollViewContent);

        GameObject lobbyPanel01InputField = GameObject.Find("LobbyPanel01InputField");
        GameObjectManager.Instance.AddGameObject("LobbyPanel01InputField", lobbyPanel01InputField);

        lobbyCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
