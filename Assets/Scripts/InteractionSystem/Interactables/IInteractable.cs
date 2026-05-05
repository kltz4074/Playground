using System;
using UnityEngine;

public class IInteractable : MonoBehaviour
{
    public virtual void OnInteract()
    {
        Debug.Log($"Interacted with {gameObject.name}");    
    }
}
