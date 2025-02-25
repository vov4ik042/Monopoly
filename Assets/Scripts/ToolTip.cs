using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject toolTipPrefab;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Enter");
        toolTipPrefab.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Exit");
        toolTipPrefab.gameObject.SetActive(false);
    }
}
