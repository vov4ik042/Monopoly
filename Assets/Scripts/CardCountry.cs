using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CardCountry : MonoBehaviour
{
    [SerializeField] private int cardIndex;

    private string CityName { get; set; }
    private int Price { get; set; }
    private int PriceHouse { get; set; }
    private int PriceHotel { get; set; }
    private int Rent { get; set; }
    private int RentOneHouse { get; set; }
    private int RentTwoHouses { get; set; }
    private int RentThreeHouses { get; set; }
    private int RentFourHouses { get; set; }
    private int RentHotel { get; set; }
    public int CardPhase { get; set; }
    public int OnClicked()
    {
        return cardIndex;
    }

    public void AddInformationCard(string cityName, int rent, int rentOne, int rentTwo, int rentThree,
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
