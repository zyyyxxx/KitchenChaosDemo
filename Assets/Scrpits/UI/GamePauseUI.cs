using System;
using System.Collections;
using System.Collections.Generic;
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
            Loader.Load(Loader.Scene.MenuScene);
        });
    }

    private void Start()
    {
        KitchenGameManager.Instance.OnGamePause += Instance_OnGamePause;
        KitchenGameManager.Instance.OnGameUnpause += Instance_OnGameUnpause;
        
        Hide();
    }

    private void Instance_OnGameUnpause(object sender, EventArgs e)
    {
        Hide();
    }

    private void Instance_OnGamePause(object sender, EventArgs e)
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
