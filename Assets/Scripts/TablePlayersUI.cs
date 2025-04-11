using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class TablePlayersUI : MonoBehaviour
{
    [SerializeField] private Transform Container;
    [SerializeField] private Transform Template;

    public static TablePlayersUI Instance;
    private List<Transform> TemplatesList;

    private void Awake()
    {
        Instance = this;
        TemplatesList = new List<Transform>();
        MonopolyMultiplayer.Instance.OnPlayerDataNetworkListChanged += MonopolyMultiplayer_OnPlayerDataNetworkListChanged;
        Template.gameObject.SetActive(false);
    }

    private void MonopolyMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdateInfo();
    }

    private void UpdateInfo()
    {
        for (int i = 0; i < TemplatesList.Count; i++)
        {
            TemplatesList[i].gameObject.GetComponent<PlayersTableSingleUI>().UpdatePlayerInfo(i);
        }
    }
    public void PutPlayersOnTableUI(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Transform transformObject = Instantiate(Template, Container);
            transformObject.gameObject.SetActive(true);
            transformObject.gameObject.GetComponent<PlayersTableSingleUI>().UpdatePlayerInfo(i);
            TemplatesList.Add(transformObject);
        }
    }
    private void OnDestroy()
    {
        if (MonopolyMultiplayer.Instance != null)
        {
            MonopolyMultiplayer.Instance.OnPlayerDataNetworkListChanged -= MonopolyMultiplayer_OnPlayerDataNetworkListChanged;
        }
    }
}
