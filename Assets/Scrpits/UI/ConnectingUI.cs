using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{
    private void Start()
    {
        KitchenGameMultiplayer.Instance.OnTryingToJoinGame += InstanceOnOnTryingToJoinGame;
        KitchenGameMultiplayer.Instance.OnFailToJoinGame += InstanceOnOnFailToJoinGame;
        Hide();
    }

    private void InstanceOnOnFailToJoinGame(object sender, EventArgs e)
    {
        Hide();
    }

    private void InstanceOnOnTryingToJoinGame(object sender, EventArgs e)
    {
        Show();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    
    private void OnDestroy()
    {
        KitchenGameMultiplayer.Instance.OnTryingToJoinGame -= InstanceOnOnTryingToJoinGame;
        KitchenGameMultiplayer.Instance.OnFailToJoinGame -= InstanceOnOnFailToJoinGame;
    }
    
}
