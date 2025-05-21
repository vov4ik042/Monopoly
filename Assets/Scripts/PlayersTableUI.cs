using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayersTableUI : MonoBehaviour
{
    [SerializeField] private Button btnBunkrupt;
    [SerializeField] private Button btnTradeKick;
    [SerializeField] private Button btnLeaveGame;

    private void Start()
    {
        Bunkrupt.Instance.PlayerBunkrupt += PlayersTableUI_PlayerBunkrupt;

        btnBunkrupt.onClick.AddListener(() =>
        {
            Bunkrupt.Instance.Show();
        });
        btnTradeKick.onClick.AddListener(() =>
        {
            TradeWindow.Instance.gameObject.SetActive(true);
        });
        btnLeaveGame.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.PlayScene(Scenes.Menu);
        });
        btnLeaveGame.gameObject.SetActive(false);
    }

    private void PlayersTableUI_PlayerBunkrupt(object sender, System.EventArgs e)
    {
        btnBunkrupt.gameObject.SetActive(false);
        btnTradeKick.gameObject.SetActive(false);
        btnLeaveGame.gameObject.SetActive(true);
    }
}
