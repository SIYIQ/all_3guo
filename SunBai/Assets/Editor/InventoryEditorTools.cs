using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// 编辑器工具：在场景中创建一个可绑定的 InventoryRoot（仅用于编辑器，方便在 Inspector 里绑定）
/// 菜单位置：Assets -> Inventory -> ...
/// </summary>
public static class InventoryEditorTools
{
    [MenuItem("Assets/Inventory/Create InventoryRoot (Editor)")]
    public static void CreateInventoryRootInScene()
    {
        // 如果场景已经存在 InventoryUI，询问用户
        InventoryUI existing = Object.FindObjectOfType<InventoryUI>();
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("Create InventoryRoot", "场景中已经存在一个 InventoryUI，是否仍要创建新的 InventoryRoot？", "Yes", "No"))
                return;
        }

        GameObject root = new GameObject("InventoryRoot");
        // 添加 InventoryUI 组件以便在 Inspector 可见
        InventoryUI ui = root.AddComponent<InventoryUI>();

        // 创建一些最小的子对象作为占位，避免在编辑器绑定时产生空引用问题
        GameObject grid = new GameObject("GridParent");
        grid.transform.SetParent(root.transform, false);
        ui.gridParent = grid.transform;

        GameObject weaponSlot = new GameObject("WeaponSlot");
        weaponSlot.transform.SetParent(root.transform, false);
        ui.weaponSlot = weaponSlot.AddComponent<EquipSlot>();

        GameObject gearSlot = new GameObject("GearSlot");
        gearSlot.transform.SetParent(root.transform, false);
        ui.gearSlot = gearSlot.AddComponent<EquipSlot>();

        GameObject consumableA = new GameObject("ConsumableA");
        consumableA.transform.SetParent(root.transform, false);
        ui.consumableSlotA = consumableA.AddComponent<EquipSlot>();

        GameObject consumableB = new GameObject("ConsumableB");
        consumableB.transform.SetParent(root.transform, false);
        ui.consumableSlotB = consumableB.AddComponent<EquipSlot>();

        // 状态条占位
        GameObject hp = new GameObject("HPBar");
        hp.transform.SetParent(root.transform, false);
        ui.hpBar = hp.AddComponent<StatusBar>();

        GameObject mp = new GameObject("MPBar");
        mp.transform.SetParent(root.transform, false);
        ui.mpBar = mp.AddComponent<StatusBar>();

        GameObject atk = new GameObject("ATKBar");
        atk.transform.SetParent(root.transform, false);
        ui.attackBar = atk.AddComponent<StatusBar>();

        GameObject range = new GameObject("RangeBar");
        range.transform.SetParent(root.transform, false);
        ui.rangeBar = range.AddComponent<StatusBar>();

        GameObject cd = new GameObject("CooldownBar");
        cd.transform.SetParent(root.transform, false);
        ui.cooldownBar = cd.AddComponent<StatusBar>();

        // 选中并保存场景对象
        Selection.activeGameObject = root;
        EditorUtility.SetDirty(root);
        Debug.Log("InventoryEditorTools: Created InventoryRoot in scene.");
    }

}
#endif


