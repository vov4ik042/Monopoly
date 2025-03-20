using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;

public class About : MonoBehaviour
{
    [SerializeField] private Button btnExit;
    private void Awake()
    {
        btnExit.onClick.AddListener(CloseWindow);
    }
    public void OpenWindow()
    {
        this.gameObject.SetActive(true);
    }

    private void CloseWindow()
    {
        this.gameObject.SetActive(false);
    }
}
