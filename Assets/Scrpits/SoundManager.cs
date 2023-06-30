using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
    [SerializeField] private AudioClipRefSO audioClipRefSO; 
    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
        Player.Instance.OnPickedSomething += Instance_OnPickedSomething;
        BaseCounter.OnAnyObjectPlacedHere += BaseCounter_OnAnyObjectPlacedHere;
        TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void TrashCounter_OnAnyObjectTrashed(object sender, EventArgs e)
    {
        TrashCounter trashCounter = sender as TrashCounter;
        PlaySouncd(audioClipRefSO.objectDrop , trashCounter.transform.position);
    }

    private void BaseCounter_OnAnyObjectPlacedHere(object sender, EventArgs e)
    {
        BaseCounter baseCounter = sender as BaseCounter;
        PlaySouncd(audioClipRefSO.objectDrop , baseCounter.transform.position);
    }

    private void Instance_OnPickedSomething(object sender, EventArgs e)
    {
        PlaySouncd(audioClipRefSO.objectPickup , Player.Instance.transform.position);
    }

    private void CuttingCounter_OnAnyCut(object sender, EventArgs e)
    {
        CuttingCounter cuttingCounter = sender as CuttingCounter;
        PlaySouncd(audioClipRefSO.chop , cuttingCounter.transform.position);
    }

    private void DeliveryManager_OnRecipeFailed(object sender, EventArgs e)
    {
        DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
        PlaySouncd(audioClipRefSO.deliveryFail , deliveryCounter.transform.position);
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, EventArgs e) 
    {
        DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
        PlaySouncd(audioClipRefSO.deliverySuccess , deliveryCounter.transform.position);
    }

    private void PlaySouncd(AudioClip[] audioClipArray , Vector3 position , float volume = 1f)
    {
        PlaySouncd(audioClipArray[Random.Range(0,audioClipArray.Length)] , position , volume);
    }
    
    private void PlaySouncd(AudioClip audioClip , Vector3 position , float volume = 1f)
    {
        AudioSource.PlayClipAtPoint(audioClip , position , volume);
    }

    public void PlayFootStepSound(Vector3 position , float volume)
    {
        PlaySouncd(audioClipRefSO.footstep , position , volume);
    }
}
