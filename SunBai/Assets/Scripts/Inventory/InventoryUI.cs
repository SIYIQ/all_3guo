using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject inventoryRoot; // 整个底板（按 I 键显示/隐藏）

    [Header("Left Top")]
    public Image portraitImage; // 立绘放这里

    [Header("Equip Slots (4)")]
    public EquipSlot weaponSlot; // allowedType = Weapon
    public EquipSlot gearSlot;   // allowedType = Gear
    public EquipSlot consumableSlotA; // allowedType = Consumable
    public EquipSlot consumableSlotB; // allowedType = Consumable

    [Header("Right Grid")]
    public GameObject slotPrefab;
    public Transform gridParent;
    public int gridSlotCount = 20;
    public Sprite emptySlotSprite;

    [Header("Tabs")]
    public Button tabEquipButton;
    public Button tabConsumableButton;

    [Header("Data")]
    public List<ItemData> inventoryItems = new List<ItemData>();

    List<InventorySlot> gridSlots = new List<InventorySlot>();
    enum Tab { Equipment, Consumables }
    Tab activeTab = Tab.Equipment;
    
    [Header("Status Bars")]
    public StatusBar attackBar;
    public StatusBar rangeBar;
    public StatusBar cooldownBar;

    [Header("Layout Tweaks")]
    public float barsYOffset = -80f; // downward offset applied to all three bars
    public float barsExtraWidth = 300f; // how much to lengthen bars (added to current width)
    public Vector2 portraitTargetSize = new Vector2(100f, 280f); // width x height for slimmer portrait

    void Start()
    {
        if (inventoryRoot != null) inventoryRoot.SetActive(false);
        CreateGridSlots();
        if (tabEquipButton != null) tabEquipButton.onClick.AddListener(() => SwitchTab(Tab.Equipment));
        if (tabConsumableButton != null) tabConsumableButton.onClick.AddListener(() => SwitchTab(Tab.Consumables));
        // Attempt to load persistent ItemData assets from Resources if none assigned
        if ((inventoryItems == null || inventoryItems.Count == 0))
        {
            var loaded = Resources.LoadAll<ItemData>("Inventory/Items");
            if (loaded != null && loaded.Length > 0)
            {
                inventoryItems = new List<ItemData>(loaded);
            }
        }
        RefreshGrid();
        UpdateTabVisuals();
        // Apply layout tweaks so bars and portrait fit design
        ApplyLayoutTweaks();
    }

    // Public: adjust status bar positions/sizes and portrait size to match design request
    public void ApplyLayoutTweaks()
    {
        // Move and lengthen status bars
        StatusBar[] bars = new StatusBar[] { attackBar, rangeBar, cooldownBar };
        foreach (var bar in bars)
        {
            if (bar == null) continue;
            var rt = bar.GetComponent<RectTransform>();
            if (rt != null)
            {
                // shift vertically
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y + barsYOffset);
                // also adjust localPosition in case a parent layout is overriding anchoredPosition
                rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y + barsYOffset, rt.localPosition.z);
                // increase width by barsExtraWidth (keep pivot)
                Vector2 size = rt.sizeDelta;
                size.x = Mathf.Max(16f, size.x + barsExtraWidth);
                rt.sizeDelta = size;
            }
            // update label if exists to ensure text stays in sync
            bar.SetLabel(bar.labelText != null ? bar.labelText.text : "");
        }

        // Resize portrait block
        if (portraitImage != null)
        {
            var prt = portraitImage.GetComponent<RectTransform>();
            if (prt != null)
            {
                prt.sizeDelta = portraitTargetSize;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        if (inventoryRoot == null) return;
        bool show = !inventoryRoot.activeSelf;
        inventoryRoot.SetActive(show);
        if (show)
        {
            // default to Equipment tab when opened
            SwitchTab(Tab.Equipment);
        }
    }

    void CreateGridSlots()
    {
        if (slotPrefab == null || gridParent == null) return;
        // Clear existing children created previously
        for (int i = gridParent.childCount - 1; i >= 0; i--)
        {
            // Use Destroy in runtime contexts; DestroyImmediate was editor-only.
            var child = gridParent.GetChild(i).gameObject;
            Destroy(child);
        }

        gridSlots.Clear();
        for (int i = 0; i < gridSlotCount; i++)
        {
            GameObject go = Instantiate(slotPrefab, gridParent);
            // ensure transform is normalized so prefab visuals and layout behave consistently
            go.transform.localScale = Vector3.one;
            var rt = go.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = Vector2.zero;
                rt.localRotation = Quaternion.identity;
            }
            InventorySlot slot = go.GetComponent<InventorySlot>();
            if (slot != null)
            {
                slot.Init(this, emptySlotSprite);
                gridSlots.Add(slot);
            }
        }
    }

    public void OnGridSlotClicked(InventorySlot slot)
    {
        if (slot == null) return;
        if (slot.Item == null) return;
        ItemData item = slot.Item;
        if (item == null) return;

        switch (item.itemType)
        {
            case ItemType.Weapon:
                if (weaponSlot != null)
                {
                    weaponSlot.SetItem(item);
                    slot.SetItem(null);
                    // remove from inventory list so it disappears from grid
                    if (inventoryItems != null && inventoryItems.Contains(item))
                        inventoryItems.Remove(item);
                }
                break;
            case ItemType.Gear:
                if (gearSlot != null)
                {
                    gearSlot.SetItem(item);
                    slot.SetItem(null);
                    if (inventoryItems != null && inventoryItems.Contains(item))
                        inventoryItems.Remove(item);
                }
                break;
            case ItemType.Consumable:
                if (consumableSlotA != null && consumableSlotA.CurrentItem == null)
                {
                    consumableSlotA.SetItem(item);
                    slot.SetItem(null);
                    if (inventoryItems != null && inventoryItems.Contains(item))
                        inventoryItems.Remove(item);
                }
                else if (consumableSlotB != null && consumableSlotB.CurrentItem == null)
                {
                    consumableSlotB.SetItem(item);
                    slot.SetItem(null);
                    if (inventoryItems != null && inventoryItems.Contains(item))
                        inventoryItems.Remove(item);
                }
                else
                {
                    Debug.Log("Consumable slots full");
                }
                break;
        }
    }

    // Show a simple popup letting player choose which equip slot to place the item into.
    // For simplicity the popup is created at runtime under the inventoryRoot.
    public void ShowEquipChoiceDialog(ItemData item, InventorySlot fromSlot)
    {
        if (inventoryRoot == null) return;
        // Remove any existing dialog
        Transform existing = inventoryRoot.transform.Find("EquipChoiceDialog");
        if (existing != null) Destroy(existing.gameObject);

        GameObject panel = new GameObject("EquipChoiceDialog");
        panel.transform.SetParent(inventoryRoot.transform, false);
        RectTransform prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.4f, 0.4f);
        prt.anchorMax = new Vector2(0.6f, 0.6f);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.95f, 0.95f, 0.85f);

        VerticalLayoutGroup vl = panel.AddComponent<VerticalLayoutGroup>();
        vl.spacing = 6f;
        vl.childForceExpandHeight = false;

        // Create buttons for valid targets
        if (item.itemType == ItemType.Weapon && weaponSlot != null)
            CreateDialogButton(panel.transform, "Equip to Weapon", () => { EquipItemToSlot(item, weaponSlot, fromSlot); Destroy(panel); });
        if (item.itemType == ItemType.Gear && gearSlot != null)
            CreateDialogButton(panel.transform, "Equip to Gear", () => { EquipItemToSlot(item, gearSlot, fromSlot); Destroy(panel); });
        if (item.itemType == ItemType.Consumable)
        {
            if (consumableSlotA != null) CreateDialogButton(panel.transform, "Equip to Consumable A", () => { EquipItemToSlot(item, consumableSlotA, fromSlot); Destroy(panel); });
            if (consumableSlotB != null) CreateDialogButton(panel.transform, "Equip to Consumable B", () => { EquipItemToSlot(item, consumableSlotB, fromSlot); Destroy(panel); });
        }

        CreateDialogButton(panel.transform, "Cancel", () => { Destroy(panel); });
    }

    void CreateDialogButton(Transform parent, string text, System.Action onClick)
    {
        GameObject go = new GameObject("Btn_" + text);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.8f, 0.8f, 0.8f);
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(() => onClick());
        GameObject txt = new GameObject("Text");
        txt.transform.SetParent(go.transform, false);
        Text t = txt.AddComponent<Text>();
        // Use LegacyRuntime.ttf for compatibility across Unity versions
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = text;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.black;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200f, 28f);
    }

    // Equip an item into a specific equip slot (and remove from origin slot if provided)
    public void EquipItemToSlot(ItemData item, EquipSlot targetSlot, InventorySlot fromSlot = null)
    {
        if (item == null || targetSlot == null) return;
        targetSlot.SetItem(item);
        if (fromSlot != null)
            fromSlot.SetItem(null);
        else
            inventoryItems.Remove(item);
        RefreshGrid();
    }

    void SwitchTab(Tab t)
    {
        activeTab = t;
        RefreshGrid();
        UpdateTabVisuals();
    }

    void UpdateTabVisuals()
    {
        if (tabEquipButton != null)
        {
            var img = tabEquipButton.GetComponent<Image>();
            if (img != null) img.color = (activeTab == Tab.Equipment) ? Color.white : new Color(0.85f, 0.85f, 0.85f);
            tabEquipButton.transform.localScale = (activeTab == Tab.Equipment) ? Vector3.one * 1.02f : Vector3.one;
        }
        if (tabConsumableButton != null)
        {
            var img = tabConsumableButton.GetComponent<Image>();
            if (img != null) img.color = (activeTab == Tab.Consumables) ? Color.white : new Color(0.85f, 0.85f, 0.85f);
            tabConsumableButton.transform.localScale = (activeTab == Tab.Consumables) ? Vector3.one * 1.02f : Vector3.one;
        }
    }

    public void RefreshGrid()
    {
        List<ItemData> filtered = new List<ItemData>();
        foreach (var it in inventoryItems)
        {
            if (it == null) continue;
            if (activeTab == Tab.Equipment && (it.itemType == ItemType.Weapon || it.itemType == ItemType.Gear))
                filtered.Add(it);
            if (activeTab == Tab.Consumables && it.itemType == ItemType.Consumable)
                filtered.Add(it);
        }

        for (int i = 0; i < gridSlots.Count; i++)
        {
            if (i < filtered.Count)
                gridSlots[i].SetItem(filtered[i]);
            else
                gridSlots[i].SetItem(null);
        }
    }

    // Helper: add item to inventory and refresh
    public void AddItemToInventory(ItemData item)
    {
        if (item == null) return;
        inventoryItems.Add(item);
        RefreshGrid();
    }
}


