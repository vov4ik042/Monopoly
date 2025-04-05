using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private TextMeshProUGUI LobbyNameText;
    [SerializeField] private TextMeshProUGUI LobbyCodeText;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            MonopolyLobby.Instance.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            SceneManager.PlayScene(Scenes.Menu);
        });
        readyButton.onClick.AddListener(() =>
        {
            CharacterSelectReady.Instance.SetPlayerReady();
        });
    }
    private void Start()//4:57
    {
        Lobby lobby = MonopolyLobby.Instance.GetLobby();

        LobbyNameText.text = "Lobby name: " + lobby.Name;
        LobbyCodeText.text = "Lobby code: " + lobby.LobbyCode;
    }
}
