using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 背包设置向导
/// 自动创建和配置背包UI组件
/// </summary>
public class InventorySetupWizard : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoSetupOnStart = true;

    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupInventoryUI();
        }
    }

    [ContextMenu("Setup Inventory UI")]
    public void SetupInventoryUI()
    {
        Debug.Log("InventorySetupWizard: Starting inventory UI setup...");

        // 查找或创建Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("InventorySetupWizard: Created Canvas");
        }

        // 查找或创建EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("InventorySetupWizard: Created EventSystem");
        }

        // 查找InventoryUI组件
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI == null)
        {
            Debug.LogError("InventorySetupWizard: No InventoryUI component found in scene! Please add InventoryUI to a GameObject.");
            return;
        }

        // 创建inventoryRoot
        if (inventoryUI.inventoryRoot == null)
        {
            GameObject root = new GameObject("InventoryRoot");
            root.transform.SetParent(canvas.transform, false);
            RectTransform rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.1f);
            rt.anchorMax = new Vector2(0.9f, 0.9f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

            inventoryUI.inventoryRoot = root;
            Debug.Log("InventorySetupWizard: Created InventoryRoot");
        }

        // 创建装备槽
        CreateEquipSlots(inventoryUI);

        // 创建网格
        CreateGrid(inventoryUI);

        // 创建标签按钮
        CreateTabButtons(inventoryUI);

        // 创建状态栏
        CreateStatusBars(inventoryUI);

        Debug.Log("InventorySetupWizard: Inventory UI setup complete!");
        Debug.Log("Press I to toggle inventory, T to add test items (if InventoryTestHelper is present)");
    }

    void CreateEquipSlots(InventoryUI inventoryUI)
    {
        if (inventoryUI.weaponSlot != null && inventoryUI.gearSlot != null &&
            inventoryUI.consumableSlotA != null && inventoryUI.consumableSlotB != null)
        {
            Debug.Log("InventorySetupWizard: Equip slots already exist, skipping...");
            return;
        }

        Transform root = inventoryUI.inventoryRoot.transform;

        // Weapon slot
        if (inventoryUI.weaponSlot == null)
        {
            GameObject weaponObj = new GameObject("WeaponSlot");
            weaponObj.transform.SetParent(root, false);
            RectTransform rt = weaponObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-200, 100);
            rt.sizeDelta = new Vector2(60, 60);

            EquipSlot equipSlot = weaponObj.AddComponent<EquipSlot>();
            equipSlot.allowedType = ItemType.Weapon;
            equipSlot.inventoryUI = inventoryUI;
            inventoryUI.weaponSlot = equipSlot;

            Image img = weaponObj.AddComponent<Image>();
            img.color = new Color(0.5f, 0.5f, 0.5f);
        }

        // Gear slot
        if (inventoryUI.gearSlot == null)
        {
            GameObject gearObj = new GameObject("GearSlot");
            gearObj.transform.SetParent(root, false);
            RectTransform rt = gearObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-200, 20);
            rt.sizeDelta = new Vector2(60, 60);

            EquipSlot equipSlot = gearObj.AddComponent<EquipSlot>();
            equipSlot.allowedType = ItemType.Gear;
            equipSlot.inventoryUI = inventoryUI;
            inventoryUI.gearSlot = equipSlot;

            Image img = gearObj.AddComponent<Image>();
            img.color = new Color(0.5f, 0.5f, 0.5f);
        }

        // Consumable slots
        if (inventoryUI.consumableSlotA == null)
        {
            GameObject consAObj = new GameObject("ConsumableSlotA");
            consAObj.transform.SetParent(root, false);
            RectTransform rt = consAObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-200, -60);
            rt.sizeDelta = new Vector2(60, 60);

            EquipSlot equipSlot = consAObj.AddComponent<EquipSlot>();
            equipSlot.allowedType = ItemType.Consumable;
            equipSlot.inventoryUI = inventoryUI;
            inventoryUI.consumableSlotA = equipSlot;

            Image img = consAObj.AddComponent<Image>();
            img.color = new Color(0.5f, 0.5f, 0.5f);
        }

        if (inventoryUI.consumableSlotB == null)
        {
            GameObject consBObj = new GameObject("ConsumableSlotB");
            consBObj.transform.SetParent(root, false);
            RectTransform rt = consBObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-200, -140);
            rt.sizeDelta = new Vector2(60, 60);

            EquipSlot equipSlot = consBObj.AddComponent<EquipSlot>();
            equipSlot.allowedType = ItemType.Consumable;
            equipSlot.inventoryUI = inventoryUI;
            inventoryUI.consumableSlotB = equipSlot;

            Image img = consBObj.AddComponent<Image>();
            img.color = new Color(0.5f, 0.5f, 0.5f);
        }

        Debug.Log("InventorySetupWizard: Created equip slots");
    }

    void CreateGrid(InventoryUI inventoryUI)
    {
        if (inventoryUI.gridParent != null)
        {
            Debug.Log("InventorySetupWizard: Grid already exists, skipping...");
            return;
        }

        Transform root = inventoryUI.inventoryRoot.transform;

        GameObject gridObj = new GameObject("GridParent");
        gridObj.transform.SetParent(root, false);
        RectTransform rt = gridObj.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(100, -50);
        rt.sizeDelta = new Vector2(400, 300);

        // 创建GridLayoutGroup
        GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(50, 50);
        grid.spacing = new Vector2(5, 5);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;

        inventoryUI.gridParent = gridObj.transform;

        // 创建slot prefab
        if (inventoryUI.slotPrefab == null)
        {
            inventoryUI.slotPrefab = CreateSlotPrefab();
        }

        Debug.Log("InventorySetupWizard: Created inventory grid");
    }

    void CreateTabButtons(InventoryUI inventoryUI)
    {
        if (inventoryUI.tabEquipButton != null && inventoryUI.tabConsumableButton != null)
        {
            Debug.Log("InventorySetupWizard: Tab buttons already exist, skipping...");
            return;
        }

        Transform root = inventoryUI.inventoryRoot.transform;

        // Equipment tab
        if (inventoryUI.tabEquipButton == null)
        {
            GameObject equipBtnObj = new GameObject("TabEquipment");
            equipBtnObj.transform.SetParent(root, false);
            RectTransform rt = equipBtnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-200, 200);
            rt.sizeDelta = new Vector2(100, 30);

            Image img = equipBtnObj.AddComponent<Image>();
            img.color = Color.white;

            Button btn = equipBtnObj.AddComponent<Button>();
            inventoryUI.tabEquipButton = btn;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(equipBtnObj.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.text = "装备";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
        }

        // Consumable tab
        if (inventoryUI.tabConsumableButton == null)
        {
            GameObject consBtnObj = new GameObject("TabConsumable");
            consBtnObj.transform.SetParent(root, false);
            RectTransform rt = consBtnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-50, 200);
            rt.sizeDelta = new Vector2(100, 30);

            Image img = consBtnObj.AddComponent<Image>();
            img.color = new Color(0.9f, 0.9f, 0.9f);

            Button btn = consBtnObj.AddComponent<Button>();
            inventoryUI.tabConsumableButton = btn;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(consBtnObj.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.text = "消耗品";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
        }

        Debug.Log("InventorySetupWizard: Created tab buttons");
    }

    void CreateStatusBars(InventoryUI inventoryUI)
    {
        if (inventoryUI.hpBar != null && inventoryUI.mpBar != null &&
            inventoryUI.attackBar != null && inventoryUI.rangeBar != null && inventoryUI.cooldownBar != null)
        {
            Debug.Log("InventorySetupWizard: Status bars already exist, skipping...");
            return;
        }

        Transform root = inventoryUI.inventoryRoot.transform;

        // HP Bar
        if (inventoryUI.hpBar == null)
        {
            GameObject hpObj = new GameObject("HPBar");
            hpObj.transform.SetParent(root, false);
            RectTransform rt = hpObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(200, 150);
            rt.sizeDelta = new Vector2(200, 20);

            StatusBar bar = hpObj.AddComponent<StatusBar>();
            bar.labelText = hpObj.AddComponent<Text>();
            inventoryUI.hpBar = bar;
        }

        // MP Bar
        if (inventoryUI.mpBar == null)
        {
            GameObject mpObj = new GameObject("MPBar");
            mpObj.transform.SetParent(root, false);
            RectTransform rt = mpObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(200, 120);
            rt.sizeDelta = new Vector2(200, 20);

            StatusBar bar = mpObj.AddComponent<StatusBar>();
            bar.labelText = mpObj.AddComponent<Text>();
            inventoryUI.mpBar = bar;
        }

        // Attack Bar
        if (inventoryUI.attackBar == null)
        {
            GameObject atkObj = new GameObject("AttackBar");
            atkObj.transform.SetParent(root, false);
            RectTransform rt = atkObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(200, 90);
            rt.sizeDelta = new Vector2(200, 20);

            StatusBar bar = atkObj.AddComponent<StatusBar>();
            bar.labelText = atkObj.AddComponent<Text>();
            inventoryUI.attackBar = bar;
        }

        // Range Bar
        if (inventoryUI.rangeBar == null)
        {
            GameObject rangeObj = new GameObject("RangeBar");
            rangeObj.transform.SetParent(root, false);
            RectTransform rt = rangeObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(200, 60);
            rt.sizeDelta = new Vector2(200, 20);

            StatusBar bar = rangeObj.AddComponent<StatusBar>();
            bar.labelText = rangeObj.AddComponent<Text>();
            inventoryUI.rangeBar = bar;
        }

        // Cooldown Bar
        if (inventoryUI.cooldownBar == null)
        {
            GameObject cdObj = new GameObject("CooldownBar");
            cdObj.transform.SetParent(root, false);
            RectTransform rt = cdObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(200, 30);
            rt.sizeDelta = new Vector2(200, 20);

            StatusBar bar = cdObj.AddComponent<StatusBar>();
            bar.labelText = cdObj.AddComponent<Text>();
            inventoryUI.cooldownBar = bar;
        }

        Debug.Log("InventorySetupWizard: Created status bars");
    }

    GameObject CreateSlotPrefab()
    {
        GameObject prefab = new GameObject("SlotPrefab");

        // Background image
        Image bg = prefab.AddComponent<Image>();
        bg.color = new Color(0.3f, 0.3f, 0.3f);

        // Button
        Button button = prefab.AddComponent<Button>();

        // InventorySlot component
        InventorySlot slot = prefab.AddComponent<InventorySlot>();
        slot.button = button;

        // Icon child
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(prefab.transform, false);
        Image icon = iconObj.AddComponent<Image>();
        icon.color = Color.white;
        RectTransform iconRt = iconObj.GetComponent<RectTransform>();
        iconRt.anchorMin = Vector2.zero;
        iconRt.anchorMax = Vector2.one;
        iconRt.offsetMin = new Vector2(2, 2);
        iconRt.offsetMax = new Vector2(-2, -2);
        slot.icon = icon;

        // Count text
        GameObject countObj = new GameObject("Count");
        countObj.transform.SetParent(prefab.transform, false);
        Text countText = countObj.AddComponent<Text>();
        countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        countText.color = Color.white;
        countText.alignment = TextAnchor.LowerRight;
        RectTransform countRt = countObj.GetComponent<RectTransform>();
        countRt.anchorMin = new Vector2(1, 0);
        countRt.anchorMax = new Vector2(1, 0);
        countRt.pivot = new Vector2(1, 0);
        countRt.anchoredPosition = new Vector2(-2, 2);
        countRt.sizeDelta = new Vector2(30, 20);
        slot.countText = countText;

        return prefab;
    }
}
