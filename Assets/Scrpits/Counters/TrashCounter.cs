using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TrashCounter : BaseCounter
{
    public static event EventHandler OnAnyObjectTrashed; 
    
    public new static void ResetStaticData()
    {
        OnAnyObjectTrashed = null;
    }
    
    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            KitchenObject.DestoryKitchenObject(player.GetKitchenObject()); // 调用清除静态函数，由服务器进行destory
            InteractLogicServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }

    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        OnAnyObjectTrashed?.Invoke(this , EventArgs.Empty);
    }
}
