using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSelectReady : NetworkBehaviour
{
    private Dictionary<ulong, bool> playerReadyDictionary;

    public static CharacterSelectReady Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }


    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientReady = true;
        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {   
            if(!playerReadyDictionary.ContainsKey(clientID) || !playerReadyDictionary[clientID])  
            {
                allClientReady = false;
                break;
            }
        }

        if (allClientReady)
        {
            Loader.LoadNetwork(Loader.Scene.GameScene);     
        }
    }
}
