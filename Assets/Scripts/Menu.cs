using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] private SettingsWindow settingsWindow;
    [SerializeField] private About aboutWindow;
    //[SerializeField] private GameObject loadingScreen;
    //[SerializeField] private Slider slider;

    [SerializeField] private Button buttonStart;
    [SerializeField] private Button buttonSettings;
    [SerializeField] private Button buttonAbout;
    [SerializeField] private Button buttonExit;

    private void Awake()
    {
        MainMenuClearUp();
        buttonStart.onClick.AddListener(PLayClick);
        buttonSettings.onClick.AddListener(SettingsClick);
        buttonAbout.onClick.AddListener(AboutClick);
        buttonExit.onClick.AddListener(Exit);
    }
    private void MainMenuClearUp()//Когда выходят из лобби например
    {
        if (MonopolyLobby.Instance != null)
        {
            Destroy(MonopolyLobby.Instance.gameObject);
        }
    }
    public void PLayClick()
    {
        SceneManager.PlayScene(Scenes.Lobby);
    }

    public void AboutClick()
    {
        aboutWindow.OpenWindow();
    }

    public void SettingsClick()
    {
        settingsWindow.OpenWindow();
    }

    public void Exit()
    {
        Application.Quit(); 
    }
}
