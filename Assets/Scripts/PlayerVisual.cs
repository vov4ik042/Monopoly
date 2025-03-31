using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer MeshRendererLod0;
    [SerializeField] private MeshRenderer MeshRendererLod1;

    private Material material;

    private void Awake()
    {
        material = new Material(MeshRendererLod0.material);
        MeshRendererLod0.material = material;
        MeshRendererLod1.material = material;
    }

    public void SetPlayerColor(Color color)
    {
        material.color = color;
    }
}
