using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TablePlayersUI : MonoBehaviour
{
    [SerializeField] private Transform Container;
    [SerializeField] private Transform Template;
    public static TablePlayersUI Instance;

    private void Awake()
    {
        Instance = this;
        Template.gameObject.SetActive(false);
    }

    public void PutPlayersOnTableUI(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Debug.Log("i: " + i);
            Transform transformObject = Instantiate(Template, Container);
            transformObject.gameObject.SetActive(true);
            transformObject.gameObject.GetComponent<PlayersTableSingleUI>().SetPlayerInfo(i);
        }
    }
}
