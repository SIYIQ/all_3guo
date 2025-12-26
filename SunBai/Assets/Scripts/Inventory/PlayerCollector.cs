using UnityEngine;

/// <summary>
/// 挂在 Player 上：检测触发器内的 ItemPickup 并拾取到 InventoryUI。
/// 需要玩家对象有 Collider2D (非 isTrigger) 和 Rigidbody2D (通常 kinematic)。
/// </summary>
public class PlayerCollector : MonoBehaviour
{
    public InventoryUI inventoryUI;

    void Start()
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
        // 简单检查：先统计背包中的数量
        int have = 0;
        if (inventoryUI != null && inventoryUI.inventoryItems != null)
        {
            foreach (var it in inventoryUI.inventoryItems)
            {
                if (InventoryUI.ItemsMatch(it, item)) have++;
            }
        }

        int needed = amount;
        // 先从背包移除可用数量
        if (have > 0)
        {
            int take = Mathf.Min(have, needed);
            inventoryUI.RemoveItemFromInventory(item, take);
            needed -= take;
        }

        // 如果还需要，从已装备的消耗槽里消耗（consumableSlotA/consumableSlotB）
        if (needed > 0 && inventoryUI != null)
        {
            if (inventoryUI.consumableSlotA != null && InventoryUI.ItemsMatch(inventoryUI.consumableSlotA.CurrentItem, item))
            {
                inventoryUI.consumableSlotA.SetItem(null);
                needed = Mathf.Max(0, needed - 1);
            }
            if (needed > 0 && inventoryUI.consumableSlotB != null && InventoryUI.ItemsMatch(inventoryUI.consumableSlotB.CurrentItem, item))
            {
                inventoryUI.consumableSlotB.SetItem(null);
                needed = Mathf.Max(0, needed - 1);
            }
        }

        if (needed > 0)
        {
            // 未能满足数量，回滚（把之前从背包移除的数量还回去）
            if (amount - needed > 0)
            {
                inventoryUI.AddItemToInventory(item, amount - needed);
            }
            Debug.Log($"PlayerCollector: Not enough items to consume {item.itemName}");
            return false;
        }

        Debug.Log($"PlayerCollector: Consumed {item.itemName} x{amount}");
        return true;
    }
}


