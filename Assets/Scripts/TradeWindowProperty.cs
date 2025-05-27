using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeWindowProperty : MonoBehaviour
{
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnCreateTrade;
    [SerializeField] private Image imagePlayer1;
    [SerializeField] private Image imagePlayer2;
    [SerializeField] private TextMeshProUGUI textPlayer1;
    [SerializeField] private TextMeshProUGUI textPlayer2;
    [SerializeField] private Slider sliderPlayer1;
    [SerializeField] private Slider sliderPlayer2;
    [SerializeField] private TMP_InputField inputPlayer1;
    [SerializeField] private TMP_InputField inputPlayer2;
    [SerializeField] private Transform containerPlayer1;
    [SerializeField] private Transform containerPlayer2;
    [SerializeField] private Transform template1;
    [SerializeField] private Transform template2;

    public static TradeWindowProperty Instance;
    private int clientIdPlayer1;
    private int clientIdPlayer2;
    private int PlayerMoney1;
    private int PlayerMoney2;
    private int PlayerMoneyMinValue = 0;

    private List<int> PlayerPropertyFirstDecided = new List<int>();
    private List<int> PlayerPropertySecondDecided = new List<int>();

    private void Awake()
    {
        Hide();
        Instance = this;
        btnClose.onClick.AddListener(() =>
        {
            Hide();
        });
        btnCreateTrade.onClick.AddListener(() =>
        {
            Hide();
            TradesContainerViewUI.instance.AddNewTradeToListServerRpc(clientIdPlayer1, clientIdPlayer2, (int)sliderPlayer1.value, (int)sliderPlayer2.value,
               PlayerPropertyFirstDecided.ToArray(), PlayerPropertySecondDecided.ToArray());
            ClearLists();
        });
        sliderPlayer1.onValueChanged.AddListener(value =>
        {
            UpdateInputField(1, (int)value);
        });
        sliderPlayer2.onValueChanged.AddListener(value =>
        {
            UpdateInputField(2, (int)value);
        });
        inputPlayer1.onValueChanged.AddListener(text =>
        {
            if (int.TryParse(text, out int result))
            {
                sliderPlayer1.value = Mathf.Clamp(result, sliderPlayer1.minValue, sliderPlayer1.maxValue);
            }
        });

        inputPlayer2.onValueChanged.AddListener(text =>
        {
            if (int.TryParse(text, out int result))
            {
                sliderPlayer2.value = Mathf.Clamp(result, sliderPlayer2.minValue, sliderPlayer2.maxValue);
            }
        });
    }
    private void OnEnable()
    {
        template1.gameObject.SetActive(false);
        template2.gameObject.SetActive(false);

        InitializeInfo();
        InitializeProperty();
    }

    private void VerifyAddOrRemoveFromListFirst(ref List<int> list, int cardId)
    {
        if (list.Contains(cardId))
        {
            list.Remove(cardId);
        }
        else
        {
            list.Add(cardId);
        }
    }

    private void InitializeInfo()
    {
        textPlayer1.text = MonopolyMultiplayer.Instance.GetPlayerNameFromPlayerId(clientIdPlayer1).ToString();
        textPlayer2.text = MonopolyMultiplayer.Instance.GetPlayerNameFromPlayerId(clientIdPlayer2).ToString();

        Color colorPlayer1 = MonopolyMultiplayer.Instance.GetPlayerColorFromPlayerId(clientIdPlayer1);
        Color colorPlayer2 = MonopolyMultiplayer.Instance.GetPlayerColorFromPlayerId(clientIdPlayer2);
        imagePlayer1.color = colorPlayer1;
        imagePlayer2.color = colorPlayer2;

        template1.GetComponent<ToggleButtonColor>().SetColor(colorPlayer1);
        template2.GetComponent<ToggleButtonColor>().SetColor(colorPlayer2);

        PlayerMoney1 = MonopolyMultiplayer.Instance.GetPlayerMoney(clientIdPlayer1);
        PlayerMoney2 = MonopolyMultiplayer.Instance.GetPlayerMoney(clientIdPlayer2);

        sliderPlayer1.minValue = PlayerMoneyMinValue;
        sliderPlayer2.minValue = PlayerMoneyMinValue;
        sliderPlayer1.maxValue = PlayerMoney1;
        sliderPlayer2.maxValue = PlayerMoney2;
        sliderPlayer1.value = 0;
        sliderPlayer2.value = 0;

        Image fillImage = sliderPlayer1.fillRect.GetComponent<Image>();
        fillImage.color = colorPlayer1;
        Image fillImage1 = sliderPlayer2.fillRect.GetComponent<Image>();
        fillImage1.color = colorPlayer2;
    }
    private void InitializeProperty()
    {
        RectTransform rectTransform1 = containerPlayer1.GetComponent<RectTransform>();
        RectTransform rectTransform2 = containerPlayer2.GetComponent<RectTransform>();

        Vector2 size1 = rectTransform1.sizeDelta;
        size1.y = 33.0f;
        rectTransform1.sizeDelta = size1;

        Vector2 size2 = rectTransform2.sizeDelta;
        size2.y = 33.0f;
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

        var playerList1 = MonopolyMultiplayer.Instance.GetPlayerListProperty((ulong)clientIdPlayer1);
        var playerList2 = MonopolyMultiplayer.Instance.GetPlayerListProperty((ulong)clientIdPlayer2);

        for (int i = 0; i < playerList1.Length; i++)
        {
            Transform property = Instantiate(template1, containerPlayer1);
            property.gameObject.SetActive(true);

            string cityName = BoardController.Instance.GetCardCityName(playerList1[i]);
            property.GetComponent<ToggleButtonColor>().SetCardId(playerList1[i]);
            TextMeshProUGUI textbtn = property.GetComponentInChildren<TextMeshProUGUI>();

            textbtn.text = cityName;

            size1.y += 33.0f;
            rectTransform1.sizeDelta = size1;

            property.GetComponent<Button>().onClick.AddListener(() =>
            {
                int cardId = property.GetComponent<ToggleButtonColor>().GetCardId();
                //Debug.Log("cardId: " + cardId);
                VerifyAddOrRemoveFromListFirst(ref PlayerPropertyFirstDecided, cardId);
            });
        }

        for (int i = 0; i < playerList2.Length; i++)
        {
            Transform property = Instantiate(template2, containerPlayer2);
            property.gameObject.SetActive(true);

            string cityName = BoardController.Instance.GetCardCityName(playerList2[i]);
            property.GetComponent<ToggleButtonColor>().SetCardId(playerList2[i]);
            TextMeshProUGUI textbtn = property.GetComponentInChildren<TextMeshProUGUI>();

            textbtn.text = cityName;

            size2.y += 33.0f;
            rectTransform2.sizeDelta = size2;

            property.GetComponent<Button>().onClick.AddListener(() =>
            {
                int cardId = property.GetComponent<ToggleButtonColor>().GetCardId();
                VerifyAddOrRemoveFromListFirst(ref PlayerPropertySecondDecided, cardId);
            });
        }
    }
    private void UpdateInputField(int player, int value)
    {
        if (player == 1)
        {
            inputPlayer1.text = value.ToString();
        }
        else
        {
            inputPlayer2.text = value.ToString();
        }
    }

    public void Show(ulong clientId, int clientId2)
    {
        clientIdPlayer1 = (int)clientId;
        clientIdPlayer2 = clientId2;
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    private void ClearLists()
    {
        PlayerPropertyFirstDecided.Clear();
        PlayerPropertySecondDecided.Clear();
    }
}
