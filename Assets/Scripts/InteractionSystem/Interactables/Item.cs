using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "InteractionSystem/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public GameObject Prefab;
    [TextArea]
    public string description;

    public bool isCustomPlaceable;
    public Vector2 InHandPos;
    public Vector2 InHandRot;
    public float InHandScale;
    public float minHoldDistance = 0.5f;
}
