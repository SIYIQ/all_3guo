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

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Create Consumables", "RedPotion and BluePotion assets created (or already exist) in Assets/Resources/Inventory/Items.", "OK");
    }
}
#endif


