using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UniRx;

public class BoardController : MonoBehaviour
{
    [SerializeField] private GameObject[] cardPrefCountries;
    [SerializeField] private GameObject[] cardPrefInfrastructures;
    [SerializeField] private List<GameObject> boardCardPositions;//Список всех полей
    [SerializeField] private Transform objectCanvas;
    [SerializeField] private GameObject PanelForCardInfoOperations;

    static public BoardController Instance;
    private GameObject currentCardOpenInfo { get; set; }//Для удаления обьектов cardINfo
    private GameObject currentCardPanelOpenInfo { get; set; }//Для удаления панели для  cardINfo
    private int currentPlayerPosition { get; set; }//Для понимая с какой карто работать

    /*private int CountrySlovakia { get; set; } = 0;//Все страны для счетиков покупки всех городов в стране
    private int CountryPoland { get; set; } = 0;
    private int CountryTurkey { get; set; } = 0;
    private int CountryGerman { get; set; } = 0;
    private int CountryUkraine { get; set; } = 0;
    private int CountryUsa { get; set; } = 0;
    private int CountryAustralia { get; set; } = 0;
    private int CountryNetherlands { get; set; } = 0;*/
    //private Dictionary<int, Card> propertiesCardInfo;

    private readonly Dictionary<string, ReactiveProperty<int>> _countries = new ();//Словарь всех стран и кол-во купленых городов
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
        InitializeAndSubscribeCountryValue();//Для реактивных значений
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ShowOrHideCardInfo();
        }
    }

    private void OnDestroy()
    {
        _compositeDisposable.Dispose();
    }

    private void InitializeAndSubscribeCountryValue()
    {
        foreach (var country in _thresholds.Keys)
        {
            _countries[country] = new ReactiveProperty<int>(0);

            _countries[country].Subscribe(value =>
            {
                if (value == _thresholds[country])
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

        Player owner = listCitiesCountry[0].GetPLayerOwner();

        bool allSameOwner = listCitiesCountry.All(city => city.GetPLayerOwner() == owner);

        if (allSameOwner)
        {
            Debug.Log($"Все города в {countryName} принадлежат игроку {owner.propertyPlayerID}!");//
        }
        else
        {
            Debug.Log($"Города в {countryName} имеют разных владельцев.");
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

        //Countries
        boardCardPositions[1].GetComponent<Card>().InitializeCardCountry("Trnava", "Slovakia", 2, 10, 30, 90, 160, 250, 60, 50, 50);
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

        boardCardPositions[8].GetComponent<Card>().InitializeCardInfrastructure("Factory", 200, 25, 50, 100, 200, 400);//Infrastructure
        boardCardPositions[13].GetComponent<Card>().InitializeCardInfrastructure("Stadium", 200, 25, 50, 100, 200, 400);
        boardCardPositions[28].GetComponent<Card>().InitializeCardInfrastructure("Drug Store", 200, 25, 50, 100, 200, 400);
        boardCardPositions[33].GetComponent<Card>().InitializeCardInfrastructure("Gas Station", 200, 25, 50, 100, 200, 400);
        boardCardPositions[36].GetComponent<Card>().InitializeCardInfrastructure("Airport", 200, 25, 50, 100, 200, 400);

        /*propertiesCardInfo = new Dictionary<int, Card>//Card Info
        {
            { 1, new Card("Trnava", 2, 10, 30, 90, 160, 250, 60, 50, 50) },
            { 3, new Card("Bratislava", 2, 10, 30, 90, 160, 250, 60, 50, 50) },
            { 4, new Card("Kosice", 4, 20, 60, 180, 320, 450, 90, 50, 50) },
            { 6, new Card("Krakow", 6, 30, 90, 270, 400, 550, 100, 50, 50) },
            { 7, new Card("Warsawa", 6, 30, 90, 270, 400, 550, 100, 50, 50) },
            { 8, new Card("Factory", 200, 25, 50, 100, 200, 400) },
            { 9, new Card("Gdansk", 8, 40, 100, 300, 450, 600, 120, 50, 50) },
            { 11, new Card("Ankara", 10, 50, 150, 450, 625, 750, 140, 100, 100) },
            { 12, new Card("Stambul", 10, 50, 150, 450, 625, 750, 140, 100, 100) },
            { 13, new Card("Stadium", 200, 25, 50, 100, 200, 400) },
            { 14, new Card("Antalya", 12, 60, 180, 500, 700, 900, 160, 100, 100) },
            { 16, new Card("Dresden", 14, 70, 200, 550, 750, 950, 180, 100, 100) },
            { 18, new Card("Berlin", 14, 70, 200, 550, 750, 950, 180, 100, 100) },
            { 19, new Card("Frankfurt", 16, 80, 220, 600, 800, 1000, 200, 100, 100) },
            { 21, new Card("Odesa", 18, 90, 250, 700, 875, 1050, 220, 150, 150) },
            { 23, new Card("Kyiv", 18, 90, 250, 700, 875, 1050, 220, 150, 150) },
            { 24, new Card("Kharkiv", 20, 100, 300, 750, 925, 1100, 240, 150, 150) },
            { 26, new Card("California", 22, 110, 330, 800, 975, 1150, 260, 150, 150) },
            { 27, new Card("Washington", 22, 110, 330, 800, 975, 1150, 260, 150, 150) },
            { 28, new Card("Drug Store", 200, 25, 50, 100, 200, 400) },
            { 29, new Card("New York", 24, 120, 360, 850, 1025, 1200, 280, 150, 150) },
            { 31, new Card("Perth", 26, 130, 390, 900, 1100, 1275, 300, 200, 200) },
            { 32, new Card("Melbourne", 26, 130, 390, 900, 1100, 1275, 300, 200, 200) },
            { 33, new Card("Gas Station", 200, 25, 50, 100, 200, 400) },
            { 34, new Card("Sydney", 28, 150, 450, 1000, 1200, 1400, 320, 200, 200) },
            { 36, new Card("Airport", 200, 25, 50, 100, 200, 400) },
            { 37, new Card("Houten", 35, 175, 500, 1100, 1300, 1500, 350, 200, 200) },
            { 39, new Card("Amsterdam", 50, 200, 600, 1400, 1700, 2000, 400, 200, 200) },
        };*/
    } 

    private void ShowOrHideCardInfo()
    {
        if (IsPointerOverUI())// Проверяем, был ли клик по UI
        {
            if (IsPointerOverUIWithTag("cardInfoPref"))
            {
                Debug.Log("Клик по UI, но на cardInfoPref — не скрываем");
                return;
            }

            Debug.Log("Клик по UI — скрываем карту");
            if (currentCardOpenInfo != null)
            {
                DeleteCardInfo();
            }

            if (currentCardPanelOpenInfo != null)
            {
                DeleteCardPanelInfo();
            }
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Card cardCountry = hit.collider.gameObject.GetComponent<Card>();
            Debug.Log("card obj " +  cardCountry);
            if (cardCountry != null)
            {
                if (currentCardOpenInfo != null)
                {
                    DeleteCardInfo();
                }

                if (currentCardPanelOpenInfo != null)
                {
                    DeleteCardPanelInfo();
                }

                int index = cardCountry.GetCardIndex();

                if (index != 2 && index != 5 && index != 15 && index != 17 && index != 22 && index != 25 &&
                    index != 35 && index != 38 && index != 0 && index != 10 && index != 20 && index != 30) // Special cards
                {
                    bool owner = VerifyCardOwner(index);
                    bool result = CurrentPLayerIsOwnThisCard(index);

                    if (index != 8 && index != 13 && index != 28 && index != 33 && index != 36)
                    {
                        if (owner)
                        {
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
                    else
                    {
                        if (owner)
                        {
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
            else
            {
                DeleteCardInfo();
                DeleteCardPanelInfo();
            }
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
            if (result.gameObject.CompareTag(tag)) // Проверяем тег
            {
                return true; // Нашли объект с нужным тегом
            }
        }
        return false;
    }

    private bool VerifyCardOwner(int index)
    {
        var card = boardCardPositions[index].GetComponent<Card>();

        if (card.GetPLayerOwner() != null)
        {
            return true;
        }

        return false;
    }

    private void CreateCardInfoUI(int index)
    {
        Vector2 position = new Vector2(-750.0f, -90.0f);

        GameObject cardInfo = TypeOfCountryPref(index);
        currentCardOpenInfo = Instantiate(cardInfo, objectCanvas);

        RectTransform rectTransform = currentCardOpenInfo.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;

        TextMeshProUGUI[] textComponent = currentCardOpenInfo.GetComponentsInChildren<TextMeshProUGUI>();

        if (index != 8 && index != 13 && index != 28 && index != 33 && index != 36)
        {
            boardCardPositions[index].GetComponent<Card>().GetInfoForCardCountryInfoUpdate(textComponent);//Update info on cards Country Info
        }
        else
        {
            boardCardPositions[index].GetComponent<Card>().GetInfoForCardInfrastructureInfoUpdate(textComponent);//Update info on cards Infrastructure Info
        }
    }

    private void CreatePanelForCardInfo(int typeOperation, int index)
    {
        Vector2 position = new();

        if (typeOperation == 0)//Only view card owner for infrastructure
        {
            position = new Vector2(-742.0f, -315.0f);
        }
        if (typeOperation == 1)//View full panel for infrastructure
        {
            position = new Vector2(-742.0f, -395.0f);
        }
        if (typeOperation == 2)//Only view card owner for country
        {
            position = new Vector2(-742.0f, -335.0f);
        }
        if (typeOperation == 3)//View full panel for country
        {
            position = new Vector2(-742.0f, -415.0f);
        }

        currentCardPanelOpenInfo = Instantiate(PanelForCardInfoOperations, objectCanvas);

        RectTransform rectTransform = currentCardPanelOpenInfo.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;

        if (typeOperation == 1)//Hide 2 buttons for infrastructure card
        {
            UnityEngine.UI.Button[] buttons = currentCardPanelOpenInfo.GetComponentsInChildren<UnityEngine.UI.Button>();
            buttons[0].gameObject.SetActive(false);
            buttons[1].gameObject.SetActive(false);
            RectTransform rectTransform1 = buttons[2].GetComponent<RectTransform>();
            rectTransform1.anchoredPosition = new Vector2(0, 16.99993f);
            //buttons[2].gameObject.transform.position = new Vector2(0, 16.99993f);
        }

        Player player = boardCardPositions[index].GetComponent<Card>().GetPLayerOwner();

        TextMeshProUGUI[] textComponent = currentCardPanelOpenInfo.GetComponentsInChildren<TextMeshProUGUI>();
        textComponent[0].text = "Owner " + player.propertyPlayerID;//Заменить на имя, в будущем
    }

    private bool CurrentPLayerIsOwnThisCard(int index)
    {
        Player player1 = GameController.Instance.GetCurrentPlayer();
        Player player2 = boardCardPositions[index].GetComponent<Card>().GetPLayerOwner();

        if (player1 == player2)
        {
            return true;
        }

        return false;
    }

    public void CurrentPlayerPosition(int field)
    {
        currentPlayerPosition = field;
    }

    public int CheckCardBoughtOrNot(Player player)
    {
        Card cardCountry = boardCardPositions[currentPlayerPosition].GetComponent<Card>();
        if (cardCountry.GetPLayerOwner() == null)
        {
            Debug.Log("Купить или выставить на аукцион");
            return 2; // 2 кнопки купить или выставить на аукцион
        }
        else
        {
            if (cardCountry.GetPLayerOwner() == player)
            {
                Debug.Log("Своя клетка");
            }
            else
            {
                int index = cardCountry.GetCardIndex();
                player.PayRent(index, cardCountry);
            }
            return 3;//Кнопку закончить ход
        }
    }
    public void CurrentOwnerCard(int num)
    {
        Card card = boardCardPositions[num].GetComponent<Card>();
        Debug.Log("owner card " + num + " " + card.GetPLayerOwner());
    }

    private void DeleteCardInfo()
    {
        Destroy(currentCardOpenInfo);
        currentCardOpenInfo = null;
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
    public int WhatCardNumber() => currentPlayerPosition;
    public int SumCardCost()
    {
        if (currentPlayerPosition != 8 && currentPlayerPosition != 13 && currentPlayerPosition != 28 && currentPlayerPosition != 33 && currentPlayerPosition != 36)
            { 
                return boardCardPositions[currentPlayerPosition].GetComponent<Card>().GetPriceCard(0);//Country
            }
        return boardCardPositions[currentPlayerPosition].GetComponent<Card>().GetPriceCard(1);//Infrastructure
    }

    public void UpdateColorCardOnBoard(Color color)
    {
        Transform obj = boardCardPositions[currentPlayerPosition].transform.Find("Color");
        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
        meshRenderer.material.color = color;
    }

    public void PutPriceOnCardsUI()
    {
        for (int i = 0; i < boardCardPositions.Count; i++)
        {
            var cardTextField = boardCardPositions[i].gameObject.GetComponentInChildren<Text>();

            if (cardTextField != null)//Cards without textField
            {
                if (i != 8 && i != 13 && i != 28 && i != 33 && i != 36)//Country cards
                {
                    cardTextField.text = boardCardPositions[i].GetComponent<Card>().GetPriceCard(0).ToString() + "$";//Country
                }
                else
                {
                    cardTextField.text = boardCardPositions[i].GetComponent<Card>().GetPriceCard(1).ToString() + "$";//Infrastructure
                }
            }
        }
    }

    public void BuyCityOrInfrastructureReact(int index)
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
                Debug.LogError($"Страна {countryName} не найдена!");
            }
        }
        else
        {
            boardCardPositions[index].GetComponent<Card>().PlayerBuyInfrastructure();//Increase count of infrastructure
        }
    }
}