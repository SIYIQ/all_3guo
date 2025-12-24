using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class DragDropManager
{
    public static InventorySlot CurrentDraggedSlot { get; private set; }
    static GameObject dragIcon;

    public static void BeginDrag(InventorySlot fromSlot, ItemData item, Sprite iconSprite)
    {
        CurrentDraggedSlot = fromSlot;
        if (dragIcon == null)
        {
            dragIcon = new GameObject("DragIcon");
            var img = dragIcon.AddComponent<Image>();
            img.raycastTarget = false;
            var canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas != null)
                dragIcon.transform.SetParent(canvas.transform, false);
        }
        var image = dragIcon.GetComponent<Image>();
        image.sprite = iconSprite;
        dragIcon.SetActive(true);
    }

    public static void OnDrag(PointerEventData eventData)
    {
        if (dragIcon == null) return;
        RectTransform rt = dragIcon.GetComponent<RectTransform>();
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragIcon.transform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out pos);
        rt.localPosition = pos;
    }

    public static void EndDrag(PointerEventData eventData)
    {
        // On end drag, if dropped over nothing, do nothing
        Clear();
    }

    public static void Clear()
    {
        CurrentDraggedSlot = null;
        if (dragIcon != null)
            dragIcon.SetActive(false);
    }
}


