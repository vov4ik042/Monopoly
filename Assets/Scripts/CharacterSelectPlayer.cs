using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectPlayer : MonoBehaviour
{
    [SerializeField] private int player_index;
    [SerializeField] private GameObject readyGameObject;
    [SerializeField] private PlayerVisual playerVisual;

    private void Start()
    {
        MonopolyMultiplayer.Instance.OnPlayerDataNetworkListChanged += MonopolyLobby_OnPlayerDataNetworkListChange;
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
        if (MonopolyMultiplayer.Instance.IsPlayerIndexConnected(player_index))
        {
            Show();

            PlayerData playerData = MonopolyMultiplayer.Instance.GetPlayerDataFromPlayerIndex(player_index);

            readyGameObject.SetActive(CharacterSelectUI.Instance.IsPlayerReady(playerData.clientId));

            playerVisual.SetPlayerColor(MonopolyMultiplayer.Instance.GetPlayerColor(playerData.colorId));
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
        if (MonopolyMultiplayer.Instance != null)
        {
            MonopolyMultiplayer.Instance.OnPlayerDataNetworkListChanged -= MonopolyLobby_OnPlayerDataNetworkListChange;
            CharacterSelectUI.Instance.OnReadyChange -= CharacterSelectUI_OnReadyChange;
        }
    }

}
