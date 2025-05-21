using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using System.Linq;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;
    public int colorId;
    public FixedString64Bytes playerName;
    public FixedString64Bytes playerId;
    public int playerMoney;
    public bool playerBankrupt;
    public FixedList128Bytes<int> playerPropertyList;

    public bool Equals(PlayerData other)
    {
        return
            clientId == other.clientId &&
            colorId == other.colorId &&
            playerId == other.playerId &&
            playerMoney == other.playerMoney &&
            playerBankrupt == other.playerBankrupt &&
            playerPropertyList.SequenceEqual(other.playerPropertyList) &&
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

        if (serializer.IsWriter)
        {
            int length = playerPropertyList.Length;
            serializer.SerializeValue(ref length);

            for (int i = 0; i < length; i++)
            {
                int value = playerPropertyList[i];
                serializer.SerializeValue(ref value);
            }
        }
        else
        {
            int length = 0;
            serializer.SerializeValue(ref length);
            playerPropertyList.Clear();

            for (int i = 0; i < length; i++)
            {
                int value = 0;
                serializer.SerializeValue(ref value);
                playerPropertyList.Add(value);
            }
        }
    }
}
