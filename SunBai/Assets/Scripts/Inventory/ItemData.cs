using UnityEngine;

public enum ItemType
{
    Weapon,
    Gear,
    Consumable
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public ItemType itemType;
    public Sprite icon;
    [TextArea] public string description;
}


