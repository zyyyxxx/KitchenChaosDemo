using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Unity.Netcode;
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
    
    private NetworkVariable<State> state = new NetworkVariable<State>(State.Idle);
    private NetworkVariable<float> fryingTimer = new NetworkVariable<float>(0f);
    private FryingRecipeSO fryingRecipeSO;
    private NetworkVariable<float> burningTimer = new NetworkVariable<float>(0f);
    private BurningRecipeSO burningRecipeSO;

    private void Start()
    {
        //state = State.Idle;
    }

    public override void OnNetworkDespawn()
    {
        fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
        burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
        state.OnValueChanged += Stete_OnValueChanged;
    }

    private void Stete_OnValueChanged(State previousvalue, State newvalue)
    {
        OnStateChanged?.Invoke(this , new OnStateChangedEventArgs
        {
            state = state.Value
        });
        if (state.Value == State.Burned || state.Value == State.Idle)
        {
            OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
            {
                progressNormalized = 0f
            });
        }
    }

    private void BurningTimer_OnValueChanged(float previousvalue, float newvalue)
    {
        // 保证fryingRecipeSO绑定之前 也有委托可以调用
        float burningTimerMax = burningRecipeSO != null ? burningRecipeSO.burningTimerMax : 1f;
        
        OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
        {
            progressNormalized = burningTimer.Value / burningTimerMax
        });
    }

    private void FryingTimer_OnValueChanged(float previousvalue, float newvalue)
    {
        // 保证fryingRecipeSO绑定之前 也有委托可以调用
        float fryingTimerMax = fryingRecipeSO != null ? fryingRecipeSO.fryingTimerMax : 1f;
        
        OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
        {
            progressNormalized = fryingTimer.Value / fryingTimerMax
        });
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }
        
        if (HasKitchenObject())
        {
            switch (state.Value)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    fryingTimer.Value += Time.deltaTime;
                    
                    /*OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
                    {
                        progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax
                    });*/
                    
                    
                    if (fryingTimer.Value > fryingRecipeSO.fryingTimerMax)
                    {
                        // To Fried
                        KitchenObject.DestoryKitchenObject(GetKitchenObject());
                        //GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);
                        Debug.Log("Object Fried");

                        //burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectS0());
                        state.Value = State.Fried;
                        burningTimer.Value = 0f;
                        SetBurningRecipeSOClientRpc(
                            KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(GetKitchenObject().GetKitchenObjectS0())
                            );

                        /*OnStateChanged?.Invoke(this , new OnStateChangedEventArgs
                        {
                            state = state
                        });*/

                        /*OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
                        {
                            progressNormalized = fryingTimer.Value / fryingRecipeSO.fryingTimerMax
                        });*/
                    }
                    break;
                case State.Fried:
                    burningTimer.Value += Time.deltaTime;
                    
                    /*OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
                    {
                        progressNormalized = burningTimer / burningRecipeSO.burningTimerMax
                    });*/
                    
                    if (burningTimer.Value > burningRecipeSO.burningTimerMax)
                    {
                        // To Burned
                        KitchenObject.DestoryKitchenObject(GetKitchenObject());
                        //GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);
                        
                        state.Value = State.Burned;
                        
                        
                        /*OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
                        {
                            progressNormalized = 0f
                        });*/
                        
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
                    // 对于多人游戏 先储存
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    player.GetKitchenObject().SetKtichenObjectParent(this);
                    
                    InteractLogicPlaceObjectOnCounterServerRpc(
                        KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObject.GetKitchenObjectS0())
                    );
                    
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
                        
                        //注意拿走之后要更新状态
                        //state.Value = State.Idle;
                        SetStateIdleServerRpc();
                        
                        /*OnStateChanged?.Invoke(this , new OnStateChangedEventArgs
                        {
                            state = state
                        });
                
                        OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
                        {
                            progressNormalized = 0f
                        });*/
                        
                    }

                }
            }
            else
            {
                // 玩家没有携带任何物品 ，拿取clearcounter上的物体
                GetKitchenObject().SetKtichenObjectParent(player);

                //state.Value = State.Idle; 必须server进行改变
                SetStateIdleServerRpc();

                /*OnStateChanged?.Invoke(this , new OnStateChangedEventArgs
                {
                    state = state
                });
                
                OnProgressBarChanged?.Invoke(this , new IHasProgress.OnProgressBarChangedEventArgs
                {
                    progressNormalized = 0f
                });*/

            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetStateIdleServerRpc()
    {
        state.Value = State.Idle;
    }
    

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjetSOIndex)
    {
        // 仅服务器可以写入network variable
        fryingTimer.Value = 0f;
        state.Value = State.Frying;
        
        SetFryingRecipeSOClientRpc(kitchenObjetSOIndex);
    }
    
    [ClientRpc]
    private void SetFryingRecipeSOClientRpc(int kitchenObjetSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjetSOIndex);
        
        //fryingRecipeSO = GetFryingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectS0());
        fryingRecipeSO = GetFryingRecipeSOWithInput(kitchenObjectSO);
                   
        //state.Value = State.Frying;
        
        //fryingTimer.Value = 0f;
        
        // 由FryingTimer_OnValueChanged调用
        /*OnStateChanged?.Invoke(this , new OnStateChangedEventArgs
        {
            state = state
        });*/
    }
    
    [ClientRpc]
    private void SetBurningRecipeSOClientRpc(int kitchenObjetSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjetSOIndex);
        burningRecipeSO = GetBurningRecipeSOWithInput(kitchenObjectSO);
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

    public bool IsFired()
    {
        return state.Value == State.Fried;
    }
}
