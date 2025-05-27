using System;
using Unity.Collections;
using System.Linq;
using Unity.Netcode;

public struct TradeData : IEquatable<TradeData>, INetworkSerializable
{
    public int FirstClientId;
    public int SecondClientId;
    public int FirstPlayerMoney;
    public int SecondPlayerMoney;
    public FixedList128Bytes<int> FirstPlayerPropertyChoosed;
    public FixedList128Bytes<int> SecondPlayerPropertyChoosed;

    public bool Equals(TradeData other)
    {
        return
            FirstClientId == other.FirstClientId &&
            SecondClientId == other.SecondClientId &&
            FirstPlayerMoney == other.FirstPlayerMoney &&
            SecondPlayerMoney == other.SecondPlayerMoney &&
            FirstPlayerPropertyChoosed.SequenceEqual(other.FirstPlayerPropertyChoosed) &&
            SecondPlayerPropertyChoosed.SequenceEqual(other.SecondPlayerPropertyChoosed);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref FirstClientId);
        serializer.SerializeValue(ref SecondClientId);
        serializer.SerializeValue(ref FirstPlayerMoney);
        serializer.SerializeValue(ref SecondPlayerMoney);

        if (serializer.IsWriter)
        {
            int length = FirstPlayerPropertyChoosed.Length;
            serializer.SerializeValue(ref length);

            for (int i = 0; i < length; i++)
            {
                int value = FirstPlayerPropertyChoosed[i];
                serializer.SerializeValue(ref value);
            }
        }
        else
        {
            int length = 0;
            serializer.SerializeValue(ref length);
            FirstPlayerPropertyChoosed.Clear();

            for (int i = 0; i < length; i++)
            {
                int value = 0;
                serializer.SerializeValue(ref value);
                FirstPlayerPropertyChoosed.Add(value);
            }
        }
        if (serializer.IsWriter)
        {
            int length = SecondPlayerPropertyChoosed.Length;
            serializer.SerializeValue(ref length);

            for (int i = 0; i < length; i++)
            {
                int value = SecondPlayerPropertyChoosed[i];
                serializer.SerializeValue(ref value);
            }
        }
        else
        {
            int length = 0;
            serializer.SerializeValue(ref length);
            SecondPlayerPropertyChoosed.Clear();

            for (int i = 0; i < length; i++)
            {
                int value = 0;
                serializer.SerializeValue(ref value);
                SecondPlayerPropertyChoosed.Add(value);
            }
        }
    }
}
