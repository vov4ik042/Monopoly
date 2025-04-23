using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Card : NetworkBehaviour
{
    [SerializeField] private int cardIndex;
    [SerializeField] private TextMeshProUGUI cardTextPrice;
    [SerializeField] private MeshRenderer colorOwnerField;

    private NetworkVariable<ulong> clientOwnerId = new NetworkVariable<ulong>(0);
    //private NetworkVariable<int> PhaseRentCountry = new NetworkVariable<int>(0);
    private int PhaseRentCountry = 0;
    private NetworkVariable<bool> CardCanUpgrade = new NetworkVariable<bool>(false);

    private Player PlayerOwner { get; set; } = null;

    private string CityName, CountryName;
    private int Rent, RentOneHouse, RentTwoHouses, RentThreeHouses, RentFourHouses, RentHotel, Price, PriceHouse, PriceHotel;//For countries card

    private string InfrastructureName;
    private int PriceInfrastructure, OneInfrastructure, TwoInfrastructure, ThreeInfrastructure, FourInfrastructure, FiveInfrastructure;//For infrastructure cards;

    public UnityEngine.Color IntToColor(int value)
    {
        byte a = (byte)((value >> 24) & 0xFF);
        byte r = (byte)((value >> 16) & 0xFF);
        byte g = (byte)((value >> 8) & 0xFF);
        byte b = (byte)(value & 0xFF);
        return new Color32(r, g, b, a);
    }
    public int ColorToInt(UnityEngine.Color color)
    {
        Color32 c32 = color;
        int value = (c32.a << 24) | (c32.r << 16) | (c32.g << 8) | c32.b;
        return value;
    }

    public void SetClientOwnerId(ulong clientOwnerId)
    {
        this.clientOwnerId.Value = clientOwnerId;
    }
    public ulong GetClientOwnerId()
    {
        return clientOwnerId.Value;
    }

    public void SetOwnerColorField(UnityEngine.Color color)
    {
        colorOwnerField.material.color = color;
        int colorInt = ColorToInt(color);
        SetOwnerColorFieldClientRpc(colorInt);
    }
    [ClientRpc]
    public void SetOwnerColorFieldClientRpc(int intColor)
    {
        UnityEngine.Color color = IntToColor(intColor);
        colorOwnerField.material.color = color;
    }

    public int GetCardIndex() => cardIndex;
    public Player GetPlayerOwner() => PlayerOwner;
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
    public void SetPhaseRentCountry(bool res)
    {
        if (res)
        {
            PhaseRentCountry++;
        }
        else
        {
            PhaseRentCountry--;
        }
        SetPhaseRentCountryClientRpc(PhaseRentCountry);
    }
    [ClientRpc]
    public void SetPhaseRentCountryClientRpc(int res)
    {
        PhaseRentCountry = res;
    }
    public string GetCityName() => CityName;
    public string GetInfrastructureName() => InfrastructureName;
    public int GetPriceHouse() => PriceHouse;
    public int GetPriceHotel() => PriceHotel;

    public void SetPlayerOwner(Player player, ServerRpcParams rpcParams = default)
    {
        PlayerOwner = player;

        if (player != null)
        {
            if (player.TryGetComponent(out NetworkObject networkObject))
            {
                var playerRef = new NetworkObjectReference(networkObject);
                SetPlayerOwnerClientRpc(playerRef);
            }
            //UnityEngine.Debug.Log("PlayerOwner ServerRpc: " + player.GetPlayerId());
        }
        else
        {
            SetPlayerOwnerNullClientRpc();
        }
    }
    [ClientRpc]
    public void SetPlayerOwnerClientRpc(NetworkObjectReference playerRef)
    {
        if (playerRef.TryGet(out NetworkObject playerNetworkObject))
        {
            Player playerComponent = playerNetworkObject.GetComponent<Player>();
            PlayerOwner = playerComponent;

            //UnityEngine.Debug.Log("PlayerOwner ClientRpc: " + PlayerOwner.GetPlayerId());
        }
    }
    [ClientRpc]
    public void SetPlayerOwnerNullClientRpc()
    {
        PlayerOwner = null;
    }
    public void ShowHideCardPriceText(bool res)
    {
        cardTextPrice.gameObject.SetActive(res);
        ShowHideCardPriceTextClientRpc(res);
    }
    [ClientRpc]
    public void ShowHideCardPriceTextClientRpc(bool res)
    {
        cardTextPrice.gameObject.SetActive(res);
    }
    public void SetCardUpgradeOrNot(bool res)
    {
        CardCanUpgrade.Value = res;
    }
    public bool GetCanCardUpgradeOrNot() => CardCanUpgrade.Value;

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
        PriceInfrastructure = priceInfrastructure;
        OneInfrastructure = oneInfrastructure;
        TwoInfrastructure = twoInfrastructure;
        ThreeInfrastructure = threeInfrastructure;
        FourInfrastructure = fourInfrastructure;
        FiveInfrastructure = fiveInfrastructure;
    }

}
