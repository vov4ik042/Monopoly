using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Player : MonoBehaviour
{

    public static int playerCounter = 0;
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

    private float moveSpeed = 5.0f;
    private float distance = 10.0f;
    private float _stopDistance = 0.01f;


    public void Move(int steps, int boardSize)
    {
        StopAllCoroutines();
        StartCoroutine(PlayerMoveCoroutine(steps, boardSize));
    }

    public void IncreaseId()
    {
        playerCounter++;
        playerID = playerCounter;
    }

    private IEnumerator PlayerMoveCoroutine(int steps, int boardSize)
    {
        /*for (int i = 1; i <= steps; i++)
        {
            int finalpos = (currentPosition + i) % boardSize;

            Vector3 goTo = ControllerPlayer.Instance.GetBoardPosition(finalpos) + playerOffSet;
            //goTo.y = playerOffSet.y;
            var position = transform.position;
            var absoluteDir = position - goTo;
            var dirNormalized = absoluteDir / absoluteDir.magnitude;

            while (Vector3.Distance(transform.position, goTo) > _stopDistance)
            {
                transform.position -= dirNormalized * (Time.deltaTime * moveSpeed);
                //transform.position = new Vector3(transform.position.x, playerOffSet.y, transform.position.z);
                yield return null;
            }
            transform.position = goTo;
            currentPosition = finalpos; 
        }*/
        for (int i = 0; i < steps; i++)
        {
            // Вычисляем следующую клетку (по кругу)
            int nextPosition = (currentPosition + 1) % boardSize;
            Vector3 goTo = ControllerPlayer.Instance.GetBoardPosition(nextPosition) + playerOffSet;

            // Дополнительно фиксируем высоту
            goTo.y = playerOffSet.y;

            // Двигаемся к следующей клетке
            while (Vector3.Distance(transform.position, goTo) > _stopDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, goTo, moveSpeed * Time.deltaTime);
                yield return null;
            }

            // Фиксируем точную позицию на клетке
            transform.position = goTo;
            currentPosition = nextPosition;
        }
        isMoving = false;
    }
}
