using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] private SettingsWindow settingsWindow;
    [SerializeField] private About aboutWindow;
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

    private void Start()
    {
        if (PlayerPrefs.HasKey("MusicVolumeKey") && PlayerPrefs.HasKey("SFXVolumeKey"))
        {
            settingsWindow.LoadVolume();
        }
        else
        {
            settingsWindow.SetValueMusic();
            settingsWindow.SetValueSFX();
        }
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
        AudioManager.Instance.PlaySFX(1);
        SceneManager.PlayScene(Scenes.Lobby);
    }

    public void AboutClick()
    {
        AudioManager.Instance.PlaySFX(1);
        aboutWindow.OpenWindow();
    }

    public void SettingsClick()
    {
        AudioManager.Instance.PlaySFX(1);
        settingsWindow.OpenWindow();
    }

    public void Exit()
    {
        AudioManager.Instance.PlaySFX(1);
        Application.Quit(); 
    }
}
