using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;
    public int colorId;
    public FixedString64Bytes playerName;
    public FixedString64Bytes playerId;
    public int playerMoney;
    public int PhaseRentInfrastructure;
    public bool Bankrupt;

    public bool Equals(PlayerData other)
    {
        return
            clientId == other.clientId &&
            colorId == other.colorId &&
            playerId == other.playerId &&
            PhaseRentInfrastructure == other.PhaseRentInfrastructure &&
            playerMoney == other.playerMoney &&
            Bankrupt == other.Bankrupt &&
            playerName == other.playerName;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref colorId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref playerMoney);
        serializer.SerializeValue(ref PhaseRentInfrastructure);
        serializer.SerializeValue(ref Bankrupt);
    }
}
