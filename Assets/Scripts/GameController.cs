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
        btnStartTurn.onClick.AddListener(() => {
            btnTurnController(4);
            RollTheDicesServerRpc();
        });
        btnEndTurn.onClick.AddListener(() => {
            btnTurnController(4);
            NextPlayerTurnServerRpc();
        });
        btnWaitingUp.onClick.AddListener(PlayerBuyCard);
        btnWaitingDown.onClick.AddListener(() => {
            btnTurnController(4);
            NextPlayerTurnServerRpc();
        });
    }
    private void Update()
    {
        if (IsServer)
            Debug.Log("Server value: " + currentPlayerIndex.Value);
        if (IsClient)
            Debug.Log("Client value: " + currentPlayerIndex.Value);
    }
    private void Awake()
    {
        Instance = this;
        NetworkManager.SceneManager.OnLoadEventCompleted += NetworkManager_OnLoadEventCompleted;
        Instance.AllClientsConnected += GameController_AllClientsConnected;
    }

    public override void OnNetworkSpawn()
    {
        steps.Skip(1).Subscribe(MovePLayer).AddTo(_compositeDisposable);
        //currentPlayerIndex.OnValueChanged += PlayersTurnChanged;
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
    private void GameController_AllClientsConnected(object sender, EventArgs e)
    {
        if (IsServer)
        {
            CreatePlayersServerRpc();
            DiceController.Instance.CreateCubesUI();
            CreateTablePlayerInfoServerRpc();
            StartGameServerRpc();
        }
        if (IsClient)
        {
            BoardController.Instance.PutPriceAndNameOnCardsUI();//Инизиализация поля с текстом Карт(стоимость карты)
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void CreateTablePlayerInfoServerRpc()
    {
        CreateTablePlayerInfoClientRpc(PlayersConnectedCountServer);
    }
    [ClientRpc]
    private void CreateTablePlayerInfoClientRpc(int count)
    {
        TablePlayersUI.Instance.PutPlayersOnTableUI(count);
    }
    private void NetworkManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (IsClient)
        {
            IncreaseCountPlayersConnectedServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseCountPlayersConnectedServerRpc(ServerRpcParams serverRpcParams = default)
    {
        PlayersConnectedCountServer++;
        if (PlayersConnectedCountServer == NetworkManager.Singleton.ConnectedClients.Count)
        {
            IncreaseCountPlayersConnectedClientRpc();
        }
    }
    [ClientRpc]
    private void IncreaseCountPlayersConnectedClientRpc()
    {
        AllClientsConnected?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetStepsValueServerRpc(int value)
    {
        steps.Value = value;
        int player = players[currentPlayerIndex.Value].propertyPlayerID;

        Debug.Log("Игроку " + player + " выпало " + steps);

        UpdateStepsClientRpc(value, player);
    }
    [ClientRpc]
    private void UpdateStepsClientRpc(int value, int player)
    {
        steps.Value = value;
        Debug.Log("Игроку " + player + " выпало " + steps);
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

    private IEnumerator RollTheDicesCoroutine()
    {
        yield return StartCoroutine(DiceController.Instance.DropDiceCoroutine());
    }

    private void MovePLayer(int value)
    {
        if (IsMyTurn())
        {
            Debug.Log("MovePLayer");
            MoveCurrentPlayerServerRpc(steps.Value);
            WhatIsANewPlayerPosition(steps.Value);
        }
        else
        {
            Debug.Log("Not my turn move");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveCurrentPlayerServerRpc(int steps)
    {
        StartCoroutine(MoveCurrentPlayerCoroutine(steps));
    }
    private IEnumerator MoveCurrentPlayerCoroutine(int steps)
    {
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
        if (IsMyTurn())
        {
            btnTurnController(typeButtonTurn);
        }
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

            newPlayer.playerPref = playerObject;

            playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClient.ClientId);
            playerObject.GetComponent<Player>().SetThisPlayer(newPlayer);
            playerObject.GetComponent<Player>().SetPlayerColor(color, i);

            players.Add(newPlayer);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc()
    {
        if (IsServer)
        {
            Debug.Log("Начинаем игру!");
            int number = 0;
            //int number = UnityEngine.Random.Range(0, NetworkManager.Singleton.ConnectedClients.Count);
            currentPlayerIndex.Value = number;
            GiveRandomPlayerButtonClientRpc(number);
        }
    }
    [ClientRpc]
    private void GiveRandomPlayerButtonClientRpc(int playerIndex)
    {
        ulong firstPlayerId = MonopolyMultiplayer.Instance.GetClientIdFromPlayerIndex(playerIndex);
        Debug.Log("GiveRandomPlayerButton: " + firstPlayerId);

        if (NetworkManager.LocalClient.ClientId == firstPlayerId)
        {
            btnTurnController(1);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void NextPlayerTurnServerRpc()
    {
        currentPlayerIndex.Value++;

        if (currentPlayerIndex.Value == players.Count)
        {
            currentPlayerIndex.Value = 0;
        }

        PlayersTurnChangedClientRpc();
        Debug.Log("currentPlayerTurn " + currentPlayerIndex.Value + " playersCount: " + players.Count);
    }
    [ClientRpc]
    private void PlayersTurnChangedClientRpc()
    {
        if (IsMyTurn())
        {
            Debug.Log("Мой ход.");
            btnTurnController(1);
        }
        else
        {
            Debug.Log("not Мой ход.");
            btnTurnController(4);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void RollTheDicesServerRpc()
    {
        StartCoroutine(RollTheDicesCoroutine());
    }

    private bool IsMyTurn()
    {
        MonopolyMultiplayer.Instance.ViewListClientsId();//
        ulong clientId = MonopolyMultiplayer.Instance.GetClientIdFromPlayerIndex(currentPlayerIndex.Value);
        Debug.Log("Turn = currentPlayerIndex.Value " + currentPlayerIndex.Value + " clientId " + clientId + " NetworkManager.LocalClient.ClientId " + NetworkManager.LocalClient.ClientId);
        return clientId == NetworkManager.LocalClient.ClientId;
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

    public override void OnDestroy()
    {
        /*if (IsClient && IsSpawned)
        {
            currentPlayerIndex.OnValueChanged -= PlayersTurnChanged;
        }*/
    }
    private void OnDisable()
    {
        /*btnStartTurn.onClick.RemoveListener(RollTheDicesServerRpc);
        btnEndTurn.onClick.RemoveListener(EndTurn);
        btnWaitingUp.onClick.RemoveListener(PlayerBuyCard);
        btnWaitingDown.onClick.RemoveListener(EndTurn);*/
    }
    public override void OnNetworkDespawn()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted -= NetworkManager_OnLoadEventCompleted;
        Instance.AllClientsConnected -= GameController_AllClientsConnected;
    }
}
