using UnityEngine;

public class IItem : IInteractable
{
    [SerializeField] public Item item;

    public override void OnInteract() { Debug.Log($"Picked up {item.name}"); }

    public virtual void OnItemUpdate() { }
    
    public virtual void OnDrop() { Debug.Log($"Dropped {item.name}"); }
}
