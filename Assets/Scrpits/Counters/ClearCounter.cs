using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    //[SerializeField] private KitchenObjectSO kitchenObjectSO;

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
                
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    //携带的是盘子 , 把clearCounter上的物品放到盘子上
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectS0()))
                    {
                        GetKitchenObject().DestroySelf();     
                    }
                    
                }
                else
                {
                    // 携带的不是盘子
                    if (GetKitchenObject().TryGetPlate(out plateKitchenObject))
                    {
                        // 此时Clear Counter上有盘子，把玩家手上的物品放在盘子上
                        if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectS0()))
                        {
                            player.GetKitchenObject().DestroySelf();
                        }

                    }
                }
            }
            else
            {
                // 玩家没有携带任何物品 ，拿取clearcounter上的物体
                GetKitchenObject().SetKtichenObjectParent(player);

            }
        }

    }

 
}
