using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UniRx;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [SerializeField] private MeshRenderer MeshRenderer1;
    [SerializeField] private MeshRenderer MeshRenderer2;

    private Material material;

    private ReactiveProperty<int> _moneyPlayer = new ReactiveProperty<int>();
    private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
    private NetworkVariable<int> currentPosition = new NetworkVariable<int>();

    private Player thisPlayer;
    public GameObject playerPref;
    public int moneyPlayer
    {
        get { return _moneyPlayer.Value; }
        set { _moneyPlayer.Value = value; }
    }

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

    public static int playerCounter = 0;
    private Vector3 startPositionPlayer = new Vector3(0, 0, 0);
    private int PhaseRentInfrastructure { get; set; } = 0;
    public bool Bankrupt { get; set; } = false;
    //public Vector3 playerOffSet { get; set; }

    private void Awake()
    {
        material = new Material(MeshRenderer1.material);
        MeshRenderer1.material = material;
        MeshRenderer2.material = material;
        startPositionPlayer = transform.position;
    }
    public override void OnNetworkSpawn()
    {
        //Debug.Log($"[{NetworkManager.LocalClientId}] IsOwner: {IsOwner}, playerPrefab: {playerPrefab}, This Object: {gameObject}");
        //MakeDefaultcurrentPositionServerRpc();
        //_moneyPlayer.Subscribe(CheckIfPlayerIsCurrentPlayer).AddTo(_compositeDisposable);
    }
    public void SetDefaultcurrentPosition()
    {
        currentPosition.Value = 0;
    }
    [ServerRpc(RequireOwnership = false)]
    public void MakeDefaultcurrentPositionServerRpc()
    {
        currentPosition.Value = 0;
        UpdateMakeDefaultcurrentPositionClientRpc();
    }
    [ClientRpc]
    private void UpdateMakeDefaultcurrentPositionClientRpc()
    {
        currentPosition.Value = 0;
    }

    public void SetPlayerColor(Color color, int playerId)
    {
        material.color = color;
        SetPlayerColorClientRpc(playerId);
    }
    [ClientRpc]
    public void SetPlayerColorClientRpc(int playerId)
    {
        Color color = MonopolyMultiplayer.Instance.GetPlayerColorFromPlayerId(playerId);
        material.color = color;
    }

    public void SetThisPlayer(Player player)
    {
        thisPlayer = player;
    }
    public int GetPhaseRentInfrastructure() => PhaseRentInfrastructure;
    public IEnumerator PlayerMoveCoroutine(int steps)
    {
        float moveDuration = .7f;

        for (int i = 0; i < steps; i++)
        {
            //Debug.Log("pos: " + currentPosition.Value);
            Vector3 startPosition = playerPref.transform.position;
            float elapsedTime = 0f;

            int nextPosition = currentPosition.Value + 1;
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

            if (currentPosition.Value == 0 || currentPosition.Value == 10 || currentPosition.Value == 20 || currentPosition.Value == 30) //Обновление позиции рассчета имеено после предыдущих вычеслений
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

            //goTo.y = playerOffSet.y;

            while (elapsedTime < moveDuration)
            {
                //Debug.Log("pos: " + playerPrefab.transform.position);
                playerPref.transform.position = Vector3.Lerp(startPosition, goTo, elapsedTime/moveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            playerPref.transform.position = goTo;
            currentPosition.Value = nextPosition;
            BoardController.Instance.CurrentPlayerPosition(currentPosition.Value);
        }
        //isMoving = false;
        Debug.Log("currentPosition " + currentPosition.Value);
    }

    /*[ClientRpc]
    private void UpdatePlayerPositionClientRpc(Vector3 newPosition, int newCurrentPosition)
    {
        playerPref.transform.position = newPosition;
        currentPosition.Value = newCurrentPosition;
        BoardController.Instance.CurrentPlayerPosition(newCurrentPosition);
    }*/
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
