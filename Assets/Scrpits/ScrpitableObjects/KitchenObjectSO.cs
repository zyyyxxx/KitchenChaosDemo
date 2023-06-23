using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class KitchenObjectSO : ScriptableObject
{
    public Transform prefab;
    public Sprite sprite; //在Unity中，精灵(Sprite)是指2D图像或动画。它们可以用于创建角色、背景、UI元素等。
    public string objecName;

}
