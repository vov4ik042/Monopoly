using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class Bunkrupt : MonoBehaviour
{
    [SerializeField] private Button buttonYes; 
    [SerializeField] private Button buttonNo;

    public static Bunkrupt Instance;
    private void Awake()
    {
        Instance = this;
        Hide();
    }
    private void Start()
    {
        buttonYes.onClick.AddListener(() =>
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;

            GameController.Instance.RemoveAllPlayerObjectsServerRpc();
            GameController.Instance.RemovePlayerFromListServerRpc(clientId);
            MonopolyMultiplayer.Instance.SetPlayerBankruptServerRpc(clientId);
            GameController.Instance.SetPlayerBunkruptServerRpc(clientId);
            TablePlayersUI.Instance.UpdateInfo();

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
}
