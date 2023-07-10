using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct PlayerData: IEquatable<PlayerData> , INetworkSerializable
{
    public ulong clientID;

    public bool Equals(PlayerData other)
    {
        return clientID == other.clientID;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientID);
    }
}
