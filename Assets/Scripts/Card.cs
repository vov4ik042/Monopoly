using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private int cardIndex;
    [SerializeField] private GameObject cardColor;
    [SerializeField] private Text cardTextPrice;

    private string CityName;
    private int Rent, RentOneHouse, RentTwoHouses, RentThreeHouses, RentFourHouses, RentHotel, Price, PriceHouse, PriceHotel;//For countries card

    private string InfrastructureName;
    private int PriceInfrastructure = 200, OneInfrastructure = 25, TwoInfrastructure = 50, ThreeInfrastructure = 100,
        FourInfrastructure = 200, FiveInfrastructure = 400;//For infrastructure cards;
    public Player PLayerOwner { get; set; }

    public int OnClicked()
    {
        return cardIndex;
    }

    public Card(string cityName, int rent, int rentOne, int rentTwo, int rentThree, int rentFour, int rentHotel, int price, int priceHouse, int priceHotel)
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

    public Card(string infrastructureName)
    {
        InfrastructureName = infrastructureName;
        //int priceInfrastructure, int oneInfrastructure, int twoInfrastructure, int threeInfrastructure, int fourInfrastructure, int fiveInfrastructure
        /*PriceInfrastructure = priceInfrastructure;
        OneInfrastructure = oneInfrastructure;
        TwoInfrastructure = twoInfrastructure;
        ThreeInfrastructure = threeInfrastructure;
        FourInfrastructure = fourInfrastructure;
        FiveInfrastructure = fiveInfrastructure;*/
    }

    public void GetInfoForCardCountryInfoUpdate(TextMeshProUGUI[] textComponent)
    {
        textComponent[0].text = CityName;
        textComponent[1].text = Price + "$";
        textComponent[2].text = PriceHouse + "$";
        textComponent[3].text = PriceHotel + "$";
        textComponent[4].text = Rent + "$";
        textComponent[5].text = RentOneHouse + "$";
        textComponent[6].text = RentTwoHouses + "$";
        textComponent[7].text = RentThreeHouses + "$";
        textComponent[8].text = RentFourHouses + "$";
        textComponent[9].text = RentHotel + "$";
    }
    public void GetInfoForCardInfrastructureInfoUpdate(TextMeshProUGUI[] textComponent)
    {
        textComponent[0].text = InfrastructureName;
        textComponent[1].text = PriceInfrastructure + "$";
        textComponent[2].text = OneInfrastructure + "$";
        textComponent[3].text = TwoInfrastructure + "$";
        textComponent[4].text = ThreeInfrastructure + "$";
        textComponent[5].text = FourInfrastructure + "$";
        textComponent[6].text = FiveInfrastructure + "$";
    }

    public void ViewColorPLayerOnCard()
    {

    }

    public void HideCardPriceText()
    {
        cardTextPrice.gameObject.SetActive(false);
    }

    public int GetPriceCard(int index)
    {
        if (index == 0) return Price;
        return PriceInfrastructure;
    }
}
