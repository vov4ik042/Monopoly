using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Player : MonoBehaviour
{

    public static int playerCounter = 0;
    public static int lol = 0;
    private List<Card> citiesPlayer = new List<Card>();

    private int _moneyPlayer;

    public int moneyPlayer
    {
        get { return _moneyPlayer; }
        set { _moneyPlayer = value; }
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
/*    public void Move(int steps)
    {
        StartCoroutine(PlayerMoveCoroutine(steps));
    }*/

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

    public void BuyCard(int num, int sum)
    {
        Card card = BoardController.Instance.ReturnCardObject(num);
        this.moneyPlayer -= sum;
        GameController.Instance.UpdatePlayersMoneyInfo();
        card.PLayerOwner = this;
        card.HideCardPriceText();
    }
    public void AuctionCard()
    {

    }
    public void PayRent()
    {

    }

    private void playerRotateModel()
    {
        transform.rotation *= Quaternion.Euler(0,90,0);
    }
}
