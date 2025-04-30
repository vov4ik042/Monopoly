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
            MonopolyMultiplayer.Instance.RemovePlayerFromListServerRpc(NetworkManager.Singleton.LocalClientId);
            GameController.Instance.RemovePlayerFromListServerRpc(NetworkManager.Singleton.LocalClientId);
            TablePlayersUI.Instance.ChangeCountListTableUIPlayersServerRpc(NetworkManager.Singleton.LocalClientId);
            //MonopolyMultiplayer.Instance.KickPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
            //GameOver.Instance.Show();
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
