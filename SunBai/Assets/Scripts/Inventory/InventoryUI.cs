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
        Debug.Log("InventoryUI: Starting initialization...");

        // 检查inventoryRoot：尝试从 Resources 预制体或场景中恢复，保证在大多数情况下能自动修复
        if (inventoryRoot == null)
        {
            Debug.LogWarning("InventoryUI: inventoryRoot is not assigned. Trying to recover from Resources prefab or scene...");
            // 尝试从 Resources 中加载预制体（路径：Resources/Inventory/Prefabs/InventoryRoot.prefab）
            GameObject prefab = Resources.Load<GameObject>("Inventory/Prefabs/InventoryRoot");
            if (prefab != null)
            {
                var inst = Instantiate(prefab);
                inst.name = "InventoryRoot_Runtime";
                inventoryRoot = inst;
                Debug.Log("InventoryUI: inventoryRoot recovered by instantiating Resources/Inventory/Prefabs/InventoryRoot");
            }
            else
            {
                // 尝试按名称在场景中查找
                var found = GameObject.Find("InventoryRoot");
                if (found != null)
                {
                    inventoryRoot = found;
                    Debug.Log("InventoryUI: inventoryRoot recovered from scene GameObject named 'InventoryRoot'");
                }
                else
                {
                    Debug.LogError("InventoryUI: inventoryRoot is not assigned and no prefab/scene fallback found. Aborting initialization.");
                    return;
                }
            }
        }

        inventoryRoot.SetActive(false);
        Debug.Log("InventoryUI: inventoryRoot found and hidden");

        CreateGridSlots();
        Debug.Log($"InventoryUI: Created {gridSlots.Count} grid slots");

        // 设置标签按钮
        if (tabEquipButton != null)
        {
            tabEquipButton.onClick.AddListener(() => SwitchTab(Tab.Equipment));
            Debug.Log("InventoryUI: Equipment tab button configured");
        }
        else
        {
            Debug.LogWarning("InventoryUI: tabEquipButton is not assigned");
        }

        if (tabConsumableButton != null)
        {
            tabConsumableButton.onClick.AddListener(() => SwitchTab(Tab.Consumables));
            Debug.Log("InventoryUI: Consumable tab button configured");
        }
        else
        {
            Debug.LogWarning("InventoryUI: tabConsumableButton is not assigned");
        }

        // 尝试加载物品数据
        if ((inventoryItems == null || inventoryItems.Count == 0))
        {
            var loaded = Resources.LoadAll<ItemData>("Inventory/Items");
            if (loaded != null && loaded.Length > 0)
            {
                inventoryItems = new List<ItemData>(loaded);
                Debug.Log($"InventoryUI: Loaded {loaded.Length} items from Resources");
            }
            else
            {
                inventoryItems = new List<ItemData>();
                Debug.Log("InventoryUI: No items loaded from Resources");
            }
        }
        else
        {
            Debug.Log($"InventoryUI: Using {inventoryItems.Count} pre-assigned items");
        }

        RefreshGrid();
        UpdateTabVisuals();
        ApplyLayoutTweaks();

        // 订阅武器系统事件
        if (WeaponSystemBridge.Instance != null)
        {
            WeaponSystemBridge.Instance.OnWeaponEquipped.AddListener((WeaponData wd) => UpdateWeaponStats());
            WeaponSystemBridge.Instance.OnWeaponUnequipped.AddListener(UpdateWeaponStats);
            UpdateWeaponStats();
            Debug.Log("InventoryUI: Weapon system events subscribed");
        }
        else
        {
            Debug.LogWarning("InventoryUI: WeaponSystemBridge.Instance is null, weapon stats won't update");
        }

        Debug.Log("InventoryUI: Initialization complete. Press I to toggle inventory.");
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
            Debug.Log("InventoryUI: I key pressed, toggling inventory");
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
        if (inventoryRoot == null)
        {
            Debug.LogError("InventoryUI: inventoryRoot is null! Make sure to assign the inventory root GameObject in the Inspector.");
            return;
        }
        bool show = !inventoryRoot.activeSelf;
        inventoryRoot.SetActive(show);
        Debug.Log($"InventoryUI: Inventory {(show ? "opened" : "closed")}, activeTab: {activeTab}");
        if (show)
        {
            // default to Equipment tab when opened
            SwitchTab(Tab.Equipment);
        }
    }

    void CreateGridSlots()
    {
        // Ensure gridParent exists; if not, try to create one under inventoryRoot
        if (gridParent == null)
        {
            if (inventoryRoot != null)
            {
                GameObject gp = new GameObject("GridParent");
                gp.transform.SetParent(inventoryRoot.transform, false);
                gridParent = gp.transform;
                Debug.Log("InventoryUI: Created runtime GridParent under InventoryRoot");
            }
            else
            {
                Debug.LogWarning("InventoryUI: gridParent is null and inventoryRoot is not available");
                return;
            }
        }

        // Ensure slotPrefab exists; if not, create a simple runtime slot prefab
        if (slotPrefab == null)
        {
            slotPrefab = CreateRuntimeSlotPrefab();
            Debug.Log("InventoryUI: Created runtime slotPrefab");
        }

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

    // Create a minimal runtime slot prefab when none provided by demo/bootstrap
    private GameObject CreateRuntimeSlotPrefab()
    {
        GameObject go = new GameObject("SlotPrefab_Runtime");
        RectTransform rt = go.AddComponent<RectTransform>();
        Image bg = go.AddComponent<Image>();
        bg.color = new Color(0.3f, 0.3f, 0.3f);
        Button btn = go.AddComponent<Button>();

        // Icon child
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(go.transform, false);
        RectTransform iconRt = iconGO.AddComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0f, 0f);
        iconRt.anchorMax = new Vector2(1f, 1f);
        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.color = Color.white;

        InventorySlot slot = go.AddComponent<InventorySlot>();
        slot.icon = iconImg;
        slot.button = btn;

        // Count text (bottom-right)
        GameObject countGO = new GameObject("Count");
        countGO.transform.SetParent(go.transform, false);
        RectTransform countRt = countGO.AddComponent<RectTransform>();
        countRt.anchorMin = new Vector2(1f, 0f);
        countRt.anchorMax = new Vector2(1f, 0f);
        countRt.pivot = new Vector2(1f, 0f);
        countRt.anchoredPosition = new Vector2(-6f, 6f);
        countRt.sizeDelta = new Vector2(40f, 20f);
        Text countText = countGO.AddComponent<Text>();
        countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        countText.color = Color.white;
        countText.alignment = TextAnchor.LowerRight;
        countText.text = "";
        slot.countText = countText;

        // Make prefab inactive template
        go.SetActive(true);
        return go;
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
        Debug.Log($"InventoryUI: RefreshGrid activeTab={activeTab} inventoryItems.Count={inventoryItems?.Count ?? 0}");
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
        Debug.Log($"InventoryUI: RefreshGrid entries.Count={entries.Count} gridSlots.Count={gridSlots.Count}");
    }

    // Helper: add item to inventory and refresh
    public void AddItemToInventory(ItemData item)
    {
        AddItemToInventory(item, 1);
    }

    public void AddItemToInventory(ItemData item, int count)
    {
        if (item == null || count <= 0) return;
        Debug.Log($"InventoryUI: AddItemToInventory {item.itemName} x{count}");

        // 查找是否已有相同物品
        bool found = false;
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (ItemsMatch(inventoryItems[i], item))
            {
                // 由于我们现在使用计数显示，这里不需要实际添加多个物品
                // RefreshGrid 会处理计数聚合
                found = true;
                break;
            }
        }

        if (!found)
        {
            // 如果是新物品，添加到列表中
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


