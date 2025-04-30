using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [SerializeField] private Button buttonMainMenu;

    public static GameOver Instance;
    private void Awake()
    {
        Instance = this;
        buttonMainMenu.onClick.AddListener(() =>
        {
            SceneManager.PlayScene(Scenes.Menu);
        });
        Hide();
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
