using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayersTableSingleUI : MonoBehaviour
{
    [SerializeField] private Image PlayerColorImage;
    [SerializeField] private TextMeshProUGUI PlayerNameText;
    [SerializeField] private TextMeshProUGUI PlayerMoneyText;

    public void UpdatePlayerInfo(int playerIndex)
    {
        PlayerData playerData = MonopolyMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
        PlayerColorImage.color = MonopolyMultiplayer.Instance.GetPlayerColor(playerData.colorId);
        PlayerNameText.text = playerData.playerName.ToString();

        if (MonopolyMultiplayer.Instance.GetPlayerBankrupt(playerIndex))
        {
            PlayerMoneyText.text = "Bankrupt";
        }
        else
        {
            PlayerMoneyText.text = MonopolyMultiplayer.Instance.GetPlayerMoney(playerIndex).ToString() + "$";
        }
    }
}
