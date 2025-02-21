using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private int cardIndex;
    [SerializeField] private Text cardTextPrice;
    private int PhaseRentCountry { get; set; } = 0;
    private int PhaseRentInfrastructure { get; set; } = 0;

    private string CityName, CountryName;
    private int Rent, RentOneHouse, RentTwoHouses, RentThreeHouses, RentFourHouses, RentHotel, Price, PriceHouse, PriceHotel;//For countries card

    private string InfrastructureName;
    private int PriceInfrastructure, OneInfrastructure, TwoInfrastructure, ThreeInfrastructure, FourInfrastructure, FiveInfrastructure;//For infrastructure cards;
    private Player PLayerOwner { get; set; }

    public int GetCardIndex() => cardIndex;
    public Player GetPLayerOwner() => PLayerOwner;
    public string GetCountryName() => CountryName;
    public int GetPriceCard(int index)
    {
        if (index == 0) return Price;
        return PriceInfrastructure;
    }

    public void SetPlayerOwner(Player player)
    {
        PLayerOwner = player;
    }
    public void HideCardPriceText()
    {
        cardTextPrice.gameObject.SetActive(false);
    }
    public void PlayerBuyInfrastructure()
    {
        PhaseRentInfrastructure++;
    }

    /*public Card(string cityName, int rent, int rentOne, int rentTwo, int rentThree, int rentFour, int rentHotel, int price, int priceHouse, int priceHotel)
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

    public Card(string infrastructureName, int priceInfrastructure, int oneInfrastructure, int twoInfrastructure, int threeInfrastructure, int fourInfrastructure, int fiveInfrastructure)
    {
        InfrastructureName = infrastructureName;
        //int priceInfrastructure, int oneInfrastructure, int twoInfrastructure, int threeInfrastructure, int fourInfrastructure, int fiveInfrastructure
        PriceInfrastructure = priceInfrastructure;
        OneInfrastructure = oneInfrastructure;
        TwoInfrastructure = twoInfrastructure;
        ThreeInfrastructure = threeInfrastructure;
        FourInfrastructure = fourInfrastructure;
        FiveInfrastructure = fiveInfrastructure;
    }
*/
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


    public int HowManyRentToPayForCountryCard()
    {
        switch (PhaseRentCountry)
        {
            case 0://rent
                {
                    return Rent;
                }
            case 1://rent for one house
                {
                    return RentOneHouse;
                }
            case 2://rent for two houses
                {
                    return RentTwoHouses;
                }
            case 3://rent for three houses
                {
                    return RentThreeHouses;
                }
            case 4://rent for four houses
                {
                    return RentFourHouses;
                }
            case 5://rent for a hotel
                {
                    return RentHotel;
                }
        }
        return 0;
    }
    public int HowManyRentToPayForInfrastructureCard()
    {
        switch (PhaseRentInfrastructure)
        {
            case 1://rent for one Infrastructure
                {
                    return OneInfrastructure;
                }
            case 2://rent for two Infrastructures
                {
                    return TwoInfrastructure;
                }
            case 3://rent for three Infrastructures
                {
                    return ThreeInfrastructure;
                }
            case 4://rent for four Infrastructures
                {
                    return FourInfrastructure;
                }
            case 5://rent for five Infrastructures
                {
                    return FiveInfrastructure;
                }
        }
        return 0;
    }

    public void InitializeCardCountry(string cityName, string countryName, int rent, int rentOne, int rentTwo, int rentThree, int rentFour, int rentHotel,
        int price, int priceHouse, int priceHotel)
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
        CountryName = countryName;
    }
    public void InitializeCardInfrastructure(string infrastructureName, int priceInfrastructure, int oneInfrastructure, int twoInfrastructure,
        int threeInfrastructure, int fourInfrastructure, int fiveInfrastructure)
    {
        InfrastructureName = infrastructureName;
        //int priceInfrastructure, int oneInfrastructure, int twoInfrastructure, int threeInfrastructure, int fourInfrastructure, int fiveInfrastructure
        PriceInfrastructure = priceInfrastructure;
        OneInfrastructure = oneInfrastructure;
        TwoInfrastructure = twoInfrastructure;
        ThreeInfrastructure = threeInfrastructure;
        FourInfrastructure = fourInfrastructure;
        FiveInfrastructure = fiveInfrastructure;
    }

}
