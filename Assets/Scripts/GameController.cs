using System.Collections;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Linq;
public class GameController : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Dictionary<int, GameObject> indexObjectPlayer;
    [SerializeField] private Transform objectCanvas;

    [SerializeField] private Button btnStartTurn;
    [SerializeField] private Button btnEndTurn;
    [SerializeField] private Button btnWaitingUp;
    [SerializeField] private Button btnWaitingDown;

    public static GameController Instance;
    private int startMoneyPlayer = 1500;//465

    private ObservableCollection<Player> players = new ObservableCollection<Player>(); // Список всех игроков
    private NetworkVariable<int> currentPlayerIndex = new NetworkVariable<int>(0); //Индекс текущего игрока
    private ReactiveProperty<int> steps = new ReactiveProperty<int>();//Кол-во клеток перемещения
    private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

    private Player cachedCurrentPlayer; // Кэш для клиента

    public event EventHandler AllClientsConnected;
    private int PlayersConnectedCountServer;

    private void OnEnable()
    {
        btnStartTurn.onClick.AddListener(RollTheDicesServerRpc);
        btnEndTurn.onClick.AddListener(EndTurn);
        btnWaitingUp.onClick.AddListener(PlayerBuyCard);
        btnWaitingDown.onClick.AddListener(EndTurn);
    }

    private void OnDisable()
    {
        btnStartTurn.onClick.RemoveListener(RollTheDicesServerRpc);
        btnEndTurn.onClick.RemoveListener(EndTurn);
        btnWaitingUp.onClick.RemoveListener(PlayerBuyCard);
        btnWaitingDown.onClick.RemoveListener(EndTurn);
    }

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted += NetworkManager_OnLoadEventCompleted;
        Instance.AllClientsConnected += GameController_AllClientsConnected;

        steps.Skip(1).Subscribe(MovePLayer).AddTo(_compositeDisposable);

        /*if (IsClient && IsSpawned) // Проверяем, что объект заспавнен
        {
            currentPlayerIndex.OnValueChanged += OnTurnChanged;
        }*/
    }

    private void GameController_AllClientsConnected(object sender, EventArgs e)
    {
        Debug.Log("GameController_AllClientsConnected called");
        if (IsServer)
        {
            Debug.Log("GameController_AllClientsConnected server called");
            CreatePlayersServerRpc();
            DiceController.Instance.CreateCubesUI();
        }
        if (IsClient)
        {
            Debug.Log("GameController_AllClientsConnected client called");
            CreateTablePlayerInfoServerRpc();
            BoardController.Instance.PutPriceAndNameOnCardsUI();//Инизиализация поля с текстом Карт(стоимость карты)
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void CreateTablePlayerInfoServerRpc()
    {
        Debug.Log("CreateTablePlayerInfoServerRpc called");
        CreateTablePlayerInfoClientRpc(PlayersConnectedCountServer);
    }
    [ClientRpc]
    private void CreateTablePlayerInfoClientRpc(int count)
    {
        Debug.Log("CreateTablePlayerInfoClientRpc called");
        TablePlayersUI.Instance.PutPlayersOnTableUI(count);
    }
    private void NetworkManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (IsClient)
        {
            Debug.Log("NetworkManager_OnLoadEventCompleted called");
            IncreaseCountPlayersConnectedServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseCountPlayersConnectedServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("IncreaseCountPlayersConnectedServerRpc called");
        PlayersConnectedCountServer++;
        if (PlayersConnectedCountServer == NetworkManager.Singleton.ConnectedClients.Count)
        {
            IncreaseCountPlayersConnectedClientRpc();
        }
    }
    [ClientRpc]
    private void IncreaseCountPlayersConnectedClientRpc()
    {
        Debug.Log("IncreaseCountPlayersConnectedClientRpc called");
        AllClientsConnected?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetStepsValueServerRpc(int value)
    {
        steps.Value = value;
        int player = players[currentPlayerIndex.Value].propertyPlayerID;
        Debug.Log("Игроку " + player + " выпало " + steps);
        //CanPlayerMove = true;
        UpdateStepsClientRpc(value, player);
    }
    [ClientRpc]
    private void UpdateStepsClientRpc(int value, int player)
    {
        steps.Value = value;
        Debug.Log("Игроку " + player + " выпало " + steps);
        //CanPlayerMove = true;
    }
    private void OnTurnChanged(int previousValue, int newValue)
    {
        if (IsMyTurn())
        {
            Debug.Log("Мой ход.");
            btnTurnController(1);
        }
        else
        {
            Debug.Log("Жду хода...");
            btnTurnController(4);
        }
    }

    public Player GetCurrentPlayer()
    {
        if (IsHost)
        {
            return players[currentPlayerIndex.Value];
        }
        else
        {
            RequestCurrentPlayerServerRpc();
            return cachedCurrentPlayer;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestCurrentPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        if (players.Count == 0) return;

        Player currentPlayer = players[currentPlayerIndex.Value];

        if (currentPlayer.TryGetComponent(out NetworkObject networkObject))
        {
            SendCurrentPlayerClientRpc(new NetworkObjectReference(networkObject), rpcParams.Receive.SenderClientId);
        }
        else
        {
            Debug.Log("Err");
        }
    }

    [ClientRpc]
    private void SendCurrentPlayerClientRpc(NetworkObjectReference playerRef, ulong clientId)
    {
        if (playerRef.TryGet(out NetworkObject playerNew))
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                cachedCurrentPlayer = playerNew.GetComponent<Player>();
            }
        }
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

    [ServerRpc(RequireOwnership = false)]
    private void RollTheDicesServerRpc()
    {
        ViewCurrentButtonForClientRpc();
        StartCoroutine(RollTheDicesCoroutine());
    }
    [ClientRpc]
    private void ViewCurrentButtonForClientRpc()
    {
        if (IsMyTurn())
        {
            Debug.Log("My turn in rolldices");
            btnTurnController(4);
            /*bool previousPlayer = currentPlayerIndex.Value == 0 ? !players[players.Count - 1].isMoving : !players[currentPlayerIndex.Value - 1].isMoving;
            if (previousPlayer)
            {
            }*/
        }
        else
        {
            Debug.Log("Not my turn in rolldices");
        }
    }

    private IEnumerator RollTheDicesCoroutine()
    {
        yield return StartCoroutine(DiceController.Instance.DropDiceCoroutine(NetworkManager.LocalClient.ClientId));

        /*if (CanPlayerMove == true)
        {
            Debug.Log("Move");
            CanPlayerMove = false;
            MoveCurrentPlayerServerRpc(steps.Value);
            WhatIsANewPlayerPosition(steps.Value);
        }
        else
        {
            Debug.Log("Can`t move");
        }*/
    }

    private void MovePLayer(int value)
    {
        if (IsMyTurn())
        {
            Debug.Log("Move");
            MoveCurrentPlayerServerRpc(steps.Value);
            WhatIsANewPlayerPosition(steps.Value);
        }
        else
        {
            Debug.Log("Not my turn");
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
    [ServerRpc(RequireOwnership = false)]
    public void MoveCurrentPlayerServerRpc(int steps)
    {
        StartCoroutine(MoveCurrentPlayerCoroutine(steps));
    }
    private IEnumerator MoveCurrentPlayerCoroutine(int steps)
    {
        //players[playerIndex].isMoving = true;
        yield return StartCoroutine(players[currentPlayerIndex.Value].PlayerMoveCoroutine(steps));
    }

    private void WhatIsANewPlayerPosition(int steps)
    {
        int typeButtonTurn;
        int playerIndex = currentPlayerIndex.Value;
        int currentPosition = BoardController.Instance.ReturnPLayerPosition();

        if (currentPosition != 0 && currentPosition != 10 && currentPosition != 20 && currentPosition != 30 && currentPosition != 2 && currentPosition != 5 &&
            currentPosition != 15 && currentPosition != 17 && currentPosition != 22 && currentPosition != 25 && currentPosition != 35 && currentPosition != 38) // All special cards
        {
            int sum = BoardController.Instance.SumCardCost();
            typeButtonTurn = BoardController.Instance.CheckCardBoughtOrNot(players[playerIndex]); // Check the card the player landed on

            if (typeButtonTurn == 2)
            {
                typeButtonTurn = CanPLayerBuyOrNot(sum);
            }

            UpdateButtonTextClientRpc(sum, typeButtonTurn);
        }
        else
        {
            if (currentPosition == 2 || currentPosition == 22)
            {
                players[playerIndex].PlayerPayTax(currentPosition);
                UpdatePlayersMoneyInfoOnTableClientRpc(playerIndex, players[playerIndex].moneyPlayer);
            }
            if (currentPosition == 5 || currentPosition == 17 || currentPosition == 35)
            {
                Debug.Log("Question");
            }
            if (currentPosition == 15 || currentPosition == 25 || currentPosition == 38)
            {
                players[playerIndex].PlayerGotTreasure();
                UpdatePlayersMoneyInfoOnTableClientRpc(playerIndex, players[playerIndex].moneyPlayer);
            }

            typeButtonTurn = 3;
            UpdateButtonTextClientRpc(0, typeButtonTurn);
            //btnTurnController(typeButtonTurn);
        }
    }

    [ClientRpc]
    private void UpdateButtonTextClientRpc(int sum, int typeButtonTurn)
    {
        if (typeButtonTurn == 2)
        {
            TextMeshProUGUI text = btnWaitingUp.GetComponentInChildren<TextMeshProUGUI>();
            text.text = "Buy\nfor " + sum.ToString() + "$";
        }

        btnTurnController(typeButtonTurn);
    }

    [ClientRpc]
    private void UpdatePlayersMoneyInfoOnTableClientRpc(int playerIndex, int money)
    {
        // Update the UI to reflect the player's new money
        players[playerIndex].moneyPlayer = money;
        // Call your UI update logic here
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
        //BoardController.Instance.UpdateColorCardOnBoard(colorPlayers[currentPlayerIndex.Value]);
    }

    private void PlayerMakeAuction()
    {
        players[currentPlayerIndex.Value].AuctionCard();
    }
    private int CanPLayerBuyOrNot(int sum)//Проверка на плетежеспособность игрока, и отключение кнопки купить
    {
        if (players[currentPlayerIndex.Value].moneyPlayer - sum >= 0)
        {
            return 2;
        }
        return 5;
    }

    [ServerRpc(RequireOwnership = false)]
    private void CreatePlayersServerRpc()
    {
        Vector3 startPlayerPosition = new Vector3(14.7f, 0.16f, -15.0f);
        Quaternion startPlayerRotation = Quaternion.Euler(0, 0, 0);

        for (int i = 0; i < NetworkManager.Singleton.ConnectedClients.Count; i++)
        {
            Color color = MonopolyMultiplayer.Instance.GetPlayerColorFromPlayerId(i);
            Vector3 offset = GetOffsetForPlayersSpawn(i);

            MonopolyMultiplayer.Instance.SetPlayerMoneyServerRpc(i, startMoneyPlayer);

            Player newPlayer = new Player();
            newPlayer.moneyPlayer = startMoneyPlayer;
            newPlayer.SetDefaultcurrentPosition();

            GameObject playerObject = Instantiate(playerPrefab, new Vector3(startPlayerPosition.x + offset.x, startPlayerPosition.y,
                startPlayerPosition.z + offset.z), startPlayerRotation);

            playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClient.ClientId);
            playerObject.GetComponent<Player>().SetThisPlayer(newPlayer);
            playerObject.GetComponent<Player>().SetPlayerColor(color, i);

            players.Add(newPlayer);
        }
    }
    /*[ServerRpc]
    private void CreatePlayersAndAddToListServerRpc(int index)
    {
        Vector3 offset;
        GameObject playerObject;

        Player newPlayer = playerPrefabs[index].GetComponent<Player>();
        newPlayer.propertyPlayerID++;
        newPlayer.moneyPlayer = startMoneyPlayer;
        newPlayer.SetDefaultcurrentPosition();
        //newPlayer.clientIdPlayer = clientId;

        if (playerPrefabs[choicePlayers[choicePlayersIndex]].CompareTag("Air"))
        {
            offset = PlacementStartPlayersOnBoard(countPlayersAir);
            playerObject = Instantiate(playerPrefabs[choicePlayers[choicePlayersIndex]], new Vector3(startPositionPlayer.x + offset.x, heightForAirPlayers,
                startPositionPlayer.z + offset.z), startRotationPlayer);

            playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClient.ClientId);

            //Debug.Log("Air player spawned Server");
            //CreatePlayer(playerObject, offset, heightForAirPlayers);

            countPlayersAir++;
        }
        else 
    //(playerPrefabs[number[choicePlayersIndex]].CompareTag("Ground"))
        {
            offset = PlacementStartPlayersOnBoard(countPlayersGround);
            playerObject = Instantiate(playerPrefabs[choicePlayers[choicePlayersIndex]], new Vector3(startPositionPlayer.x + offset.x, heightForGroundPlayers,
                startPositionPlayer.z + offset.z), startRotationPlayer);

            playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClient.ClientId);

            //Debug.Log("Ground player spawned Server");
            //CreatePlayer(playerObject, offset, heightForGroundPlayers);

            countPlayersGround++;
        }
        newPlayer.playerPrefab = playerObject;
        //Debug.Log($"Spawned {playerObject} for ClientId: {clientId}, IsOwner: {IsOwner}");
        choicePlayersIndex++;
        //Debug.Log($"Player {newPlayer.propertyPlayerID} created Host+Client");
        //SyncPlayersListClientRpc(newPlayer.propertyPlayerID, newPlayer.moneyPlayer);
        players.Add(newPlayer);
    }*/

    private void StartGame()
    {
        if (IsOwner == players[currentPlayerIndex.Value])
        {
            if (IsHost)
            {
                Debug.Log("Все игроки подключены. Начинаем игру!");
                GiveFirstPlayerButtonClientRpc();
            }
        }
    }
    [ClientRpc]
    private void GiveFirstPlayerButtonClientRpc()
    {
        if (!IsClient) return;

        ulong firstPlayerId = NetworkManager.Singleton.ConnectedClients.Keys.First();

        if (NetworkManager.LocalClient.ClientId == firstPlayerId)
        {
            btnTurnController(1);
        }
    }

    private Vector3 GetOffsetForPlayersSpawn(int phase)
    {
        float offsetX = 1.3f;
        float offsetZ = 2.0f;
        Vector3 result = new Vector3(0, 0, 0);

        switch (phase)
        {
            case 0:
                {
                    return result;
                }
            case 1:
                {
                    result.z -= offsetZ;
                    break;
                }
            case 2:
                {
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
                    result.z -= 2 * offsetZ;
                    break;
                }
            case 5:
                {
                    result.z -= 2 * offsetZ;
                    result.x += 2 * offsetX;
                    break;
                }
        }
        return result;
    }

    public void UpdatePlayersMoneyInfoOnTable()//Візуально сверху
    {
        /*for (int i = 0; i < playersInfoTable.Count; i++)
        {
            TextMeshProUGUI[] textComponent = playersInfoTable[i].GetComponentsInChildren<TextMeshProUGUI>();
            if (textComponent.Length > 0)
            {
                textComponent[1].text = players[i].moneyPlayer + "$";
            }
        }*/
    }

    [ServerRpc(RequireOwnership = false)]
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
    public override void OnDestroy()
    {
        if (IsClient && IsSpawned)
        {
            currentPlayerIndex.OnValueChanged -= OnTurnChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted -= NetworkManager_OnLoadEventCompleted;
        Instance.AllClientsConnected -= GameController_AllClientsConnected;
    }
}
