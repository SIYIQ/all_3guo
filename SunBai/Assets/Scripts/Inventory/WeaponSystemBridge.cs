using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

// 引用 DuanYiBo 的类型（通过命名空间或直接引用）
// 注意：确保 DuanYiBo 项目在 Unity 中被正确引用

public class WeaponSystemBridge : MonoBehaviour
{
    private static WeaponSystemBridge _instance;
    public static WeaponSystemBridge Instance => _instance;

    [Header("DuanYiBo References")]
    [SerializeField] private GameObject playerObject;
    private Component playerCombatComponent; // 存储 PlayerCombat 组件引用
    private Component weaponSlotComponent; // 存储 WeaponSlot 组件引用

    [Header("Events")]
    public UnityEvent<WeaponData> OnWeaponEquipped = new UnityEvent<WeaponData>();
    public UnityEvent OnWeaponUnequipped = new UnityEvent();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        FindDuanYiBoComponents();
    }

    private void FindDuanYiBoComponents()
    {
        // 查找玩家对象（如果没有手动赋值）
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerObject != null)
        {
            // 查找 PlayerCombat 组件
            playerCombatComponent = playerObject.GetComponent("PlayerCombat");

            // 查找 WeaponSlot 组件（可能在子对象中）
            weaponSlotComponent = playerObject.GetComponentInChildren(System.Type.GetType("WeaponSlot"));
        }
        else
        {
            Debug.LogWarning("WeaponSystemBridge: Player object not found!");
        }
    }

    /// <summary>
    /// 装备武器
    /// </summary>
    public bool EquipWeapon(WeaponData weaponData)
    {
        if (playerCombatComponent == null)
        {
            Debug.LogError("WeaponSystemBridge: PlayerCombat component not found!");
            return false;
        }

        if (weaponData == null)
        {
            Debug.LogWarning("WeaponSystemBridge: WeaponData is null!");
            return false;
        }

        try
        {
            // 调用 DuanYiBo 的 EquipWeapon 方法
            var canEquipMethod = playerCombatComponent.GetType().GetMethod("CanEquip");
            if (canEquipMethod != null)
            {
                bool canEquip = (bool)canEquipMethod.Invoke(playerCombatComponent, new object[] { weaponData });
                if (!canEquip)
                {
                    Debug.LogWarning("WeaponSystemBridge: Cannot equip this weapon!");
                    return false;
                }
            }

            var equipMethod = playerCombatComponent.GetType().GetMethod("EquipWeapon");
            if (equipMethod != null)
            {
                equipMethod.Invoke(playerCombatComponent, new object[] { weaponData });
                Debug.Log($"WeaponSystemBridge: EquipWeapon invoked for '{(weaponData != null ? weaponData.displayName : "null")}'");
                OnWeaponEquipped?.Invoke(weaponData);
                // 输出当前攻击力以便调试（如果可用）
                try
                {
                    int ap = GetCurrentAttackPower();
                    Debug.Log($\"WeaponSystemBridge: CurrentAttackPower = {ap}\");
                }
                catch { }
                return true;
            }
            else
            {
                Debug.LogError("WeaponSystemBridge: EquipWeapon method not found!");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"WeaponSystemBridge: Error equipping weapon: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 卸载当前武器
    /// </summary>
    public void UnequipWeapon()
    {
        if (playerCombatComponent == null)
        {
            Debug.LogError("WeaponSystemBridge: PlayerCombat component not found!");
            return;
        }

        try
        {
            var unequipMethod = playerCombatComponent.GetType().GetMethod("UnequipWeapon");
            if (unequipMethod != null)
            {
                unequipMethod.Invoke(playerCombatComponent, null);
                Debug.Log(\"WeaponSystemBridge: UnequipWeapon invoked\");
                OnWeaponUnequipped?.Invoke();
            }
            else
            {
                Debug.LogError("WeaponSystemBridge: UnequipWeapon method not found!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"WeaponSystemBridge: Error unequipping weapon: {e.Message}");
        }
    }

    /// <summary>
    /// 获取当前装备的武器
    /// </summary>
    public WeaponData GetCurrentWeapon()
    {
        if (playerCombatComponent == null)
        {
            return null;
        }

        try
        {
            var currentWeaponProperty = playerCombatComponent.GetType().GetProperty("CurrentWeapon");
            if (currentWeaponProperty != null)
            {
                return (WeaponData)currentWeaponProperty.GetValue(playerCombatComponent);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"WeaponSystemBridge: Error getting current weapon: {e.Message}");
        }

        return null;
    }

    /// <summary>
    /// 获取当前攻击力
    /// </summary>
    public int GetCurrentAttackPower()
    {
        if (playerCombatComponent == null)
        {
            return 0;
        }

        try
        {
            var attackPowerProperty = playerCombatComponent.GetType().GetProperty("CurrentAttackPower");
            if (attackPowerProperty != null)
            {
                return (int)attackPowerProperty.GetValue(playerCombatComponent);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"WeaponSystemBridge: Error getting attack power: {e.Message}");
        }

        return 0;
    }

    /// <summary>
    /// 获取当前攻击范围
    /// </summary>
    public float GetCurrentAttackRange()
    {
        if (playerCombatComponent == null)
        {
            return 1.2f; // 默认值
        }

        try
        {
            var attackRangeProperty = playerCombatComponent.GetType().GetProperty("CurrentAttackRange");
            if (attackRangeProperty != null)
            {
                return (float)attackRangeProperty.GetValue(playerCombatComponent);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"WeaponSystemBridge: Error getting attack range: {e.Message}");
        }

        return 1.2f;
    }

    /// <summary>
    /// 获取当前攻击冷却时间
    /// </summary>
    public float GetCurrentAttackCooldown()
    {
        if (playerCombatComponent == null)
        {
            return 0.6f; // 默认值
        }

        try
        {
            var attackCooldownProperty = playerCombatComponent.GetType().GetProperty("CurrentAttackCooldown");
            if (attackCooldownProperty != null)
            {
                return (float)attackCooldownProperty.GetValue(playerCombatComponent);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"WeaponSystemBridge: Error getting attack cooldown: {e.Message}");
        }

        return 0.6f;
    }
}
