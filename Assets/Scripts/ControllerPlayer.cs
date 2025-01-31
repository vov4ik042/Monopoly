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
    private int[] choicePlayers = new int[] { 5,0,1,2,7 };//
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
        CreatePlayersOnBoard(choicePlayers);
        //indexObjectPlayer.Add(123, prefabsPlayers[0]); //
    }
    private void Start()
    {
        boardSize = boardCardPositions.Count;
        currentPlayerindex = 0;
        //CreatePlayer();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentPlayerindex == 0 || !players[currentPlayerindex - 1].isMoving)
            {
                RollDice();
                MoveCurrentPlayer(steps);
            }

        }
    }
    private void RollDice()
    {
        steps = Random.Range(1, 13); //Розрахунок дальності ходу (від 1 до 12)
        Debug.Log("Игроку " + players[currentPlayerindex].propertyID + " выпало " + steps);
    }

    private void MoveCurrentPlayer(int steps)
    {

        if (currentPlayerindex == 3)
        {

        }

        players[currentPlayerindex].isMoving = true;
        players[currentPlayerindex].Move(steps, boardCardPositions.Count);

        currentPlayerindex++;
        if (currentPlayerindex == players.Count)
        {
            currentPlayerindex = 0;
        }
    }
    public void CreatePlayer(GameObject playerObject, Vector3 offset, float height)
    {
        Player newPlayer = playerObject.AddComponent<Player>();
        newPlayer.propertyID++;
        newPlayer.playerOffSet = new Vector3(offset.x, height, offset.z);
        players.Add(newPlayer);
        Debug.Log($"Player {newPlayer.propertyID} created and added.");
    }

    private void CreatePlayersOnBoard(int[] number)
    {
        int countPlayersAir = 0;
        int countPlayersGround = 0;

        Vector3 offset;

        for (int i = 0; i < number.Length; i++)
        {
            GameObject playerObject;
            if (prefabsPlayers[number[i]].CompareTag("Air"))
            {
                offset = PlacementStartPlayersOnBoard(countPlayersAir);
                playerObject = Instantiate(prefabsPlayers[number[i]], new Vector3(startPositionPlayer.x + offset.x, heightForAirPlayers,
                    startPositionPlayer.z + offset.z), startRotationPlayer);
                CreatePlayer(playerObject, offset, heightForAirPlayers);
                countPlayersAir++;
            }
            if(prefabsPlayers[number[i]].CompareTag("Ground"))
            {
                offset = PlacementStartPlayersOnBoard(countPlayersGround);
                playerObject = Instantiate(prefabsPlayers[number[i]], new Vector3(startPositionPlayer.x + offset.x, heightForGroundPlayers,
                    startPositionPlayer.z + offset.z), startRotationPlayer);
                CreatePlayer(playerObject, offset, heightForGroundPlayers);
                countPlayersGround++;
            }
        }
    }

    private Vector3 PlacementStartPlayersOnBoard(int phase)
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
