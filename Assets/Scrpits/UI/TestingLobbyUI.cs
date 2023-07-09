using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestingLobbyUI : MonoBehaviour
{
    [SerializeField] private Button creatGameButton;
    [SerializeField] private Button joinGameButton;

    private void Awake()
    {
        creatGameButton.onClick.AddListener((() =>
        {
            KitchenGameMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        }));
        
        joinGameButton.onClick.AddListener((() =>
        {
            KitchenGameMultiplayer.Instance.StartClient();
        }));
    }
}
