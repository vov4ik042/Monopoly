using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControllerPlayer : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabsPlayers = new GameObject[8]; //Условный список всех игроков (обьектов) пока
    [SerializeField] private List<Transform> boardCardPositions;
    [SerializeField] private Dictionary<int, GameObject> indexObjectPlayer;
    [SerializeField] private List<Player> players = new List<Player>(); // Список всех игроков
    [SerializeField] private GameObject playersTablePref;
    [SerializeField] private Transform objectCanvas;

    [SerializeField] private Button btnStartTurn;
    [SerializeField] private Button btnEndTurn;
    [SerializeField] private Button btnWaiting;

    public static ControllerPlayer Instance;
    private int[] choicePlayers = new int[] { 0,1,2,3,4,5 };//
    private float heightForAirPlayers = 2.0f;
    private float heightForGroundPlayers = 0.15f;
    //private Vector3 startPositionPlayer = new Vector3(14.5f, 0, -15);
    private Vector3 startPositionPlayer = new Vector3(16, 0, -16);
    private Quaternion startRotationPlayer = Quaternion.Euler(0, -90, 0);
    private int startMoneyPlayer = 1500;
    private float boardSize;
    private byte currentPlayerindex; //Индекс текущего игрока

    private int _steps;
    public int steps 
    {
        get
        {
            return _steps;
        }
        set
        {
            _steps = value;
            MoveCurrentPlayer(_steps);
            Debug.Log("Игроку " + players[currentPlayerindex].propertyPlayerID + " выпало " + _steps);
        }
    }

    private void OnEnable()
    {
        btnStartTurn.onClick.AddListener(RollTheDices);
        btnEndTurn.onClick.AddListener(EndTurn);
    }

    private void Awake()
    {
        btnTurnController(1);
        Instance = this;
        CreatePlayersOnBoard(choicePlayers);
        PlacementPlayersOnTable();
    }

    private void Start()
    {
        boardSize = boardCardPositions.Count;
        currentPlayerindex = 0;
    }

    private void Update()
    {
 
    }
    private void btnTurnController(int phase)
    {
        switch (phase)
        {
            case 1:
                {
                    btnStartTurn.gameObject.SetActive(true);
                    btnWaiting.gameObject.SetActive(false);
                    btnEndTurn.gameObject.SetActive(false);
                    break;
                }
            case 2:
                {
                    btnStartTurn.gameObject.SetActive(false);
                    btnWaiting.gameObject.SetActive(true);
                    btnEndTurn.gameObject.SetActive(false);
                    break;
                }
            case 3:
                {
                    btnStartTurn.gameObject.SetActive(false);
                    btnWaiting.gameObject.SetActive(false);
                    btnEndTurn.gameObject.SetActive(true);
                    break;
                }
        }
    }

    private void RollTheDices()
    {
        btnTurnController(2);

        bool previousPlayer = currentPlayerindex == 0 ? !players[players.Count - 1].isMoving : !players[currentPlayerindex - 1].isMoving;
        if (previousPlayer)
        {
            DiceController.Instance.DropDice();
        }
    }

    private void EndTurn()
    {
        btnTurnController(1);

        currentPlayerindex++;
        if (currentPlayerindex == players.Count)
        {
            currentPlayerindex = 0;
        }
        Debug.Log("currentPlayerindex " + currentPlayerindex);
    }

    private void MoveCurrentPlayer(int steps)
    {
        StartCoroutine(MoveCurrentPlayerCoroutine(steps));
    }
    private IEnumerator MoveCurrentPlayerCoroutine(int steps)
    {
        players[currentPlayerindex].isMoving = true;

        // Ждем, пока игрок завершит движение
        yield return StartCoroutine(players[currentPlayerindex].PlayerMoveCoroutine(steps, boardCardPositions.Count));

        btnTurnController(3);
    }


    public void CreatePlayer(GameObject playerObject, Vector3 offset, float height)
    {
        Player newPlayer = playerObject.AddComponent<Player>();
        newPlayer.playerPrefab = playerObject;
        newPlayer.propertyPlayerID++;
        newPlayer.moneyPlayer = startMoneyPlayer;
        newPlayer.playerOffSet = new Vector3(offset.x, height, offset.z);
        players.Add(newPlayer);
        Debug.Log($"Player {newPlayer.propertyPlayerID} created and added.");
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

    private void PlacementPlayersOnTable()
    {
        Vector3 startPoint = new Vector3(-40, 425, -20);
        float offsetY = 60.0f;
        int j = 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (j == 2)
            {
                offsetY += 125.0f;// Дистанция для обьектов, по два
                j = 1;
            }
            else j++;

            Vector2 position = new Vector2(startPoint.x + ((i % 2 == 0) ? offsetY : -offsetY), startPoint.y);
            GameObject obj = Instantiate(playersTablePref, objectCanvas);

            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            //rectTransform.localScale = Vector3.one;

            TextMeshProUGUI[] textComponent = obj.GetComponentsInChildren<TextMeshProUGUI>();
            if (textComponent.Length > 0)
            {
                textComponent[0].text = players[i].propertyPlayerID + "";
                textComponent[1].text = players[i].moneyPlayer + "$";
            }
            //Debug.Log(i + " " + position);
        }
    }

    public Vector3 GetBoardPosition(int index)
    {
        return boardCardPositions[index].position;
    }

}
