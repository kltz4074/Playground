using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "InteractionSystem/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    [TextArea]
    public string description;

    public bool isCustomPlaceable;
    public Vector2 InHandPos;
    public Vector3 InHandRot;
    public float holdDistance = 0.5f;
    public float throwForce = 2f;

    public float RotationSpeed = 40f;
    public float PositionSpeed = 5f;
}
