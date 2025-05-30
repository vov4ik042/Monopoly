using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GameController : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private CinemachineVirtualCamera[] VirtualCameras;
    [SerializeField] private Transform objectCanvas;
    [SerializeField] private Button playerBunkruptButton;
    [SerializeField] private Button btnStartTurn;
    [SerializeField] private Button btnEndTurn;
    [SerializeField] private Button btnBuy;
    [SerializeField] private Button btnEndAndBuy;
    [SerializeField] private Button btnNotEnoughMoney;

    public static GameController Instance;

    private int startMoneyPlayer = 650;//465
    public int steps = 8;//Кол-во клеток перемещения
    private int PlayersConnectedCountServer;
    private int CountPlayersVisitingJailServerOnly = 0;
    private int CountMoneyForPlayerVacationServerOnly = 0;

    private bool[] playersBunkrupt;
    private int[] cubesTwoResults = new int[2];//ServerOnly
    private List<Player> playersList = new List<Player>();
    private Dictionary<ulong, NetworkObject> playersListNetworkObjects = new Dictionary<ulong, NetworkObject>();
    private NetworkVariable<int> currentPlayerIndex = new NetworkVariable<int>(0); //Индекс текущего игрока
    public event EventHandler AllClientsConnected;
    public event EventHandler PlayerLeave;

    private void OnEnable()
    {
        btnStartTurn.onClick.AddListener(() => {
            btnTurnController(4);
            RollTheDicesServerRpc(NetworkManager.Singleton.LocalClientId);
        });
        btnEndTurn.onClick.AddListener(() => {
            EndTurn();
        });
        btnBuy.onClick.AddListener(() => {
            btnTurnController(5);
            PlayerBuyCard();
        });
        btnEndAndBuy.onClick.AddListener(() => {
            EndTurn();
        });
    }

    private void Awake()
    {
        Instance = this;
        NetworkManager.SceneManager.OnLoadEventCompleted += NetworkManager_OnLoadEventCompleted;
        Instance.AllClientsConnected += GameController_AllClientsConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += GameController_OnClientDisconnectCallback;
    }

    public void GameController_OnClientDisconnectCallback(ulong clientId)
    {
        RemoveAllPlayerObjectsServerRpc(clientId);

        PlayerLeave?.Invoke(this, EventArgs.Empty);

        MonopolyMultiplayer.Instance.SetPlayerBankruptServerRpc(clientId);
        SetPlayerBunkruptServerRpc(clientId);
        //TablePlayersUI.Instance.DeleteTemplate(clientId);

        int currentPlayerIndex = GetCurrentPlayerIndex();
        if (currentPlayerIndex == (int)clientId)
        {
            NextPlayerTurnServerRpc();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void GivePlayerMoneyForOneCicleGameBoardServerRpc()
    {
        playersList[currentPlayerIndex.Value].SetPlayerMoney(300);
        Debug.Log($"Give {playersList[currentPlayerIndex.Value].GetPlayerId()} 300$");   
    }
    private void btnTurnController(int phase)
    {
        switch (phase)
        {
            case 1://DiceRoll
                {
                    //DiceController.Instance.SwitchSetActiveDices(true);
                    btnStartTurn.gameObject.SetActive(true);
                    btnBuy.gameObject.SetActive(false);
                    btnBuy.interactable = true;//off buy
                    btnEndAndBuy.gameObject.SetActive(false);
                    btnEndTurn.gameObject.SetActive(false);
                    break;
                }
            case 2://BuyOrEndTurn
                {
                    btnStartTurn.gameObject.SetActive(false);
                    btnBuy.gameObject.SetActive(true);
                    btnEndAndBuy.gameObject.SetActive(true);
                    btnEndTurn.gameObject.SetActive(false);
                    break;
                }
            case 3://EntTurn
                {
                    btnStartTurn.gameObject.SetActive(false);
                    btnBuy.gameObject.SetActive(false);
                    btnEndAndBuy.gameObject.SetActive(false);
                    btnEndTurn.gameObject.SetActive(true);
                    btnEndTurn.interactable = true;//off end
                    break;
                }
            case 4://Disable all
                {
                    btnStartTurn.gameObject.SetActive(false);
                    btnBuy.gameObject.SetActive(false);
                    btnEndAndBuy.gameObject.SetActive(false);
                    btnEndTurn.gameObject.SetActive(false);
                    break;
                }
            case 5://Player cant buy
                {
                    btnStartTurn.gameObject.SetActive(false);
                    btnBuy.gameObject.SetActive(true);
                    btnBuy.interactable = false;//off buy
                    btnEndAndBuy.gameObject.SetActive(true);
                    btnEndTurn.gameObject.SetActive(false);
                    break;
                }
            case 6://NotEnoughMoney
                {
                    btnNotEnoughMoney.gameObject.SetActive(true);
                    break;
                }
            case 7://Cancel NotEnoughMoney
                {
                    btnNotEnoughMoney.gameObject.SetActive(false);
                    break;
                }
        }
    }
    public void TurnOnOffButtons(int phase)
    {
        btnTurnController(phase);
    }
    public Player GetPlayerFromClientId(int clientId)
    {
        for (int i = 0; i < playersList.Count; i++)
        {
            if (playersList[i].GetPlayerId() == clientId)
            {
                return playersList[i];
            }
        }
        return null;
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerMoneyServerRpc(int clientId1, int clientId2, int playerMoney1, int playerMoney2)
    {
        Player player1 = GetPlayerFromClientId(clientId1);
        player1.SetPlayerMoney(-playerMoney1);
        player1.SetPlayerMoney(+playerMoney2);

        Player player2 = GetPlayerFromClientId(clientId2);
        player2.SetPlayerMoney(-playerMoney2);
        player2.SetPlayerMoney(+playerMoney1);
    }
    private void GameController_AllClientsConnected(object sender, EventArgs e)
    {
        if (IsServer)
        {
            CreatePlayersServerRpc();
            DiceController.Instance.CreateCubesUI();
            CreateTablePlayerInfoServerRpc();
            StartGameServerRpc();
            InitializePlayersBunkruptList();
        }
        if (IsClient)
        {
            BoardController.Instance.PutPriceAndNameOnCardsUI();//Инизиализация поля с текстом Карт(стоимость карты)
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerBunkruptServerRpc(ulong clientId)//ПРоверить
    {
        if (playersBunkrupt.Length > 0)
        {
            playersBunkrupt[clientId] = true;

            int playerIndex = -1;
            byte count = 0;

            for (int i = 0; i < playersBunkrupt.Length; i++)
            {
                if (playersBunkrupt[i] == false)
                {
                    count++;
                    playerIndex = i;
                }
            }

            if (count == 1)
            {
                string playerName = MonopolyMultiplayer.Instance.GetPlayerNameFromPlayerData(playerIndex).ToString();
                SetPlayerBunkruptClientRpc(playerName);
            }
        }
    }
    [ClientRpc]
    private void SetPlayerBunkruptClientRpc(string name)
    {
        ResultWindow.Instance.Show(name);
    }

    private void InitializePlayersBunkruptList()
    {
        playersBunkrupt = new bool[PlayersConnectedCountServer];
        for (int i = 0; i < playersBunkrupt.Length; i++)
        {
            playersBunkrupt[i] = false;
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
    public void SetStepsValueServerRpc(int cube1, int cube2)
    {
        cubesTwoResults[0] = cube1;
        cubesTwoResults[1] = cube2;
        int value = cube1 + cube2;
        //steps = value;
        int player = playersList[currentPlayerIndex.Value].GetPlayerId();

        Debug.Log("Игроку " + player + " выпало " + steps);

        UpdateStepsClientRpc(value, player);
    }
    [ClientRpc]
    private void UpdateStepsClientRpc(int value, int player)
    {
        //steps = value;
        Debug.Log("Игроку " + player + " выпало " + steps);
    }

    private void LogicForPrisonForServerOnly(ulong clientId)
    {
        int cube1 = cubesTwoResults[0];
        int cube2 = cubesTwoResults[1];
        //Debug.Log("playersList[currentPlayerIndex.Value].GetCountTimeInPrison(): " + playersList[currentPlayerIndex.Value].GetCountTimeInPrison());
        if (playersList[currentPlayerIndex.Value].GetCountTimeInPrison() == 2)
        {
            Debug.Log("Time to go out from jail, congratulations");
            playersList[currentPlayerIndex.Value].SetJail(false);
            playersList[currentPlayerIndex.Value].SetCountTimeInPrison(-2);

            CountPlayersVisitingJailServerOnly++;
            Vector3 position = NewPositionForJustVisitingJailPlayer();
            StartCoroutine(playersList[currentPlayerIndex.Value].PlayerMoveToPointInJailVisit(position));
        }
        else
        {
            if (cube1 == cube2)
            {
                playersList[currentPlayerIndex.Value].SetJail(false);

                CountPlayersVisitingJailServerOnly++;
                Vector3 position = NewPositionForJustVisitingJailPlayer();
                StartCoroutine(playersList[currentPlayerIndex.Value].PlayerMoveToPointInJailVisit(position));
                Debug.Log("Double cubes, congratulations");
            }
            else
            {
                playersList[currentPlayerIndex.Value].SetCountTimeInPrison(1);
                Debug.Log("Not double cubes, unlucky");
            }
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId },
            }
        };
        UpdateButtonOnClientRpc(3, clientRpcParams);
    }

    public int GetCurrentPlayerIndex()
    {
        return currentPlayerIndex.Value;
    }

    public Player GetCurrentPlayerServerOnly()
    {
        return playersList[currentPlayerIndex.Value];
    }

    [ServerRpc(RequireOwnership = false)]
    private void RollTheDicesServerRpc(ulong clientId)
    {
        StartCoroutine(RollTheDicesCoroutine(clientId));
    }
    private IEnumerator RollTheDicesCoroutine(ulong clientId)
    {
        yield return StartCoroutine(DiceController.Instance.DropDiceCoroutine());
        if (playersList[currentPlayerIndex.Value].GetJail() == false)
        { 
            MoveCurrentPlayerServerRpc(steps, clientId);
        }
        else
        {
            LogicForPrisonForServerOnly(clientId);
        }
    }

    [ClientRpc]
    private void UpdateButtonOnClientRpc(int typeOperation, ClientRpcParams clientRpcParams)
    {
        btnTurnController(typeOperation);
    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveCurrentPlayerServerRpc(int steps, ulong clientId)
    {
        StartCoroutine(MoveCurrentPlayerCoroutine(steps, clientId));
    }
    private IEnumerator MoveCurrentPlayerCoroutine(int steps, ulong clientId)
    {
        //Debug.Log("currentPlayerIndex: " + currentPlayerIndex.Value);

        yield return StartCoroutine(playersList[currentPlayerIndex.Value].PlayerMoveCoroutine(steps));
        WhatIsANewPlayerPosition(clientId);
    }
    private void WhatIsANewPlayerPosition(ulong clientId)
    {
        int typeButtonTurn;
        int currentPlayer = currentPlayerIndex.Value;
        int currentPosition = BoardController.Instance.ReturnPlayerPosition();

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId },
            }
        };
        SwitchCameraClientRpc(currentPosition, clientRpcParams);

        if (currentPosition != 0 && currentPosition != 10 && currentPosition != 20 && currentPosition != 30 && currentPosition != 2 && currentPosition != 5 &&
            currentPosition != 15 && currentPosition != 17 && currentPosition != 22 && currentPosition != 25 && currentPosition != 35 && currentPosition != 38)
        {
            int sum = BoardController.Instance.SumCardCost();

            typeButtonTurn = BoardController.Instance.CheckCardBoughtOrNot(playersList[currentPlayer]);

            if (typeButtonTurn == 2)
            {
                typeButtonTurn = CanPlayerBuyOrNot(sum);
            }

            UpdateButtonTextClientRpc(sum, typeButtonTurn);
        }
        else
        {
            if (currentPosition == 2 || currentPosition == 22)
            {
                int result = playersList[currentPlayer].PlayerPayTax(currentPosition);
                CountMoneyForPlayerVacationServerOnly += result;
            }
            if (currentPosition == 5 || currentPosition == 17 || currentPosition == 35)
            {
                Debug.Log("Question");
            }
            if (currentPosition == 15 || currentPosition == 25 || currentPosition == 38)
            {
                playersList[currentPlayer].PlayerGotTreasure();
            }
            if (currentPosition == 30)
            {
                StartCoroutine(playersList[currentPlayerIndex.Value].PlayerMoveToJail());
                playersList[currentPlayerIndex.Value].SetJail(true);
                //Debug.Log("currentPlayerIndex1: " + currentPlayerIndex.Value);
            }
            if (currentPosition == 10)
            {
                CountPlayersVisitingJailServerOnly++;
                Vector3 position = NewPositionForJustVisitingJailPlayer();
                StartCoroutine(playersList[currentPlayerIndex.Value].PlayerMoveToPointInJailVisit(position));
            }
            if (currentPosition == 20)
            {
                playersList[currentPlayerIndex.Value].SetVacationTimeLeftAndGiveMoney(CountMoneyForPlayerVacationServerOnly);
                CountMoneyForPlayerVacationServerOnly = 0;
            }
            //Debug.Log("currentPlayerIndex2: " + currentPlayerIndex.Value);
            typeButtonTurn = 3;
            UpdateButtonTextClientRpc(0, typeButtonTurn);
        }
    }
    [ClientRpc]
    private void UpdateButtonTextClientRpc(int sum, int typeButtonTurn)
    {
        //Debug.Log("currentPlayerIndex3: " + currentPlayerIndex.Value);
        if (IsMyTurn())
        {
            if (typeButtonTurn != 3)
            {
                TextMeshProUGUI text = btnBuy.GetComponentInChildren<TextMeshProUGUI>();
                text.text = $"Buy\nfor {sum}$";
                Debug.Log("text:" + text.text);
            }
            btnTurnController(typeButtonTurn);
        }
    }
    [ClientRpc]
    private void SwitchCameraClientRpc(int currentPosition, ClientRpcParams clientRpcParams)
    {
        if (currentPosition > 0 && currentPosition <= 10)
        {
            VirtualCameras[0].Priority = 4;
            VirtualCameras[1].Priority = 1;
            VirtualCameras[2].Priority = 1;
            VirtualCameras[3].Priority = 1;
        }
        if (currentPosition > 10 && currentPosition <= 20)
        {
            VirtualCameras[0].Priority = 1;
            VirtualCameras[1].Priority = 4;
            VirtualCameras[2].Priority = 1;
            VirtualCameras[3].Priority = 1;
        }
        if (currentPosition > 20 && currentPosition <= 30)
        {
            VirtualCameras[0].Priority = 1;
            VirtualCameras[1].Priority = 1;
            VirtualCameras[2].Priority = 4;
            VirtualCameras[3].Priority = 1;
        }
        if ((currentPosition > 30 && currentPosition <= 39) || currentPosition == 0)
        {
            VirtualCameras[0].Priority = 1;
            VirtualCameras[1].Priority = 1;
            VirtualCameras[2].Priority = 1;
            VirtualCameras[3].Priority = 4;
        }
    }
    private Vector3 NewPositionForJustVisitingJailPlayer()
    {
        if (CountPlayersVisitingJailServerOnly == 7)
        {
            CountPlayersVisitingJailServerOnly = 1;
        }
        Vector3 newPosition;
        switch (CountPlayersVisitingJailServerOnly)
        {
            case 1:
                {
                    return newPosition = new Vector3(-14.23f, 0, -17.93f);
                }
            case 2:
                {
                    return newPosition = new Vector3(-15.51f, 0, -17.93f);
                }
            case 3:
                {
                    return newPosition = new Vector3(-16.75f, 0, -17.93f);
                }
            case 4:
                {
                    return newPosition = new Vector3(-17.95f, 0, -16.729f);
                }
            case 5:
                {
                    return newPosition = new Vector3(-17.95f, 0, -15.489f);
                }
            case 6:
                {
                    return newPosition = new Vector3(-17.95f, 0, -14.324f);
                }
        }
        return Vector3.zero;
    }

    public void PlayerBuyCard()
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;
        int cardIndex = BoardController.Instance.WhatCardNumber();

        MonopolyMultiplayer.Instance.AddToPlayerListPropertyServerRpc(localId, cardIndex);

        PlayerBuyCardServerRpc(localId, cardIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerBuyCardServerRpc(ulong clientId, int cardIndex)
    {
        int cardCost = BoardController.Instance.SumCardCost();

        playersList[currentPlayerIndex.Value].BuyCard(cardIndex, cardCost, playersList[currentPlayerIndex.Value], clientId);

        BoardController.Instance.BuyCityOrInfrastructureReact(cardIndex, playersList[currentPlayerIndex.Value]);
        BoardController.Instance.UpdateColorCardOnBoard(currentPlayerIndex.Value);
        BoardController.Instance.ChangesInfoCardServerRpc(cardIndex, clientId);
        BoardController.Instance.CurrentOwnerCard(cardIndex);//Debug.log
    }
    [ServerRpc(RequireOwnership = false)]
    public void PlayerBuyCardForTradeServerRpc(ulong clientId, int cardIndex)
    {
        Player player = GetPlayerFromClientId((int)clientId);
        player.BuyCardForTrade(cardIndex, player, clientId);

        BoardController.Instance.BuyCityOrInfrastructureReact(cardIndex, player);
        BoardController.Instance.UpdateColorCardOnBoardForTrade(cardIndex, (int)clientId);
        BoardController.Instance.ChangesInfoCardServerRpc(cardIndex, clientId);
    }

    private void PlayerMakeAuction()
    {
        playersList[currentPlayerIndex.Value].AuctionCard();
    }
    private int CanPlayerBuyOrNot(int sum)//Проверка на плетежеспособность игрока, и отключение кнопки купить
    {
        if (playersList[currentPlayerIndex.Value].GetPlayerMoney() - sum >= 0)
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
            UnityEngine.Color color = MonopolyMultiplayer.Instance.GetPlayerColorFromPlayerId(i);
            Vector3 offset = GetOffsetForPlayersSpawn(i);

            MonopolyMultiplayer.Instance.SetPlayerMoneyServerRpc(i, startMoneyPlayer);

            GameObject playerObject = Instantiate(playerPrefab, new Vector3(startPlayerPosition.x + offset.x, startPlayerPosition.y,
                startPlayerPosition.z + offset.z), startPlayerRotation);

            playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClient.ClientId);
            playerObject.GetComponent<PlayerVisualGameBoard>().SetPlayerColor(color, i);

            Player newPlayer = playerObject.GetComponent<Player>();
            newPlayer.SetPlayerMoney(startMoneyPlayer);
            newPlayer.SetPlayerIdServerRpc(i);

            newPlayer.playerCircleGameBoard += GivePlayerMoney_playerCircleGameBoard;

            playersList.Add(newPlayer);
            playersListNetworkObjects[(ulong)i] = playerObject.GetComponent<NetworkObject>();
        }
    }

    private void GivePlayerMoney_playerCircleGameBoard(object sender, EventArgs e)
    {
        GivePlayerMoneyForOneCicleGameBoardServerRpc();
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

        if (NetworkManager.LocalClient.ClientId == firstPlayerId)
        {
            btnTurnController(1);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void NextPlayerTurnServerRpc()
    {
        do
        {
            currentPlayerIndex.Value++;
            if (currentPlayerIndex.Value == playersBunkrupt.Length)
            {
                currentPlayerIndex.Value = 0;
            }
        }
        while (playersBunkrupt[currentPlayerIndex.Value] == true);

        PlayersTurnChangedClientRpc(currentPlayerIndex.Value, playersList[currentPlayerIndex.Value].GetVacationTimeLeft());
    }
    [ClientRpc]
    private void PlayersTurnChangedClientRpc(int playerIndex, int timeVacation)
    {
        ulong clientId = MonopolyMultiplayer.Instance.GetClientIdFromPlayerIndex(playerIndex);
        if (clientId == NetworkManager.LocalClient.ClientId)
        {
            if (timeVacation == 0)
            {
                //Debug.Log("Мой ход.");
                btnTurnController(1);
            }
            else
            {
                DecreasePlayerVacationTimeServerRpc(playerIndex);
                EndTurn();
            }
        }
        else
        {
            //Debug.Log("not Мой ход.");
            btnTurnController(4);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DecreasePlayerVacationTimeServerRpc(int playerIndex)
    {
        playersList[playerIndex].SetVacationTimeLeft(-1);
    }

    private void EndTurn()
    {
        btnTurnController(4);
        NextPlayerTurnServerRpc();
    }
    private bool IsMyTurn()
    {
        ulong clientId = MonopolyMultiplayer.Instance.GetClientIdFromPlayerIndex(currentPlayerIndex.Value);
        //Debug.Log("Turn = currentPlayerIndex.Value " + currentPlayerIndex.Value + " clientId " + clientId + " NetworkManager.LocalClient.ClientId " + NetworkManager.LocalClient.ClientId);
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
    [ServerRpc(RequireOwnership = false)]
    public void RemovePlayerFromListServerRpc(ulong clientId)
    {
        Debug.Log("PlayerslistCount: " + playersList.Count + " clientId: " + clientId);
        playersList.RemoveAt((int)clientId);
    }
    private void OnDisable()
    {
        btnStartTurn.onClick.RemoveListener(() => {
            btnTurnController(4);
            RollTheDicesServerRpc(NetworkManager.Singleton.LocalClientId);
        });
        btnEndTurn.onClick.RemoveListener(() => {
            EndTurn();
        });
        btnBuy.onClick.RemoveListener(() => {
            btnTurnController(5);
            PlayerBuyCard();
        });
        btnEndAndBuy.onClick.RemoveListener(() => {
            EndTurn();
        });
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeleteInstanceAndClearListsServerRpc()
    { 
        playersList.Clear();
        Array.Clear(playersBunkrupt, 0, playersBunkrupt.Length);
    }
    public override void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /*[ServerRpc(RequireOwnership = false)]
    public void RemoveAllPlayerObjectsServerRpc(ulong clientId)
    {
        if (playersListNetworkObjects.Count > 0 && playersListNetworkObjects[(int)clientId] != null)
        {
            playersListNetworkObjects[(int)clientId].Despawn();
            playersListNetworkObjects.RemoveAt((int)clientId);
        }
    }*/
    [ServerRpc(RequireOwnership = false)]
    public void RemoveAllPlayerObjectsServerRpc(ulong clientId)
    {
        if (IsServer)
        {
            if (playersListNetworkObjects.Count > 0)
            {
                if (playersListNetworkObjects.TryGetValue(clientId, out NetworkObject obj))
                {
                    if (obj != null && obj.IsSpawned)
                    {
                        obj.Despawn();
                    }

                    playersListNetworkObjects.Remove(clientId);
                }
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted -= NetworkManager_OnLoadEventCompleted;
        Instance.AllClientsConnected -= GameController_AllClientsConnected;
    }
}
