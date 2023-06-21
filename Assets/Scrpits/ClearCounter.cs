using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            // 没有物品在clearcounter上
            if (player.HasKitchenObject())
            {
                // 玩家携带物品
                player.GetKitchenObject().SetKtichenObjectParent(this);
            }
            else
            {
                // 玩家没有携带物品
                
            }
        }
        else
        {
            // 有物品在clearcounter上
            if (player.HasKitchenObject())
            {
                // 玩家携带物品
            }
            else
            {
                // 玩家没有携带任何物品 ，拿取clearcounter上的物体
                GetKitchenObject().SetKtichenObjectParent(player);

            }
        }

    }

 
}
