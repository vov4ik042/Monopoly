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
    private int[] choicePlayers = new int[] { 0,1,2,3,4,5,6,7 };//
    private float heightForAirPlayers = 2.0f;
    private float heightForGroundPlayers = 0.15f;
    //private Vector3 startPositionPlayer = new Vector3(14.5f, 0, -15);
    private Vector3 startPositionPlayer = new Vector3(16, 0, -16);
    private Quaternion startRotationPlayer = Quaternion.Euler(0, -90, 0);
    private int steps;
    private float boardSize;
    private byte currentPlayerindex; //Индекс текущего игрока

    private void Awake()
    {
        Instance = this;
        CreatePlayersOnBoard(choicePlayers);
    }
    private void Start()
    {
        boardSize = boardCardPositions.Count;
        currentPlayerindex = 0;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool previousPlayer = currentPlayerindex == 0 ? !players[players.Count - 1].isMoving : !players[currentPlayerindex - 1].isMoving;
            if (previousPlayer)
            {
                steps = DiceController.Instance.startThrow();
                Debug.Log("Игроку " + players[currentPlayerindex].propertyID + " выпало " + steps);
                //MoveCurrentPlayer(steps);
            }
        }
    }

    private void MoveCurrentPlayer(int steps)
    {
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
        int countPlayersAir = 1;
        int countPlayersGround = 1;

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

        float offsetX = 1.3f;
        float offsetZ = 1.3f;
        Vector3 result = new Vector3(0, 0, 0);

        switch (phase)
        {
            case 1:
                {
                    result.z += offsetZ;
                    result.x -= offsetX;
                    break;
                }
            case 2:
                {
                    result.z += offsetZ;
                    result.x += offsetX;
                    break;
                }
            case 3:
                {
                    result.z -= offsetZ;
                    result.x += offsetX;
                    break;
                }
            case 4:
                {
                    result.z -= offsetZ;
                    result.x -= offsetX;
                    break;
                }
        }
        return result;
    }

    public Vector3 GetBoardPosition(int index)
    {
        return boardCardPositions[index].position;
    }

}
