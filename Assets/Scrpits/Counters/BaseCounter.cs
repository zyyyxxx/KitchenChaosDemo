using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseCounter : NetworkBehaviour , IKitchenObjectParent
{
    public static event EventHandler OnAnyObjectPlacedHere;//静态事件可以通知全局，因为它们只与类本身相关，
                                                           //而不是与类的实例相关。这意味着您可以在不实例化类或对象的情况下触发静态事件，
                                                           //并且所有订阅了该事件的对象都会接收到通知。

    public static void ResetStaticData()
    {
        OnAnyObjectPlacedHere = null;
    }
                                                           
    [SerializeField] private Transform counterTopPoint;

    private KitchenObject kitchenObject;
    public virtual void Interact(Player player)
    {
        Debug.Log("BaseCounter.Interact();");
    }
    
    public virtual void InteractAlternate(Player player)
    {
        Debug.Log("BaseCounter.InteractAlternate();");
    }
    
    // 接口工具函数
    public Transform GetKitchenObjectFollowTransform()
    {
        return counterTopPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null)
        {
            OnAnyObjectPlacedHere?.Invoke(this , EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
