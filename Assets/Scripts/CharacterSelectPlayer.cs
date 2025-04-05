using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPlayer : MonoBehaviour
{
    [SerializeField] private int player_index;
    [SerializeField] private GameObject readyGameObject;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private Button kickPlayerButton;
    [SerializeField] private TextMeshPro playerNameText;

    private void Awake()
    {
        kickPlayerButton.onClick.AddListener(() =>
        {
            PlayerData playerData = MonopolyMultiplayer.Instance.GetPlayerDataFromPlayerIndex(player_index);
            MonopolyLobby.Instance.KickPlayer(playerData.playerId.ToString());
            MonopolyMultiplayer.Instance.KickPlayer(playerData.clientId);
        });
    }

    private void Start()
    {
        MonopolyMultiplayer.Instance.OnPlayerDataNetworkListChanged += MonopolyLobby_OnPlayerDataNetworkListChange;
        CharacterSelectReady.Instance.OnReadyChange += CharacterSelectUI_OnReadyChange;

        kickPlayerButton.gameObject.SetActive(NetworkManager.Singleton.IsServer);

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

            readyGameObject.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerData.clientId));

            playerNameText.text = playerData.playerName.ToString();

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
            CharacterSelectReady.Instance.OnReadyChange -= CharacterSelectUI_OnReadyChange;
        }
    }

}
