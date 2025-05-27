using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;

public class TradesContainerViewUI : NetworkBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private Transform template;
    [SerializeField] private Transform WindowOpenTrade;

    public static TradesContainerViewUI instance;
    private NetworkList<TradeData> tradeList;
    private NetworkVariable<int> currentCountTrade = new NetworkVariable<int>(0);

    private void Awake()
    {
        instance = this;
        tradeList = new NetworkList<TradeData>();
        tradeList.OnListChanged += TradesContainerViewUI_OnListChanged;
    }

    private void TradesContainerViewUI_OnListChanged(NetworkListEvent<TradeData> changeEvent)
    {
        CreateTradeViewServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CreateTradeViewServerRpc()
    {
        CreateTradeViewClientRpc();
    }

    private void OnEnable()
    {
        template.gameObject.SetActive(false);
    }

    [ClientRpc]
    public void CreateTradeViewClientRpc()
    {
        foreach (Transform child in container)
        {
            if (child == template) continue;
            Destroy(child.gameObject);
        }

        RectTransform rectTransform = container.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 0);
        //Debug.Log("count tradeList: " + tradeList.Count);
        for (int i = 0; i < tradeList.Count; i++)
        {
            string playerName1 = MonopolyMultiplayer.Instance.GetPlayerNameFromPlayerId(tradeList[i].FirstClientId).ToString();
            string playerName2 = MonopolyMultiplayer.Instance.GetPlayerNameFromPlayerId(tradeList[i].SecondClientId).ToString();

            Transform viewtrade = Instantiate(template, container);
            viewtrade.gameObject.SetActive(true);
            viewtrade.GetComponentInChildren<TextMeshProUGUI>().text = $"{playerName1} -> {playerName2}";
            viewtrade.GetComponent<Button>().onClick.AddListener(() =>
            {
                WindowOpenTrade.gameObject.GetComponent<WindowTradeView>().Show(currentCountTrade.Value);
            });

            Vector2 vector2 = rectTransform.sizeDelta;
            vector2.y += 30.0f;
            rectTransform.sizeDelta = vector2;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void AddNewTradeToListServerRpc(int clientId1, int clientId2, int moneyPlayer1, int moneyPlayer2, int[] listFirstPlayer, int[] listSecondPlayer)
    {
        /*for (int i = 0;i < listFirstPlayer.Length; i++)
        {
            Debug.Log("items choosed1: " + listFirstPlayer[i]);
        }
        for (int i = 0; i < listSecondPlayer.Length; i++)
        {
            Debug.Log("items choosed2: " + listSecondPlayer[i]);
        }*/
        tradeList.Add(new TradeData
        {
            FirstClientId = clientId1,
            SecondClientId = clientId2,
            FirstPlayerMoney = moneyPlayer1,
            SecondPlayerMoney = moneyPlayer2,
            FirstPlayerPropertyChoosed = ConvertArrayToFixedList(listFirstPlayer),
            SecondPlayerPropertyChoosed = ConvertArrayToFixedList(listSecondPlayer)
        });
        currentCountTrade.Value++;
    }
    public FixedList128Bytes<int> ConvertArrayToFixedList(int[] array)
    {
        FixedList128Bytes<int> fixedList = new FixedList128Bytes<int>();

        for (int i = 0; i < array.Length && i < fixedList.Capacity; i++)
        {
            fixedList.Add(array[i]);
        }

        return fixedList;
    }

    public TradeData GetTradeInfoFromIndex(int index)
    {
        return tradeList[index];
    }
    public int GetFirstClientIdInfoFromIndex(int index)
    {
        return tradeList[index].FirstClientId;
    }
    public int GetSecondClientIdInfoFromIndex(int index)
    {
        return tradeList[index].SecondClientId;
    }
    [ServerRpc(RequireOwnership = false)]
    public void DeleteTradeFromListServerRpc(int tradeIndex)
    {
        for (int i = 0; i < tradeList.Count; i++)
        {
            if (tradeIndex == i)
            {
                currentCountTrade.Value--;
                tradeList.RemoveAt(i);
                CloseWindowsWithThisTradeClientRpc(tradeIndex);
                break;
            }
        }
    }
    [ClientRpc]
    private void CloseWindowsWithThisTradeClientRpc(int tradeIndex)
    {
        WindowTradeView windowTradeView = WindowOpenTrade.GetComponent<WindowTradeView>();
        if (windowTradeView.IsWindowActive())
        {
            if (windowTradeView.GetIndexTrade() == tradeIndex)
            {
                windowTradeView.Hide();
            }
        }
    }
}
