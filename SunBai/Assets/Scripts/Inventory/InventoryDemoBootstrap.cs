using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Demo bootstrap to create a runnable scene UI at runtime for testing the inventory system.
// Attach this script to an empty GameObject in a new Scene, press Play to see the UI and test with I key.
public class InventoryDemoBootstrap : MonoBehaviour
{
    GameObject inventoryRoot;
    InventoryUI inventoryUI;

    void Awake()
    {
        // Ensure there's a Camera (Game view shows "No cameras rendering" if none present)
        if (Camera.main == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            Camera cam = camGO.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }

        // Ensure there's a Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
            canvas = CreateCanvas("Canvas");

        // Ensure there's an EventSystem for UI interactions
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Create InventoryRoot panel
        inventoryRoot = CreateUIObject("InventoryRoot", canvas.transform);
        RectTransform invRt = inventoryRoot.AddComponent<RectTransform>();
        Image invImage = inventoryRoot.AddComponent<Image>();
        invImage.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        invRt.anchorMin = new Vector2(0f, 0f);
        invRt.anchorMax = new Vector2(1f, 1f);
        invRt.offsetMin = new Vector2(10f, 10f);
        invRt.offsetMax = new Vector2(-10f, -10f);

        // Add InventoryUI component
        inventoryUI = inventoryRoot.AddComponent<InventoryUI>();
        inventoryUI.inventoryRoot = inventoryRoot;

        // Left area - portrait and equip grid
        GameObject leftArea = CreateUIObject("LeftArea", inventoryRoot.transform);
        RectTransform leftRt = leftArea.AddComponent<RectTransform>();
        leftRt.anchorMin = new Vector2(0f, 0f);
        leftRt.anchorMax = new Vector2(0.55f, 1f);
        leftRt.offsetMin = new Vector2(10f, 10f);
        leftRt.offsetMax = new Vector2(-10f, -10f);

        // Portrait image (top-left) — make taller and narrower
        GameObject portraitGO = CreateUIObject("Portrait", leftArea.transform);
        RectTransform portraitRt = portraitGO.AddComponent<RectTransform>();
        // narrower (x up to 0.45) and slightly taller (start at 0.6)
        portraitRt.anchorMin = new Vector2(0f, 0.6f);
        portraitRt.anchorMax = new Vector2(0.45f, 1f);
        portraitRt.offsetMin = new Vector2(10f, -10f);
        portraitRt.offsetMax = new Vector2(-10f, -10f);
        Image portraitImage = portraitGO.AddComponent<Image>();
        portraitImage.color = Color.gray;
        inventoryUI.portraitImage = portraitImage;

        // EquipGrid (2x2)
        GameObject equipGrid = CreateUIObject("EquipGrid", leftArea.transform);
        RectTransform equipRt = equipGrid.AddComponent<RectTransform>();
        equipRt.anchorMin = new Vector2(0.6f, 0.55f);
        equipRt.anchorMax = new Vector2(1f, 1f);
        equipRt.offsetMin = new Vector2(10f, -10f);
        equipRt.offsetMax = new Vector2(-10f, -10f);
        GridLayoutGroup equipLayout = equipGrid.AddComponent<GridLayoutGroup>();
        equipLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        equipLayout.constraintCount = 2;
        equipLayout.cellSize = new Vector2(96f, 96f);
        equipLayout.spacing = new Vector2(8f, 8f);

        // Create 4 EquipSlot objects and assign to inventoryUI
        inventoryUI.weaponSlot = CreateEquipSlot(equipGrid.transform, ItemType.Weapon, "WeaponSlot");
        inventoryUI.gearSlot = CreateEquipSlot(equipGrid.transform, ItemType.Gear, "GearSlot");
        inventoryUI.consumableSlotA = CreateEquipSlot(equipGrid.transform, ItemType.Consumable, "ConsumableA");
        inventoryUI.consumableSlotB = CreateEquipSlot(equipGrid.transform, ItemType.Consumable, "ConsumableB");

        // Left bottom - status bars
        GameObject statusArea = CreateUIObject("StatusArea", leftArea.transform);
        RectTransform statusRt = statusArea.AddComponent<RectTransform>();
        statusRt.anchorMin = new Vector2(0f, 0f);
        // moved down to free more vertical space for taller portrait
        statusRt.anchorMax = new Vector2(1f, 0.45f);
        statusRt.offsetMin = new Vector2(10f, 10f);
        statusRt.offsetMax = new Vector2(-10f, -10f);
        VerticalLayoutGroup statusLayout = statusArea.AddComponent<VerticalLayoutGroup>();
        statusLayout.spacing = 8f;
        statusLayout.childControlHeight = false;
        statusLayout.childForceExpandHeight = false;

        // HP, MP, EXP bars
        StatusBar hp = CreateStatusBar(statusArea.transform, "HP", Color.red);
        StatusBar mp = CreateStatusBar(statusArea.transform, "MP", Color.cyan);
        StatusBar atk = CreateStatusBar(statusArea.transform, "ATK", new Color(1f, 0.85f, 0f));

        hp.SetValue(80f, 100f);
        mp.SetValue(40f, 100f);
        atk.SetValue(30f, 100f);
        // 绑定 hp/mp 到 InventoryUI，以便后续通过接口修改
        inventoryUI.hpBar = hp;
        inventoryUI.mpBar = mp;

        // 武器状态栏（添加到 InventoryUI）
        inventoryUI.attackBar = CreateStatusBar(statusArea.transform, "攻击力", Color.red);
        inventoryUI.rangeBar = CreateStatusBar(statusArea.transform, "范围", Color.blue);
        inventoryUI.cooldownBar = CreateStatusBar(statusArea.transform, "冷却", Color.green);

        // Right area - tabs + grid
        GameObject rightArea = CreateUIObject("RightArea", inventoryRoot.transform);
        RectTransform rightRt = rightArea.AddComponent<RectTransform>();
        rightRt.anchorMin = new Vector2(0.55f, 0f);
        rightRt.anchorMax = new Vector2(1f, 1f);
        rightRt.offsetMin = new Vector2(10f, 10f);
        rightRt.offsetMax = new Vector2(-10f, -10f);

        // Tabs row
        GameObject tabsRow = CreateUIObject("TabsRow", rightArea.transform);
        RectTransform tabsRt = tabsRow.AddComponent<RectTransform>();
        tabsRt.anchorMin = new Vector2(0f, 0.9f);
        tabsRt.anchorMax = new Vector2(1f, 1f);
        tabsRt.offsetMin = new Vector2(10f, -10f);
        tabsRt.offsetMax = new Vector2(-10f, -10f);
        HorizontalLayoutGroup tabsLayout = tabsRow.AddComponent<HorizontalLayoutGroup>();
        tabsLayout.spacing = 8f;

        Button tabEquip = CreateButton(tabsRow.transform, "EquipTab", "Equip");
        Button tabConsumable = CreateButton(tabsRow.transform, "ConsumableTab", "Items");
        inventoryUI.tabEquipButton = tabEquip;
        inventoryUI.tabConsumableButton = tabConsumable;

        // Grid area
        GameObject gridArea = CreateUIObject("GridArea", rightArea.transform);
        RectTransform gridRt = gridArea.AddComponent<RectTransform>();
        gridRt.anchorMin = new Vector2(0f, 0f);
        // reduce top anchor to leave room for tabs
        gridRt.anchorMax = new Vector2(1f, 0.85f);
        gridRt.offsetMin = new Vector2(10f, 10f);
        gridRt.offsetMax = new Vector2(-10f, -10f);
        GridLayoutGroup gridLayout = gridArea.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(64f, 64f);
        gridLayout.spacing = new Vector2(10f, 10f);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 5;
        inventoryUI.gridParent = gridArea.transform;

        // Try to load a saved slot prefab from Resources/Inventory/Prefabs first; fall back to runtime creation
        GameObject loadedSlotPrefab = Resources.Load<GameObject>("Inventory/Prefabs/SlotPrefab");
        if (loadedSlotPrefab != null)
        {
            inventoryUI.slotPrefab = loadedSlotPrefab;
        }
        else
        {
            GameObject slotPrefab = CreateSlotPrefab();
            inventoryUI.slotPrefab = slotPrefab;
        }
        inventoryUI.gridSlotCount = 20;
        inventoryUI.emptySlotSprite = GenerateColoredSprite(Color.grey);

        // Populate runtime items only if no persistent ItemData assets exist in Resources
        var persistent = Resources.LoadAll<ItemData>("Inventory/Items");
        if (persistent == null || persistent.Length == 0)
        {
            var sword = ScriptableObject.CreateInstance<ItemData>();
            sword.itemName = "Sword";
            sword.itemType = ItemType.Weapon;
            sword.icon = GenerateColoredSprite(new Color(0.2f, 0.6f, 1f));

            var armor = ScriptableObject.CreateInstance<ItemData>();
            armor.itemName = "Armor";
            armor.itemType = ItemType.Gear;
            armor.icon = GenerateColoredSprite(new Color(1f, 0.6f, 0.2f));

            // Add several items to inventory to test pagination
            inventoryUI.AddItemToInventory(sword);
            inventoryUI.AddItemToInventory(armor);
            for (int i = 0; i < 8; i++)
            {
                var p = ScriptableObject.CreateInstance<ItemData>();
                p.itemName = "Potion " + (i + 1);
                p.itemType = ItemType.Consumable;
                p.icon = GenerateColoredSprite(new Color(0.6f, 0.6f, 0.6f));
                inventoryUI.AddItemToInventory(p);
            }
        }

        // Keep inventory hidden initially
        inventoryRoot.SetActive(false);

        // 初始化武器系统桥接器
        GameObject bridgeGO = new GameObject("WeaponSystemBridge");
        WeaponSystemBridge weaponBridge = bridgeGO.AddComponent<WeaponSystemBridge>();
        // 如果有玩家对象，在这里设置引用
        // weaponBridge.playerObject = playerGameObject;

        // 添加武器集成测试脚本
        bridgeGO.AddComponent<TestWeaponIntegration>();
        // 生成测试拾取物（运行时放在世界前方，便于调试）
        SpawnTestPickups();
    }

