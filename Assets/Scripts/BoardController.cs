using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;

public class BoardController : MonoBehaviour
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

    [SerializeField] private List<GameObject> boardCardPositions;//Список всех полей

    static public BoardController Instance;
    private Dictionary<int, PropertyData> properties;
    private GameObject currentCardOpenInfo { get; set; }//Для удаления обьектов cardINfo

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach (Transform child in transform)
        {
            boardCardPositions.Add(child.gameObject);
        }

        properties = new Dictionary<int, PropertyData>//Card Info
        {
            { 1, new PropertyData("Trnava", 2, 10, 30, 90, 160, 250, 60, 50, 50) },
            { 3, new PropertyData("Bratislava", 2, 10, 30, 90, 160, 250, 60, 50, 50) },
            { 4, new PropertyData("Kosice", 4, 20, 60, 180, 320, 450, 90, 50, 50) },
            { 6, new PropertyData("Krakow", 6, 30, 90, 270, 400, 550, 100, 50, 50) },
            { 7, new PropertyData("Warsawa", 6, 30, 90, 270, 400, 550, 100, 50, 50) },
            { 9, new PropertyData("Gdansk", 8, 40, 100, 300, 450, 600, 120, 50, 50) },
            { 11, new PropertyData("Ankara", 10, 50, 150, 450, 625, 750, 140, 100, 100) },
            { 12, new PropertyData("Stambul", 10, 50, 150, 450, 625, 750, 140, 100, 100) },
            { 14, new PropertyData("Antalya", 12, 60, 180, 500, 700, 900, 160, 100, 100) },
            { 16, new PropertyData("Dresden", 14, 70, 200, 550, 750, 950, 180, 100, 100) },
            { 18, new PropertyData("Berlin", 14, 70, 200, 550, 750, 950, 180, 100, 100) },
            { 19, new PropertyData("Frankfurt", 16, 80, 220, 600, 800, 1000, 200, 100, 100) },
            { 21, new PropertyData("Odesa", 18, 90, 250, 700, 875, 1050, 220, 150, 150) },
            { 23, new PropertyData("Kyiv", 18, 90, 250, 700, 875, 1050, 220, 150, 150) },
            { 24, new PropertyData("Kharkiv", 20, 100, 300, 750, 925, 1100, 240, 150, 150) },
            { 26, new PropertyData("California", 22, 110, 330, 800, 975, 1150, 260, 150, 150) },
            { 27, new PropertyData("Washington", 22, 110, 330, 800, 975, 1150, 260, 150, 150) },
            { 29, new PropertyData("New York", 24, 120, 360, 850, 1025, 1200, 280, 150, 150) },
            { 31, new PropertyData("Perth", 26, 130, 390, 900, 1100, 1275, 300, 200, 200) },
            { 32, new PropertyData("Melbourne", 26, 130, 390, 900, 1100, 1275, 300, 200, 200) },
            { 34, new PropertyData("Sydney", 28, 150, 450, 1000, 1200, 1400, 320, 200, 200) },
            { 37, new PropertyData("Houten", 35, 175, 500, 1100, 1300, 1500, 350, 200, 200) },
            { 39, new PropertyData("Amsterdam", 50, 200, 600, 1400, 1700, 2000, 400, 200, 200) },
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
                    if (currentCardOpenInfo != null)
                    {
                        DeleteCardInfo();
                    }
                    int index = cardCountry.OnClicked();
                    if (index != 2 && index != 5 && index != 8 && index != 13 && index != 15 && index != 17
                        && index != 22 && index != 25 && index != 28 && index != 33 && index != 35 && index != 36 && index != 38) // Special cards
                    {
                        CreateCardInfo(index);
                    }
                }
            }
            else
            {
                DeleteCardInfo();
            }
        }
    }

    private void CreateCardInfo(int index)
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

        GameObject cardInfo = TypeOfCountryPref(index);
        currentCardOpenInfo = Instantiate(cardInfo, objectCanvas);

        RectTransform rectTransform = currentCardOpenInfo.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;

        TextMeshProUGUI[] textComponent = currentCardOpenInfo.GetComponentsInChildren<TextMeshProUGUI>();

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

    private void DeleteCardInfo()
    {
        Destroy(currentCardOpenInfo);
        currentCardOpenInfo = null;
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

    private GameObject TypeOfCountryPref(int index)//For gameObject 
    {
        switch(index)
        {
            case 1:
            case 3:
            case 4:
                    return cardPrefSlovakia;
            case 6:
            case 7:
            case 9:
                return cardPrefPoland;
            case 11:
            case 12:
            case 14:
                return cardPrefTurkey;
            case 16:
            case 18:
            case 19:
                return cardPrefGermany;
            case 21:
            case 23:
            case 24:
                return cardPrefUkraine;
            case 26:
            case 27:
            case 29:
                return cardPrefUsa;
            case 31:
            case 32:
            case 34:
                return cardPrefAustralia;
            case 37:
            case 39:
                return cardPrefNetherlands;
        }
        return null;
    }

    public Vector3 GetBoardPosition(int index) => boardCardPositions[index].transform.position;
    public int BoardCardCount() => boardCardPositions.Count;

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