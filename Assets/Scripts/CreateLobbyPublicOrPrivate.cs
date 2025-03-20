using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyPublicOrPrivate : MonoBehaviour
{
    [SerializeField] private Button buttonExit;
    [SerializeField] private Button buttonMainMenu;
    [SerializeField] private Button buttonPublic;
    [SerializeField] private Button buttonPrivate;

    private void Awake()
    {
        buttonExit.onClick.AddListener(CloseWindow);
        buttonMainMenu.onClick.AddListener(MainMenuExit);
    }
    public void ButtonClickStartGame()
    {
        SceneManager.PlayScene(Scenes.GameBoard);
    }

    private void CloseWindow()
    {
        this.gameObject.SetActive(false);
    }
    private void MainMenuExit()
    {
        SceneManager.PlayScene(Scenes.Menu);
    }
}
