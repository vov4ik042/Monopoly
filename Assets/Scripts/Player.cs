using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    private int playerID;
    private int moneyPlayer;
    private int currentPosition;
    private NetworkVariable<int> PhaseRentInfrastructure = new NetworkVariable<int>(0);

    private void Start()
    {
        currentPosition = 0;
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerIdServerRpc(int index)
    {
        playerID = index;
        SetPlayerIdClientRpc(index);
    }
    [ClientRpc]
    public void SetPlayerIdClientRpc(int index)
    {
        playerID = index;
    }
    public int GetPlayerId()
    {
        return playerID;
    }
    public void SetPlayerMoney(int value)
    {
        moneyPlayer += value;
        MonopolyMultiplayer.Instance.SetPlayerMoneyServerRpc(playerID, moneyPlayer);
        TablePlayersUI.Instance.UpdateInfo();
    }
    public int GetPlayerMoney()
    { 
        return moneyPlayer;
    }

    public int GetPhaseRentInfrastructure() => PhaseRentInfrastructure.Value;
    public IEnumerator PlayerMoveCoroutine(int steps)
    {
        float playerHeight = 0.16f;
        float moveDuration = .6f;

        for (int i = 0; i < steps; i++)
        {
            Vector3 startPosition = gameObject.transform.position;
            float elapsedTime = 0f;

            int nextPosition = currentPosition + 1;

            if (nextPosition == 40)
            {
                nextPosition = 0;
            }

            Vector3 goTo = BoardController.Instance.GetBoardPosition(nextPosition);
            
            if (nextPosition == 2 || nextPosition == 5 || nextPosition == 22 || nextPosition == 25)//Для красивого позиционирования игрока на специальных картах
            {
                goTo.z = startPosition.z;
            }
            if (nextPosition == 15 || nextPosition == 17 || nextPosition == 35 || nextPosition == 38)//Для красивого позиционирования игрока на специальных картах
            {
                goTo.x = startPosition.x;
            }
            goTo.y = playerHeight;

            while (elapsedTime < moveDuration)
            {
                gameObject.transform.position = Vector3.Lerp(startPosition, goTo, elapsedTime/moveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            gameObject.transform.position = goTo;
            currentPosition = nextPosition;
            BoardController.Instance.CurrentPlayerPosition(currentPosition);
        }
        //Debug.Log("currentPosition " + currentPosition.Value);
    }

    public void BuyCard(int cardIndex, int cardCost, Player player, ulong clientId)//ServerRpc
    {
        Card card = BoardController.Instance.GetCardObject(cardIndex);
        SetPlayerMoney(-cardCost);
        card.SetPlayerOwner(player);
        card.SetClientOwnerId(clientId);
        card.ShowHideCardPriceText(false);
    }
    public void SellCard(int cardPrice, int index)
    {
        Card card = BoardController.Instance.GetCardObject(index);
        SetPlayerMoney(cardPrice / 2);//При продаже возвращается только 50% от стоимости клетки
        card.SetPlayerOwner(null);
        card.SetClientOwnerId(0);
        card.ShowHideCardPriceText(true);
        Debug.Log("Карта: " + card + " продана, текущий владелец " + card.GetPlayerOwner());
    }

    public void AuctionCard()
    {

    }
    public void PayRent(int index, Card card)//ServerRpc
    {
        Player OwnerCardPlayer = card.GetPlayerOwner();
        int sumToPay;
        if (index != 8 && index != 13 && index != 28 && index != 33 && index != 36)//Country card
        {
            sumToPay = card.HowManyRentToPayForCountryCard();
        }
        else
        {
            sumToPay = card.HowManyRentToPayForInfrastructureCard(OwnerCardPlayer);
        }

        Debug.Log("Плата за ренту: " + sumToPay);
        SetPlayerMoney(-sumToPay);
        OwnerCardPlayer.SetPlayerMoney(+sumToPay);

        //StartCoroutine(VerifyPlayerMoney());//Чтобы если баланс отрицательный то пользователь должен продать или обанкротится
    }

    public void PlayerBuyCardInfrastructure(int index) => PhaseRentInfrastructure.Value++;
    public void PlayerSellCardInfrastructure(int index) => PhaseRentInfrastructure.Value--;

    public void PlayerGotTreasure()
    {
        int treasure = UnityEngine.Random.Range(25, 325);
        SetPlayerMoney(treasure);
        Debug.Log("player " + playerID + " got " + treasure);
    }
    public void PlayerPayTax(int number)
    {
        int result;
        if (number == 2)//15% tax
        {
            result = GetPlayerMoney() * 15 / 100;
        }
        else//5% tax
        {
            result = GetPlayerMoney() * 5 / 100;
        }
        SetPlayerMoney(-result);
        Debug.Log("player " + playerID + " paid " + result + " for tax");
    }

    public void UpgradeOrDemoteCity(int sumToPay)
    {
        SetPlayerMoney(-sumToPay);
    }
    public bool PlayerHasEnoughMoneyToUpgrade(int sumToPay)
    {
        if (GetPlayerMoney() - sumToPay >= 0)
        {
            return true;
        }
        return false;
    }
}
