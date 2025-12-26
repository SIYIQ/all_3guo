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

    // 武器相关数据（仅在 itemType == Weapon 时使用）
    public WeaponData weaponData;
}


