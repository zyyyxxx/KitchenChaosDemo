using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMultiplayerUI : MonoBehaviour
{
    private void Start()
    {
        KitchenGameManager.Instance.OnMultiplayerPause += InstanceOnOnMultiplayerPause;
        KitchenGameManager.Instance.OnMultiplayerUnpause += InstanceOnOnMultiplayerUnpause;
        
        Hide();
    }

    private void InstanceOnOnMultiplayerUnpause(object sender, EventArgs e)
    {
        Hide();
    }

    private void InstanceOnOnMultiplayerPause(object sender, EventArgs e)
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
