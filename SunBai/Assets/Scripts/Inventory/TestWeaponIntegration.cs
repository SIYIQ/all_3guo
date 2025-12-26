using UnityEngine;

/// <summary>
/// 测试武器系统集成的脚本
/// 在运行时设置武器物品的 WeaponData 引用
/// </summary>
public class TestWeaponIntegration : MonoBehaviour
{
    void Start()
    {
        // 查找 GuanDao WeaponData
        WeaponData guanDaoData = Resources.Load<WeaponData>("GuanDao");

        if (guanDaoData != null)
        {
            // 查找 Sword ItemData
            ItemData swordItem = Resources.Load<ItemData>("Inventory/Items/Sword");

            if (swordItem != null)
            {
                // 设置 WeaponData 引用
                swordItem.weaponData = guanDaoData;
                Debug.Log("成功设置 Sword 的 WeaponData 引用: " + guanDaoData.displayName);
            }
            else
            {
                Debug.LogError("找不到 Sword ItemData");
            }
        }
        else
        {
            Debug.LogError("找不到 GuanDao WeaponData，请确保 DuanYiBo 项目被正确引用");
        }
    }
}
