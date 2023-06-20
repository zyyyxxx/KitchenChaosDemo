using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : MonoBehaviour , IKitchenObjectParent
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    [SerializeField] private Transform counterTopPoint;

    private KitchenObject kitchenObject;
    public void Interact(Player player)
    {
        if (kitchenObject == null)
        {
            Debug.Log("Interact!");
            Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab , counterTopPoint);//生成物体
            kitchenObjectTransform.GetComponent<KitchenObject>().SetKtichenObjectParent(this);//生成的物体的counter引用
            kitchenObjectTransform.localPosition = Vector3.zero;
            
            kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
            kitchenObject.SetKtichenObjectParent(this);
        }
        else
        {
            // Give the Object to the player
            kitchenObject.SetKtichenObjectParent(player);
        }
        
    }

    // 接口工具函数
    public Transform GetKitchenObjectFollowTransform()
    {
        return counterTopPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
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
}
