using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobbyEndButton : MonoBehaviour
{
    public void OnButtonClick()
    {
        bool isActive = GameObjectManager.Instance.GetGameObject("ErrorPanel").activeSelf;
        if(!isActive)
        {
            // 서버 연결 해제
            NetworkManager.Instance.Disconnect();

            // 타이틀로 이동
            GameObjectManager.Instance.UpdateGameObjectState("LobbyCanvas", false);
            GameObjectManager.Instance.UpdateGameObjectState("TitleCanvas", true);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
