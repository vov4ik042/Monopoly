using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UpdatePropertyTableUI : MonoBehaviour
{
    [SerializeField] private Transform ButtonTemplate;
    [SerializeField] private Transform Container;
    private List<int> cardsList = new List<int>();

    private void Start()
    {
        ButtonTemplate.gameObject.SetActive(false);
        GameController.Instance.AddPropertyListLocalClient += AddPropertyTableUI_PropertyListLocalClientChanged;
        BoardController.Instance.DeletePropertyListLocalClient += DeletePropertyTableUI_DeletePropertyListLocalClient;
    }

    private void DeletePropertyTableUI_DeletePropertyListLocalClient(object sender, int e)
    {
        for (int i = 0; i < cardsList.Count; i++)
        {
            if (cardsList[i] == e)
            {
                cardsList.RemoveAt(i);
            }
        }

        foreach (Transform child in Container.transform)
        {
            if (child == ButtonTemplate) continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < cardsList.Count; i++)
        {
            string cityName = BoardController.Instance.GetCardCityName(cardsList[i]);
            int cityPrice = BoardController.Instance.GetCardCost(cardsList[i]);
            AddPropertyToTableUI(cityName, cityPrice, cardsList[i], NetworkManager.Singleton.LocalClientId);
        }
    }

    private void AddPropertyTableUI_PropertyListLocalClientChanged(object sender, int e)
    {
        cardsList.Add(e);
        string cityName = BoardController.Instance.GetCardCityName(e);
        int cityPrice = BoardController.Instance.GetCardCost(e);
        AddPropertyToTableUI(cityName, cityPrice, e, NetworkManager.Singleton.LocalClientId);
    }

    private void AddPropertyToTableUI(string name, int price, int id, ulong clientId)
    {
        RectTransform rect = Container.GetComponent<RectTransform>();
        Vector2 size = rect.sizeDelta;
        size.y += 36.0f;
        rect.sizeDelta = size;

        Transform newItem = Instantiate(ButtonTemplate, Container);
        newItem.gameObject.SetActive(true);
        newItem.GetComponent<UpdatePropertySingleUI>().UpdateInfo(name, price, id, clientId);
    }
}
