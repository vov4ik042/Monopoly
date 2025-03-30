using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectPlayer : MonoBehaviour
{
    [SerializeField] private int player_index;
    [SerializeField] private GameObject readyGameObject;

    private void Start()
    {
        MonopolyLobby.Instance.OnPlayerDataNetworkListChange += MonopolyLobby_OnPlayerDataNetworkListChange;
        CharacterSelectUI.Instance.OnReadyChange += CharacterSelectUI_OnReadyChange;
        UpdatePlayer();
    }

    private void CharacterSelectUI_OnReadyChange(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    private void MonopolyLobby_OnPlayerDataNetworkListChange(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }
    
    private void UpdatePlayer()
    {
        if (MonopolyLobby.Instance.IsPlayerIndexConnected(player_index))
        {
            Show();

            PlayerData playerData = MonopolyLobby.Instance.GetPlayerDataFromPlayerIndex(player_index);

            readyGameObject.SetActive(CharacterSelectUI.Instance.IsPlayerReady(playerData.clientId));
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (MonopolyLobby.Instance != null)
        {
            MonopolyLobby.Instance.OnPlayerDataNetworkListChange -= MonopolyLobby_OnPlayerDataNetworkListChange;
        }
    }

}
