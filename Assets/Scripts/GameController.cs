using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameController : NetworkBehaviour
{
    [SerializeField] private GameObject[] playerPrefabs = new GameObject[8]; //Условный список всех игроков (обьектов)
    [SerializeField] private Dictionary<int, GameObject> indexObjectPlayer;
    [SerializeField] private List<Player> players = new List<Player>(); // Список всех игроков
    [SerializeField] private List<GameObject> playersInfoTable = new List<GameObject>(); // Список всех игроков в таблице
    [SerializeField] private GameObject playersTablePref;
    [SerializeField] private Transform objectCanvas;

    [SerializeField] private Button btnStartTurn;
    [SerializeField] private Button btnEndTurn;
    [SerializeField] private Button btnWaitingUp;
    [SerializeField] private Button btnWaitingDown;
    //[SerializeField] private int stepppMyValue;

    public static GameController Instance;

    private int[] choicePlayers = new int[] { 0, 6 };//

    private int choicePlayersIndex = 0;//
    private int countPlayersAir = 1;//
    private int countPlayersGround = 1;//

    private List<Color> colorPlayers = new List<Color>();

    private float heightForAirPlayers = 2.0f;
    private float heightForGroundPlayers = 0.15f;

    private Vector3 startPositionPlayer = new Vector3(16, 0, -16);
    private Quaternion startRotationPlayer = Quaternion.Euler(0, -90, 0);

    private int startMoneyPlayer = 1500;//465
    private float boardSize;
    //private byte currentPlayerindex.Value; //Индекс текущего игрока
    private NetworkVariable<int> currentPlayerIndex = new NetworkVariable<int>(0); //Индекс текущего игрока

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
            Debug.Log("Игроку " + players[currentPlayerIndex.Value].propertyPlayerID + " выпало " + _steps);
        }
    }

    private void OnEnable()
    {
        btnStartTurn.onClick.AddListener(RollTheDices);
        btnEndTurn.onClick.AddListener(EndTurn);
        btnWaitingUp.onClick.AddListener(PlayerBuyCard);
        btnWaitingDown.onClick.AddListener(EndTurn);
    }

    private void OnDisable()
    {
        btnStartTurn.onClick.RemoveListener(RollTheDices);
        btnEndTurn.onClick.RemoveListener(EndTurn);
        btnWaitingUp.onClick.RemoveListener(PlayerBuyCard);
        btnWaitingDown.onClick.RemoveListener(EndTurn);
    }

    private void Awake()
    {
        Instance = this;
        ColorsGaveForPlayers();//
        PlacementPlayersOnTable();
    }

    public override void OnNetworkSpawn()
    {
        //currentPlayerIndex.Value = 0;
        CreatePLayers();
        boardSize = BoardController.Instance.BoardCardCount();
        BoardController.Instance.PutPriceAndNameOnCardsUI();//Инизиализация поля с текстом Карт(стоимость карты)
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayersOnBoard;
        if (IsClient && IsSpawned) // Проверяем, что объект заспавнен
        {
            currentPlayerIndex.OnValueChanged += OnTurnChanged;
        }
    }

    public override void OnDestroy()
    {
        if (IsClient && IsSpawned)
        {
            currentPlayerIndex.OnValueChanged -= OnTurnChanged;
        }
    }

    private void Update()
    {

    }
    private void OnTurnChanged(int previousValue, int newValue)
    {
        if ((ulong)newValue == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Мой ход! Показать кнопку.");
            btnTurnController(1);
        }
        else
        {
            Debug.Log("Жду хода...");
            btnTurnController(4);
        }
    }
    public Player GetCurrentPlayer() => players[currentPlayerIndex.Value];
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
                    //DiceController.Instance.SwitchSetActiveDices(true);
                    btnStartTurn.gameObject.SetActive(true);
                    btnWaitingUp.gameObject.SetActive(false);
                    btnWaitingUp.interactable = true;//off buy
                    btnWaitingDown.gameObject.SetActive(false);
                    btnEndTurn.gameObject.SetActive(false);
                    break;
                }
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
    private void CreatePLayers()
    {
        for (int i = 0; i < choicePlayers.Length; i++)
        {
            CreatePlayer(playerPrefabs[choicePlayers[i]]);
        }
    }
    private void RollTheDices()
    {
        if (IsMyTurn())
        {
            btnTurnController(4);
            //bool previousPlayer = currentPlayerIndex.Value == 0 ? !players[players.Count - 1].isMoving : !players[currentPlayerIndex.Value - 1].isMoving;
           // if (previousPlayer)
            //{
                DiceController.Instance.DropDice();
            //}
        }
        else
        {
            Debug.Log("Not my turn in rolldices");
        }
    }

    private void EndTurn()
    {
        if (IsMyTurn())
        {
            btnTurnController(4);
            NextPlayerTurnServerRpc();
        }
    }

    private void PlayerBuyCard()
    {
        if (IsMyTurn())
        {
            int num = BoardController.Instance.WhatCardNumber();
            int sum = BoardController.Instance.SumCardCost();
            players[currentPlayerIndex.Value].BuyCard(num, sum);
            UpdatePlayersMoneyInfoOnTable();
            UpdatePlayerColorCardOnBoard();//
            BoardController.Instance.BuyCityOrInfrastructureReact(num, players[currentPlayerIndex.Value]);
            BoardController.Instance.CurrentOwnerCard(num);//Debug.log
            btnTurnController(5);
        }
    }

    public void UpdatePlayerColorCardOnBoard()
    {
        BoardController.Instance.UpdateColorCardOnBoard(colorPlayers[currentPlayerIndex.Value]);
    }

    private void PlayerMakeAuction()
    {
        players[currentPlayerIndex.Value].AuctionCard();
    }

    private void MoveCurrentPlayer(int steps)
    {
        if (IsMyTurn())
        {
            StartCoroutine(MoveCurrentPlayerCoroutine(steps));
            Debug.Log("my turn");
        }
        else
        {
            Debug.Log("not my turn");
        }
    }
    private IEnumerator MoveCurrentPlayerCoroutine(int steps)
    {
        int typeButtonTurn;
        players[currentPlayerIndex.Value].isMoving = true;

        // Ждем, пока игрок завершит движение
        yield return StartCoroutine(players[currentPlayerIndex.Value].PlayerMoveCoroutine(steps));

        int currentPosition = BoardController.Instance.ReturnPLayerPosition();

        if (currentPosition != 0 && currentPosition != 10 && currentPosition != 20 && currentPosition != 30 && currentPosition != 2 && currentPosition != 5 &&
            currentPosition != 15 && currentPosition != 17 && currentPosition != 22 && currentPosition != 25 && currentPosition != 35 && currentPosition != 38)//All special cards
        {
            int sum = BoardController.Instance.SumCardCost();
            typeButtonTurn = BoardController.Instance.CheckCardBoughtOrNot(players[currentPlayerIndex.Value]);//Смотрим на какую клетку стал игрок

            if (typeButtonTurn == 2)
            {
                typeButtonTurn = CanPLayerBuyOrNot(sum);
            }

            TextMeshProUGUI text = btnWaitingUp.GetComponentInChildren<TextMeshProUGUI>(); //Обновить значения на кнопки купить
            text.text = "Buy\nfor " + sum.ToString() + "$";
        }
        else
        {
            if (currentPosition == 2 || currentPosition == 22)
            {
                players[currentPlayerIndex.Value].PlayerPayTax(currentPosition);
                UpdatePlayersMoneyInfoOnTable();
            }
            if (currentPosition == 5 || currentPosition == 17 || currentPosition == 35)
            {
                Debug.Log("Question");
            }
            if (currentPosition == 15 || currentPosition == 25 || currentPosition == 38)
            {
                players[currentPlayerIndex.Value].PlayerGotTreasure();
                UpdatePlayersMoneyInfoOnTable();
            }
            typeButtonTurn = 3;
        }
        //Debug.Log("typeButtonTurn " + typeButtonTurn);
        btnTurnController(typeButtonTurn);
    }

    private int CanPLayerBuyOrNot(int sum)//Проверка на плетежеспособность игрока, и отключение кнопки купить
    {
        if (players[currentPlayerIndex.Value].moneyPlayer - sum >= 0)
        {
            return 2;
        }
        return 5;
    }

    public void CreatePlayer(GameObject playerObject)
    {
        Player newPlayer = playerObject.GetComponent<Player>();
        //newPlayer.playerPrefab = playerObject;//
        newPlayer.propertyPlayerID++;
        newPlayer.moneyPlayer = startMoneyPlayer;
        //newPlayer.playerOffSet = new Vector3(offset.x, height, offset.z);
        players.Add(newPlayer);
        Debug.Log($"Player {newPlayer.propertyPlayerID} created and added.");
    }

    private void SpawnPlayersOnBoard(ulong clientId)
    {
        if (!IsHost)
        {
            return;
        }

        Vector3 offset;
        GameObject playerObject;

        if (playerPrefabs[choicePlayers[choicePlayersIndex]].CompareTag("Air"))
        {
            offset = PlacementStartPlayersOnBoard(countPlayersAir);
            playerObject = Instantiate(playerPrefabs[choicePlayers[choicePlayersIndex]], new Vector3(startPositionPlayer.x + offset.x, heightForAirPlayers,
                startPositionPlayer.z + offset.z), startRotationPlayer);

            players[choicePlayersIndex].playerPrefab = playerObject;//choicePlayersIndex потому что идет по порядку
            playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            Debug.Log("Air player spawned");

            //CreatePlayer(playerObject, offset, heightForAirPlayers);
            countPlayersAir++;
        }
        else /*(playerPrefabs[number[choicePlayersIndex]].CompareTag("Ground"))*/
        {
            offset = PlacementStartPlayersOnBoard(countPlayersGround);
            playerObject = Instantiate(playerPrefabs[choicePlayers[choicePlayersIndex]], new Vector3(startPositionPlayer.x + offset.x, heightForGroundPlayers,
                startPositionPlayer.z + offset.z), startRotationPlayer);

            players[choicePlayersIndex].playerPrefab = playerObject;//choicePlayersIndex потому что идет по порядку
            playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            Debug.Log("Ground player spawned");

            //CreatePlayer(playerObject, offset, heightForGroundPlayers);
            countPlayersGround++;
        }
        choicePlayersIndex++;

        if (players.Count == NetworkManager.Singleton.ConnectedClientsList.Count)
        {
            StartGame();
        }
    }
    private void StartGame()
    {
        Debug.Log("Все игроки подключены. Начинаем игру!");
        if (IsOwner == players[currentPlayerIndex.Value])
        {
            if (IsHost)
            {
                DiceController.Instance.SpawnCubesServeServerRpc();
                btnTurnController(1);
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
        Vector3 startPoint = new Vector3(-40, -135, -20);//425
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

            TextMeshProUGUI[] textComponent = obj.GetComponentsInChildren<TextMeshProUGUI>();
            if (textComponent.Length > 0)
            {
                textComponent[0].text = players[i].propertyPlayerID + "";
                textComponent[1].text = players[i].moneyPlayer + "$";
            }
        }
    }

    public void UpdatePlayersMoneyInfoOnTable()//Візуально сверху
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


    [ServerRpc]
    private void NextPlayerTurnServerRpc()
    {
        currentPlayerIndex.Value++;
        if (currentPlayerIndex.Value == players.Count)
        {
            currentPlayerIndex.Value = 0;
        }
        //DiceController.Instance.ChangeDicesOwner((ulong)currentPlayerIndex.Value);
        Debug.Log("currentPlayerIndex.Value " + currentPlayerIndex.Value);
    }

    private bool IsMyTurn()
    {
        //Debug.Log("currentPlayerIndex: " + (ulong)currentPlayerIndex.Value + " LocalClientId: " + NetworkManager.Singleton.LocalClientId);
        //return currentPlayerIndex.Value == (int)OwnerClientId;
        return (ulong)currentPlayerIndex.Value == NetworkManager.Singleton.LocalClientId;
    }
}
