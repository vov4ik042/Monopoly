using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Player : MonoBehaviour
{

    public static int playerCounter = 0;
    public static int lol = 0;
    private List<CardCountry> citiesPlayer = new List<CardCountry>();
    public int moneyPlayer { get; set; }
    public bool isMoving { get; set; }

    private int playerID;
    public int propertyID
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
    public void Move(int steps, int boardSize)
    {
        StartCoroutine(PlayerMoveCoroutine(steps, boardSize));
    }

    private IEnumerator PlayerMoveCoroutine(int steps, int boardSize)
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
            Vector3 goTo = ControllerPlayer.Instance.GetBoardPosition(nextPosition);

            if (nextPosition == 0 || nextPosition == 20)//Для сохранения своей линии относительно клетки
            {
                goTo.z = -startPositionPlayer.z;
            }
            if (nextPosition == 10 || nextPosition == 30)
            {
                goTo.x = -startPositionPlayer.x;
            }

            if (currentPosition == 0 || currentPosition == 10 || currentPosition == 20 || currentPosition == 30)//Обновление позиции рассчета имеено после предыдущих вычеслений
            {
                startPositionPlayer = startPosition;
            }

            if ((nextPosition > 10 && nextPosition <= 20) || (nextPosition > 30 && nextPosition < 40) || nextPosition == 0)//Фиксирование X оси на двух сторонах доски
            {
                goTo.x = startPositionPlayer.x;
            }
            if ((nextPosition > 0 && nextPosition <= 10) || (nextPosition > 20 && nextPosition <= 30))//Фиксирование Z оси на двух сторонах доски
            {
                goTo.z = startPositionPlayer.z;
            }
            //Debug.Log("goTo" + goTo + "startPositionPlayer" + startPositionPlayer);

            goTo.y = playerOffSet.y;

            // Двигаемся к следующей клетке
            while (elapsedTime < moveDuration)
            {
                //transform.position = Vector3.MoveTowards(transform.position, goTo, moveSpeed * Time.deltaTime);
                transform.position = Vector3.Lerp(startPosition, goTo, elapsedTime/moveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = goTo;
            currentPosition = nextPosition;
        }
        isMoving = false;
    }

    private void playerRotateModel()
    {
        transform.rotation *= Quaternion.Euler(0,90,0);
    }
}