    void SpawnTestPickups()
    {
        // 尝试加载 ItemData 资源（Resources/Inventory/Items 下）
        // 优先加载专用的 RedPotion / BluePotion，如果不存在再回退到旧的 Potion 资源
        ItemData redPotion = Resources.Load<ItemData>("Inventory/Items/RedPotion");
        ItemData bluePotion = Resources.Load<ItemData>("Inventory/Items/BluePotion");
        ItemData genericPotion = Resources.Load<ItemData>("Inventory/Items/Potion");
        ItemData sword = Resources.Load<ItemData>("Inventory/Items/Sword");

        ItemData potion = redPotion != null ? redPotion : (bluePotion != null ? bluePotion : genericPotion);

        // 如果没有找到资源，记录日志并返回
        if (potion == null && sword == null)
        {
            Debug.LogWarning("SpawnTestPickups: 未找到 Potion 或 Sword 的 ItemData（Resources/Inventory/Items）");
            return;
        }

        // 放置位置（相对于世界原点，便于主角走动测试）
        Vector3 basePos = new Vector3(2f, 0.5f, 0f);
        // 第一个为小红瓶（若存在），第二个为小蓝瓶（若存在），否则都使用 generic potion
        CreatePickupAt(redPotion != null ? redPotion : potion, 2, basePos + Vector3.right * 0f);
        CreatePickupAt(bluePotion != null ? bluePotion : potion, 1, basePos + Vector3.right * 1.5f);
        CreatePickupAt(sword, 1, basePos + Vector3.right * 3f);
    }

