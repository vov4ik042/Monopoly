using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Player : MonoBehaviour
{

    private List<CardCountry> citiesPlayer = new List<CardCountry>();
    public int moneyPlayer { get; set; }
    public int playerID { get; set; }
    public string playerName { get; set; }
    public int currentPosition { get; set; } = 0;
    public GameObject playerPrefab { get; set; }

    private float moveSpeed = 5.0f;
    private float distance = 10.0f;
    private float _stopDistance = 0.05f;

    public void Move(int steps, int boardSize)
    {
        StopAllCoroutines();
        StartCoroutine(PlayerMoveCoroutine(steps, boardSize));
    }


    private IEnumerator PlayerMoveCoroutine(int steps, int boardSize)
    {
        int finalpos = (currentPosition + steps) % boardSize;

        Vector3 goTo = ControllerPlayer.Instance.GetBoardPosition(finalpos);
        var position = transform.position;
        var absoluteDir = position - goTo;
        var dirNormalized = absoluteDir / absoluteDir.magnitude;

        while (Vector3.Distance(transform.position, goTo) > _stopDistance)
        {
            transform.position -= dirNormalized * (Time.deltaTime * moveSpeed);
            yield return null;
        }

        currentPosition = finalpos;
    }
}
