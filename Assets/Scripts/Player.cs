using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Networking.Transport;

public class Player : NetworkBehaviour
{
    private int playerID;
    private int currentCardIndexPlayerStayonServerOnly;
    private NetworkVariable<bool> inJail = new NetworkVariable<bool>(false);
    private NetworkVariable<int> moneyPlayer = new NetworkVariable<int>(0);
    private NetworkVariable<int> PhaseRentInfrastructure = new NetworkVariable<int>(0);
    private int currentPosition;
    private int VisitJailPositionIndex;//Server
    private int countTimeInPrison = 0;//Server
    private int countTimeInVacationLeft = 0;//Server
    public event EventHandler playerCircleGameBoard;

    private void Start()
    {
        currentPosition = 0;
    }
    private void OnEnable()
    {
        moneyPlayer.OnValueChanged += OnMoneyPlayerChanged;
    }
    private void OnDisable()
    {
        moneyPlayer.OnValueChanged -= OnMoneyPlayerChanged;
    }

    public void SetCurrentCardIndexPlayerStayon(int index)
    {
        currentCardIndexPlayerStayonServerOnly = index;
    }
    public int GetCurrentCardIndexPlayerStayon()
    {
        return currentCardIndexPlayerStayonServerOnly;
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
        moneyPlayer.Value += value;
    }
    public int GetPlayerMoney()
    { 
        return moneyPlayer.Value;
    }
    public void SetJail(bool value)
    {
        inJail.Value = value;
    }
    public bool GetJail()
    {
        return inJail.Value;
    }
    public void SetJailVisit(int value)
    {
        VisitJailPositionIndex = value;
    }
    public int GetJailVisit()
    {
        return VisitJailPositionIndex;
    }
    public void SetCountTimeInPrison(int value)
    {
        countTimeInPrison += value;
    }
    public int GetCountTimeInPrison()
    {
        return countTimeInPrison;
    }
    public void SetVacationTimeLeftAndGiveMoney(int value)
    {
        SetPlayerMoney(value);
        //Debug.Log("player: " + playerID + " get from vacation: " + value + "$");
        string playerName = MonopolyMultiplayer.Instance.GetPlayerNameFromPlayerId(playerID).ToString();
        string res = $"Monopoly: {playerName} gets from vacation {value}$";

        ChatManager.Instance.SendMessageServerRpc(res, -1);
        SetVacationTimeLeft(2);
    }
    public int GetVacationTimeLeft()
    {
        return countTimeInVacationLeft;
    }
    public void SetVacationTimeLeft(int value)
    {
        countTimeInVacationLeft += value;
    }
    private void OnMoneyPlayerChanged(int previousValue, int newValue)
    {
        MonopolyMultiplayer.Instance.SetPlayerMoneyServerRpc(playerID, newValue);
        TablePlayersUI.Instance.UpdateInfo();

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { (ulong)playerID }
            }
        };

        VerifyPlayerMoneyTargerClientRpc(newValue, clientRpcParams);
    }

    [ClientRpc]
    private void VerifyPlayerMoneyTargerClientRpc(int newValue, ClientRpcParams clientRpcParams)
    {
        //Debug.Log("Verify moneyPLayer: " + newValue);

        if ((ulong)playerID == NetworkManager.Singleton.LocalClientId)
        {
            if (newValue < 0)
            {
                GameController.Instance.TurnOnOffButtons(6);
            }
            else
            {
                GameController.Instance.TurnOnOffButtons(7);
            }
        }
    }

    public int GetPhaseRentInfrastructure() => PhaseRentInfrastructure.Value;
    public Vector3 GetGameObject() => gameObject.transform.position;
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
                playerCircleGameBoard?.Invoke(this, EventArgs.Empty);
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

    public IEnumerator PlayerMoveToJail()
    {
        int nextPosition = 10;
        float playerHeight = 0.16f;
        float elapsedTime = 0f;
        float moveDuration = 1.9f;
        Vector3 startPosition = gameObject.transform.position;
        Vector3 goTo = BoardController.Instance.GetBoardPosition(nextPosition);
        goTo.y = playerHeight;

        while (elapsedTime < moveDuration)
        {
            gameObject.transform.position = Vector3.Lerp(startPosition, goTo, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        currentPosition = 10;
    }
    public IEnumerator PlayerMoveToPoint(Vector3 goTo)
    {
        float playerHeight = 0.16f;
        float elapsedTime = 0f;
        float moveDuration = .6f;

        Vector3 startPosition = gameObject.transform.position;

        goTo.y = playerHeight;

        //Debug.Log("goto: " + goTo);
        while (elapsedTime < moveDuration)
        {
            gameObject.transform.position = Vector3.Lerp(startPosition, goTo, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void BuyCard(int cardIndex, int cardCost, Player player, ulong clientId)//ServerRpc
    {
        Card card = BoardController.Instance.GetCardObject(cardIndex);
        SetPlayerMoney(-cardCost);
        card.SetPlayerOwner(player);
        card.SetClientOwnerId(clientId);
        card.ShowHideCardPriceText(false);
    }
    public void BuyCardForTrade(int cardIndex, Player player, ulong clientId)//ServerRpc
    {
        Card card = BoardController.Instance.GetCardObject(cardIndex);
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
        //Debug.Log("Карта: " + card + " продана, текущий владелец " + card.GetPlayerOwner());
    }
    public void SellCardForTrade(int index)
    {
        Card card = BoardController.Instance.GetCardObject(index);
        card.SetPlayerOwner(null);
        card.SetClientOwnerId(0);
        card.ShowHideCardPriceText(true);
        //Debug.Log("Карта: " + card + " продана при обмене, текущий владелец " + card.GetPlayerOwner());
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

        string playerNameOwnerCard = MonopolyMultiplayer.Instance.GetPlayerNameFromPlayerId(OwnerCardPlayer.playerID).ToString();
        string playerName = MonopolyMultiplayer.Instance.GetPlayerNameFromPlayerId(playerID).ToString();
        string res = $"Monopoly: {playerName} pays {sumToPay}$ {playerNameOwnerCard}";

        ChatManager.Instance.SendMessageServerRpc(res, -1);
    }

    public void PlayerBuyCardInfrastructure(int index) => PhaseRentInfrastructure.Value++;
    public void PlayerSellCardInfrastructure(int index) => PhaseRentInfrastructure.Value--;

    public void PlayerGotTreasure()
    {
        int treasure = UnityEngine.Random.Range(25, 325);
        SetPlayerMoney(treasure);
        string playerName = MonopolyMultiplayer.Instance.GetPlayerNameFromPlayerId(playerID).ToString();
        string res = $"Monopoly: {playerName} gets {treasure}$";

        ChatManager.Instance.SendMessageServerRpc(res, -1);
        //Debug.Log("player " + playerID + " got " + treasure);
    }
    public int PlayerPayTax(int number)
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
        string playerName = MonopolyMultiplayer.Instance.GetPlayerNameFromPlayerId(playerID).ToString();
        string res = $"Monopoly: {playerName} pays {result}$ for taxes";

        ChatManager.Instance.SendMessageServerRpc(res, -1);
        //Debug.Log("player " + playerID + " paid " + result + " for tax");
        return result;
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
