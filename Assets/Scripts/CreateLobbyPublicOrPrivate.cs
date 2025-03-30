using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyPublicOrPrivate : MonoBehaviour
{
    [SerializeField] private Button buttonExit;
    [SerializeField] private Button buttonPublic;
    [SerializeField] private Button buttonPrivate;
    [SerializeField] private TextMeshProUGUI lobbyNameInput;

    private void Awake()
    {
        buttonPublic.onClick.AddListener(() =>
        {
            MonopolyLobby.Instance.CreateLobby(lobbyNameInput.text, false);
        });
        buttonPrivate.onClick.AddListener(() =>
        {
            MonopolyLobby.Instance.CreateLobby(lobbyNameInput.text, true);
        });
        buttonExit.onClick.AddListener(CloseWindow);
    }
    public void ButtonClickStartGame()
    {
        SceneManager.PlayScene(Scenes.GameBoard);
    }

    private void CloseWindow()
    {
        this.gameObject.SetActive(false);
    }
}
