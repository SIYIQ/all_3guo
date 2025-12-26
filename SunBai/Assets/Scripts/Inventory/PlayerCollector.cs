using UnityEngine;

/// <summary>
/// 挂在 Player 上：检测触发器内的 ItemPickup 并拾取到 InventoryUI。
/// 需要玩家对象有 Collider2D (非 isTrigger) 和 Rigidbody2D (通常 kinematic)。
/// </summary>
public class PlayerCollector : MonoBehaviour
{
    public InventoryUI inventoryUI;

    void Awake()
    {
        if (inventoryUI == null)
        {
            inventoryUI = FindObjectOfType<InventoryUI>();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var pickup = other.GetComponent<ItemPickup>();
        if (pickup != null && pickup.item != null)
        {
            inventoryUI.AddItemToInventory(pickup.item, pickup.amount);
            Debug.Log($"PlayerCollector: Picked up {pickup.item.itemName} x{pickup.amount}");
            Destroy(pickup.gameObject);
        }
    }

    /// <summary>
    /// 消耗物品（由玩家脚本或 UI 调用）
    /// </summary>
    public bool ConsumeItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return false;
        // 简单检查：是否存在足够数量
        int have = 0;
        foreach (var it in inventoryUI.inventoryItems)
        {
            if (it == item) have++;
        }
        if (have < amount) return false;

        inventoryUI.RemoveItemFromInventory(item, amount);
        Debug.Log($"PlayerCollector: Consumed {item.itemName} x{amount}");
        return true;
    }
}


