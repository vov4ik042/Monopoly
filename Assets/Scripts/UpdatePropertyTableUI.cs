using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UpdatePropertyTableUI : MonoBehaviour
{
    [SerializeField] private Transform ButtonTemplate;
    [SerializeField] private Transform Container;

    private void Start()
    {
        ButtonTemplate.gameObject.SetActive(false);
        //GameController.Instance.AddPropertyFromPlayerList += UpdatePropertyTableUI_OnPlayerPropertyListChanged;
        //BoardController.Instance.RemovePropertyFromPlayerList += UpdatePropertyTableUI_OnPlayerPropertyListChanged;
        MonopolyMultiplayer.Instance.AddPropertyFromPlayerList += UpdatePropertyTableUI_OnPlayerPropertyListChanged;
        Bunkrupt.Instance.PlayerBunkrupt += Bunkrupt_PlayerBunkrupt;
        GameController.Instance.PlayerLeave += Bunkrupt_PlayerBunkrupt;
    }

    private void UpdatePropertyTableUI_OnPlayerPropertyListChanged(object sender, System.EventArgs e)
    {
        RectTransform rect = Container.GetComponent<RectTransform>();
        Vector2 size = rect.sizeDelta;
        size.y = 36.0f;
        rect.sizeDelta = size;

        foreach (Transform child in Container)
        {
            if (child == ButtonTemplate) continue;
            Destroy(child.gameObject);
        }
        var rawList = MonopolyMultiplayer.Instance.GetPlayerListProperty(NetworkManager.Singleton.LocalClientId);
        List<int> cardsList = new List<int>();
        //Debug.Log("cardsListCount: " + cardsList.Count);
        for (int i = 0; i < rawList.Length; i++)
        {
            cardsList.Add(rawList[i]);
        }

        for (int i = 0; i < cardsList.Count; i++)
        {
            string cityName = BoardController.Instance.GetCardCityName(cardsList[i]);
            int cityPrice = BoardController.Instance.GetCardCost(cardsList[i]);
            AddPropertyToTableUI(cityName, cityPrice, cardsList[i], NetworkManager.Singleton.LocalClientId);
        }
    }

    private void Bunkrupt_PlayerBunkrupt(object sender, System.EventArgs e)
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;
        var rawList = MonopolyMultiplayer.Instance.GetPlayerListProperty(NetworkManager.Singleton.LocalClientId);

        for (int i = 0; i < rawList.Length; i++)
        {
            BoardController.Instance.PlayerSellCardBunkruptServerRpc(rawList[i], localId);
            MonopolyMultiplayer.Instance.RemoveFromPlayerListPropertyServerRpc(localId, rawList[i]);
        }

        foreach (Transform child in Container)
        {
            if (child == ButtonTemplate) continue;
            Destroy(child.gameObject);
        }
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

    private void OnDestroy()
    {
        /*GameController.Instance.AddPropertyListLocalClient -= AddPropertyTableUI_PropertyListLocalClientChanged;
        BoardController.Instance.DeletePropertyListLocalClient -= DeletePropertyTableUI_DeletePropertyListLocalClient;
        Bunkrupt.Instance.PlayerBunkrupt -= Bunkrupt_PlayerBunkrupt;
        GameController.Instance.PlayerLeave -= Bunkrupt_PlayerBunkrupt;*/
    }
}
