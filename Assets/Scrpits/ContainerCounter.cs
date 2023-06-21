using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ContainerCounter : BaseCounter
{
    public event EventHandler OnPlayerGrabbedObject;
    
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            return;
        }
        
        Debug.Log("ContainerCounter Interact!");
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);//生成物体
        kitchenObjectTransform.GetComponent<KitchenObject>().SetKtichenObjectParent(player);
        
        OnPlayerGrabbedObject?.Invoke(this , EventArgs.Empty);
    }
    
   
}
