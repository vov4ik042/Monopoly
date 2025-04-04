using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MainMenuCleanUp : MonoBehaviour
{
    private void Awake()
    {
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if (MonopolyMultiplayer.Instance != null)
        {
            Destroy(MonopolyMultiplayer.Instance.gameObject);
        }
        if (MonopolyLobby.Instance != null)
        {
            Destroy(MonopolyLobby.Instance.gameObject);
        }
    }
}
