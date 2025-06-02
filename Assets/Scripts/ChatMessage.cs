using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshPro;

    public void SetTextAndColor(string text, Color color)
    {
        textMeshPro.text = text;
        textMeshPro.color = color;
    }
}
