using System;
using System.Collections.Generic;
using System.Net;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class MonopolyMultiplayer : NetworkBehaviour
{
    [SerializeField] private List<UnityEngine.Color> PlayerColorList;
    public static MonopolyMultiplayer Instance { get; private set; }

    private int MaxPlayersNumber = 6;
    private string playerName;
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";
    private NetworkList<PlayerData> playerDataNetworkList;

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "PlayerName" + UnityEngine.Random.Range(100,1000));
        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    public string GetPlayerName()
    {
        return playerName;
    }
    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;

        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
    }
    
    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_ServerConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }
    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerName = playerName;

        playerDataNetworkList[playerDataIndex] = playerData;
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerId = playerId;

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId)
            {
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    private void NetworkManager_ServerConnectedCallback(ulong obj)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = obj,
            colorId = GetFirstUnusedColorId(),
        });
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }
    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }
    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest,
    NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != Scenes.CharacterSelect.ToString())
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started";
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsList.Count >= MaxPlayersNumber)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full";
            return;
        }

        connectionApprovalResponse.Approved = true;
    }
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        //Debug.Log("playerDataNetworkList:" + playerDataNetworkList.Count + " playerIndex: " + playerIndex);
        return playerDataNetworkList[playerIndex];
    }
    public bool GetPlayerDataNetworkListNotNull()
    {
        return playerDataNetworkList.Count > 0;
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerBankruptServerRpc(ulong clientId)
    {
        if (playerDataNetworkList.Count > 0)
        {
            int playerDataIndex = GetPlayerDataIndexFromClientId(clientId);
            if(playerDataIndex != -1)
            {
                PlayerData playerData = playerDataNetworkList[playerDataIndex];

                playerData.playerBankrupt = true;

                playerDataNetworkList[playerDataIndex] = playerData;
            }
        }
    }
    public bool GetPlayerBankrupt(int playerIndex)
    {
        return playerDataNetworkList[playerIndex].playerBankrupt;
    }
    public ulong GetClientIdFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex].clientId;
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerMoneyServerRpc(int playerIndex, int money)
    {
        PlayerData playerData = playerDataNetworkList[playerIndex];

        playerData.playerMoney = money;

        playerDataNetworkList[playerIndex] = playerData;
        //Debug.Log("MOnopoly moneyPLayer: " + playerData.playerMoney);
        TablePlayersUI.Instance.UpdateInfo();
    }

    public int GetPlayerMoney(int playerIndex)
    {
        return playerDataNetworkList[playerIndex].playerMoney;
    }
    public int GetPlayerDataNetworkListCount()
    {
        return playerDataNetworkList.Count;
    }
    public FixedString64Bytes GetPlayerNameFromPlayerData(int playerIndex)
    {
        return playerDataNetworkList[playerIndex].playerName;
    }
    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach(var playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
            {
                return playerData;
            }
        }

        return default;
    }
    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }

        return -1;
    }
    public PlayerData GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public UnityEngine.Color GetPlayerColor(int colorId)
    {
        return PlayerColorList[colorId];
    }
    public UnityEngine.Color GetPlayerColorFromPlayerId(int playerIndex)
    {
        PlayerData playerData = playerDataNetworkList[playerIndex];
        return PlayerColorList[playerData.colorId];
    }
    public FixedString64Bytes GetPlayerNameFromPlayerId(int playerIndex)
    {
        return playerDataNetworkList[playerIndex].playerName;
    }

    public void ChangePlayerColor(int colorId)
    {
        ChangePlayerColorServerRpc(colorId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerColorServerRpc(int colorId, ServerRpcParams serverRpcParams = default)
    {
        if (!IsColorAvailable(colorId))
        {
            return;
        }

        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.colorId = colorId;

        playerDataNetworkList[playerDataIndex] = playerData;

    }

    private bool IsColorAvailable(int colorId)
    {
        foreach (var playerData in playerDataNetworkList)
        {
            if (playerData.colorId == colorId)
            {
                return false;
            }
        }
        return true;
    }

    private int GetFirstUnusedColorId()
    {
        for (int i = 0; i < PlayerColorList.Count; i++)
        {
            if (IsColorAvailable(i))
            {
                return i;
            }
        }
        return -1;
    }
    [ServerRpc(RequireOwnership = false)]
    public void RemovePlayerFromListServerRpc(ulong clientId)
    {
        NetworkManager_Server_OnClientDisconnectCallback(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void KickPlayerServerRpc(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManager_Server_OnClientDisconnectCallback(clientId);
    }
    [ServerRpc(RequireOwnership = false)]
    public void DeleteInstanceAndClearListsServerRpc()
    {
        if (playerDataNetworkList != null)
        {
            playerDataNetworkList.Clear();
        }
    }
    public override void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        if (playerDataNetworkList != null)
        {
            playerDataNetworkList.OnListChanged -= PlayerDataNetworkList_OnListChanged;
            playerDataNetworkList.Dispose();
        }
    }
}
