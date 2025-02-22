using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabsPlayers = new GameObject[8]; //Условный список всех игроков (обьектов)
    [SerializeField] private Dictionary<int, GameObject> indexObjectPlayer;
    [SerializeField] private List<Player> players = new List<Player>(); // Список всех игроков
    [SerializeField] private List<GameObject> playersInfoTable = new List<GameObject>(); // Список всех игроков в таблице
    [SerializeField] private GameObject playersTablePref;
    [SerializeField] private Transform objectCanvas;

    [SerializeField] private Button btnStartTurn;
    [SerializeField] private Button btnEndTurn;
    [SerializeField] private Button btnWaitingUp;
    [SerializeField] private Button btnWaitingDown;
    [SerializeField] private int stepppMyValue;

    public static GameController Instance;
    private int[] choicePlayers = new int[] { 0, 1 };//
    private List<Color> colorPlayers = new List<Color>();
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
            _steps = stepppMyValue;
            MoveCurrentPlayer(_steps);
            Debug.Log("Игроку " + players[currentPlayerindex].propertyPlayerID + " выпало " + _steps);
        }
    }

    private void OnEnable()
    {
        btnStartTurn.onClick.AddListener(RollTheDices);
        btnEndTurn.onClick.AddListener(EndTurn);
        btnWaitingUp.onClick.AddListener(PlayerBuyCard);
        btnWaitingDown.onClick.AddListener(EndTurn);
    }

    private void Awake()
    {
        ColorsGaveForPlayers();//
        btnTurnController(1);
        Instance = this;
        CreatePlayersOnBoard(choicePlayers);
        PlacementPlayersOnTable();
    }

    private void Start()
    {
        //BoardController.Instance.AddCardsToListAndInitialize();
        boardSize = BoardController.Instance.BoardCardCount();
        currentPlayerindex = 0;
        BoardController.Instance.PutPriceOnCardsUI();//Инизиализация поля с текстом Карт(стоимость карты)
    }

    private void Update()
    {

    }

    public void ViewPhaseRentInfrastructure(int i)
    {
        Debug.Log("PhaseRentInfrastructure " + i);
    }

    public Player GetCurrentPlayer() => players[currentPlayerindex];
    private void ColorsGaveForPlayers()
    {
        colorPlayers.Add(Color.red);
        colorPlayers.Add(Color.green);
        colorPlayers.Add(Color.blue);
        colorPlayers.Add(Color.black);
        colorPlayers.Add(Color.yellow);
        colorPlayers.Add(Color.white);
    }
    private void btnTurnController(int phase)
    {
        switch (phase)
        {
            case 1://DiceRoll
                {
                    btnStartTurn.gameObject.SetActive(true);
                    btnWaitingUp.gameObject.SetActive(false);
                    btnWaitingUp.interactable = true;//off buy
                    btnWaitingDown.gameObject.SetActive(false);
                    btnEndTurn.gameObject.SetActive(false);
                    break;
                }
            /*case 2://BuyOrAuction
                {
                    btnStartTurn.gameObject.SetActive(false);
                    btnWaitingUp.gameObject.SetActive(true);
                    btnWaitingDown.gameObject.SetActive(true);
                    btnEndTurn.gameObject.SetActive(false);
                    break;
                }*/
            case 2://BuyOrEndTurn
                {
                    btnStartTurn.gameObject.SetActive(false);
                    btnWaitingUp.gameObject.SetActive(true);
                    btnWaitingDown.gameObject.SetActive(true);
                    btnEndTurn.gameObject.SetActive(false);
                    break;
                }
            case 3://EntTurn
                {
                    btnStartTurn.gameObject.SetActive(false);
                    btnWaitingUp.gameObject.SetActive(false);
                    btnWaitingDown.gameObject.SetActive(false);
                    btnEndTurn.gameObject.SetActive(true);
                    break;
                }
            case 4://Disable all
                {
                    btnStartTurn.gameObject.SetActive(false);
                    btnWaitingUp.gameObject.SetActive(false);
                    btnWaitingDown.gameObject.SetActive(false);
                    btnEndTurn.gameObject.SetActive(false);
                    break;
                }
            case 5://Player cant buy
                {
                    btnStartTurn.gameObject.SetActive(false);
                    btnWaitingUp.gameObject.SetActive(true);
                    btnWaitingUp.interactable = false;//off buy
                    btnWaitingDown.gameObject.SetActive(true);
                    btnEndTurn.gameObject.SetActive(false);
                    break;
                }
        }
    }

    private void RollTheDices()
    {
        btnTurnController(4);
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

    private void PlayerBuyCard()
    {
        int num = BoardController.Instance.WhatCardNumber();
        int sum = BoardController.Instance.SumCardCost();
        players[currentPlayerindex].BuyCard(num, sum);
        UpdatePlayersMoneyInfo();
        UpdatePlayerColorCardOnBoard();//
        BoardController.Instance.BuyCityOrInfrastructureReact(num, players[currentPlayerindex]);
        BoardController.Instance.CurrentOwnerCard(num);//Debug.log
        btnTurnController(5);
    }

    public void UpdatePlayerColorCardOnBoard()
    {
        BoardController.Instance.UpdateColorCardOnBoard(colorPlayers[currentPlayerindex]);
    }

    private void PlayerMakeAuction()
    {
        players[currentPlayerindex].AuctionCard();
    }

    private void MoveCurrentPlayer(int steps)
    {
        StartCoroutine(MoveCurrentPlayerCoroutine(steps));
    }
    private IEnumerator MoveCurrentPlayerCoroutine(int steps)
    {
        int typeButtonTurn;
        players[currentPlayerindex].isMoving = true;

        // Ждем, пока игрок завершит движение
        yield return StartCoroutine(players[currentPlayerindex].PlayerMoveCoroutine(steps));

        int currentPosition = BoardController.Instance.ReturnPLayerPosition();

        if (currentPosition != 0 && currentPosition != 10 && currentPosition != 20 && currentPosition != 30 && currentPosition != 2 && currentPosition != 5 &&
            currentPosition != 15 && currentPosition != 17 && currentPosition != 22 && currentPosition != 25 && currentPosition != 35 && currentPosition != 38)//All special cards
        {
            int sum = BoardController.Instance.SumCardCost();
            typeButtonTurn = BoardController.Instance.CheckCardBoughtOrNot(players[currentPlayerindex]);//Смотрим на какую клетку стал игрок

            if (typeButtonTurn == 2)
            {
                typeButtonTurn = CanPLayerBuyOrNot(sum);
            }

            TextMeshProUGUI text = btnWaitingUp.GetComponentInChildren<TextMeshProUGUI>(); //Обновить значения на кнопки купить
            text.text = "Buy\nfor " + sum.ToString() + "$";
        }
        else
        {
            typeButtonTurn = 3;
            Debug.Log("Special Card");
        }
        //Debug.Log("typeButtonTurn " + typeButtonTurn);
        btnTurnController(typeButtonTurn);
    }

    private int CanPLayerBuyOrNot(int sum)//Проверка на плетежеспособность игрока, и отключение кнопки купить
    {
        if (players[currentPlayerindex].moneyPlayer - sum > 0)
        {
            return 2;
        }
        return 5;
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
            playersInfoTable.Add(obj);

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

    public void UpdatePlayersMoneyInfo()//Візуально сверху
    {
        for (int i = 0; i < playersInfoTable.Count; i++)
        {
            TextMeshProUGUI[] textComponent = playersInfoTable[i].GetComponentsInChildren<TextMeshProUGUI>();
            if (textComponent.Length > 0)
            {
                textComponent[1].text = players[i].moneyPlayer + "$";
            }
        }
    }
}
