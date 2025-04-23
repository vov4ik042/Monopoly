using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class PlayerVisualGameBoard : NetworkBehaviour
{
    [SerializeField] private MeshRenderer MeshRenderer1;
    [SerializeField] private MeshRenderer MeshRenderer2;

    private Material material;

    private void Awake()
    {
        material = new Material(MeshRenderer1.material);
        MeshRenderer1.material = material;
        MeshRenderer2.material = material;
    }

    public void SetPlayerColor(Color color, int playerId)
    {
        material.color = color;
        SetPlayerColorClientRpc(playerId);
    }
    [ClientRpc]
    public void SetPlayerColorClientRpc(int playerId)
    {
        Color color = MonopolyMultiplayer.Instance.GetPlayerColorFromPlayerId(playerId);
        material.color = color;
    }
}
