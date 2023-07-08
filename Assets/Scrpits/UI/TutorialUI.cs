using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI tutorialText;
    
    private void Start()
    {
        KitchenGameManager.Instance.OnLocalPlayerReadyChanged += Instance_OnLocalPlayerReadyChanged;
        KitchenGameManager.Instance.OnStateChanged += InstanceOnOnStateChanged;
        Show();
    }

    private void InstanceOnOnStateChanged(object sender, EventArgs e)
    {
        if (KitchenGameManager.Instance.IsCountdownToStartActive())
        {
            Hide();
        }
    }

    private void Instance_OnLocalPlayerReadyChanged(object sender, EventArgs e)
    {
        if (KitchenGameManager.Instance.IsLocalPlayReady())
        {
            tutorialText.text = "WAIT FOR OTHER PLAYERS";
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
