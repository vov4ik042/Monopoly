using System;
using UnityEngine;

public class MessageUIGameBoard : MonoBehaviour
{
    private void Awake()
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
