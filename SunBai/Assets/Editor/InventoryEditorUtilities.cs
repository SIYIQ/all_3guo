#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public static class InventoryEditorUtilities
{
    [MenuItem("Inventory/Create Slot Prefab and Sample Items")]
    public static void CreateSlotPrefabAndSamples()
    {
        // Ensure directories
        string prefabsDir = "Assets/Inventory/Prefabs";
        if (!Directory.Exists(prefabsDir)) Directory.CreateDirectory(prefabsDir);

        string resourcesItemsDir = "Assets/Resources/Inventory/Items";
        if (!Directory.Exists(resourcesItemsDir)) Directory.CreateDirectory(resourcesItemsDir);

        // Create a basic SlotPrefab (a Button with child Icon) in scene and save as prefab
        GameObject slot = new GameObject("SlotPrefab_Temp");
        var rt = slot.AddComponent<RectTransform>();
        var img = slot.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f);
        var btn = slot.AddComponent<UnityEngine.UI.Button>();
        GameObject icon = new GameObject("Icon");
        icon.transform.SetParent(slot.transform, false);
        var iconImg = icon.AddComponent<UnityEngine.UI.Image>();
        iconImg.color = Color.white;
        slot.AddComponent<InventorySlot>().icon = iconImg;
        slot.GetComponent<InventorySlot>().button = btn;

        string prefabPath = Path.Combine(prefabsDir, "SlotPrefab.prefab");
        PrefabUtility.SaveAsPrefabAsset(slot, prefabPath);

        // Also save a copy into Resources for runtime loading convenience
        string resourcesPrefabsDir = "Assets/Resources/Inventory/Prefabs";
        if (!Directory.Exists(resourcesPrefabsDir)) Directory.CreateDirectory(resourcesPrefabsDir);
        PrefabUtility.SaveAsPrefabAsset(slot, Path.Combine(resourcesPrefabsDir, "SlotPrefab.prefab"));

        GameObject.DestroyImmediate(slot);

        // Create several sample ItemData assets with generated colored textures
        CreateSampleItem("Sword", ItemType.Weapon, new Color(0.2f, 0.6f, 1f), resourcesItemsDir);
        CreateSampleItem("Armor", ItemType.Gear, new Color(1f, 0.6f, 0.2f), resourcesItemsDir);
        CreateSampleItem("Potion", ItemType.Consumable, new Color(0.2f, 1f, 0.4f), resourcesItemsDir);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created SlotPrefab and sample ItemData assets.");
    }

    static void CreateSampleItem(string name, ItemType type, Color color, string folder)
    {
        // Create texture
        Texture2D tex = new Texture2D(32, 32);
        Color[] cols = new Color[32 * 32];
        for (int i = 0; i < cols.Length; i++) cols[i] = color;
        tex.SetPixels(cols);
        tex.Apply();

        string texPath = Path.Combine(folder, name + "_tex.png");
        File.WriteAllBytes(texPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath);
        TextureImporter ti = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.SaveAndReimport();
        }
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);

        // Create ItemData asset
        ItemData item = ScriptableObject.CreateInstance<ItemData>();
        item.itemName = name;
        item.itemType = type;
        item.icon = sprite;
        string assetPath = Path.Combine(folder, name + ".asset");
        AssetDatabase.CreateAsset(item, assetPath);
    }

    [MenuItem("Inventory/Bind Sprites and Create InventoryRoot Prefab")]
    public static void CreateInventoryRootPrefabFromSprites()
    {
        string spritesDir = "Assets/Inventory/Sprites";
        if (!Directory.Exists(spritesDir))
        {
            Debug.LogError("Sprites folder not found: " + spritesDir);
            return;
        }

        // Load all sprite assets in the sprites directory
        var spriteFiles = Directory.GetFiles(spritesDir);
        var sprites = new System.Collections.Generic.Dictionary<string, Sprite>();
        foreach (var f in spriteFiles)
        {
            if (f.EndsWith(".meta")) continue;
            string assetPath = f.Replace("\\", "/");
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (s != null)
            {
                string key = Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
                sprites[key] = s;
            }
        }

        // Ensure prefabs dir
        string prefabsDir = "Assets/Inventory/Prefabs";
        if (!Directory.Exists(prefabsDir)) Directory.CreateDirectory(prefabsDir);

        // Try to load existing SlotPrefab asset
        GameObject slotPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Inventory/Prefabs/SlotPrefab.prefab");
        if (slotPrefabAsset == null)
        {
            // try Resources copy
            slotPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Inventory/Prefabs/SlotPrefab.prefab");
        }

        // Build InventoryRoot GameObject
        GameObject root = new GameObject("InventoryRoot_Prefab");
        var rt = root.AddComponent<RectTransform>();
        var img = root.AddComponent<UnityEngine.UI.Image>();
        if (sprites.TryGetValue("ui_panel_bg", out Sprite bg))
            img.sprite = bg;
        else
            img.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        // Add InventoryUI component
        InventoryUI invUI = root.AddComponent<InventoryUI>();

        // Left area
        GameObject left = new GameObject("LeftArea");
        left.transform.SetParent(root.transform, false);
        var leftRt = left.AddComponent<RectTransform>();
        leftRt.anchorMin = new Vector2(0f, 0f);
        leftRt.anchorMax = new Vector2(0.55f, 1f);

        // Portrait
        GameObject portrait = new GameObject("Portrait");
        portrait.transform.SetParent(left.transform, false);
        var portraitImg = portrait.AddComponent<UnityEngine.UI.Image>();
        if (sprites.TryGetValue("portrait", out Sprite p)) portraitImg.sprite = p;
        invUI.portraitImage = portraitImg;

        // Equip grid (2x2)
        GameObject equipGrid = new GameObject("EquipGrid");
        equipGrid.transform.SetParent(left.transform, false);
        var gridLayout = equipGrid.AddComponent<UnityEngine.UI.GridLayoutGroup>();
        gridLayout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 2;
        gridLayout.cellSize = new Vector2(96f, 96f);

        // Create 4 equip slots using slot prefab asset if available
        for (int i = 0; i < 4; i++)
        {
            GameObject slotGO;
            if (slotPrefabAsset != null)
                slotGO = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefabAsset);
            else
                slotGO = new GameObject("EquipSlot_Raw");
            slotGO.transform.SetParent(equipGrid.transform, false);
            var equipSlot = slotGO.GetComponent<EquipSlot>();
            if (equipSlot == null) equipSlot = slotGO.AddComponent<EquipSlot>();
            // set empty sprite if available
            if (sprites.TryGetValue("empty", out Sprite emptyS))
                equipSlot.emptySprite = emptyS;
        }

        // Status bars area
        GameObject statusArea = new GameObject("StatusArea");
        statusArea.transform.SetParent(left.transform, false);
        // Create HP/MP/ATK bars
        CreateStatusBarForPrefab(statusArea.transform, "HP", sprites, invUI);
        CreateStatusBarForPrefab(statusArea.transform, "MP", sprites, invUI);
        CreateStatusBarForPrefab(statusArea.transform, "ATK", sprites, invUI);

        // Right area with tabs and grid
        GameObject right = new GameObject("RightArea");
        right.transform.SetParent(root.transform, false);
        GameObject tabs = new GameObject("TabsRow");
        tabs.transform.SetParent(right.transform, false);
        // create smaller tab buttons
        CreatePrefabButton(tabs.transform, "Equip", invUI, sprites);
        CreatePrefabButton(tabs.transform, "Items", invUI, sprites);

        GameObject gridArea = new GameObject("GridArea");
        gridArea.transform.SetParent(right.transform, false);
        var grid = gridArea.AddComponent<UnityEngine.UI.GridLayoutGroup>();
        grid.cellSize = new Vector2(64f, 64f);
        grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;

        // Use slot prefab asset for grid items (assign to InventoryUI.slotPrefab asset reference)
        if (slotPrefabAsset != null)
        {
            invUI.slotPrefab = slotPrefabAsset;
        }

        // Save prefab to Assets/Inventory/Prefabs/InventoryRoot.prefab and to Resources
        string outPath = "Assets/Inventory/Prefabs/InventoryRoot.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, outPath);
        string resourcesOutDir = "Assets/Resources/Inventory/Prefabs";
        if (!Directory.Exists(resourcesOutDir)) Directory.CreateDirectory(resourcesOutDir);
        PrefabUtility.SaveAsPrefabAsset(root, Path.Combine(resourcesOutDir, "InventoryRoot.prefab"));

        GameObject.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created InventoryRoot prefab at " + outPath + " and bound available sprites.");
    }

    static void CreateStatusBarForPrefab(Transform parent, string label, System.Collections.Generic.Dictionary<string, Sprite> sprites, InventoryUI invUI)
    {
        GameObject go = new GameObject(label + "Bar");
        go.transform.SetParent(parent, false);
        var textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);
        var t = textGO.AddComponent<UnityEngine.UI.Text>();
        t.text = label;
        t.color = Color.white;
        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(go.transform, false);
        var img = fillGO.AddComponent<UnityEngine.UI.Image>();
        if (sprites.TryGetValue(label.ToLowerInvariant(), out Sprite fillS))
            img.sprite = fillS;
        // No need to set invUI fields here — runtime demo will wire StatusBar components when instantiated
    }

    static void CreatePrefabButton(Transform parent, string text, InventoryUI invUI, System.Collections.Generic.Dictionary<string, Sprite> sprites)
    {
        GameObject go = new GameObject("Tab_" + text);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.85f, 0.85f, 0.85f);
        var btn = go.AddComponent<UnityEngine.UI.Button>();
        var txt = new GameObject("Text");
        txt.transform.SetParent(go.transform, false);
        var t = txt.AddComponent<UnityEngine.UI.Text>();
        t.text = text;
        t.color = Color.black;
    }
}
#endif


