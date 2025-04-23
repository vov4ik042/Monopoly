using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameOver : MonoBehaviour
{
    [SerializeField] private Button buttonMainMenu;

    public static GameOver Instance;
    private void Awake()
    {
        Instance = this;
        buttonMainMenu.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);
            SceneManager.PlayScene(Scenes.Menu);
        });
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
