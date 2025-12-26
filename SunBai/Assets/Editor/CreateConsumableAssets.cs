using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;

public static class CreateConsumableAssets
{
    [MenuItem("Assets/Inventory/Create Default Consumables (Red/Blue)")]
    public static void CreateDefaultConsumables()
    {
        string resourcesPath = "Assets/Resources/Inventory/Items";
        if (!Directory.Exists(resourcesPath))
            Directory.CreateDirectory(resourcesPath);

        // Red Potion
        string redPath = Path.Combine(resourcesPath, "RedPotion.asset");
        if (!File.Exists(redPath))
        {
            ItemData red = ScriptableObject.CreateInstance<ItemData>();
            red.itemName = "RedPotion";
            red.itemType = ItemType.Consumable;
            red.restoreHP = 50;
            AssetDatabase.CreateAsset(red, redPath);
            Debug.Log("Created RedPotion ItemData at " + redPath);
        }
        else
        {
            Debug.Log("RedPotion.asset already exists, skipping.");
        }

        // Blue Potion
        string bluePath = Path.Combine(resourcesPath, "BluePotion.asset");
        if (!File.Exists(bluePath))
        {
            ItemData blue = ScriptableObject.CreateInstance<ItemData>();
            blue.itemName = "BluePotion";
            blue.itemType = ItemType.Consumable;
            blue.restoreMP = 30;
            AssetDatabase.CreateAsset(blue, bluePath);
            Debug.Log("Created BluePotion ItemData at " + bluePath);
        }
        else
        {
            Debug.Log("BluePotion.asset already exists, skipping.");
        }

        // Create simple colored PNG icons and assign to the ItemData assets if missing
        CreateAndAssignIcon(resourcesPath, "RedPotion", new Color(0.9f, 0.2f, 0.2f));
        CreateAndAssignIcon(resourcesPath, "BluePotion", new Color(0.2f, 0.45f, 0.9f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Create Consumables", "RedPotion and BluePotion assets created (or already exist) in Assets/Resources/Inventory/Items.", "OK");
    }

    static void CreateAndAssignIcon(string resourcesPath, string baseName, Color color)
    {
        string pngPath = Path.Combine(resourcesPath, baseName + "_tex.png");
        string assetPath = pngPath.Replace("\\", "/");
        // create PNG if not exists
        if (!File.Exists(pngPath))
        {
            Texture2D tex = new Texture2D(32, 32);
            Color[] cols = new Color[32 * 32];
            for (int i = 0; i < cols.Length; i++) cols[i] = color;
            tex.SetPixels(cols);
            tex.Apply();
            File.WriteAllBytes(pngPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            // set importer to Sprite
            var ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.SaveAndReimport();
            }
        }

        // assign sprite to ItemData if missing
        string itemAssetPath = Path.Combine(resourcesPath, baseName + ".asset").Replace("\\", "/");
        var item = AssetDatabase.LoadAssetAtPath<ItemData>(itemAssetPath);
        if (item != null)
        {
            if (item.icon == null)
            {
                var spr = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (spr != null)
                {
                    item.icon = spr;
                    EditorUtility.SetDirty(item);
                    Debug.Log($"Assigned icon to {baseName}");
                }
            }
        }
    }
}
#endif


