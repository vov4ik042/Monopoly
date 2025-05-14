using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdatePropertySingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI CityName;
    [SerializeField] private TextMeshProUGUI CardPrice;
    private int cardIndex;
    private ulong ownerId;

    private void Awake()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            BoardController.Instance.VerifyAndCreateCardInfoAndPanel(cardIndex, ownerId);
        });
    }

    public void UpdateInfo(string name, int price, int index, ulong id)
    {
        CityName.text = name;
        CardPrice.text = price.ToString() + "$";
        cardIndex = index;
        ownerId = id;
    }
}
