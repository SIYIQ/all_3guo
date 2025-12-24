using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Image icon;
    public Button button;
    public Sprite emptySprite;

    public ItemData Item { get; private set; }
    public InventoryUI parentUI;

    public void Init(InventoryUI parent, Sprite empty)
    {
        parentUI = parent;
        emptySprite = empty;
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
        UpdateVisual();
    }

    public void SetItem(ItemData data)
    {
        Item = data;
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (icon == null) return;
        icon.sprite = Item != null ? Item.icon : emptySprite;
        icon.color = Item != null ? Color.white : new Color(1f, 1f, 1f, 0.6f);
    }

    void OnClick()
    {
        if (parentUI != null)
            parentUI.OnGridSlotClicked(this);
    }

    // Drag & Drop
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Item == null) return;
        DragDropManager.BeginDrag(this, Item, icon.sprite);
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragDropManager.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragDropManager.EndDrag(eventData);
    }

    // Right-click: open equip choice dialog (if applicable)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (Item == null) return;
            if (parentUI != null)
                parentUI.ShowEquipChoiceDialog(Item, this);
        }
    }
}


