using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;
    public int colorId;
    public FixedString64Bytes playerName;
    public FixedString64Bytes playerId;
    public int playerMoney;
    public bool playerBankrupt;


    public bool Equals(PlayerData other)
    {
        return
            clientId == other.clientId &&
            colorId == other.colorId &&
            playerId == other.playerId &&
            playerMoney == other.playerMoney &&
            playerBankrupt == other.playerBankrupt &&
            playerName == other.playerName;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref colorId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref playerMoney);
        serializer.SerializeValue(ref playerBankrupt);
    }
}
