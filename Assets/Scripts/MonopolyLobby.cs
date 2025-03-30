using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using Unity.VisualScripting;

public class MonopolyLobby : NetworkBehaviour
{
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button QuickJoinLobbyButton;
    [SerializeField] private Button CodeJoinLobbyButton;
    //[SerializeField] private Button ListLobbiesButton;
    [SerializeField] private Button buttonMainMenu;
    [SerializeField] private GameObject CreateLobbyPublicPrivate;
    [SerializeField] private TextMeshProUGUI lobbyCodeInput;

    private int MaxPlayersNumber = 6;
    public static MonopolyLobby Instance { get; private set; }

    private Lobby joinedLobby;
    private float heartbeatTimer;

    private NetworkList<PlayerData> playerDataNetworkList;

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChange;

    /*private Lobby hostLobby;
    private float lobbyUpdateTimer;
    private string playerName;*/


    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeAuthentication();

        createLobbyButton.onClick.AddListener(() =>
        {
            CreateLobbyOpenWindow();
        });
        QuickJoinLobbyButton.onClick.AddListener(() =>
        {
            QuickJoin();
        });
        CodeJoinLobbyButton.onClick.AddListener(() =>
        {
            JoinWithCode(lobbyCodeInput.text);
        });

        buttonMainMenu.onClick.AddListener(MainMenuExit);
    }

    private void Start()
    {
        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChange?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        HandleLobbyHeartBeat();
        //HandleLobbyPollForUpdates();
    }

    private void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    private void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NewworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        playerDataNetworkList.Add(new PlayerData {
            clientId = obj
        });
    }

    private async void InitializeAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions options = new InitializationOptions();
            options.SetProfile(UnityEngine.Random.Range(0,1000).ToString());

            await UnityServices.InitializeAsync(options);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MaxPlayersNumber, new CreateLobbyOptions
            {
                IsPrivate = isPrivate
            });
            Debug.Log("Create lobby! " + joinedLobby.Name + " " + joinedLobby.MaxPlayers + " " + joinedLobby.Id + " " + joinedLobby.LobbyCode);

            StartHost();

            SceneManager.PlaySceneNetwork(Scenes.CharacterSelect);
        } catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private void NewworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest,
        NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)//3:45:35//Destroy object 1:57:00
    {
        //3:26:38
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
    public async void QuickJoin()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();

            StartClient();
            Debug.Log("Quick Joined");
        } catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public async void JoinWithCode(string lobbyCode)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            StartClient();
        } catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public Lobby GetLobby()
    {
        return joinedLobby;
    }
    private async void HandleLobbyHeartBeat()
    {
        if (IsLobbyHost())
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0f)
            {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    /*    private async void PlayerConnectedToLobby()
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            playerName = "vov4ik042" + UnityEngine.Random.Range(10, 90);
            Debug.Log(playerName);
        }
        private async void CreateLobby()
        {
            try
            {
                string lobbyName = "MyLobby";
                int maxPlayers = 8;
                CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Player = GetPlayer(),
                    Data = new Dictionary<string, DataObject>
                    {
                        { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "CaptureTheFlag") },
                    }
                };

                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

                hostLobby = lobby;
                joinedLobby = hostLobby;

                Debug.Log("Create lobby! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
                PrintPlayers(hostLobby);
            } catch (LobbyServiceException e){
                Debug.Log(e);
            }
        }

        private async void HandleLobbyPollForUpdates()
        {
            if (joinedLobby != null)
            {
                lobbyUpdateTimer -= Time.deltaTime;
                if (lobbyUpdateTimer < 0f)
                {
                    float lobbyUpdateTimerMax = 1.1f;
                    lobbyUpdateTimer = lobbyUpdateTimerMax;

                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                    joinedLobby = lobby;
                }
            }
        }

        private async void ListLobbies()
        {
            try
            {
                QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
                {
                    Count = 25,
                    Filters = new List<QueryFilter>
                    {
                        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                    },
                    Order = new List<QueryOrder>
                    {
                        new QueryOrder(false, QueryOrder.FieldOptions.Created)//from the oldest to the new
                    }
                };

                QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

                Debug.Log("Lobbies found: " + queryResponse.Results.Count);
                foreach (Lobby lobby in queryResponse.Results)
                {
                    Debug.Log(lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Data["GameMode"].Value);
                }
            } catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        private async void JoinLobbyByCode(string lobbyCode)
        {
            try {
                JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
                {
                    Player = GetPlayer()
                };
                Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
                joinedLobby = lobby;
                Debug.Log("Joined lobby with code " + lobbyCode);

                PrintPlayers(lobby);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }

        private async void QuickJoinLobby()
        {
            try
            {
                await LobbyService.Instance.QuickJoinLobbyAsync();
                Debug.Log("Quick Joined");
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
        private Unity.Services.Lobbies.Models.Player GetPlayer()
        {
            return new Unity.Services.Lobbies.Models.Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
                    }
            };
        }

        private void PrintPlayers(Lobby lobby)
        {
            Debug.Log("PLayers in lobby " + lobby.Name + " " + lobby.Data["GameMode"].Value);
            foreach (Unity.Services.Lobbies.Models.Player player in lobby.Players)
            {
                Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
            }
        }

        private async void UpdateLobbyGameMode(string gameMode)
        {
            try
            {
                hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
                    }
                });
                joinedLobby = hostLobby;
                PrintPlayers(hostLobby);
            } catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        private async void UpdatePLayerName(string newPlayerName)
        {
            try
            {
                playerName = newPlayerName;
                await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
                    }
                });
            } catch (LobbyServiceException e){
                Debug.Log(e);
            }
        } 

        private async void LeaveLobby()
        {
            try{
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            } catch (LobbyServiceException e){
                Debug.Log(e);
            }
        }

        private async void KickPLayer()
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        private async void MigrateLobbyHost()
        {
            try
            {
                hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
                {
                    HostId = joinedLobby.Players[1].Id
                });
                joinedLobby = hostLobby;
                PrintPlayers(hostLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        private async void DeleteLobby()
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
            } catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }*/

    private void CreateLobbyOpenWindow()
    {
        CreateLobbyPublicPrivate.gameObject.SetActive(true);
    }
    private void MainMenuExit()
    {
        SceneManager.PlayScene(Scenes.Menu);
    }

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }

    public override void OnDestroy()
    {
        if (playerDataNetworkList != null)
        {
            playerDataNetworkList.OnListChanged -= PlayerDataNetworkList_OnListChanged;
            playerDataNetworkList.Dispose();
            playerDataNetworkList = null;
        }
    }

}
