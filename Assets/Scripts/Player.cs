using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UniRx;

public class Player : MonoBehaviour
{

    public static int playerCounter = 0;
    private List<Card> citiesPlayer = new List<Card>();

    private ReactiveProperty<int> _moneyPlayer = new ReactiveProperty<int>();
    private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

    private int PhaseRentInfrastructure { get; set; } = 0;
    public bool Bankrupt { get; set; } = false;

    public int moneyPlayer
    {
        get { return _moneyPlayer.Value; }
        set { _moneyPlayer.Value = value; }
    }

    public bool isMoving { get; set; }

    private int playerID;
    public int propertyPlayerID
    {
        get { return playerID; }
        set 
        {
            playerCounter++;
            playerID = playerCounter;
        }
    }

    public Vector3 playerOffSet { get; set; }
    public string playerName { get; set; }
    public int currentPosition { get; set; } = 0;
    public GameObject playerPrefab { get; set; }
    private Vector3 startPositionPlayer = new Vector3(0, 0, 0);
    private void Awake()
    {
        startPositionPlayer = transform.position;
    }
    private void Start()
    {
        _moneyPlayer.Subscribe(CheckIfPlayerIsCurrentPlayer).AddTo(_compositeDisposable);
    }

    public int GetPhaseRentInfrastructure() => PhaseRentInfrastructure;
    public IEnumerator PlayerMoveCoroutine(int steps)
    {
        float moveDuration = .7f;

        for (int i = 0; i < steps; i++)
        {
            Vector3 startPosition = transform.position;
            float elapsedTime = 0f;

            // Вычисляем следующую клетку (по кругу)
            int nextPosition = currentPosition + 1;
            if (nextPosition == 40)
            {
                nextPosition = 0;
            }
            Vector3 goTo = BoardController.Instance.GetBoardPosition(nextPosition);

            if (nextPosition == 0 || nextPosition == 20) //Для сохранения своей линии относительно клетки
            {
                goTo.z = -startPositionPlayer.z;
            }
            if (nextPosition == 10 || nextPosition == 30) //Для сохранения своей линии относительно клетки
            {
                goTo.x = -startPositionPlayer.x;
            }

            if (currentPosition == 0 || currentPosition == 10 || currentPosition == 20 || currentPosition == 30) //Обновление позиции рассчета имеено после предыдущих вычеслений
            {
                startPositionPlayer = startPosition;
            }

            if ((nextPosition > 10 && nextPosition <= 20) || (nextPosition > 30 && nextPosition < 40) || nextPosition == 0) //Фиксирование X оси на двух сторонах доски
            {
                goTo.x = startPositionPlayer.x;
            }
            if ((nextPosition > 0 && nextPosition <= 10) || (nextPosition > 20 && nextPosition <= 30)) //Фиксирование Z оси на двух сторонах доски
            {
                goTo.z = startPositionPlayer.z;
            }

            goTo.y = playerOffSet.y;

            while (elapsedTime < moveDuration)
            {
                transform.position = Vector3.Lerp(startPosition, goTo, elapsedTime/moveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = goTo;
            currentPosition = nextPosition;
            BoardController.Instance.CurrentPlayerPosition(currentPosition);
        }
        isMoving = false;
        Debug.Log("currentPosition " + currentPosition);
    }
    private void CheckIfPlayerIsCurrentPlayer(int moneyPlayerNew)
    {
        if (this == GameController.Instance.GetCurrentPlayer())
        {
            if (BoardController.Instance.GetPanelForCardInfoIsCreated())
            {
                int sum = BoardController.Instance.SumCardCostForOpenInfo();

                if (PlayerHasEnoughMoneyToUpgrade(sum))
                {
                    Debug.Log("Player can upgrade even when money update");
                    BoardController.Instance.TurnOffButtonsUpgradeDemote(true, true);
                }
            }
        }
    }
    public void BuyCard(int num, int sum)
    {
        Card card = BoardController.Instance.ReturnCardObject(num);
        moneyPlayer -= sum;
        card.SetPlayerOwner(this);
        card.ShowHideCardPriceText(false);
    }

    public void SellCard(int cardPrice, int index)
    {
        Card card = BoardController.Instance.ReturnCardObject(index);
        moneyPlayer += cardPrice / 2;//При продаже возвращается только 50% от стоимости клетки
        card.SetPlayerOwner(null);
        card.ShowHideCardPriceText(true);
        Debug.Log("Карта: " + card + " продана, текущий владелец " + card.GetPLayerOwner());
    }

    public void AuctionCard()
    {

    }
    public void PayRent(int index, Card card)
    {
        Player OwnerCardPlayer = card.GetPLayerOwner();
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
        moneyPlayer -= sumToPay;
        OwnerCardPlayer.moneyPlayer += sumToPay;
        GameController.Instance.UpdatePlayersMoneyInfoOnTable();
        StartCoroutine(VerifyPlayerMoney());//Чтобы если баланс отрицательный то пользователь должен продать или обанкротится
    }

    private IEnumerator VerifyPlayerMoney()
    {
        while (moneyPlayer < 0 && Bankrupt == false)
        {
            yield return null;
        }
    }
    public void PlayerBuyCardInfrastructure(int index) => PhaseRentInfrastructure++;
    public void PlayerSellCardInfrastructure(int index) => PhaseRentInfrastructure--;
    private void playerRotateModel()
    {
        transform.rotation *= Quaternion.Euler(0,90,0);
    }

    public void PlayerGotTreasure()
    {
        int treasure = UnityEngine.Random.Range(25, 325);
        moneyPlayer += treasure;
        Debug.Log("player " + playerID + " got " + treasure);
    }
    public void PlayerPayTax(int number)
    {
        int result;
        if (number == 2)//15% tax
        {
            result = moneyPlayer * 15 / 100;
        }
        else//5% tax
        {
            result = moneyPlayer * 5 / 100;
        }
            moneyPlayer -= result;
        Debug.Log("player " + playerID + " paid " + result + " for tax");
    }

    public void UpgradeOrDemoteCity(int sumToPay)
    {
        moneyPlayer -= sumToPay;
    }
    public bool PlayerHasEnoughMoneyToUpgrade(int sumToPay)
    {
        if (moneyPlayer - sumToPay >= 0)
        {
            return true;
        }
        return false;
    }
}
