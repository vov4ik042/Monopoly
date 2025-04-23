using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bunkrupt : MonoBehaviour
{
    [SerializeField] private Button buttonYes; 
    [SerializeField] private Button buttonNo;

    public static Bunkrupt Instance;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        buttonYes.onClick.AddListener(() =>
        {
            //GameOver.Instance.Show();
            Hide();
        });
        buttonNo.onClick.AddListener(() =>
        {
            Hide();
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
