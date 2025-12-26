using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Linq;

/// <summary>
/// 编辑器工具：将项目中的 WeaponData 自动关联到 ItemData.weaponData（仅在编辑器中运行）。
/// 用途：当 WeaponData 不在 Resources 下导致运行时找不到时，可在编辑期批量修复引用，避免运行时警告。
/// 菜单：Assets -> Inventory -> Link WeaponData To ItemData
/// </summary>
public static class WeaponDataLinker
{
    [MenuItem("Assets/Inventory/Link WeaponData To ItemData")]
    public static void LinkWeaponDataToItems()
    {
        // 查找所有 WeaponData 资产
        string[] weaponGuids = AssetDatabase.FindAssets("t:WeaponData");
        var weapons = weaponGuids.Select(g => AssetDatabase.GUIDToAssetPath(g)).ToArray();

        if (weapons.Length == 0)
        {
            Debug.LogWarning("WeaponDataLinker: 未找到任何 WeaponData 资产。");
            return;
        }

        // 加载所有 ItemData
        string[] itemGuids = AssetDatabase.FindAssets("t:ItemData");
        int assignedCount = 0;

        foreach (var ig in itemGuids)
        {
            string itemPath = AssetDatabase.GUIDToAssetPath(ig);
            var item = AssetDatabase.LoadAssetAtPath<ItemData>(itemPath);
            if (item == null) continue;
            if (item.itemType != ItemType.Weapon) continue;
            if (item.weaponData != null) continue; // 已有关联，跳过

            // 尝试按文件名匹配 WeaponData
            string itemName = Path.GetFileNameWithoutExtension(itemPath);
            string matchedWeaponPath = weapons.FirstOrDefault(wp => Path.GetFileNameWithoutExtension(wp).Equals(itemName, System.StringComparison.OrdinalIgnoreCase));

            WeaponData assign = null;
            if (!string.IsNullOrEmpty(matchedWeaponPath))
            {
                assign = AssetDatabase.LoadAssetAtPath<WeaponData>(matchedWeaponPath);
            }
            else if (weapons.Length == 1)
            {
                // 项目仅有一个 WeaponData，自动关联到所有 Weapon ItemData（有风险，但方便快速测试）
                assign = AssetDatabase.LoadAssetAtPath<WeaponData>(weapons[0]);
            }

            if (assign != null)
            {
                item.weaponData = assign;
                EditorUtility.SetDirty(item);
                assignedCount++;
                Debug.Log($"WeaponDataLinker: 为 ItemData '{item.name}' 关联 WeaponData '{assign.name}'");
            }
            else
            {
                Debug.Log($"WeaponDataLinker: 未为 ItemData '{item.name}' 找到匹配的 WeaponData（路径候选数 {weapons.Length}）");
            }
        }

        if (assignedCount > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"WeaponDataLinker: 完成关联，共关联 {assignedCount} 条 ItemData。");
        }
        else
        {
            Debug.Log("WeaponDataLinker: 未做任何关联。");
        }
    }
}
#endif


