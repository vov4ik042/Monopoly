using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyUI : MonoBehaviour
{
    [SerializeField] private Button buttonPublic;
    [SerializeField] private Button buttonPrivate;
    [SerializeField] private Button buttonExit;
    [SerializeField] private TMP_InputField lobbyNameInput;

    private void Awake()
    {
        buttonPublic.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX(1);
            MonopolyLobby.Instance.CreateLobby(lobbyNameInput.text, false);
        });
        buttonPrivate.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX(1);
            MonopolyLobby.Instance.CreateLobby(lobbyNameInput.text, true);
        });
        buttonExit.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX(1);
            CloseWindow();
        });
    }

    private void CloseWindow()
    {
        gameObject.SetActive(false);
    }
}
