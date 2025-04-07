using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MessageUIGameBoard : MonoBehaviour
{
    private void Start()
    {
        GameController.Instance.AllClientsConnected += GameController_CountOfConnectedClientsChanged;
    }

    private void GameController_CountOfConnectedClientsChanged(object sender, EventArgs e)
    {
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
