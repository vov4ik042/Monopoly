using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource MusicSource;
    [SerializeField] private AudioSource SFXSource;
    [SerializeField] private AudioMixer audioMixer;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] listClips;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        MusicSource.clip = listClips[0];
        MusicSource.Play();

        if (PlayerPrefs.HasKey("MusicVolumeKey") && PlayerPrefs.HasKey("SFXVolumeKey"))
        {
            float music = PlayerPrefs.GetFloat("MusicVolumeKey");
            float sfx = PlayerPrefs.GetFloat("SFXVolumeKey");
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(music * 2) * 20);
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfx) * 20);
        }
    }

    public void PlaySFX(int clip)
    {
        SFXSource.PlayOneShot(listClips[clip]);
    }
}
