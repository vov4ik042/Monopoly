using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class SettingsWindow : MonoBehaviour
{
    [SerializeField] private Toggle ToggleFullScreen;
    [SerializeField] private Button btnExit;
    [SerializeField] private Slider sliderMusic;
    [SerializeField] private Slider sliderSounds;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private TMP_Dropdown resolutionDropDown;

    private Resolution[] AllResolutions;
    private List<Resolution> SelectedResolutionList = new List<Resolution>();
    private int SelectedResolution;
    private int LastSelectedResolutionHeight;
    private int LastSelectedResolutionWidth;

    private void Awake()
    {
        btnExit.onClick.AddListener(CloseWindow);
    }

    private void Start()
    {
        InitializeListResolutions();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseWindow();
        }
    }

    public void OpenWindow()
    {
        gameObject.SetActive(true);
    }

    private void CloseWindow()
    {
        AudioManager.Instance.PlaySFX(1);
        gameObject.SetActive(false);
    }

    public void SetValueMusic()
    {
        float volume = sliderMusic.value;
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume * 2) * 20);
        PlayerPrefs.SetFloat("MusicVolumeKey", volume);
    }
    public void SetValueSFX()
    {
        float volume = sliderSounds.value;
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolumeKey", volume);
    }

    public void LoadVolume()
    {
        sliderMusic.value = PlayerPrefs.GetFloat("MusicVolumeKey");
        sliderSounds.value = PlayerPrefs.GetFloat("SFXVolumeKey");
        SetValueMusic();
        SetValueSFX();
    }

    public void InitializeListResolutions()
    {
        List<string> options = new List<string>();
        string result;

        AllResolutions = Screen.resolutions;

        foreach (Resolution resolution in AllResolutions)
        {
            result = resolution.height.ToString() + " x " + resolution.width.ToString();
            if (!options.Contains(result))
            {
                options.Add(result);
                SelectedResolutionList.Add(resolution);
            }
        }

        resolutionDropDown.AddOptions(options);
    }

    public void ChangeResolution()
    {
        AudioManager.Instance.PlaySFX(1);
        SelectedResolution = resolutionDropDown.value;
        Screen.SetResolution(SelectedResolutionList[SelectedResolution].width, SelectedResolutionList[SelectedResolution].height, false);
    }
    public void ChangeFullScreenMode()
    {
        AudioManager.Instance.PlaySFX(1);
        bool IsFullScreen = ToggleFullScreen.isOn;

        if (IsFullScreen)
        {
            RememberLastResolutions();
            Screen.SetResolution(SelectedResolutionList[SelectedResolutionList.Count - 1].width,
                SelectedResolutionList[SelectedResolutionList.Count - 1].height, IsFullScreen);
        }
        else
        {
            Screen.SetResolution(LastSelectedResolutionWidth, LastSelectedResolutionHeight, IsFullScreen);
        }
    }

    private void RememberLastResolutions()
    {
        LastSelectedResolutionWidth = Screen.width;
        LastSelectedResolutionHeight = Screen.height;
    }
}
