using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyUI : MonoBehaviour
{
    [SerializeField] private Button buttonPublic;
    [SerializeField] private Button buttonPrivate;
    [SerializeField] private Button buttonExit;
    [SerializeField] private TMP_InputField lobbyNameInput;

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

    private void CloseWindow()
    {
        this.gameObject.SetActive(false);
    }
}
