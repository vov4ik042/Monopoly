using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
            GameController.Instance.DeleteInstanceServerRpc();
            BoardController.Instance.DeleteInstanceServerRpc();
            MonopolyLobby.Instance.DeleteInstanceServerRpc();
            MonopolyMultiplayer.Instance.DeleteInstanceServerRpc();
            SceneManager.PlayScene(Scenes.Menu);
        });
        buttonStayInGame.onClick.AddListener(() =>
        {
            Hide();
        });
        Hide();
    }
    public void Show(string playerName)
    {
        DiceController.Instance.DeleteCubesServerRpc();
        this.playerName.text = playerName;
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
