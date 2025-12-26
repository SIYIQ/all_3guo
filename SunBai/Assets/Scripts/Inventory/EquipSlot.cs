using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public ItemType allowedType;
    public Image icon;
    public Text nameText;
    public Sprite emptySprite;
    public InventoryUI inventoryUI;

    public ItemData CurrentItem { get; private set; }

    public void SetItem(ItemData data)
    {
        CurrentItem = data;
        if (icon != null)
        {
            // 优先显示 ItemData.weaponData 的图标（如果存在），否则使用 ItemData.icon
            Sprite displaySprite = null;
            if (data != null && data.itemType == ItemType.Weapon && data.weaponData != null && data.weaponData.icon != null)
                displaySprite = data.weaponData.icon;
            else
                displaySprite = data != null ? data.icon : null;

            icon.sprite = displaySprite != null ? displaySprite : emptySprite;
            icon.color = displaySprite != null ? Color.white : new Color(1f, 1f, 1f, 0.6f);
            // 名称显示（如果存在）
            if (nameText != null)
            {
                if (data != null)
                {
                    if (data.itemType == ItemType.Weapon && data.weaponData != null && !string.IsNullOrEmpty(data.weaponData.displayName))
                        nameText.text = data.weaponData.displayName;
                    else
                        nameText.text = data.itemName;
                }
                else
                {
                    nameText.text = "";
                }
            }
        }

        // 如果是武器装备槽，通知武器系统
        if (allowedType == ItemType.Weapon && WeaponSystemBridge.Instance != null)
        {
            if (data != null && data.weaponData != null)
            {
                WeaponSystemBridge.Instance.EquipWeapon(data.weaponData);
            }
            else
            {
                WeaponSystemBridge.Instance.UnequipWeapon();
            }
        }
    }

    public void Clear()
    {
        // 如果是武器装备槽，先卸载武器
        if (allowedType == ItemType.Weapon && WeaponSystemBridge.Instance != null)
        {
            WeaponSystemBridge.Instance.UnequipWeapon();
        }

        // return to inventory if possible
        if (CurrentItem != null && inventoryUI != null)
        {
            inventoryUI.AddItemToInventory(CurrentItem);
        }
        SetItem(null);
    }

    // Handle drop from InventorySlot
    public void OnDrop(PointerEventData eventData)
    {
        var from = DragDropManager.CurrentDraggedSlot;
        if (from == null) return;
        if (from.Item == null) return;
        if (from.Item.itemType != allowedType && !(allowedType == ItemType.Consumable && from.Item.itemType == ItemType.Consumable)) return;

        // Equip to this slot
        var item = from.Item;
        SetItem(item);
        // remove from inventory list if present
        if (inventoryUI != null && inventoryUI.inventoryItems != null && inventoryUI.inventoryItems.Contains(item))
        {
            inventoryUI.inventoryItems.Remove(item);
        }
        from.SetItem(null);
        DragDropManager.Clear();
    }

    // Right-click to unequip
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Clear();
        }
    }
}


