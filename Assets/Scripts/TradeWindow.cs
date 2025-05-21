using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TradeWindow : MonoBehaviour
{
    [SerializeField] private Transform Container;
    [SerializeField] private Transform Template;
    [SerializeField] private TextMeshProUGUI textUp;
    [SerializeField] private Button btnClose;

    public static TradeWindow Instance;
    private float heightForText = 15.0f;
    private float heightForImage = 36.0f;
    private float heightForBtn = 15.0f;
    private void Awake()
    {
        Hide();
        Instance = this;
        btnClose.onClick.AddListener(() =>
        {
            Hide();
        });
    }
    private void Start()
    {
        Template.gameObject.SetActive(false);
        InitializeListPlayers();
    }

    private void InitializeListPlayers()
    {
        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        foreach (Transform child in Container)
        {
            if (child == Template) continue;
            Destroy(child.gameObject);
        }

        int countPlayers = MonopolyMultiplayer.Instance.GetPlayerDataNetworkListCount();
        
        for (int i = 0; i < countPlayers; i++)
        {
            if ((int)localClientId == i)
            {
                continue;
            }

            Transform createdObject = Instantiate(Template, Container);
            createdObject.gameObject.SetActive(true);

            RectTransform rectTranText = textUp.rectTransform;
            RectTransform rectTranBtn = btnClose.gameObject.GetComponent<RectTransform>();
            RectTransform rectTranContainer = Container.gameObject.GetComponent<RectTransform>();

            Vector2 sizeContainer = rectTranContainer.sizeDelta;
            sizeContainer.y += heightForImage;
            rectTranContainer.sizeDelta = sizeContainer;

            Vector2 currentPosBtn = rectTranBtn.anchoredPosition;
            rectTranBtn.anchoredPosition = new Vector2(currentPosBtn.x, currentPosBtn.y + heightForBtn);

            Vector2 currentPosText = rectTranText.anchoredPosition;
            rectTranText.anchoredPosition = new Vector2(currentPosText.x, currentPosText.y + heightForText);

            TextMeshProUGUI textMeshProUGUI = createdObject.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            FixedString64Bytes fixedString = MonopolyMultiplayer.Instance.GetPlayerNameFromPlayerId(i);

            textMeshProUGUI.text = fixedString.ToString();

            int playerId2 = i;

            createdObject.gameObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                TradeWindowProperty.Instance.Show(localClientId, playerId2);
            });
        }
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
