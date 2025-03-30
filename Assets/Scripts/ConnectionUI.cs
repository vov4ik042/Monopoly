using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{
    private void Start()
    {
        MonopolyLobby.Instance.OnTryingToJoinGame += MonopolyLobby_OnTryingToJoinGame;
        MonopolyLobby.Instance.OnFailedToJoinGame += MonopolyLobby_OnFailedToJoinGame;

        Hide();
    }

    private void MonopolyLobby_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void MonopolyLobby_OnTryingToJoinGame(object sender, System.EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        MonopolyLobby.Instance.OnTryingToJoinGame -= MonopolyLobby_OnTryingToJoinGame;
        MonopolyLobby.Instance.OnFailedToJoinGame -= MonopolyLobby_OnFailedToJoinGame;
    }
}//надо проверить
