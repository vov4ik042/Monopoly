using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class SettingsWindow : MonoBehaviour
{
    [SerializeField]private Toggle btnFullScreen;
    [SerializeField]private Button btnExit;
    [SerializeField]private Slider sliderMusic;
    [SerializeField]private Slider sliderSounds;
    [SerializeField]private AudioMixer audioMixer;
    [SerializeField]private TMP_Dropdown resolutionDropDown;

    Resolution[] resolutions;
    List<string> options = new List<string>();

    private void Awake()
    {
        //sliderMusic.onValueChanged.AddListener(SetValueMusic);
        //sliderSounds.onValueChanged.AddListener(SliderSoundsValueChanged);
        //btnFullScreen.onValueChanged.AddListener(ResizeWindow);
        btnExit.onClick.AddListener(CloseWindow);
    }

    private void Start()
    {
        resolutionDropDown.ClearOptions();
        resolutions = Screen.resolutions;

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            var option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropDown.AddOptions(options);
        resolutionDropDown.value = currentResolutionIndex;
        resolutionDropDown.RefreshShownValue();

    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            CloseWindow();
        }
    }

    public void OpenWindow(SettingsWindow settingsWindow)
    {
        settingsWindow.gameObject.SetActive(true);
    }

    private void CloseWindow()
    {
        this.gameObject.SetActive(false);
    }

    public void SetValueMusic()
    {
        audioMixer.SetFloat("Musicvolume", sliderMusic.value);
        Debug.Log(sliderMusic.value);
    }

    public void SetValueSFX()
    {
        audioMixer.SetFloat("SFXvolume", sliderSounds.value);
        Debug.Log(sliderSounds.value);
    }

    public void FullScreenSize(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
