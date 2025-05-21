using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ResultWindow : NetworkBehaviour
{
    [SerializeField] private Button buttonMainMenu;
    [SerializeField] private Button buttonStayInGame;
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI textTimer;
    [SerializeField] private TextMeshProUGUI textVote;

    public static ResultWindow Instance;
    private Dictionary<ulong, bool> playerReadyDictionary = new Dictionary<ulong, bool>();
    private NetworkVariable<byte> CountVotePlayers = new NetworkVariable<byte>(0);
    private NetworkVariable<float> CountdownTime = new NetworkVariable<float>(30.0f);
    //private NetworkVariable<byte> CountPlayers;
    private float timeOneSecond;

    private void Awake()
    {
        Instance = this;
        Hide();

        CountVotePlayers.OnValueChanged += UpdateTextVote;
        CountdownTime.OnValueChanged += UpdateTextTimer;

        buttonMainMenu.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.PlayScene(Scenes.Menu);
        });
        buttonStayInGame.onClick.AddListener(() =>
        {
            SetPlayerReadyServerRpc(NetworkManager.Singleton.LocalClientId);
        });
    }

    private void Update()
    {
        if (IsServer)
        {
            StartTimerHost();
        }
    }

    private void UpdateTextTimer(float oldValue, float newValue)
    {
        textTimer.text = newValue.ToString();
    }
    private void UpdateTextVote(byte oldValue, byte newValue)
    {
        textVote.gameObject.SetActive(true);
        //textVote.text = $"{newValue} + / + {CountPlayers.Value}";
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ulong clientId)
    {
        CountVotePlayers.Value++;

        SetPlayerReadyClientRpc(clientId);
        playerReadyDictionary[clientId] = true;

        bool allClientsReady = true;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(!playerReadyDictionary.ContainsKey(client) && !playerReadyDictionary[client])
            {
                allClientsReady = false;
                break;
            }
        }
        if (allClientsReady)
        {
            DeleteInstances();
            SceneManager.PlaySceneNetwork(Scenes.CharacterSelect);
        }
    }
    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId)
    {
        playerReadyDictionary[clientId] = true;
    }

    public void Show(string playerName)
    {
        if (DiceController.Instance != null)
        {
            DiceController.Instance.DeleteCubesServerRpc();
        }
        this.playerName.text = playerName;
        gameObject.SetActive(true);
    }
    public void StartTimerHost()
    {
        timeOneSecond += 1 * Time.deltaTime;
        if (timeOneSecond >= 1)
        {
            CountdownTime.Value -= 1;
            timeOneSecond = 0;
        }
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    private void DeleteInstances()
    {
        if (GameController.Instance != null)
        {
            Destroy(GameController.Instance.gameObject);
        }
        if (BoardController.Instance != null)
        {
            Destroy(BoardController.Instance.gameObject);
        }
        if (DiceController.Instance != null)
        {
            Destroy(DiceController.Instance.gameObject);
        }
        if (TablePlayersUI.Instance != null)
        {
            Destroy(TablePlayersUI.Instance.gameObject);
        }
    }
}
