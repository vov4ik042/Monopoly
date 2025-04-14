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
    public GameObject playerPref;

    private NetworkVariable<int> _moneyPlayer = new NetworkVariable<int>(0);
    private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
    private NetworkVariable<int> currentPosition = new NetworkVariable<int>(0);

    public int playerID { get; set; }

    public static int playerCounter = 0;
    private int PhaseRentInfrastructure { get; set; } = 0;
    public bool Bankrupt { get; set; } = false;

    private void Awake()
    {
        material = new Material(MeshRenderer1.material);
        MeshRenderer1.material = material;
        MeshRenderer2.material = material;
    }

    public override void OnNetworkSpawn()
    {
        //Debug.Log($"[{NetworkManager.LocalClientId}] IsOwner: {IsOwner}, playerPrefab: {playerPrefab}, This Object: {gameObject}");
        //MakeDefaultcurrentPositionServerRpc();
        //_moneyPlayer.Subscribe(CheckIfPlayerIsCurrentPlayer).AddTo(_compositeDisposable);
    }

    public void SetPlayerMoney(int value)
    {
        _moneyPlayer.Value += value;
        MonopolyMultiplayer.Instance.SetPlayerMoneyServerRpc(playerID, _moneyPlayer.Value);
    }
    public int GetPlayerMoney()
    { 
        return _moneyPlayer.Value;
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

    public int GetPhaseRentInfrastructure() => PhaseRentInfrastructure;
    public IEnumerator PlayerMoveCoroutine(int steps)
    {
        float playerHeight = 0.16f;
        float moveDuration = .6f;

        for (int i = 0; i < steps; i++)
        {
            Vector3 startPosition = playerPref.transform.position;
            float elapsedTime = 0f;

            int nextPosition = currentPosition.Value + 1;

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
                playerPref.transform.position = Vector3.Lerp(startPosition, goTo, elapsedTime/moveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            playerPref.transform.position = goTo;
            currentPosition.Value = nextPosition;
            BoardController.Instance.CurrentPlayerPosition(currentPosition.Value);
        }
        //Debug.Log("currentPosition " + currentPosition.Value);
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

    public void BuyCard(int cardIndex, int cardCost, Player player)
    {
        Card card = BoardController.Instance.ReturnCardObject(cardIndex);
        SetPlayerMoney(-cardCost);
        card.SetPlayerOwner(player);
        card.ShowHideCardPriceText(false);
    }

    public void SellCard(int cardPrice, int index)
    {
        Card card = BoardController.Instance.ReturnCardObject(index);
        SetPlayerMoney(cardPrice / 2);//При продаже возвращается только 50% от стоимости клетки
        card.SetPlayerOwner(null);
        card.ShowHideCardPriceText(true);
        Debug.Log("Карта: " + card + " продана, текущий владелец " + card.GetPlayerOwner());
    }

    public void AuctionCard()
    {

    }
    public void PayRent(int index, Card card)
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
        //TablePlayersUI.Instance.UpdateInfo();
        StartCoroutine(VerifyPlayerMoney());//Чтобы если баланс отрицательный то пользователь должен продать или обанкротится
    }

    private IEnumerator VerifyPlayerMoney()
    {
        while (GetPlayerMoney() < 0 && Bankrupt == false)
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
