using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    // Helper: compare two ItemData for game-equivalence (by name and type).
    public static bool ItemsMatch(ItemData a, ItemData b)
    {
        if (a == null || b == null) return false;
        return a.itemType == b.itemType && !string.IsNullOrEmpty(a.itemName) && a.itemName == b.itemName;
    }
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
    // 可变的生命/魔法条（供技能系统/物品修改）
    public StatusBar hpBar;
    public StatusBar mpBar;

    // 可配置的当前生命/魔法值
    public float currentHP = 100f;
    public float maxHP = 100f;
    public float currentMP = 50f;
    public float maxMP = 50f;

    [Header("Layout Tweaks")]
    public float barsYOffset = -100f; // downward offset applied to all three bars
    public float barsExtraWidth = 300f; // how much to lengthen bars (added to current width)
    public Vector2 portraitTargetSize = new Vector2(30f, 50f); // width x height for slimmer portrait

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

        // 订阅武器系统事件
        if (WeaponSystemBridge.Instance != null)
        {
            // OnWeaponEquipped 是 UnityEvent<WeaponData>，需要带参数的回调签名。
            // 使用 lambda 忽略参数并调用无参的 UpdateWeaponStats。
            WeaponSystemBridge.Instance.OnWeaponEquipped.AddListener((WeaponData wd) => UpdateWeaponStats());
            WeaponSystemBridge.Instance.OnWeaponUnequipped.AddListener(UpdateWeaponStats);
            UpdateWeaponStats(); // 初始化状态栏
        }
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

    // Public helper: 打开背包并切换到消耗品标签
    public void OpenConsumablesTab()
    {
        if (inventoryRoot == null) return;
        if (!inventoryRoot.activeSelf) inventoryRoot.SetActive(true);
        SwitchTab(Tab.Consumables);
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
        // 按照 inventoryItems 的顺序聚合相同 ItemData（保留顺序，确保同类堆叠连续显示）
        List<KeyValuePair<ItemData, int>> entries = new List<KeyValuePair<ItemData, int>>();
        Dictionary<string, int> indexMap = new Dictionary<string, int>();
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            var it = inventoryItems[i];
            if (it == null) continue;
            if (activeTab == Tab.Equipment && !(it.itemType == ItemType.Weapon || it.itemType == ItemType.Gear)) continue;
            if (activeTab == Tab.Consumables && it.itemType != ItemType.Consumable) continue;

            string key = $"{it.itemType}:{it.itemName}";
            if (indexMap.TryGetValue(key, out int idx))
            {
                var kv = entries[idx];
                kv = new KeyValuePair<ItemData, int>(kv.Key, kv.Value + 1);
                entries[idx] = kv;
            }
            else
            {
                indexMap[key] = entries.Count;
                entries.Add(new KeyValuePair<ItemData, int>(it, 1));
            }
        }

        for (int i = 0; i < gridSlots.Count; i++)
        {
            if (i < entries.Count)
                gridSlots[i].SetItem(entries[i].Key, entries[i].Value);
            else
                gridSlots[i].SetItem(null, 0);
        }
    }

    // Helper: add item to inventory and refresh
    public void AddItemToInventory(ItemData item)
    {
        AddItemToInventory(item, 1);
    }

    public void AddItemToInventory(ItemData item, int count)
    {
        if (item == null || count <= 0) return;
        for (int i = 0; i < count; i++)
        {
            // 如果已有同类物品，则插入到最后一个同类物品后面以保持分组
            int lastIndex = -1;
            for (int j = inventoryItems.Count - 1; j >= 0; j--)
            {
                if (ItemsMatch(inventoryItems[j], item))
                {
                    lastIndex = j;
                    break;
                }
            }
            if (lastIndex >= 0)
                inventoryItems.Insert(lastIndex + 1, item);
            else
                inventoryItems.Add(item);
        }
        RefreshGrid();
    }

    public void RemoveItemFromInventory(ItemData item, int count)
    {
        if (item == null || count <= 0) return;
        int removed = 0;
        for (int i = inventoryItems.Count - 1; i >= 0 && removed < count; i--)
        {
            if (ItemsMatch(inventoryItems[i], item))
            {
                inventoryItems.RemoveAt(i);
                removed++;
            }
        }
        RefreshGrid();
    }

    /// <summary>
    /// 更新武器状态栏显示
    /// </summary>
    public void UpdateWeaponStats()
    {
        if (WeaponSystemBridge.Instance == null) return;

        // 更新攻击力状态栏
        if (attackBar != null)
        {
            int attackPower = WeaponSystemBridge.Instance.GetCurrentAttackPower();
            // 根据当前值自动计算一个合理的最大值，确保条的变化可见
            float attackMax = Mathf.Max(10f, attackPower * 1.5f,  (float)attackPower + 10f);
            attackBar.SetValue(attackPower, attackMax);
            attackBar.SetLabel($"攻击力: {attackPower}");
        }

        // 更新攻击范围状态栏
        if (rangeBar != null)
        {
            float attackRange = WeaponSystemBridge.Instance.GetCurrentAttackRange();
            float rangeMax = Mathf.Max(1f, attackRange * 1.2f, 3f);
            rangeBar.SetValue(attackRange, rangeMax);
            rangeBar.SetLabel($"攻击范围: {attackRange:F1}");
        }

        // 更新攻击冷却状态栏
        if (cooldownBar != null)
        {
            float attackCooldown = WeaponSystemBridge.Instance.GetCurrentAttackCooldown();
            // 以冷却上限为基准，冷却越低数值越高显示越满
            float cooldownMax = Mathf.Max(0.5f, attackCooldown * 2f, 2f);
            cooldownBar.SetValue(cooldownMax - attackCooldown, cooldownMax);
            cooldownBar.SetLabel($"攻击冷却: {attackCooldown:F1}s");
        }
        // 更新 HP/MP 显示（如果存在）
        if (hpBar != null)
        {
            hpBar.SetValue(currentHP, Mathf.Max(1f, maxHP));
            hpBar.SetLabel($"HP: {currentHP}/{maxHP}");
        }
        if (mpBar != null)
        {
            mpBar.SetValue(currentMP, Mathf.Max(1f, maxMP));
            mpBar.SetLabel($"MP: {currentMP}/{maxMP}");
        }
    }

    // 以下为 HP/MP 的公开接口，供技能/消耗品调用
    public void SetHP(float cur, float max)
    {
        maxHP = Mathf.Max(1f, max);
        currentHP = Mathf.Clamp(cur, 0f, maxHP);
        if (hpBar != null)
        {
            hpBar.SetValue(currentHP, maxHP);
            hpBar.SetLabel($"HP: {currentHP}/{maxHP}");
        }
    }

    public void SetMP(float cur, float max)
    {
        maxMP = Mathf.Max(1f, max);
        currentMP = Mathf.Clamp(cur, 0f, maxMP);
        if (mpBar != null)
        {
            mpBar.SetValue(currentMP, maxMP);
            mpBar.SetLabel($"MP: {currentMP}/{maxMP}");
        }
    }

    public void AddHP(float delta)
    {
        SetHP(currentHP + delta, maxHP);
    }

    public void AddMP(float delta)
    {
        SetMP(currentMP + delta, maxMP);
    }
}


