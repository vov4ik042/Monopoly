using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;
using System;

public class BoardController : MonoBehaviour
{
    [SerializeField] private GameObject[] cardPrefCountries;
    [SerializeField] private GameObject[] cardPrefInfrastructures;
    [SerializeField] private List<GameObject> boardCardPositions;//Список всех полей

    [SerializeField] private Transform objectCanvas;

    static public BoardController Instance;
    private Dictionary<int, Card> propertiesCardInfo;
    private GameObject currentCardOpenInfo { get; set; }//Для удаления обьектов cardINfo

    private int currentPlayerPosition { get; set; }//Для понимая с какой карто работать

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        AddCardsToListAndInitialize();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ShowOrHideCardInfo();
        }
    }

    private void AddCardsToListAndInitialize()
    {
        foreach (Transform child in transform)
        {
            boardCardPositions.Add(child.gameObject);
        }

        propertiesCardInfo = new Dictionary<int, Card>//Card Info
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
        };
    } 

    private void ShowOrHideCardInfo()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Card cardCountry = hit.collider.gameObject.GetComponent<Card>();
            if (cardCountry != null)
            {
                if (currentCardOpenInfo != null)
                {
                    DeleteCardInfo();
                }
                int index = cardCountry.GetCardIndex();
                if (index != 2 && index != 5 && index != 15 && index != 17 && index != 22 && index != 25 && index != 35 && index != 38 &&
                    index != 0 && index != 10 && index != 20 && index != 30) // Special and Infrastructure cards
                {
                    CreateCardInfoUI(index);
                }
            }
        }
        else
        {
            DeleteCardInfo();
        }
    }

    private void CreateCardInfoUI(int index)
    {
        Vector2 position = new Vector2(-750.0f, -110.0f);

        GameObject cardInfo = TypeOfCountryPref(index);
        currentCardOpenInfo = Instantiate(cardInfo, objectCanvas);

        RectTransform rectTransform = currentCardOpenInfo.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;

        TextMeshProUGUI[] textComponent = currentCardOpenInfo.GetComponentsInChildren<TextMeshProUGUI>();

        if (propertiesCardInfo.ContainsKey(index))
        {
            if (index != 8 && index != 13 && index != 28 && index != 33 && index != 36)
            {
                propertiesCardInfo[index].GetInfoForCardCountryInfoUpdate(textComponent);//Update info on cards Country Info
            }
            else
            {
                propertiesCardInfo[index].GetInfoForCardInfrastructureInfoUpdate(textComponent);//Update info on cards Infrastructure Info
            }
        }
    }

    public void CurrentPlayerPosition(int field)
    {
        currentPlayerPosition = field;
    }

    public int CheckCardBoughtOrNot(Player player)
    {
        Card cardCountry = boardCardPositions[currentPlayerPosition].GetComponent<Card>();
        if (cardCountry.PLayerOwner == null)
        {
            Debug.Log("Купить или выставить на аукцион");
            return 2; // 2 кнопки купить или выставить на аукцион
        }
        else
        {
            if (cardCountry.PLayerOwner == player)
            {
                Debug.Log("Своя клетка");
            }
            else
            {
                int index = cardCountry.GetCardIndex();
                player.PayRent(index, propertiesCardInfo[currentPlayerPosition]);
            }
            return 3;//Кнопку закончить ход
        }
    }
    public void CurrentOwnerCard(int num)
    {
        Card card = boardCardPositions[num].GetComponent<Card>();
        Debug.Log("owner card " + num + " " + card.PLayerOwner);
    }

    private void DeleteCardInfo()
    {
        Destroy(currentCardOpenInfo);
        currentCardOpenInfo = null;
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
        if (propertiesCardInfo.ContainsKey(currentPlayerPosition))
        {
            if (currentPlayerPosition != 8 && currentPlayerPosition != 13 && currentPlayerPosition != 28 && currentPlayerPosition != 33 && currentPlayerPosition != 36)
            { 
                return propertiesCardInfo[currentPlayerPosition].GetPriceCard(0);//Country
            }
        }
        return propertiesCardInfo[currentPlayerPosition].GetPriceCard(1);//Infrastructure
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
            if (propertiesCardInfo.TryGetValue(i, out Card data) && cardTextField != null)
            {
                if (i != 8 && i != 13 && i != 28 && i != 33 && i != 36)
                {
                    cardTextField.text = propertiesCardInfo[i].GetPriceCard(0).ToString() + "$";//Country
                }
                else
                {
                    cardTextField.text = propertiesCardInfo[i].GetPriceCard(1).ToString() + "$";//Infrastructure
                }
            }
        }
    }
}