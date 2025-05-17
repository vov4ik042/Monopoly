using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultWindow : MonoBehaviour
{
    [SerializeField] private Button buttonMainMenu;
    [SerializeField] private Button buttonStayInGame;
    [SerializeField] private TextMeshProUGUI playerName;

    public static ResultWindow Instance;
    private void Awake()
    {
        Instance = this;
        buttonMainMenu.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.PlayScene(Scenes.Menu);
        });
        buttonStayInGame.onClick.AddListener(() =>
        {
            if (GameController.Instance != null)
            {
                Destroy(GameController.Instance.gameObject);
            }
            if (BoardController.Instance != null)
            {
                Destroy(BoardController.Instance.gameObject);
            }
            if (DiceController.Instance != null)
            {
                Destroy(DiceController.Instance.gameObject);
            }
            if (TablePlayersUI.Instance != null)
            {
                Destroy(TablePlayersUI.Instance.gameObject);
            }
            SceneManager.PlaySceneNetwork(Scenes.CharacterSelect);
        });
        Hide();
    }
    public void Show(string playerName)
    {
        if (DiceController.Instance != null)
        {
            DiceController.Instance.DeleteCubesServerRpc();
        }
        this.playerName.text = playerName;
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
