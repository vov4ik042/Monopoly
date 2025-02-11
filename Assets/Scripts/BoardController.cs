using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;

public class Board : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefUsa;
    [SerializeField] private GameObject cardPrefSlovakia;
    [SerializeField] private GameObject cardPrefUkraine;
    [SerializeField] private GameObject cardPrefTurkey;
    [SerializeField] private GameObject cardPrefNetherlands;
    [SerializeField] private GameObject cardPrefAustralia;
    [SerializeField] private GameObject cardPrefPoland;
    [SerializeField] private GameObject cardPrefGermany;
    [SerializeField] private Transform objectCanvas;

    static public Board Instance;
    private Dictionary<int, PropertyData> properties;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        properties = new Dictionary<int, PropertyData>
        {
            { 1, new PropertyData("Trnava", 2, 10, 30, 90, 160, 250, 60, 50, 50) },
            { 3, new PropertyData("Bratislava", 2, 10, 30, 90, 160, 250, 60, 50, 50) },
            { 4, new PropertyData("Kosice", 4, 20, 60, 180, 320, 450, 90, 50, 50) },
        };
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                CardCountry cardCountry = hit.collider.gameObject.GetComponent<CardCountry>();
                if (cardCountry != null)
                {
                    int index = cardCountry.OnClicked();
                    if (index != 2 && index != 5 && index != 8 && index != 13 && index != 15 && index != 17
                        && index != 22 && index != 25 && index != 28 && index != 33 && index != 35 && index != 37) // Special cards
                    {
                        ParametersCardInfo(index);
                    }
                }
            }
        }
    }

    private void ParametersCardInfo(int index)
    {
        Vector2 position = new Vector2(743.45f, -138.0f);
        /*GameObject cardPref = new GameObject();
        int rent = 0, rentOneHouse = 0, rentTwoHouses = 0, rentThreeHouses = 0,
            rentFourHouses = 0, rentHotel = 0, price = 0, priceHouse = 0, priceHotel = 0;
        string cityName = "";
        switch (index)
        {
            case int n when (n == 1 || n == 3 || n == 4): //Slovakia
                {
                    cardPref = cardPrefSlovakia;
                    priceHouse = 50; priceHotel = 50;

                    if (index == 1)
                    {
                        cityName = "Trnava";
                        rent = 2; rentOneHouse = 10; rentTwoHouses = 30; rentThreeHouses = 90; rentFourHouses = 160;
                        rentHotel = 250; price = 60;
                    }
                    if (index == 3)
                    {
                        cityName = "Bratislava";
                        rent = 2; rentOneHouse = 10; rentTwoHouses = 30; rentThreeHouses = 90; rentFourHouses = 160;
                        rentHotel = 250; price = 60;
                    }
                    if (index == 4)
                    {
                        cityName = "Kosice";
                        rent = 4; rentOneHouse = 20; rentTwoHouses = 60; rentThreeHouses = 180; rentFourHouses = 320;
                        rentHotel = 450; price = 90;
                    }
                    break;
                }
        }*/

        GameObject cardPref = TypeOfCountryPref(index);
        GameObject obj = Instantiate(cardPref, objectCanvas);

        RectTransform rectTransform = obj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;

        TextMeshProUGUI[] textComponent = obj.GetComponentsInChildren<TextMeshProUGUI>();

        if (properties.TryGetValue(index, out PropertyData data))
        {
            textComponent[0].text = data.CityName;
            textComponent[1].text = data.Price + "$";
            textComponent[2].text = data.PriceHouse + "$";
            textComponent[3].text = data.PriceHotel + "$";
            textComponent[4].text = data.Rent + "$";
            textComponent[5].text = data.RentOneHouse + "$";
            textComponent[6].text = data.RentTwoHouses + "$";
            textComponent[7].text = data.RentThreeHouses + "$";
            textComponent[8].text = data.RentFourHouses + "$";
            textComponent[9].text = data.RentHotel + "$";
        }
        /*if (textComponent.Length > 0)
        {
            textComponent[0].text = cityName;
            textComponent[1].text = price + "$";
            textComponent[2].text = priceHouse + "$";
            textComponent[3].text = priceHotel + "$";
            textComponent[4].text = rent + "$";
            textComponent[5].text = rentOneHouse + "$";
            textComponent[6].text = rentTwoHouses + "$";
            textComponent[7].text = rentThreeHouses + "$";
            textComponent[8].text = rentFourHouses + "$";
            textComponent[9].text = rentHotel + "$";
        }*/

    }
    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        FillNodes();
        for (int i = 0; i < childList.Count; i++)
        {
            Vector3 currentPos = childList[i].position;
            if(i > 0)
            {
                Vector3 previousPos = childList[i - 1].position;
                Gizmos.DrawLine(previousPos, currentPos);
            }
        }
    }
    private void FillNodes()
    {
        childList.Clear();

        for (int i = 0;i < transform.childCount;i++)
        {
            childList.Add(transform.GetChild(i));
        }
    }*/

    private GameObject TypeOfCountryPref(int index)
    {
        switch(index)
        {
            case 1:
            case 3:
            case 4:
                    return cardPrefSlovakia;
        }
        return null;
    }
}

internal class PropertyData
{
    public string CityName;
    public int Rent, RentOneHouse, RentTwoHouses, RentThreeHouses, RentFourHouses, RentHotel, Price, PriceHouse, PriceHotel;

    public PropertyData(string cityName, int rent, int rentOne, int rentTwo, int rentThree,
        int rentFour, int rentHotel, int price, int priceHouse, int priceHotel)
    {
        CityName = cityName;
        Price = price;
        PriceHouse = priceHouse;
        PriceHotel = priceHotel;
        Rent = rent;
        RentOneHouse = rentOne;
        RentTwoHouses = rentTwo;
        RentThreeHouses = rentThree;
        RentFourHouses = rentFour;
        RentHotel = rentHotel;
    }
}