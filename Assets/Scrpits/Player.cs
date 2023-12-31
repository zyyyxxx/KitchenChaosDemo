using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour , IKitchenObjectParent
{

    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPickedSomething; 

    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
    }
    
    public static Player LocalInstance { get; private set; }


    public event EventHandler OnPickedSomething; 
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }
    
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private LayerMask CollisionsLayerMaskk;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    [SerializeField] private List<Vector3> spawnPositionList;
    
    private bool isWalking;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    
    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }

        transform.position = spawnPositionList[(int)OwnerClientId];
        
        OnAnyPlayerSpawned?.Invoke(this , EventArgs.Empty);

        if (IsServer)
        {
            // The callback to invoke when a client disconnects.
            // This callback is only ran on the server and on the local client 
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientID)
    {
        if (clientID == OwnerClientId && HasKitchenObject())
        {
            KitchenObject.DestoryKitchenObject(GetKitchenObject());
        }
    }


    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }


    // Update is called once per frame
    private void Update()
    {
        if (!IsOwner) return;
        HandleMovement();
        //HandleMovementServerAuth();
        HandleInteractions();
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    private void HandleInteractions()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        
        // 防止不移动后射线检测失效
        if (moveDir != Vector3.zero)
        {
            lastInteractDir = moveDir;
        }

        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance , countersLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                // Has ClearCounter
                if (baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }else {
                SetSelectedCounter(null);
            }
        }else {
            SetSelectedCounter(null);
        }
    }
    
    
    // Multiplayer Server Authority
    private void HandleMovementServerAuth()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        HandleMovementServerRpc(inputVector);
    }
    
    [ServerRpc(RequireOwnership = false)] //// Whether or not the ServerRpc should only be run if executed by the owner of the object
    private void HandleMovementServerRpc(Vector2 inputVector)
    {
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = Time.deltaTime * moveSpeed;
        float playerRadius = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position, 
            transform.position + Vector3.up * playerHeight , 
            playerRadius, moveDir ,moveDistance , countersLayerMask);

        if (!canMove)
        {   
            // 我们希望贴着墙壁进行对角线移动时，可以左右移动
            
            // Cannot move towards moveDir
            
            // Attempt only X movement
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, 
                transform.position + Vector3.up * playerHeight , 
                playerRadius, moveDirX ,moveDistance , countersLayerMask);

            if (canMove)
            {
                // Can mvoe only on the X
                moveDir = moveDirX;
            }
            else
            {
                // 无法在X轴移动，尝试Z轴
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position, 
                    transform.position + Vector3.up * playerHeight , 
                    playerRadius, moveDirZ ,moveDistance , countersLayerMask);

                if (canMove)
                {
                    moveDir = moveDirZ;
                }
                else
                {
                    // 完全不能移动
                }
            }
            
        }
        
        if (canMove)
        {
            transform.position += moveDir * moveDistance;    
        }
        
        isWalking = moveDir != Vector3.zero;
        
        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward , moveDir , Time.deltaTime * rotateSpeed);
    }
    
    
    
    
    
    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = Time.deltaTime * moveSpeed;
        float playerRadius = .7f; 
        //float playerHeight = 2f;
        bool canMove = !Physics.BoxCast(transform.position, 
            Vector3.one * playerRadius, moveDir ,
            quaternion.identity , moveDistance,CollisionsLayerMaskk);

        if (!canMove)
        {   
            // 我们希望贴着墙壁进行对角线移动时，可以左右移动
            
            // Cannot move towards moveDir
            
            // Attempt only X movement
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = moveDir.x != 0 && !Physics.BoxCast(transform.position, 
                Vector3.one * playerRadius, moveDirX ,
                quaternion.identity , moveDistance,CollisionsLayerMaskk);

            if (canMove)
            { 
                // Can mvoe only on the X
                moveDir = moveDirX;
            }
            else
            {
                // 无法在X轴移动，尝试Z轴
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = moveDir.z != 0 && !Physics.BoxCast(transform.position, 
                    Vector3.one * playerRadius, moveDirZ ,
                    quaternion.identity , moveDistance,CollisionsLayerMaskk);

                if (canMove)
                {
                    moveDir = moveDirZ;
                }
                else
                {
                    // 完全不能移动
                }
            }
            
        }
        
        if (canMove)
        {
            transform.position += moveDir * moveDistance;    
        }
        
        isWalking = moveDir != Vector3.zero;
        
        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward , moveDir , Time.deltaTime * rotateSpeed);
    }

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;
        
        OnSelectedCounterChanged?.Invoke(this  , new OnSelectedCounterChangedEventArgs
        {
            selectedCounter =  selectedCounter
        });
    }

    // 接口工具函数
    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this,EventArgs.Empty);
            OnAnyPickedSomething?.Invoke(this ,EventArgs.Empty);
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
