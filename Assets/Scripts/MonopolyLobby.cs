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
    public static MonopolyLobby Instance { get; private set; }
    private int MaxPlayersNumber = 2;
    private Lobby joinedLobby;
    private float heartbeatTimer;

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        InitializeAuthentication();
    }

    private void Update()
    {
        HandleLobbyHeartBeat();
        //HandleLobbyPollForUpdates();
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
                IsPrivate = isPrivate,
            });
            //Debug.Log("Create lobby! " + joinedLobby.Name + " " + joinedLobby.MaxPlayers + " " + joinedLobby.Id + " " + joinedLobby.LobbyCode);

            MonopolyMultiplayer.Instance.StartHost();

            SceneManager.PlaySceneNetwork(Scenes.CharacterSelect);
        } catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public async void QuickJoin()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            MonopolyMultiplayer.Instance.StartClient();
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

            MonopolyMultiplayer.Instance.StartClient();
        } catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public Lobby GetLobby()
    {
        return joinedLobby;
    }
    private void HandleLobbyHeartBeat()
    {
        if (IsLobbyHost())
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0f)
            {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;

                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }
}
