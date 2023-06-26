using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenObject : MonoBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectS0;

    private IKitchenObjectParent kitchenObjectParent; // 拥有此物品的父亲

    public KitchenObjectSO GetKitchenObjectS0()
    {
        return kitchenObjectS0;
    }

    public void SetKtichenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        if (this.kitchenObjectParent != null)
        {
            this.kitchenObjectParent.ClearKitchenObject();  
        }

        this.kitchenObjectParent = kitchenObjectParent;

        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.LogError("IKitchenObjectParent already has a KitchenObject!");
        }
        
        kitchenObjectParent.SetKitchenObject(this); // 父亲拥有此物品
        
        this.transform.parent = kitchenObjectParent.GetKitchenObjectFollowTransform();
        this.transform.localPosition = Vector3.zero;
    }

    public IKitchenObjectParent GetKitchenObjectParent()
    {
        return kitchenObjectParent;
    }

    public void DestroySelf()
    {   
        kitchenObjectParent.ClearKitchenObject();
        
        Destroy(gameObject);
    }

    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if (this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        else
        {
            plateKitchenObject = null;
            return false;
        }
    }
    
    public static KitchenObject SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        //生成物体并设置parent
        
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);//生成物体
       
        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
        
        kitchenObject.SetKtichenObjectParent(kitchenObjectParent);
        
        return kitchenObject;
    }
    
}
