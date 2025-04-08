using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayersTableSingleUI : MonoBehaviour
{
    [SerializeField] private Image PlayerColorImage;
    [SerializeField] private TextMeshProUGUI PlayerNameText;
    [SerializeField] private TextMeshProUGUI PlayerMoneyText;

    public void SetPlayerInfo(int playerIndex)
    {
        PlayerData playerData = MonopolyMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
        PlayerColorImage.color = MonopolyMultiplayer.Instance.GetPlayerColor(playerData.colorId);
        PlayerNameText.text = playerData.playerName.ToString();
        PlayerMoneyText.text = MonopolyMultiplayer.Instance.GetPlayerMoney(playerIndex).ToString() + "$";
    }
}
