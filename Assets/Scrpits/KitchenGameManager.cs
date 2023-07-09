using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameManager : NetworkBehaviour
{
    public static KitchenGameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalGamePause;
    public event EventHandler OnLocalGameUnpause;
    public event EventHandler OnLocalPlayerReadyChanged;
    public event EventHandler OnMultiplayerPause;
    public event EventHandler OnMultiplayerUnpause;
    
    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    [SerializeField] private Transform playerPrefab;
    
    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);

    private bool isLocalPlayerReady;
    private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private float gamePlayingTimerMax = 90f;
    private bool isLocalGamePause = false;
    private NetworkVariable<bool> isGamePause = new NetworkVariable<bool>(false);
    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, bool> playerPauseDictionary;
    private bool autoTestGamePausedState;

    private void Awake()
    {
        Instance = this;
        state.Value = State.WaitingToStart;
        playerReadyDictionary = new Dictionary<ulong, bool>();
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_ValueChanged;
        isGamePause.OnValueChanged += OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += SingletonOnOnClientConnectedCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
    {
        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID , true);
        }
    }
    
    private void SingletonOnOnClientConnectedCallback(ulong clientID)
    {
        autoTestGamePausedState = true;
        TestGamePauseState();
    }

    private void OnValueChanged(bool previousvalue, bool newvalue)
    {
        if (isGamePause.Value)
        {
            Time.timeScale = 0f;
            
            OnMultiplayerPause?.Invoke(this ,EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            
            OnMultiplayerUnpause?.Invoke(this , EventArgs.Empty);
        }
    }
    

    private void State_ValueChanged(State previousvalue, State newvalue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (state.Value == State.WaitingToStart)
        {
            isLocalPlayerReady = true;
            
            OnLocalPlayerReadyChanged?.Invoke(this , EventArgs.Empty);
            
            SetPlayerReadyServerRpc();
            
            //OnLocalPlayerReadyChanged?.Invoke(this , EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientReady = true;
        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {   
            if(!playerReadyDictionary.ContainsKey(clientID) || !playerReadyDictionary[clientID])  
            {
                allClientReady = false;
                break;
            }
        }

        if (allClientReady)
        {
            state.Value = State.CountdownToStart;
        }
    }
    
    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    
    private void Update()
    {
        if (!IsServer)
        {
            return;
        }
        switch (state.Value)
        {
            case State.WaitingToStart:
                
                break;
            
            case State.CountdownToStart:
                countdownToStartTimer.Value -= Time.deltaTime;
                if (countdownToStartTimer.Value < 0f)
                {
                    state.Value = State.GamePlaying;
                    gamePlayingTimer.Value = gamePlayingTimerMax;
                    //OnStateChanged?.Invoke(this, EventArgs.Empty);
                }

                break;
            
            case State.GamePlaying:
                gamePlayingTimer.Value -= Time.deltaTime;
                if (gamePlayingTimer.Value < 0f)
                {
                    state.Value = State.GameOver;
                    //OnStateChanged?.Invoke(this, EventArgs.Empty);
                }

                break;
            
            case State.GameOver:
                break;

        }


    }

    private void LateUpdate()
    {
        if (autoTestGamePausedState)
        {
            autoTestGamePausedState = false;
            TestGamePauseState();
        }
    }

    public bool IsGamePlaying()
    {
        return state.Value == State.GamePlaying;
    }

    public bool IsCountdownToStartActive()
    {
        return state.Value == State.CountdownToStart;
    }

    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer.Value;
    }

    public bool IsGameOver()
    {
        return state.Value == State.GameOver;
    }

    public bool IsWaitingToStart()
    {
        return state.Value == State.WaitingToStart;
    }
    
    public bool IsLocalPlayReady()
    {
        return isLocalPlayerReady;
    }
    
    public float GetPlayingTimerNormalized()
    {
        return 1 - (gamePlayingTimer.Value / gamePlayingTimerMax);
    }

    public void TogglePauseGame()
    {
        isLocalGamePause = !isLocalGamePause;
        if (isLocalGamePause)
        {
            PauseGameServerRpc();
            
            OnLocalGamePause?.Invoke(this , EventArgs.Empty);
        }
        else
        {
            UnpauseGameServerRpc();
            
            OnLocalGameUnpause?.Invoke(this ,EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPauseDictionary[serverRpcParams.Receive.SenderClientId] = true;
        
        TestGamePauseState();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPauseDictionary[serverRpcParams.Receive.SenderClientId] = false;   
        TestGamePauseState();
    }

    private void TestGamePauseState()
    {
        foreach (var clientsId in Unity.Netcode.NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPauseDictionary.ContainsKey(clientsId) && playerPauseDictionary[clientsId])
            {
                //this player is paused
                isGamePause.Value = true;
                return;
            }
        }
        
        //全都unpause
        isGamePause.Value = false;
    }
}
