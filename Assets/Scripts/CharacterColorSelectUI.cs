using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterColorSelectUI : MonoBehaviour
{
    [SerializeField] private int colorId;
    [SerializeField] private Image image;
    [SerializeField] private GameObject selectedGameObject;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => { 
            MonopolyMultiplayer.Instance.ChangePlayerColor(colorId);
        });
    }

    private void Start()
    {
        MonopolyMultiplayer.Instance.OnPlayerDataNetworkListChanged += MonopolyMultiplayer_OnPlayerDataNetworkListChanged;
        image.color = MonopolyMultiplayer.Instance.GetPlayerColor(colorId);
        UpdateIsSelected();
    }

    private void MonopolyMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdateIsSelected();
    }

    private void UpdateIsSelected()
    {
        if (MonopolyMultiplayer.Instance.GetPlayerData().colorId == colorId)
        {
            selectedGameObject.SetActive(true);
        }
        else
        {
            selectedGameObject.SetActive(false);
        }
    }
    private void OnDestroy()
    {
        MonopolyMultiplayer.Instance.OnPlayerDataNetworkListChanged -= MonopolyMultiplayer_OnPlayerDataNetworkListChanged;
    }
}
