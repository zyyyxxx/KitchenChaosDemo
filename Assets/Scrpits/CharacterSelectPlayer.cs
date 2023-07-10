using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class CharacterSelectPlayer : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    private void Start()
    {
        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged += InstanceOnOnPlayerDataNetworkListChanged; 
        UpdatePlayer();
    }

    private void InstanceOnOnPlayerDataNetworkListChanged(object sender, EventArgs e)
    {
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        if (KitchenGameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex))
        {
            Show();
        }
        else
        {
            Hide();
        }
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
