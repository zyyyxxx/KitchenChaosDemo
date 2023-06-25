using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private GameObject hasProgressGameObject; //不直接用IHasProgress是因为unity不会把接口SerializeField
    [SerializeField] private Image barImage;

    private IHasProgress hasProgress;
    
    private void Start()
    {
        hasProgress = hasProgressGameObject.GetComponent<IHasProgress>();
        if (hasProgress == null)
        {
            Debug.LogError("Game Object" + hasProgressGameObject + "does not have that implements IHasProgress");
        }
        hasProgress.OnProgressBarChanged += HasProgress_OnProgressBarChanged;

        barImage.fillAmount = 0f;
        Hide();
    }

    private void HasProgress_OnProgressBarChanged(object sender, IHasProgress.OnProgressBarChangedEventArgs e)
    {
        barImage.fillAmount = e.progressNormalized;
        if (e.progressNormalized == 0f || e.progressNormalized == 1f)
        {
            Hide();
        }
        else
        {
            Show();
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
