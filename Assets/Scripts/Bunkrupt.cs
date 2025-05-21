using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.EventSystems;
using System;
public class Bunkrupt : MonoBehaviour
{
    [SerializeField] private Button buttonYes; 
    [SerializeField] private Button buttonNo;

    public static Bunkrupt Instance;
    public event EventHandler PlayerBunkrupt;

    private void Awake()
    {
        Instance = this;
        Hide();
        buttonYes.onClick.AddListener(() =>
        {
            NullPlayerInfo();
            Hide();
        });
        buttonNo.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void NullPlayerInfo()
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;

        PlayerBunkrupt?.Invoke(this, EventArgs.Empty);
        GameController.Instance.RemoveAllPlayerObjectsServerRpc(clientId);
        //GameController.Instance.RemovePlayerFromListServerRpc(clientId);
        MonopolyMultiplayer.Instance.SetPlayerBankruptServerRpc(clientId);
        GameController.Instance.SetPlayerBunkruptServerRpc(clientId);
        TablePlayersUI.Instance.UpdateInfo();

        int currentPlayerIndex = GameController.Instance.GetCurrentPlayerIndex();
        if (currentPlayerIndex == (int)clientId)
        {
            GameController.Instance.NextPlayerTurnServerRpc();
        }

    }
}
