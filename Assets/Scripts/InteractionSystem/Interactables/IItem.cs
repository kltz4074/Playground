using UnityEngine;

public class ItemBase : Interactable
{
    [SerializeField] public Item ScItem;
    
    [HideInInspector] public bool Grabbed;
    public override void OnInteract() { Debug.Log($"Picked up {ScItem.name}"); }

    public virtual void OnItemUpdate() { }
    
    public virtual void OnDrop() { Debug.Log($"Dropped {ScItem.name}"); }
}
