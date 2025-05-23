using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HostDisconnectUi : NetworkBehaviour
{
    [SerializeField] private Button playAgainButton;
    public static HostDisconnectUi Instance;

    private void Awake()
    {
        Instance = this;
        playAgainButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX(1);
            SceneManager.PlayScene(Scenes.Menu);
        });
    }
    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        Hide();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == NetworkManager.ServerClientId)
        {
            ShowForAllPlayersClientRpc();
        }
        else
        {
            if (!IsServer)
            {
                if (clientId == NetworkManager.Singleton.LocalClientId)
                {
                    Show();
               }
            }
        }
    }

    [ClientRpc(RequireOwnership = false)]
    public void ShowForAllPlayersClientRpc()
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

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
        }
    }
}
