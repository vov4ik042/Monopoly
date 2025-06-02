using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CardSpecial : NetworkBehaviour
{
    [SerializeField] private int cardIndex;
    private string CardName, PriceName;
    private int currentCountPlayers = 0;
    private List<int> playersOnCard = new List<int>();
    public void InitializeCard(string cityName, string priceName)
    {
        CardName = cityName;
        PriceName = priceName;
    }
    public void SetCurrentCountPlayers(int value, int playerIndex)
    {
        if (value == 1)
        {
            currentCountPlayers++;
            playersOnCard.Add(playerIndex);
        }
        else
        {
            if (currentCountPlayers != 0)
            {
                currentCountPlayers--;
                playersOnCard.Remove(playerIndex);
            }
        }
    }
    public int GetCurrentCountPlayers()
    {
        return currentCountPlayers;
    }
    public List<int> GetPlayersOnCardList()
    {
        return playersOnCard;
    }
    public Vector3 GetGameObject() => gameObject.transform.position;
    public string GetCardName() => CardName;
    public string GetPriceName() => PriceName;
}
