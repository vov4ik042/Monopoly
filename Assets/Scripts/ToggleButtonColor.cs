using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButtonColor : MonoBehaviour
{
    [SerializeField] private Color activeColor;
    [SerializeField] private Color originalColor;
    private Button button;
    //private Color originalColor;
    private bool isActive = false;

    private void Awake()
    {
        button = GetComponent<Button>();
    }
    private void OnEnable()
    {
        originalColor = button.image.color;

        button.onClick.AddListener(() =>
        {
            ToggleColor();
        });
    }

    private void ToggleColor()
    {
        Debug.Log("ColorChange");
        isActive = !isActive;
        button.image.color = isActive ? activeColor : originalColor;
    }

    public void SetColor(Color color)
    {
        Debug.Log("Color:" + color);
        activeColor = color;
    }
}
