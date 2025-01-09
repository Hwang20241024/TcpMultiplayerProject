using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class TitleAlertButton : MonoBehaviour
{
    public GameObject alertPanel; // 패널 연결

    public void OnButtonClick()
    {
        alertPanel.SetActive(false);

    }

}


