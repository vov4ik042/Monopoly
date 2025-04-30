using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies.Models;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button CreateLobbyButton;
    [SerializeField] private Button QuickJoinButton;
    [SerializeField] private Button JoinCodeButton;
    [SerializeField] private Button MainMenuButton;
    [SerializeField] private GameObject CreateLobbyUI;
    [SerializeField] private TMP_InputField lobbyCodeInput;
    [SerializeField] private TMP_InputField PlayerNameInput;
    [SerializeField] private Transform LobbyContainer;
    [SerializeField] private Transform LobbyTemplate;

    private void Awake()
    {
        CreateLobbyButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX(1);
            CreateLobbyUI.gameObject.SetActive(true);
        });
        QuickJoinButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX(1);
            MonopolyLobby.Instance.QuickJoin();
        });
        JoinCodeButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX(1);
            MonopolyLobby.Instance.JoinWithCode(lobbyCodeInput.text);
        });
        MainMenuButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX(1);
            MonopolyLobby.Instance.LeaveLobby();
            SceneManager.PlayScene(Scenes.Menu);
        });

        LobbyTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        PlayerNameInput.text = MonopolyMultiplayer.Instance.GetPlayerName();
        PlayerNameInput.onValueChanged.AddListener((string playerName) =>
        {
            MonopolyMultiplayer.Instance.SetPlayerName(playerName);
        });

        MonopolyLobby.Instance.OnLobbyListChanged += MonopolyLobby_OnLobbyListChanged;

        UpdateLobbyList(new List<Lobby>());
    }

    private void MonopolyLobby_OnLobbyListChanged(object sender, MonopolyLobby.OnLobbyChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach(Transform child in LobbyContainer)
        {
            if (child == LobbyTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach(Lobby lobby in lobbyList)
        {
            Transform lobbyTransform = Instantiate(LobbyTemplate, LobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
        }
    }

    private void OnDestroy()
    {
        MonopolyLobby.Instance.OnLobbyListChanged -= MonopolyLobby_OnLobbyListChanged;
    }
}