    void CreatePickupAt(ItemData item, int amount, Vector3 pos)
    {
        if (item == null) return;
        GameObject go = new GameObject("Pickup_" + item.itemName);
        go.transform.position = pos;
        var pickup = go.AddComponent<ItemPickup>();
        pickup.item = item;
        pickup.amount = amount;
        // 添加触发器碰撞体以便 PlayerCollector 检测（运行时创建）
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        // 可选：添加一个可见的 SpriteRenderer 作为示意（使用 item.icon 如果存在）
        if (item.icon != null)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = item.icon;
            sr.sortingOrder = 10;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && inventoryRoot != null)
        {
            bool show = !inventoryRoot.activeSelf;
            inventoryRoot.SetActive(show);
            if (show && inventoryUI != null)
                inventoryUI.RefreshGrid();
        }
    }

    // -- Helper builders --
    Canvas CreateCanvas(string name)
    {
        GameObject go = new GameObject(name);
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();
        return c;
    }

    GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }

    EquipSlot CreateEquipSlot(Transform parent, ItemType type, string name)
    {
        GameObject go = CreateUIObject(name, parent);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.5f, 0.5f, 0.5f);
        EquipSlot slot = go.AddComponent<EquipSlot>();
        slot.allowedType = type;
        slot.icon = img;
        slot.emptySprite = GenerateColoredSprite(Color.grey);
        // assign the inventoryUI reference so the slot can remove items from inventory list on drop
        slot.inventoryUI = inventoryUI;
        // 创建名称文本（位于槽下方）
        GameObject nameGO = CreateUIObject("Name", go.transform);
        RectTransform nameRt = nameGO.AddComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0f, 0f);
        nameRt.anchorMax = new Vector2(1f, 0f);
        nameRt.pivot = new Vector2(0.5f, 1f);
        nameRt.anchoredPosition = new Vector2(0f, -18f);
        nameRt.sizeDelta = new Vector2(0f, 18f);
        Text nameText = nameGO.AddComponent<Text>();
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.alignment = TextAnchor.UpperCenter;
        nameText.color = Color.white;
        nameText.text = "";
        slot.nameText = nameText;
        return slot;
    }

    StatusBar CreateStatusBar(Transform parent, string label, Color fillColor)
    {
        // Slightly narrower and shorter bars to make portrait taller
        GameObject go = CreateUIObject(label + "Bar", parent);
        RectTransform goRt = go.AddComponent<RectTransform>();
        goRt.sizeDelta = new Vector2(320f, 24f);
        HorizontalLayoutGroup hl = go.AddComponent<HorizontalLayoutGroup>();
        hl.spacing = 6f;
        hl.childForceExpandWidth = false;
        hl.childControlWidth = false;

        GameObject labelGO = CreateUIObject("Label", go.transform);
        RectTransform labelRt = labelGO.AddComponent<RectTransform>();
        labelRt.sizeDelta = new Vector2(64f, 20f);
        Text t = labelGO.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = label;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleLeft;

        RectTransform fillRt = CreateUIObject("Fill", go.transform).AddComponent<RectTransform>();
        fillRt.sizeDelta = new Vector2(240f, 20f);
        Image fill = fillRt.gameObject.AddComponent<Image>();
        fill.color = fillColor;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;

        // Add LayoutElement so HorizontalLayoutGroup respects sizes
        var leLabel = labelGO.AddComponent<LayoutElement>();
        leLabel.preferredWidth = 64f;
        var leFill = fillRt.gameObject.AddComponent<LayoutElement>();
        leFill.preferredWidth = 240f;

        // 值文本（右侧）
        GameObject valueGO = CreateUIObject("Value", go.transform);
        RectTransform valueRt = valueGO.AddComponent<RectTransform>();
        valueRt.sizeDelta = new Vector2(80f, 20f);
        Text valueText = valueGO.AddComponent<Text>();
        valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        valueText.color = Color.white;
        valueText.alignment = TextAnchor.MiddleRight;
        var leValue = valueGO.AddComponent<LayoutElement>();
        leValue.preferredWidth = 80f;

        StatusBar sb = go.AddComponent<StatusBar>();
        sb.fillImage = fill;
        sb.labelText = t;
        sb.valueText = valueText;
        return sb;
    }

    Button CreateButton(Transform parent, string name, string text)
    {
        GameObject go = CreateUIObject(name, parent);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.8f, 0.8f, 0.8f);
        Button btn = go.AddComponent<Button>();
        GameObject txt = CreateUIObject("Text", go.transform);
        Text t = txt.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = text;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.black;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(90f, 28f);
        return btn;
    }

    GameObject CreateSlotPrefab()
    {
        GameObject go = new GameObject("SlotPrefab");
        RectTransform rt = go.AddComponent<RectTransform>();
        Image bg = go.AddComponent<Image>();
        bg.color = new Color(0.3f, 0.3f, 0.3f);
        Button btn = go.AddComponent<Button>();
        // Icon child
        GameObject iconGO = CreateUIObject("Icon", go.transform);
        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.color = Color.white;
        InventorySlot slot = go.AddComponent<InventorySlot>();
        slot.icon = iconImg;
        slot.button = btn;
        // Count text (bottom-right)
        GameObject countGO = CreateUIObject("Count", go.transform);
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
        return go;
    }

    // Generate a simple 32x32 colored sprite
    Sprite GenerateColoredSprite(Color c)
    {
        Texture2D tex = new Texture2D(32, 32);
        Color[] cols = new Color[32 * 32];
        for (int i = 0; i < cols.Length; i++) cols[i] = c;
        tex.SetPixels(cols);
        tex.Apply();
        Rect rect = new Rect(0, 0, tex.width, tex.height);
        return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f));
    }
}


