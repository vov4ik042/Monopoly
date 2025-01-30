using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class ControllerPlayer : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabsPlayers = new GameObject[8]; //Условный список всех игроков (обьектов) пока
    [SerializeField] private List<Transform> boardCardPositions;
    [SerializeField] private Dictionary<int, GameObject> indexObjectPlayer;
    public List<Player> players = new List<Player>(); // Список всех игроков

    public static ControllerPlayer Instance;
    private int[] choicePlayers = new int[] { 0 };//
    private float heightForAirPlayers = 2.0f;
    private float heightForGroundPlayers = 0.15f;
    private Vector3 startPositionPlayer = new Vector3(14.5f, 0, -15);
    private Quaternion startRotationPlayer = Quaternion.Euler(0, -90, 0);
    private int steps;
    private int boardSize;
    private byte currentPlayerindex; //Индекс текущего игрока

    private void Awake()
    {
        Instance = this;
        //createPlayersOnBoard(choicePlayers);
        //indexObjectPlayer.Add(123, prefabsPlayers[0]); //
    }
    private void Start()
    {
        boardSize = boardCardPositions.Count;
        currentPlayerindex = 0;
        CreatePlayer();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RollDice();
            MoveCurrentPlayer(steps);
            //StartCoroutine(Move());
            //FirstMoveEffect(0);
        }
    }
    private void RollDice()
    {
        steps = Random.Range(1, 13); //Розрахунок дальності ходу (від 1 до 12)
        Debug.Log("Dice rolled a number " + steps);
    }
    public void CreatePlayer()
    {
        GameObject playerObject = Instantiate(prefabsPlayers[currentPlayerindex], startPositionPlayer, startRotationPlayer);
        Player newPlayer = playerObject.AddComponent<Player>();

        /*Player newPlayer = new Player();
        newPlayer.playerID = currentPlayerindex;
        newPlayer.playerPrefab = Instantiate(prefabsPlayers[currentPlayerindex], new Vector3(0, 0, 0), Quaternion.identity);
        newPlayer.playerPrefab.SetActive(true);*/
        players.Add(newPlayer);
        Debug.Log($"Player {newPlayer.playerID} created and added.");
    }

    private void MoveCurrentPlayer(int steps)
    {
        if(players.Count == 0)
        {
            Debug.LogError("Нет игроков в списке!");
            return;
        }
        if (players[currentPlayerindex] == null)
        {
            Debug.LogError(currentPlayerindex);
            Debug.LogError("Текущий игрок равен null!");
            return;
        }

        /*Player currentPlayer = players[currentPlayerindex];
        currentPlayer.Move(steps, boardCardPositions.Count);*/

        players[currentPlayerindex].Move(steps, boardCardPositions.Count);

        /*currentPlayerindex++;

        if(currentPlayerindex == players.Count)
        {
            currentPlayerindex = 0;
        }*/
    }

    private void createPlayersOnBoard(int[] number)
    {
        int countPlayersAir = 0;
        int countPlayersGround = 0;

        Vector3 offset = new Vector3(0, 0, 0);

        for (int i = 0; i < number.Length; i++)
        {
            if (number[i] <= 3) //Air Players
            {
                offset = placementStartPlayersOnBoard(countPlayersAir);
                Instantiate(prefabsPlayers[number[i]], new Vector3(startPositionPlayer.x + offset.x, heightForAirPlayers, startPositionPlayer.z + offset.z), startRotationPlayer);
                countPlayersAir++;
            }
            else //Ground PLayers
            {
                offset = placementStartPlayersOnBoard(countPlayersGround);
                Instantiate(prefabsPlayers[number[i]], new Vector3(startPositionPlayer.x + offset.x, heightForGroundPlayers, startPositionPlayer.z + offset.z), startRotationPlayer);
                countPlayersGround++;
            }
        }
    }

    private Vector3 placementStartPlayersOnBoard(int phase)
    {
        float offsetBack = 3f;
        float offsetLeft = 2f;
        Vector3 mass = new Vector3(0, 0, 0);
        switch (phase)
        {
            case 1:
                {
                    mass.z = -offsetLeft;
                    break;
                }
            case 2:
                {
                    mass.x = offsetBack; mass.z = -offsetLeft;
                    break;
                }
            case 3:
                {
                    mass.x = offsetBack;
                    break;
                }
        }
        return mass;
    }

    public Vector3 GetBoardPosition(int index)
    {
        return boardCardPositions[index].position;
    }

}
