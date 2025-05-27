using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class WindowTradeView : NetworkBehaviour
{
    [SerializeField] private Transform containerPlayer1;
    [SerializeField] private Transform containerPlayer2;
    [SerializeField] private Transform template1;
    [SerializeField] private Transform template2;
    [SerializeField] private TextMeshProUGUI Player1TextName;
    [SerializeField] private TextMeshProUGUI Player2TextName;
    [SerializeField] private TextMeshProUGUI Player1TextMoney;
    [SerializeField] private TextMeshProUGUI Player2TextMoney;
    [SerializeField] private Button btnAccept;
    [SerializeField] private Button btnDecline;
    [SerializeField] private Button btnDeleteTrade;
    [SerializeField] private Button btnClose;

    private NetworkVariable<int> indexTrade = new NetworkVariable<int>();

    private void Awake()
    {
        Hide();
        template1.gameObject.SetActive(false);
        template2.gameObject.SetActive(false);

        btnAccept.onClick.AddListener(() =>
        {
            AcceptTrade();
            DeleteTrade();
            Hide();
        });
        btnDecline.onClick.AddListener(() =>
        {
            DeleteTrade();
            Hide();
        });
        btnDeleteTrade.onClick.AddListener(() =>
        {
            DeleteTrade();
            Hide();
        });
        btnClose.onClick.AddListener(() =>
        {
            Hide();
        });
    }
    public int GetIndexTrade()
    {
        return indexTrade.Value - 1;
    }
    public bool IsWindowActive()
    {
        return gameObject.activeSelf;
    }
    public void Show(int index)
    {
        indexTrade.Value = index;
        InitializeProperty();
        VerifyClientId();
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void InitializeProperty()
    {
        TradeData tradeData = TradesContainerViewUI.instance.GetTradeInfoFromIndex(indexTrade.Value - 1);

        //var playerName1 = MonopolyMultiplayer.Instance.GetPlayerNameFromPlayerData(tradeData.FirstClientId).ToString();
        //var playerName2 = MonopolyMultiplayer.Instance.GetPlayerNameFromPlayerData(tradeData.SecondClientId).ToString();

        Player1TextName.text = $"{MonopolyMultiplayer.Instance.GetPlayerNameFromPlayerData(tradeData.FirstClientId)}";
        Player2TextName.text = $"{MonopolyMultiplayer.Instance.GetPlayerNameFromPlayerData(tradeData.SecondClientId)}";

        Player1TextMoney.text = tradeData.FirstPlayerMoney != 0 ? tradeData.FirstPlayerMoney.ToString() + "$" : "0$";
        Player2TextMoney.text = tradeData.SecondPlayerMoney != 0 ? tradeData.SecondPlayerMoney.ToString() + "$" : "0$";

        RectTransform rectTransform1 = containerPlayer1.GetComponent<RectTransform>();
        RectTransform rectTransform2 = containerPlayer2.GetComponent<RectTransform>();

        Vector2 size1 = rectTransform1.sizeDelta;
        size1.y = 28.0f;
        rectTransform1.sizeDelta = size1;

        Vector2 size2 = rectTransform2.sizeDelta;
        size2.y = 28.0f;
        rectTransform2.sizeDelta = size2;

        foreach (Transform child in containerPlayer1)
        {
            if (child == template1) continue;
            Destroy(child.gameObject);
        }

        foreach (Transform child in containerPlayer2)
        {
            if (child == template2) continue;
            Destroy(child.gameObject);
        }

        var playerList1 = tradeData.FirstPlayerPropertyChoosed;
        var playerList2 = tradeData.SecondPlayerPropertyChoosed;

        for (int i = 0; i < playerList1.Length; i++)
        {
            Transform property = Instantiate(template1, containerPlayer1);
            property.gameObject.SetActive(true);

            //Debug.Log("Board Instance: " + BoardController.Instance);
            string cityName = BoardController.Instance.GetCardCityName(playerList1[i]);
            property.GetComponent<ToggleButtonColor>().SetCardId(playerList1[i]);
            TextMeshProUGUI textbtn = property.GetComponentInChildren<TextMeshProUGUI>();

            textbtn.text = cityName;

            size1.y += 28.0f;
            rectTransform1.sizeDelta = size1;
        }

        for (int i = 0; i < playerList2.Length; i++)
        {
            Transform property = Instantiate(template2, containerPlayer2);
            property.gameObject.SetActive(true);

            string cityName = BoardController.Instance.GetCardCityName(playerList2[i]);
            property.GetComponent<ToggleButtonColor>().SetCardId(playerList2[i]);
            TextMeshProUGUI textbtn = property.GetComponentInChildren<TextMeshProUGUI>();

            textbtn.text = cityName;

            size2.y += 28.0f;
            rectTransform2.sizeDelta = size2;
        }
    }

    private void VerifyClientId()
    {
        ulong firstClientiId = (ulong)TradesContainerViewUI.instance.GetFirstClientIdInfoFromIndex(indexTrade.Value - 1);
        ulong secondClientiId = (ulong)TradesContainerViewUI.instance.GetSecondClientIdInfoFromIndex(indexTrade.Value - 1);

        if (firstClientiId == NetworkManager.Singleton.LocalClientId)
        {
            btnAccept.gameObject.SetActive(false);
            btnDecline.gameObject.SetActive(false);
            btnDeleteTrade.gameObject.SetActive(true);
        }
        if (secondClientiId == NetworkManager.Singleton.LocalClientId)
        {
            btnAccept.gameObject.SetActive(true);
            btnDecline.gameObject.SetActive(true);
            btnDeleteTrade.gameObject.SetActive(false);
        }
        if (firstClientiId != NetworkManager.Singleton.LocalClientId && secondClientiId != NetworkManager.Singleton.LocalClientId)
        {
            btnAccept.gameObject.SetActive(false);
            btnDecline.gameObject.SetActive(false);
            btnDeleteTrade.gameObject.SetActive(false);
        }
    }

    private void DeleteTrade()
    {
        TradesContainerViewUI.instance.DeleteTradeFromListServerRpc(indexTrade.Value - 1);
    }
    private void AcceptTrade()
    {
        TradeData tradeData = TradesContainerViewUI.instance.GetTradeInfoFromIndex(indexTrade.Value - 1);

        var playerList1 = tradeData.FirstPlayerPropertyChoosed;
        var playerList2 = tradeData.SecondPlayerPropertyChoosed;

        for (int i = 0; i < playerList1.Length; i++)
        {
            MonopolyMultiplayer.Instance.RemoveFromPlayerListPropertyServerRpc((ulong)tradeData.FirstClientId, playerList1[i]);
            BoardController.Instance.PlayerSellCardForTradeServerRpc(playerList1[i], (ulong)tradeData.FirstClientId);
        }
        for (int i = 0; i < playerList2.Length; i++)
        {
            MonopolyMultiplayer.Instance.RemoveFromPlayerListPropertyServerRpc((ulong)tradeData.SecondClientId, playerList2[i]);
            BoardController.Instance.PlayerSellCardForTradeServerRpc(playerList2[i], (ulong)tradeData.SecondClientId);
        }
        for (int i = 0; i < playerList1.Length; i++)
        {
            MonopolyMultiplayer.Instance.AddToPlayerListPropertyServerRpc((ulong)tradeData.SecondClientId, playerList1[i]);
            GameController.Instance.PlayerBuyCardForTradeServerRpc((ulong)tradeData.SecondClientId, playerList1[i]);
        }
        for (int i = 0; i < playerList2.Length; i++)
        {
            MonopolyMultiplayer.Instance.AddToPlayerListPropertyServerRpc((ulong)tradeData.FirstClientId, playerList2[i]);
            GameController.Instance.PlayerBuyCardForTradeServerRpc((ulong)tradeData.FirstClientId, playerList2[i]);
        }
        GameController.Instance.SetPlayerMoneyServerRpc(tradeData.FirstClientId, tradeData.SecondClientId, tradeData.FirstPlayerMoney, tradeData.SecondPlayerMoney);
    }
}
