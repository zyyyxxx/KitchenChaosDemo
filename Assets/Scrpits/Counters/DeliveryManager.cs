using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;


public class DeliveryManager : NetworkBehaviour
{

    public event EventHandler OnRecipeSpawn;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;
    
    public static DeliveryManager Instance { get; private set; }
    [SerializeField] private RecipeListSO recipeListSO;
    
    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimer = 4f;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipeMax = 4;
    private int successfulRecipeAmount; // 用于UI

    private void Awake()
    {
        Instance = this;  
        waitingRecipeSOList = new List<RecipeSO>();
        
    }

    private void Update()
    {
        if (!IsServer) return;
        // Server Only
        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;
            if (waitingRecipeSOList.Count < waitingRecipeMax)
            {
                // 获取一个配方SO,并加入等待列表
                int waitingRecipeSOIndex = Random.Range(0, recipeListSO.recipeSOList.Count);
                 
                
                SpawnNewWaitingRecipeClientRpc(waitingRecipeSOIndex);
                
                
            }
            
        }
    }
    
    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSOIndex)
    {
        RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[waitingRecipeSOIndex];
        
        waitingRecipeSOList.Add(waitingRecipeSO);
                
        OnRecipeSpawn?.Invoke(this ,EventArgs.Empty);
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];
            if (waitingRecipeSO.KitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                // 含有相同数量的配料，再比对
                
                bool plateContentMatchRecipe = true;
                // 遍历等待列表配方中的配料
                foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.KitchenObjectSOList)
                {   
                    bool ingredientFound = false;
                    // 再遍历盘子中的所有配料
                    foreach (var plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        if (recipeKitchenObjectSO == plateKitchenObjectSO)
                        {
                            // 找到相同的就退出对某个配料的遍历
                            ingredientFound = true;
                            break;
                        }
                    }
                    // 当前waiting配方未找到任意一个
                    if (!ingredientFound)
                    {
                        plateContentMatchRecipe = false;
                    }
                }

                if (plateContentMatchRecipe)
                {
                    // 提交正确
                    DeliverCorrectRecipeServerRpc(i);
                    return;
                }
                
            }
        }
        // 所有都未匹配成功
        DeliverIncorrectRecipeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverIncorrectRecipeServerRpc()
    {
        DeliverIncorrectRecipeClientRpc();
    }
    
    [ClientRpc]
    private void DeliverIncorrectRecipeClientRpc()
    {
        OnRecipeFailed?.Invoke(this,EventArgs.Empty);
        Debug.Log("Player did not deliver a correct recipe");
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRpc(int waitingRecipeSOListIndex)
    {
        DeliverCorrectRecipeClientRpc(waitingRecipeSOListIndex);
    }

    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int waitingRecipeSOListIndex)
    {
        successfulRecipeAmount++;
                    
        waitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);
                    
        OnRecipeCompleted?.Invoke(this,EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this,EventArgs.Empty);
    }
    
    public List<RecipeSO> GetWaitingrRecipeSOList()
    {
        return waitingRecipeSOList;
    }

    public int GetSuccessfulRecipesAmount()
    {
        return successfulRecipeAmount;
    }
}
