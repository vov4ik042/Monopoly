using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource MusicSource;
    [SerializeField] private AudioSource SFXSource;

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
    }

    public void PlaySFX(int clip)
    {
        SFXSource.PlayOneShot(listClips[clip]);
    }
}
