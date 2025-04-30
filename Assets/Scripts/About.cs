using UnityEngine;
using UnityEngine.UI;

public class About : MonoBehaviour
{
    [SerializeField] private Button btnExit;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseWindow();
        }
    }
    private void Awake()
    {
        btnExit.onClick.AddListener(CloseWindow);
    }
    public void OpenWindow()
    {
        gameObject.SetActive(true);
    }

    private void CloseWindow()
    {
        AudioManager.Instance.PlaySFX(1);
        gameObject.SetActive(false);
    }
}
