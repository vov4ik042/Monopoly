using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardCountry : MonoBehaviour
{
    [SerializeField] private int cardIndex;
    public int cardPrice { get; private set; }
    public byte cardIdCountry { get; private set; }

    public int OnClicked()
    {
        Debug.Log("Нажата клетка: " + cardIndex);
        return cardIndex;
    }
}
