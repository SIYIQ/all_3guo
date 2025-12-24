using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public ItemType allowedType;
    public Image icon;
    public Sprite emptySprite;
    public InventoryUI inventoryUI;

    public ItemData CurrentItem { get; private set; }

    public void SetItem(ItemData data)
    {
        CurrentItem = data;
        if (icon != null)
        {
            icon.sprite = data != null ? data.icon : emptySprite;
            icon.color = data != null ? Color.white : new Color(1f, 1f, 1f, 0.6f);
        }
    }

    public void Clear()
    {
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


