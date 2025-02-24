using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private int cardIndex;
    [SerializeField] private Text cardTextPrice;
    private bool CardCanUpgrade { get; set; } = false;
    private int PhaseRentCountry { get; set; } = 0;

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
        if (index != 8 && index != 13 && index != 28 && index != 33 && index != 36)
        {
            return Price;
        }

        return PriceInfrastructure;
    }
    public int GetPhaseRentCountry() => PhaseRentCountry;
    public void PlayerUpgradeCity()
    {
        PhaseRentCountry++;

        if (PhaseRentCountry == 1)
        {
            BoardController.Instance.TurnOffButtonsUpgradeDemote(true, true);
            BoardController.Instance.TurnOffButtonSellCard(false);
        }
        if (PhaseRentCountry == 5)
        {
            BoardController.Instance.TurnOffButtonsUpgradeDemote(false,true);
        }
    }
    public void PlayerDemoteCity()
    {
        PhaseRentCountry--;

        if (PhaseRentCountry == 0)
        {
            BoardController.Instance.TurnOffButtonsUpgradeDemote(true, false);
            BoardController.Instance.TurnOffButtonSellCard(true);
        }
        if (PhaseRentCountry == 4)
        {
            BoardController.Instance.TurnOffButtonsUpgradeDemote(true, true);
        }
    }
    public string GetCityName() => CityName;
    public int GetPriceHouse() => PriceHouse;
    public int GetPriceHotel() => PriceHotel;

    public void SetPlayerOwner(Player player)
    {
        PLayerOwner = player;
    }
    public void ShowHideCardPriceText(bool res)
    {
        cardTextPrice.gameObject.SetActive(res);
    }
    public void SetCardUpgradeOrNot(bool res)
    {
        CardCanUpgrade = res;
    }
    public bool GetCanCardUpgradeOrNot() => CardCanUpgrade;

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
    public int HowManyRentToPayForInfrastructureCard(Player player)
    {
        int phase = player.GetPhaseRentInfrastructure();
        switch (phase)
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
