using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class TitleAlertButton : MonoBehaviour
{
    public GameObject alertPanel; // �г� ����

    public void OnButtonClick()
    {
        alertPanel.SetActive(false);

    }

}


