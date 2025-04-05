using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        MonopolyMultiplayer.Instance.OnFailedToJoinGame += MonopolyLobby_OnFailedToJoinGame;
        MonopolyLobby.Instance.OnCreateLobbyStarted += MonopolyLobby_OnCreateLobbyStarted;
        MonopolyLobby.Instance.OnCreateLobbyFailed += MonopolyLobby_OnCreateLobbyFailed;
        MonopolyLobby.Instance.OnJoinStarted += MonopolyLobby_OnJoinStarted;
        MonopolyLobby.Instance.OnJoinFailed += MonopolyLobby_OnJoinFailed;
        MonopolyLobby.Instance.OnQuickJoinFailed += MonopolyLobby_OnQuickJoinFailed;

        Hide();
    }

    private void MonopolyLobby_OnQuickJoinFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Could not find a Lobby to Quick Join!");
    }

    private void MonopolyLobby_OnJoinFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Failed to join Lobby!");
    }

    private void MonopolyLobby_OnJoinStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Joining Lobby...");
    }

    private void MonopolyLobby_OnCreateLobbyFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Failed to creat Lobby!");
    }

    private void MonopolyLobby_OnCreateLobbyStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Creating Lobby...");
    }

    private void MonopolyLobby_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        if (NetworkManager.Singleton.DisconnectReason == "")
        {
            ShowMessage("Failed to connect");
        }
        else
        {
            ShowMessage(NetworkManager.Singleton.DisconnectReason);
        }
    }

    private void ShowMessage(string message)
    {
        Show();

        messageText.text = message;
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
        MonopolyMultiplayer.Instance.OnFailedToJoinGame -= MonopolyLobby_OnFailedToJoinGame;
        MonopolyLobby.Instance.OnCreateLobbyStarted -= MonopolyLobby_OnCreateLobbyStarted;
        MonopolyLobby.Instance.OnCreateLobbyFailed -= MonopolyLobby_OnCreateLobbyFailed;
        MonopolyLobby.Instance.OnJoinStarted -= MonopolyLobby_OnJoinStarted;
        MonopolyLobby.Instance.OnJoinFailed -= MonopolyLobby_OnJoinFailed;
        MonopolyLobby.Instance.OnQuickJoinFailed -= MonopolyLobby_OnQuickJoinFailed;
    }
}
