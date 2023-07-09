using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        resumeButton.onClick.AddListener(() =>
        {
            KitchenGameManager.Instance.TogglePauseGame(); 
        });
        
        mainMenuButton.onClick.AddListener(() =>
        {
            // Disconnects clients if connected and stops server if running.
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MenuScene);
        });
    }

    private void Start()
    {
        KitchenGameManager.Instance.OnLocalGamePause += Instance_OnLocalGamePause;
        KitchenGameManager.Instance.OnLocalGameUnpause += Instance_OnLocalGameUnpause;
        
        Hide();
    }

    private void Instance_OnLocalGameUnpause(object sender, EventArgs e)
    {
        Hide();
    }

    private void Instance_OnLocalGamePause(object sender, EventArgs e)
    {
        Show(); 
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
