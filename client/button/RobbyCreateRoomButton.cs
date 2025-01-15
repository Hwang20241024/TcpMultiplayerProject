using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobbyCreateRoomButton : MonoBehaviour
{
    public async void OnButtonClick()
    {
        bool isActive = GameObjectManager.Instance.GetGameObject("ErrorPanel").activeSelf;
        if (!isActive)
        {
            //LobbyManager.Instance.CreateRoomButton("test", "test","test",3, 5);
            await NetworkManager.Instance.SendInitialRoomPacketAsync();
        }

    }
}
