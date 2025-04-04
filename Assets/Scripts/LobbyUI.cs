using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button CreateLobbyButton;
    [SerializeField] private Button QuickJoinButton;
    [SerializeField] private Button JoinCodeButton;
    [SerializeField] private Button MainMenuButton;
    [SerializeField] private GameObject CreateLobbyUI;
    [SerializeField] private TMP_InputField lobbyCodeInput;

    private void Awake()
    {
        CreateLobbyButton.onClick.AddListener(() =>
        {
            CreateLobbyUI.gameObject.SetActive(true);
        });
        QuickJoinButton.onClick.AddListener(() =>
        {
            //MonopolyMultiplayer.Instance.StartClient();
            MonopolyLobby.Instance.QuickJoin();
        });
        JoinCodeButton.onClick.AddListener(() =>
        {
            MonopolyLobby.Instance.JoinWithCode(lobbyCodeInput.text);
        });
        MainMenuButton.onClick.AddListener(() =>
        {
            SceneManager.PlayScene(Scenes.Menu);
        });
    }
}
