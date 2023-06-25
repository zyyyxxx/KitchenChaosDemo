using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using System;

public class CuttingCounter : BaseCounter , IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressBarChangedEventArgs> OnProgressBarChanged;

    // 用于触发动画
    public event EventHandler OnCut;
    
    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    private int cuttingProgress;
    
        
    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            // 没有物品在clearcounter上
            if (player.HasKitchenObject())
            {
                // 玩家携带物品
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectS0()))
                {
                    // 可以切 再放下
                    player.GetKitchenObject().SetKtichenObjectParent(this);
                    cuttingProgress = 0;
                    // 改变UI
                    CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectS0());
                    OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
                    {
                        progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttringProgressMax
                    });
                }
                
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

    public override void InteractAlternate(Player player)
    {
        if(HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectS0()))
        {
            cuttingProgress++;
            
            OnCut?.Invoke(this , EventArgs.Empty);
            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectS0());
            // 修改UI
            OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
            {
                progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttringProgressMax
            });
            
            if (cuttingProgress >= cuttingRecipeSO.cuttringProgressMax)
            {
                // 有物品 且是可以切的物品 , 销毁物品 
                KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectS0());
            
                GetKitchenObject().DestroySelf();
            
                Debug.Log("Cutting");
            
                // 生成切过的物品
                KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
            }
            
            

        }
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        return cuttingRecipeSO != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        return cuttingRecipeSO != null ? cuttingRecipeSO.output : null;
    }

    private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipeSOArray)
        {
            if (cuttingRecipeSO.input == inputKitchenObjectSO)
            {
                return cuttingRecipeSO;
            }
        }

        return null;
    }
}
