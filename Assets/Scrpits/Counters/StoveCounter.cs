using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

public class StoveCounter : BaseCounter , IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressBarChangedEventArgs> OnProgressBarChanged;
    
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;

    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }
    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned,
    }
    
    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;
    
    private State state;
    private float fryingTimer;
    private FryingRecipeSO fryingRecipeSO;
    private float burningTimer;
    private BurningRecipeSO burningRecipeSO;

    private void Start()
    {
        state = State.Idle;
    }

    private void Update()
    {
        if (HasKitchenObject())
        {
            switch (state)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    fryingTimer += Time.deltaTime;
                    
                    OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
                    {
                        progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax
                    });
                    
                    
                    if (fryingTimer > fryingRecipeSO.fryingTimerMax)
                    {
                        // To Fried
                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);
                        Debug.Log("Object Fried");

                        burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectS0());
                        state = State.Fried;
                        burningTimer = 0f;
                        
                        OnStateChanged?.Invoke(this , new OnStateChangedEventArgs
                        {
                            state = state
                        });
                        
                        OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
                        {
                            progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax
                        });
                    }
                    break;
                case State.Fried:
                    burningTimer += Time.deltaTime;
                    
                    OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
                    {
                        progressNormalized = burningTimer / burningRecipeSO.burningTimerMax
                    });
                    
                    if (burningTimer > burningRecipeSO.burningTimerMax)
                    {
                        // To Burned
                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);
                        state = State.Burned;
                        
                        OnStateChanged?.Invoke(this , new OnStateChangedEventArgs
                        {
                            state = state
                        });
                        
                        OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
                        {
                            progressNormalized = 0f
                        });
                        
                    }
                    break;
                case State.Burned:
                    break;
            }

        }
    }


    /*使用协程
    private void Start()
    {
        StartCoroutine(HandleFryTimer());
    }

    private IEnumerator HandleFryTimer() 
    {
        yield return new WaitForSeconds(1f);
    }
    */
    
    
    
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
                    // 可以fried 再放下
                    player.GetKitchenObject().SetKtichenObjectParent(this);

                    fryingRecipeSO = GetFryingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectS0());
                    state = State.Frying;
                    fryingTimer = 0f;
                    OnStateChanged?.Invoke(this , new OnStateChangedEventArgs
                    {
                        state = state
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
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    //携带的是盘子 , 把clearCounter上的物品放到盘子上
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectS0()))
                    {
                        GetKitchenObject().DestroySelf();
                        
                        //注意拿走之后要更新状态
                        state = State.Idle;
                        OnStateChanged?.Invoke(this , new OnStateChangedEventArgs
                        {
                            state = state
                        });
                
                        OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
                        {
                            progressNormalized = 0f
                        });
                        
                    }

                }
            }
            else
            {
                // 玩家没有携带任何物品 ，拿取clearcounter上的物体
                GetKitchenObject().SetKtichenObjectParent(player);

                state = State.Idle;
                OnStateChanged?.Invoke(this , new OnStateChangedEventArgs
                {
                    state = state
                });
                
                OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
                {
                    progressNormalized = 0f
                });

            }
        }
    }
    
    
    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        return fryingRecipeSO != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        return fryingRecipeSO != null ? fryingRecipeSO.output : null;
    }

    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (FryingRecipeSO fryingRecipeSO in fryingRecipeSOArray)
        {
            if (fryingRecipeSO.input == inputKitchenObjectSO)
            {
                return fryingRecipeSO;
            }
        }

        return null;
    }
    

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOArray)
        {
            if (burningRecipeSO.input == inputKitchenObjectSO)
            {
                return burningRecipeSO;
            }
        }

        return null;
    }
}
