using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoardController : NetworkBehaviour
{
    [SerializeField] private GameObject[] cardPrefCountries;
    [SerializeField] private GameObject[] cardPrefInfrastructures;
    [SerializeField] private List<GameObject> boardCardPositions;//Список всех полей
    [SerializeField] private Transform objectCanvas;
    [SerializeField] private GameObject PanelForCardInfoOperations;
    [SerializeField] private GameObject ToolTipsEmptryPref;

    //private UnityEngine.UI.Button[] ButtonsPanelForCardInfoOperations;

    static public BoardController Instance;
    private GameObject currentCardOpenInfo { get; set; }//Для удаления обьектов cardINfo
    private GameObject currentCardPanelOpenInfo { get; set; }//Для удаления панели для  cardINfo
    private int currentPlayerPosition;//Для понимая с какой картой работать
    private int currentCardInfoIndex { get; set; }//Для работы кнопок на панели
    private UnityEngine.UI.Button[] ButtonsPanelCardInfo;//Кнопки панели

    private readonly Dictionary<string, ReactiveProperty<int>> _countries = new ();//Словарь всех стран и кол-во купленых городов
    private int[] cardsOpenClients = new int[6];//Список открытых карт инфо
    private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
    private readonly Dictionary<string, int> _thresholds = new()
    {
        { "Slovakia", 3 },
        { "Poland", 3 },
        { "Turkey", 3 },
        { "German", 3 },
        { "Ukraine", 3 },
        { "Usa", 3 },
        { "Australia", 3 },
        { "Netherlands", 2 }// У Нидерландов свой лимит
    };

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        AddCardsToListAndInitialize();//Для инициализация списка карт
        if (IsServer)
        {
            InitializeAndSubscribeCountryValue();//Для реактивных значений
            InitializeListCardsInfo();
        }
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ShowOrHideCardInfo();
        }
    }
    private void InitializeListCardsInfo()
    {
        for (int i = 0; i < 6; i++)
        {
            cardsOpenClients[i] = -1;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetValueListCardsInfoServerRpc(int value, ulong clientId)
    {
        Debug.Log("SetValue card:" + value);
        cardsOpenClients[clientId] = value;
    }
    private void GetButtonsFromPanelAndAddListener(GameObject PanelCardInfo)
    {
        /*UnityEngine.UI.Button[]*/ ButtonsPanelCardInfo = PanelCardInfo.gameObject.GetComponentsInChildren<UnityEngine.UI.Button>();

        ButtonsPanelCardInfo[0].onClick.AddListener(PlayerUpgradeCard);
        ButtonsPanelCardInfo[1].onClick.AddListener(PlayerDemoteCard);
        ButtonsPanelCardInfo[2].onClick.AddListener(PlayerSellCard);
    }
    private void InitializeAndSubscribeCountryValue()
    {
        foreach (var country in _thresholds.Keys)
        {
            _countries[country] = new ReactiveProperty<int>(0);

            _countries[country].Subscribe(value =>
            {
                if (value > 1)
                {
                    VerifyAllCountryBelongToOnePlayer(country);
                }

            }).AddTo(_compositeDisposable);
        }
    }
    private void VerifyAllCountryBelongToOnePlayer(string countryName)
    {
        var listCitiesCountry = GetCitiesByCountry(countryName);

        if (listCitiesCountry.Count == 0)
        {
            Debug.Log($"В стране {countryName} не нашли городов.");
            return;
        }

        Player owner = listCitiesCountry[0].GetPlayerOwner();

        bool allSameOwner = listCitiesCountry.All(city => city.GetPlayerOwner() == owner);

        if (allSameOwner)
        {
            Debug.Log($"Все города в {countryName} принадлежат игроку {owner.GetPlayerId()}!");//
            MakeCardsCountryPossibleToUpgrade(listCitiesCountry, true);
        }
        else
        {
            Debug.Log($"Города в {countryName} имеют разных владельцев.");
            MakeCardsCountryPossibleToUpgrade(listCitiesCountry, false);
        }
    }
    private void MakeCardsCountryPossibleToUpgrade(List<Card> values, bool res)
    {
        for (int i = 0; i < values.Count; i++)
        {
            values[i].SetCardUpgradeOrNot(res);
        }
    }
    private List<Card> GetCitiesByCountry(string countryName)
    {
        return boardCardPositions
            .Select(obj => obj != null ? obj.GetComponent<Card>() : null)
            .Where(card => card != null && card.GetCountryName() == countryName)
            .ToList();
    }

    private void AddCardsToListAndInitialize()
    {
        foreach (Transform child in transform)
        {
            boardCardPositions.Add(child.gameObject);
        }

        boardCardPositions[1].GetComponent<Card>().InitializeCardCountry("Trnava", "Slovakia", 2, 10, 30, 90, 160, 250, 60, 50, 50);//Countries
        boardCardPositions[3].GetComponent<Card>().InitializeCardCountry("Bratislava", "Slovakia", 2, 10, 30, 90, 160, 250, 60, 50, 50);
        boardCardPositions[4].GetComponent<Card>().InitializeCardCountry("Kosice", "Slovakia", 4, 20, 60, 180, 320, 450, 90, 50, 50);
        boardCardPositions[6].GetComponent<Card>().InitializeCardCountry("Krakow", "Poland", 6, 30, 90, 270, 400, 550, 100, 50, 50);
        boardCardPositions[7].GetComponent<Card>().InitializeCardCountry("Warsawa", "Poland", 6, 30, 90, 270, 400, 550, 100, 50, 50);
        boardCardPositions[9].GetComponent<Card>().InitializeCardCountry("Gdansk", "Poland", 8, 40, 100, 300, 450, 600, 120, 50, 50);
        boardCardPositions[11].GetComponent<Card>().InitializeCardCountry("Ankara", "Turkey", 10, 50, 150, 450, 625, 750, 140, 100, 100);
        boardCardPositions[12].GetComponent<Card>().InitializeCardCountry("Stambul", "Turkey", 10, 50, 150, 450, 625, 750, 140, 100, 100);
        boardCardPositions[14].GetComponent<Card>().InitializeCardCountry("Antalya", "Turkey", 12, 60, 180, 500, 700, 900, 160, 100, 100);
        boardCardPositions[16].GetComponent<Card>().InitializeCardCountry("Dresden", "German", 14, 70, 200, 550, 750, 950, 180, 100, 100);
        boardCardPositions[18].GetComponent<Card>().InitializeCardCountry("Berlin", "German", 14, 70, 200, 550, 750, 950, 180, 100, 100);
        boardCardPositions[19].GetComponent<Card>().InitializeCardCountry("Frankfurt", "German", 16, 80, 220, 600, 800, 1000, 200, 100, 100);
        boardCardPositions[21].GetComponent<Card>().InitializeCardCountry("Odesa", "Ukraine", 18, 90, 250, 700, 875, 1050, 220, 150, 150);
        boardCardPositions[23].GetComponent<Card>().InitializeCardCountry("Kyiv", "Ukraine", 18, 90, 250, 700, 875, 1050, 220, 150, 150);
        boardCardPositions[24].GetComponent<Card>().InitializeCardCountry("Kharkiv", "Ukraine", 20, 100, 300, 750, 925, 1100, 240, 150, 150);
        boardCardPositions[26].GetComponent<Card>().InitializeCardCountry("California", "Usa", 22, 110, 330, 800, 975, 1150, 260, 150, 150);
        boardCardPositions[27].GetComponent<Card>().InitializeCardCountry("Washington", "Usa", 22, 110, 330, 800, 975, 1150, 260, 150, 150);
        boardCardPositions[29].GetComponent<Card>().InitializeCardCountry("New York", "Usa", 24, 120, 360, 850, 1025, 1200, 280, 150, 150);
        boardCardPositions[31].GetComponent<Card>().InitializeCardCountry("Perth", "Australia", 26, 130, 390, 900, 1100, 1275, 300, 200, 200);
        boardCardPositions[32].GetComponent<Card>().InitializeCardCountry("Melbourne", "Australia", 26, 130, 390, 900, 1100, 1275, 300, 200, 200);
        boardCardPositions[34].GetComponent<Card>().InitializeCardCountry("Sydney", "Australia", 28, 150, 450, 1000, 1200, 1400, 320, 200, 200);
        boardCardPositions[37].GetComponent<Card>().InitializeCardCountry("Houten", "Netherlands", 35, 175, 500, 1100, 1300, 1500, 350, 200, 200);
        boardCardPositions[39].GetComponent<Card>().InitializeCardCountry("Amsterdam", "Netherlands", 50, 200, 600, 1400, 1700, 2000, 400, 200, 200);

        boardCardPositions[2].GetComponent<CardSpecial>().InitializeCard("Luxury\nTax", "15%");
        boardCardPositions[22].GetComponent<CardSpecial>().InitializeCard("Poor\nTax", "5%");

        boardCardPositions[8].GetComponent<Card>().InitializeCardInfrastructure("Factory", 200, 25, 50, 100, 200, 400);//Infrastructure
        boardCardPositions[13].GetComponent<Card>().InitializeCardInfrastructure("Stadium", 200, 25, 50, 100, 200, 400);
        boardCardPositions[28].GetComponent<Card>().InitializeCardInfrastructure("Drug\nStore", 200, 25, 50, 100, 200, 400);
        boardCardPositions[33].GetComponent<Card>().InitializeCardInfrastructure("Gas\nStation", 200, 25, 50, 100, 200, 400);
        boardCardPositions[36].GetComponent<Card>().InitializeCardInfrastructure("Airport", 200, 25, 50, 100, 200, 400);
    }
    private void ShowOrHideCardInfo()
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;

        if (!IsPointerOverUIWithTag("CardBoardTextField") && IsPointerOverUI())//Проверяем, был ли клик по UI
        {
            if (IsPointerOverUIWithTag("cardInfoPref"))
            {
                Debug.Log("Клик по UI, но на cardInfoPref — не скрываем");
                return;
            }

            Debug.Log("Клик по UI — скрываем карту");
            if (currentCardOpenInfo != null)
            {
                DeleteCardInfo(localId);
            }

            if (currentCardPanelOpenInfo != null)
            {
                DeleteCardPanelInfo();
            }
        }
        else//If not UI
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = LayerMask.GetMask("CardsCountryLayer");

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                Card cardCountry = hit.collider.gameObject.GetComponent<Card>();

                if (cardCountry != null)
                {
                    if (currentCardOpenInfo != null)
                    {
                        DeleteCardInfo(localId);
                    }

                    if (currentCardPanelOpenInfo != null)
                    {
                        DeleteCardPanelInfo();
                    }

                    int index = cardCountry.GetCardIndex();

                    GetAndSetCardInfoAndPanelInfo(index, localId);
                }
                else//If not cardObject
                {
                    DeleteCardInfo(localId);
                    DeleteCardPanelInfo();
                }
            }
            else//If nothing
            {
                DeleteCardInfo(localId);
                DeleteCardPanelInfo();
            }
        }
    }
    private void GetAndSetCardInfoAndPanelInfo(int index, ulong localId)
    {
        SetValueListCardsInfoServerRpc(index, localId);

        if (index != 2 && index != 5 && index != 15 && index != 17 && index != 22 && index != 25 &&
                index != 35 && index != 38 && index != 0 && index != 10 && index != 20 && index != 30) // Special cards
        {
            bool owner = VerifyCardOwner(index);

            if (index != 8 && index != 13 && index != 28 && index != 33 && index != 36)//Countries cards
            {
                if (owner)
                {
                    bool result = CurrentPlayerIsOwnThisCard(index, localId);

                    if (result)
                    {
                        CreatePanelForCardInfo(3, index);
                    }
                    else
                    {
                        CreatePanelForCardInfo(2, index);
                    }
                }
            }
            else//Infrastructuries cards
            {
                if (owner)
                {
                    bool result = CurrentPlayerIsOwnThisCard(index, localId);

                    if (result)
                    {
                        CreatePanelForCardInfo(1, index);
                    }
                    else
                    {
                        CreatePanelForCardInfo(0, index);
                    }
                }
            }
            CreateCardInfoUI(index);
        }
    }

    private bool IsPointerOverUI()//понять как работает
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
    private bool IsPointerOverUIWithTag(string tag)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag(tag))
            {
                return true; // Нашли объект с нужным тегом
            }
        }
        return false;
    }

    private bool VerifyCardOwner(int index)
    {
        var card = boardCardPositions[index].GetComponent<Card>();

        if (card.GetPlayerOwner() != null)
        {

            return true;
        }

        return false;
    }

    private void CreateCardInfoUI(int index)
    {
        currentCardInfoIndex = index;//Для дальнейшего использования конкретной карты

        Card card = boardCardPositions[index].GetComponent<Card>();
        Vector2 position = new Vector2(-750.0f, -90.0f);

        GameObject cardInfo = TypeOfCountryPref(index);
        currentCardOpenInfo = Instantiate(cardInfo, objectCanvas);

        //CreateToolTipsEmptyPref();//Создание подсказок, пока невидимых

        RectTransform rectTransform = currentCardOpenInfo.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;

        TextMeshProUGUI[] textComponent = currentCardOpenInfo.GetComponentsInChildren<TextMeshProUGUI>();

        if (index != 8 && index != 13 && index != 28 && index != 33 && index != 36)
        {
            card.GetInfoForCardCountryInfoUpdate(textComponent);//Update info on cards Country Info
            CreateUIForRentPrice(1, index);
        }
        else
        {
            card.GetInfoForCardInfrastructureInfoUpdate(textComponent);//Update info on cards Infrastructure Info
            CreateUIForRentPrice(2, index);
        }
    }
    private void CreateUIForRentPrice(int operation, int index)
    {
        Transform gameObject1 = currentCardOpenInfo.transform.Find("UiPriceRent");
        Card card = boardCardPositions[index].GetComponent<Card>();

        if (card.GetPlayerOwner() != null)
        {
            Vector2 position;
            Player player = card.GetPlayerOwner();

            int StartPositionY = operation == 1 ? 118 : 92;
            int phase = operation == 1 ? card.GetPhaseRentCountry() : player.GetPhaseRentInfrastructure() - 1;
            Debug.Log("phase: " + phase);
            if (phase != 0)
            {
                position = new Vector2(129, StartPositionY - (phase * 51));
            }
            else
            {
                position = new Vector2(129, StartPositionY);
            }
            //gameObject1.position = position;
            RectTransform rectTransform = gameObject1.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
        }
        else
        {
            gameObject1.gameObject.SetActive(false);
        }
    }
    private void CreatePanelForCardInfo(int typeOperation, int index)
    {
        currentCardPanelOpenInfo = Instantiate(PanelForCardInfoOperations, objectCanvas);
        GetButtonsFromPanelAndAddListener(currentCardPanelOpenInfo);//Для получения доступа к кнопкам на панели

        Vector2 position = TypeOfOperationForCreatingPanelForCardInfo(typeOperation, index);

        RectTransform rectTransform = currentCardPanelOpenInfo.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;

        ulong clientOwnerId = boardCardPositions[index].GetComponent<Card>().GetClientOwnerId();
        //Debug.Log("panel playerID:" + clientOwnerId + " index: " + index);
        TextMeshProUGUI[] textComponent = currentCardPanelOpenInfo.GetComponentsInChildren<TextMeshProUGUI>();
        textComponent[0].text = "Owner " + MonopolyMultiplayer.Instance.GetPlayerNameFromPlayerId((int)clientOwnerId);
    }
    private Vector2 TypeOfOperationForCreatingPanelForCardInfo(int typeOperation, int index)
    {
        Vector2 position = new();

        ButtonsPanelCardInfo = currentCardPanelOpenInfo.GetComponentsInChildren<UnityEngine.UI.Button>();

        switch (typeOperation)
        {
            case 0://Only view card owner for infrastructure
                {
                    //Debug.Log("case 0");
                    position = new Vector2(-742.0f, -315.0f);
                    break;
                }
            case 1://View full panel for infrastructure
                {
                    //Debug.Log("case 1");
                    position = new Vector2(-742.0f, -395.0f);

                    ButtonsPanelCardInfo[0].gameObject.SetActive(false);
                    ButtonsPanelCardInfo[1].gameObject.SetActive(false);

                    RectTransform rectTransform1 = ButtonsPanelCardInfo[2].GetComponent<RectTransform>();
                    rectTransform1.anchoredPosition = new Vector2(0, 16.99993f);
                    break;
                }
            case 2://Only view card owner for country
                {
                    //Debug.Log("case 2");
                    position = new Vector2(-742.0f, -335.0f);
                    break;
                }
            case 3://View full panel for country
                {
                    //Debug.Log("case 3");
                    position = new Vector2(-742.0f, -415.0f);
                    GetAndSetButtonsForPanelInfo(index);
                    break;
                }
        }
        return position;
    }
    public void GetAndSetButtonsForPanelInfo(int index)//Поставить видимость кнопок изходя из ситуации
    {
        Card card = boardCardPositions[index].GetComponent<Card>();
        bool cardCanUpgrade = card.GetCanCardUpgradeOrNot();
        Debug.Log("cardCanUpgrade: " + cardCanUpgrade);
        if (cardCanUpgrade)
        {
            int phase = boardCardPositions[index].GetComponent<Card>().GetPhaseRentCountry();
            Debug.Log("phase: " + phase);
            if (CheckPlayerHasEnoughMoneyToUpgrade(index))//Проверка на наличие денег у игрока на улучшение
            {
                ShowHideUpgradeButton(true);
            }
            else
            {
                ShowHideUpgradeButton(false);
            }

            if (phase == 0)
            {
                ShowHideDemoteButton(false);
            }
            else
            {
                ShowHideDemoteButton(true);
            }

            if (phase == 5)
            { 
                ShowHideUpgradeButton(false);
            }

            if (FindAllCitisThisCountryAndIfOneHasUpgradeHideSellButtons(index))
            {
                ShowHideSellButton(true);
            }
            else
            {
                ShowHideSellButton(false);
            }
        }
    }
    public void ShowHideUpgradeButton(bool res)
    {
        //Debug.Log("ButtonsPanelCardInfo[0]: " + ButtonsPanelCardInfo[0]);
        if (ButtonsPanelCardInfo[0] != null)
        {
            ButtonsPanelCardInfo[0].interactable = res;
        }
    }
    public void ShowHideDemoteButton(bool res)
    {
        if (ButtonsPanelCardInfo[1] != null)
        {
            ButtonsPanelCardInfo[1].interactable = res;
        }
    }
    public void ShowHideSellButton(bool res)
    {
        if (ButtonsPanelCardInfo[2] != null)
        {
            ButtonsPanelCardInfo[2].interactable = res;
        }
    }

    private bool CurrentPlayerIsOwnThisCard(int index, ulong clientId)
    {
        Card card = boardCardPositions[index].GetComponent<Card>();
        //Debug.Log("card: " + card + " card.GetClientOwnerId: " + card.GetClientOwnerId() + " clientId: " + clientId);
        if (card.GetClientOwnerId() == clientId)
        {
            int currentPlayerIndex = GameController.Instance.GetCurrentPlayerIndex();
            int playerOwnerId = (int)boardCardPositions[index].GetComponent<Card>().GetClientOwnerId();

            if (currentPlayerIndex == playerOwnerId)
            {
                return true;
            }
        }

        return false;
    }

    public void CurrentPlayerPosition(int field)
    {
        currentPlayerPosition = field;
    }

    public int CheckCardBoughtOrNot(Player player)//ServerRpc
    {
        Card cardCountry = boardCardPositions[currentPlayerPosition].GetComponent<Card>();

        if (cardCountry.GetPlayerOwner() == null)
        {
            Debug.Log("Купить или выставить на аукцион");
            return 2;
        }
        else
        {
            if (cardCountry.GetPlayerOwner() == player)
            {
                Debug.Log("Своя клетка");
            }
            else
            {
                Debug.Log("Плачу ренту");
                int index = cardCountry.GetCardIndex();
                player.PayRent(index, cardCountry);
            }
            return 3;
        }
    }
    public void CurrentOwnerCard(int num)
    {
        Card card = boardCardPositions[num].GetComponent<Card>();
        Debug.Log("owner card " + num + " " + card.GetPlayerOwner());
    }

    private void DeleteCardInfo(ulong clientId)
    {
        Destroy(currentCardOpenInfo);
        currentCardOpenInfo = null;
        SetValueListCardsInfoServerRpc(-1, clientId);
    }

    private void DeleteCardPanelInfo()
    {
        Destroy(currentCardPanelOpenInfo);
        currentCardPanelOpenInfo = null;
    }

    private GameObject TypeOfCountryPref(int index)//For gameObject visual 
    {
        switch(index)
        {
            //Countries
            case 1:
            case 3:
            case 4:
                return cardPrefCountries[0];
            case 6:
            case 7:
            case 9:
                return cardPrefCountries[1];
            case 11:
            case 12:
            case 14:
                return cardPrefCountries[2];
            case 16:
            case 18:
            case 19:
                return cardPrefCountries[3];
            case 21:
            case 23:
            case 24:
                return cardPrefCountries[4];
            case 26:
            case 27:
            case 29:
                return cardPrefCountries[5];
            case 31:
            case 32:
            case 34:
                return cardPrefCountries[6];
            case 37:
            case 39:
                return cardPrefCountries[7];
            //Infrastructures
            case 8:
                return cardPrefInfrastructures[0];
            case 13:
                return cardPrefInfrastructures[1];
            case 28:
                return cardPrefInfrastructures[2];
            case 33:
                return cardPrefInfrastructures[3];
            case 36:
                return cardPrefInfrastructures[4];
        }
        return null;
    }

    public int ReturnPLayerPosition() => currentPlayerPosition;
    public Card ReturnCardObject(int num) => boardCardPositions[num].GetComponent<Card>();
    public Vector3 GetBoardPosition(int index) => boardCardPositions[index].transform.position;
    public int BoardCardCount() => boardCardPositions.Count;
    public bool GetPanelForCardInfoIsCreated()
    {
        if (currentCardPanelOpenInfo != null)
            return true;
        return false;
    }
    public int WhatCardNumber() => currentPlayerPosition;
    public int SumCardCost() => boardCardPositions[currentPlayerPosition].GetComponent<Card>().GetPriceCard(currentPlayerPosition);//Infrastructure
    public int SumCardCostForOpenInfo() => boardCardPositions[currentCardInfoIndex].GetComponent<Card>().GetPriceCard(currentCardInfoIndex);//Infrastructure

    public void UpdateColorCardOnBoard(int playerIndex)
    {
        UnityEngine.Color color = MonopolyMultiplayer.Instance.GetPlayerColorFromPlayerId(playerIndex);
        boardCardPositions[currentPlayerPosition].GetComponent<Card>().SetOwnerColorField(color);
    }

    public void SetDefaultColorCardOnBoard(int playerIndex)
    {
        boardCardPositions[playerIndex].GetComponent<Card>().SetOwnerColorField(new UnityEngine.Color(0.4056604f, 0.4056604f, 0.4056604f, 1));
    }

    public void PutPriceAndNameOnCardsUI()
    {
        for (int i = 0; i < boardCardPositions.Count; i++)
        {
            TextMeshProUGUI[] cardTextField = boardCardPositions[i].gameObject.GetComponentsInChildren<TextMeshProUGUI>();

            if (cardTextField.Length == 2)
            {
                if (i == 5 || i == 15 || i == 17 || i == 25 || i == 35 || i == 38) 
                    continue;
                if (i != 2 && i != 22)
                {
                    cardTextField[0].text = boardCardPositions[i].GetComponent<Card>().GetPriceCard(i).ToString() + "$";

                    if (i != 8 && i != 13 && i != 28 && i != 33 && i != 36)
                    {
                        cardTextField[1].text = boardCardPositions[i].GetComponent<Card>().GetCityName();
                    }
                    else
                    {
                        cardTextField[1].text = boardCardPositions[i].GetComponent<Card>().GetInfrastructureName();
                    }
                }
                if (i == 2)
                {
                    cardTextField[0].text = boardCardPositions[i].GetComponent<CardSpecial>().GetCardName();
                    cardTextField[1].text = boardCardPositions[i].GetComponent<CardSpecial>().GetPriceName();
                }
                if (i == 22)
                {
                    cardTextField[0].text = boardCardPositions[i].GetComponent<CardSpecial>().GetCardName();
                    cardTextField[1].text = boardCardPositions[i].GetComponent<CardSpecial>().GetPriceName();
                }
            }
        }
    }

    public void BuyCityOrInfrastructureReact(int index, Player player)//ServerRpc
    {
        string countryName = boardCardPositions[index].GetComponent<Card>().GetCountryName();

        if (index != 8 && index != 13 && index != 28 && index != 33 && index != 36)
        {
            if (_countries.ContainsKey(countryName))
            {
                _countries[countryName].Value++;
            }
            else
            {
                Debug.LogError($"Країна {countryName} не знайдена!");
            }
        }
        else
        {
            player.PlayerBuyCardInfrastructure(index);// Increase count of infrastructure
        }
    }

    private void SellCityOrInfrastructureReact(int index, Player player)
    {
        string countryName = boardCardPositions[index].GetComponent<Card>().GetCountryName();

        if (index != 8 && index != 13 && index != 28 && index != 33 && index != 36)
        {
            if (_countries.ContainsKey(countryName))
            {
                _countries[countryName].Value--;
            }
        }
        else
        {
            player.PlayerSellCardInfrastructure(index);// Increase count of infrastructure
        }
    }
    private void PlayerUpgradeCard()
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;
        PlayerUpgradeCardServerRpc(currentCardInfoIndex, localId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerUpgradeCardServerRpc(int currentCardIndex, ulong localId)
    {
        Player player = GameController.Instance.GetCurrentPlayerServerOnly();
        Card card = boardCardPositions[currentCardIndex].GetComponent<Card>();
        int priceToUpgrade = card.GetPriceHotel();

        player.UpgradeOrDemoteCity(priceToUpgrade);
        card.SetPhaseRentCountry(true);

        ChangesInfoCardServerRpc(currentCardIndex, localId);

        Debug.Log("Куплен дом на " + card.GetCityName() + " Цена ренты теперь " +  card.HowManyRentToPayForCountryCard());
    }
    private void PlayerDemoteCard()
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;
        PlayerDemoteCardServerRpc(currentCardInfoIndex, localId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerDemoteCardServerRpc(int currentCardIndex, ulong localId)
    {
        Player player = GameController.Instance.GetCurrentPlayerServerOnly();
        Card card = boardCardPositions[currentCardIndex].GetComponent<Card>();

        int priceToDemote = card.GetPriceHotel() / 2;

        player.UpgradeOrDemoteCity(-priceToDemote);
        card.SetPhaseRentCountry(false);

        ChangesInfoCardServerRpc(currentCardIndex, localId);

        Debug.Log("Продан дом на " + card.GetCityName() + " Цена ренты теперь " + card.HowManyRentToPayForCountryCard());
    }

    private void PlayerSellCard()
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;
        PlayerSellCardServerRpc(currentCardInfoIndex, localId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void PlayerSellCardServerRpc(int currentCardIndex, ulong localId)
    {
        if (currentCardIndex != 0)
        {
            Player player = GameController.Instance.GetCurrentPlayerServerOnly();
            Card card = boardCardPositions[currentCardIndex].GetComponent<Card>();

            int cardPrice = card.GetPriceCard(currentCardIndex);

            player.SellCard(cardPrice, currentCardIndex);

            SellCityOrInfrastructureReact(currentCardIndex, player);
            SetDefaultColorCardOnBoard(currentCardIndex);

            ChangesInfoCardServerRpc(currentCardIndex, localId);

            Debug.Log($"Продан {card.GetCityName()} в " + card.GetCountryName() + " Владелец тепер: " + card.GetPlayerOwner());
        }
        else
        {
            Debug.Log("currentCardInfoIndex = 0 or NULL");
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void ChangesInfoCardServerRpc(int index, ulong clientId)
    {
        for (int i = 0; i < cardsOpenClients.Length; i++)
        {
            if (cardsOpenClients[i] == index)
            {
                Debug.Log("ChangesInfoCardServerRpc");
                ChangesInfoCardClientRpc(cardsOpenClients[i], (ulong)i);
            }
        }
    }
    [ClientRpc]
    private void ChangesInfoCardClientRpc(int cardIndex, ulong clientId)
    {
        Debug.Log("clientId: " + clientId + " LocalClientId: " + NetworkManager.Singleton.LocalClientId);
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            if (currentCardOpenInfo != null)
            {
                DeleteCardInfo(clientId);

                if (currentCardPanelOpenInfo != null)
                {
                    DeleteCardPanelInfo();
                }
                Debug.Log("Card Updated");

                GetAndSetCardInfoAndPanelInfo(cardIndex, clientId);
            }
        }
    }

    public bool CheckPlayerHasEnoughMoneyToUpgrade(int index)
    {
        int currentPlayerIndex = GameController.Instance.GetCurrentPlayerIndex();
        int playerMoney = MonopolyMultiplayer.Instance.GetPlayerMoney(currentPlayerIndex);

        Card card = boardCardPositions[index].GetComponent<Card>();
        int priceToUpgrade = card.GetPriceHotel();

        if (playerMoney - priceToUpgrade >= 0)
        {
            Debug.Log("true");
            return true;
        }
        Debug.Log("false");
        return false;
    }

    public bool FindAllCitisThisCountryAndIfOneHasUpgradeHideSellButtons(int index)
    {
        var listCities = GetCitiesByCountry(boardCardPositions[index].GetComponent<Card>().GetCountryName());
        bool result = true;
        foreach (var c in listCities)
        {
            if (c.GetPhaseRentCountry() == 0)
            {
                result = true;
            }
            else
            {
                return false;
            }
        }
        return result;
    }

    /*private void CreateToolTipsEmptyPref()
    {
        GameObject gameObject = Instantiate(ToolTipsEmptryPref, objectCanvas);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0,-4);
    }*/

    public override void OnDestroy()
    {
        _compositeDisposable.Dispose();
    }
}