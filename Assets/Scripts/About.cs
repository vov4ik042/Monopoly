using UnityEngine;
using UnityEngine.UI;

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
