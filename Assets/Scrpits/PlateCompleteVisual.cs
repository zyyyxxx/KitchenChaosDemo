using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [Serializable] //把自定义结构体暴露给unity
    public struct KitchenObjectSO_GameObject
    {
        public KitchenObjectSO KitchenObjectSO;
        public GameObject gameObject;
    }
    
    [SerializeField] private PlateKitchenObject plateKitchenObject;
    [SerializeField] private List<KitchenObjectSO_GameObject> kitchenObjectSOGameObjectList;

    private void Start()
    {
        plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;
        
        foreach (KitchenObjectSO_GameObject kitchenObjectSOGameObject in kitchenObjectSOGameObjectList)
        {
            kitchenObjectSOGameObject.gameObject.SetActive(false);
        }
    }

    private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        foreach (KitchenObjectSO_GameObject kitchenObjectSOGameObject in kitchenObjectSOGameObjectList)
        {
            if (kitchenObjectSOGameObject.KitchenObjectSO == e.KitchenObjectSO)
            {
                // 判断添加的是哪一个SO，显示对应的预制体
                kitchenObjectSOGameObject.gameObject.SetActive(true);
                
            }
        }
        
    }
}
