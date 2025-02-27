using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CardSpecial : MonoBehaviour
{
    [SerializeField] private int cardIndex;
    private string CardName, PriceName;

    public void InitializeCard(string cityName, string priceName)
    {
        CardName = cityName;
        PriceName = priceName;
    }

    public string GetCardName() => CardName;
    public string GetPriceName() => PriceName;
}
