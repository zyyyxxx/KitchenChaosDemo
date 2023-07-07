using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using Unity.Netcode;

public class CuttingCounter : BaseCounter , IHasProgress
{
    public static event EventHandler OnAnyCut; //用于所有cutting 静态事件可以通知全局，因为它们只与类本身相关，而不是与类的实例相关。这意味着您可以在不实例化类或对象的情况下触发静态事件，并且所有订阅了该事件的对象都会接收到通知。

    public new static void ResetStaticData()
    {
        OnAnyCut = null;
    }
    
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
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    player.GetKitchenObject().SetKtichenObjectParent(this);
                    
                    InteractLogicPlateObjectOnCounterClientRpc();
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
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    //携带的是盘子 , 把clearCounter上的物品放到盘子上
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectS0()))
                    {
                        KitchenObject.DestoryKitchenObject(GetKitchenObject());
                        //GetKitchenObject().DestroySelf();
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
    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlateObjectOnCounterServerRpc()
    {
        InteractLogicPlateObjectOnCounterClientRpc();
    }
    
    [ClientRpc]
    private void InteractLogicPlateObjectOnCounterClientRpc()
    {
        cuttingProgress = 0;
        // 改变UI
          
        //此处我们进需要重置，不需要再引用cuttingRecipeSO
        // 多人游戏时，GetKitchenObject()可能还未被设置（等待RPC设置父类），object的parent还未被设置为cuttingcounter
        //CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectS0());
        //CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(kitchenObject.GetKitchenObjectS0());
                    
        OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
        {
            progressNormalized = 0f
        });
    }

    public override void InteractAlternate(Player player)
    {
        if(HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectS0()))
        {
            CutObjectServerRpc();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc()
    {
        CutObjectClientRpc();
        TestCuttingProgressDoneServerRpc();
    }

    [ClientRpc]
    private void CutObjectClientRpc()
    {
        cuttingProgress++;
            
        OnCut?.Invoke(this , EventArgs.Empty);
        OnAnyCut?.Invoke(this ,EventArgs.Empty);
            
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectS0());
        // 修改UI
        OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
        {
            progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttringProgressMax
        });
            
        

    }

    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressDoneServerRpc()
    {
        // 若放在CutObjectClientRpc的原有流程中， 每个客户端都会运行这段代码 会导致在调用服务端进行destory时产生冲突
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectS0());
        if (cuttingProgress >= cuttingRecipeSO.cuttringProgressMax)
        {
            // 有物品 且是可以切的物品 , 销毁物品 
            KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectS0());
            
            //GetKitchenObject().DestroySelf();
            KitchenObject.DestoryKitchenObject(GetKitchenObject());
            
            Debug.Log("Cutting");
            
            // 生成切过的物品
            KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
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
