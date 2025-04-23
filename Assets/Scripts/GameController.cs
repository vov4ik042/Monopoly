using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameController : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform objectCanvas;
    [SerializeField] private Button playerBunkruptButton;

    [SerializeField] private Button btnStartTurn;
    [SerializeField] private Button btnEndTurn;
    [SerializeField] private Button btnBuy;
    [SerializeField] private Button btnEndAndBuy;

    public static GameController Instance;
    private int startMoneyPlayer = 110;//465
    public int steps = 8;//Кол-во клеток перемещения
    private int PlayersConnectedCountServer;

    private List<Player> playersList = new List<Player>();
    private NetworkVariable<int> currentPlayerIndex = new NetworkVariable<int>(0); //Индекс текущего игрока

    private Player cachedCurrentPlayer; // Кэш для клиента

    public event EventHandler AllClientsConnected;

    private void OnEnable()
    {
        btnStartTurn.onClick.AddListener(() => {
            btnTurnController(4);
            RollTheDicesServerRpc();
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
    }
    /*private void Start()
    {
        playerBunkruptButton.onClick.AddListener(() =>
        {
            Bunkrupt.Instance.Show();
        });
    }*/

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

    public int GetCurrentPlayerIndex()
    {
        return currentPlayerIndex.Value;
    }

    public Player GetCurrentPlayerServerOnly()
    {
        return playersList[currentPlayerIndex.Value];
    }

    [ServerRpc(RequireOwnership = false)]
    private void RollTheDicesServerRpc()
    {
        StartCoroutine(RollTheDicesCoroutine());
    }
    private IEnumerator RollTheDicesCoroutine()
    {
        yield return StartCoroutine(DiceController.Instance.DropDiceCoroutine());
        MoveCurrentPlayerServerRpc(steps);
    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveCurrentPlayerServerRpc(int steps)
    {
        StartCoroutine(MoveCurrentPlayerCoroutine(steps));
    }
    private IEnumerator MoveCurrentPlayerCoroutine(int steps)
    {
        yield return StartCoroutine(playersList[currentPlayerIndex.Value].PlayerMoveCoroutine(steps));
        WhatIsANewPlayerPosition(steps);
    }

    private void WhatIsANewPlayerPosition(int steps)
    {
        int typeButtonTurn;
        int currentPlayer = currentPlayerIndex.Value;
        int currentPosition = BoardController.Instance.ReturnPLayerPosition();

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
                playersList[currentPlayer].PlayerPayTax(currentPosition);
            }
            if (currentPosition == 5 || currentPosition == 17 || currentPosition == 35)
            {
                Debug.Log("Question");
            }
            if (currentPosition == 15 || currentPosition == 25 || currentPosition == 38)
            {
                playersList[currentPlayer].PlayerGotTreasure();
            }

            typeButtonTurn = 3;
            UpdateButtonTextClientRpc(0, typeButtonTurn);
        }
    }
    [ClientRpc]
    private void UpdateButtonTextClientRpc(int sum, int typeButtonTurn)
    {
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

    public void PlayerBuyCard()
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;
        //Debug.Log("PlayerBuyCard client id: " + localId);
        PlayerBuyCardServerRpc(localId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerBuyCardServerRpc(ulong clientId)
    {
        int cardIndex = BoardController.Instance.WhatCardNumber();
        int cardCost = BoardController.Instance.SumCardCost();

        playersList[currentPlayerIndex.Value].BuyCard(cardIndex, cardCost, playersList[currentPlayerIndex.Value], clientId);

        BoardController.Instance.BuyCityOrInfrastructureReact(cardIndex, playersList[currentPlayerIndex.Value]);
        BoardController.Instance.UpdateColorCardOnBoard(currentPlayerIndex.Value);
        BoardController.Instance.ChangesInfoCardServerRpc(cardIndex, clientId);
        BoardController.Instance.CurrentOwnerCard(cardIndex);//Debug.log
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

            playersList.Add(newPlayer);
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

        if (NetworkManager.LocalClient.ClientId == firstPlayerId)
        {
            btnTurnController(1);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void NextPlayerTurnServerRpc()
    {
        currentPlayerIndex.Value++;

        if (currentPlayerIndex.Value == playersList.Count)
        {
            currentPlayerIndex.Value = 0;
        }

        PlayersTurnChangedClientRpc(currentPlayerIndex.Value);
    }
    [ClientRpc]
    private void PlayersTurnChangedClientRpc(int playerIndex)
    {
        ulong clientId = MonopolyMultiplayer.Instance.GetClientIdFromPlayerIndex(playerIndex);
        if (clientId == NetworkManager.LocalClient.ClientId)
        {
            //Debug.Log("Мой ход.");
            btnTurnController(1);
        }
        else
        {
            //Debug.Log("not Мой ход.");
            btnTurnController(4);
        }
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

    private void OnDisable()
    {
        btnStartTurn.onClick.RemoveListener(() => {
            btnTurnController(4);
            RollTheDicesServerRpc();
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
    public override void OnNetworkDespawn()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted -= NetworkManager_OnLoadEventCompleted;
        Instance.AllClientsConnected -= GameController_AllClientsConnected;
    }
}
