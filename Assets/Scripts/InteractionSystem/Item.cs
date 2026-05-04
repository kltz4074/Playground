using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "InteractionSystem/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public GameObject Prefab;
    [TextArea]
    public string description;
}
