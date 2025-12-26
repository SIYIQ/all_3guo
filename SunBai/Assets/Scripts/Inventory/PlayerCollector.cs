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
        // Debug: 记录进入触发器的对象信息，便于诊断碰撞/图层问题
        string otherLayerName = LayerMask.LayerToName(other.gameObject.layer);
        string myLayerName = LayerMask.LayerToName(gameObject.layer);
        Debug.Log($"PlayerCollector: OnTriggerEnter2D other.name={other.gameObject.name} other.isTrigger={other.isTrigger} other.layer={otherLayerName} myLayer={myLayerName} other.attachedRigidbody={(other.attachedRigidbody!=null?other.attachedRigidbody.bodyType.ToString():"null")}");

        var pickup = other.GetComponent<ItemPickup>();
        if (pickup != null && pickup.item != null)
        {
            inventoryUI.AddItemToInventory(pickup.item, pickup.amount);
            Debug.Log($"PlayerCollector: Picked up {pickup.item.itemName} x{pickup.amount}");
            // 如果是消耗品，打开背包并切换到 Consumables 标签以便立即查看
            if (pickup.item.itemType == ItemType.Consumable)
            {
                inventoryUI.OpenConsumablesTab();
            }
            Destroy(pickup.gameObject);
        }
    }

    /// <summary>
    /// 消耗物品（由玩家脚本或 UI 调用）
    /// </summary>
    public bool ConsumeItem(ItemData item, int amount)
    {
        // 严格逻辑：只允许从已装备的 consumable 槽位消耗，背包内物品必须先装备到槽位才能被使用
        if (item == null || amount <= 0) return false;

        if (inventoryUI == null)
        {
            Debug.LogWarning("PlayerCollector: inventoryUI 未绑定，无法消耗道具");
            return false;
        }

        int available = 0;
        if (inventoryUI.consumableSlotA != null && InventoryUI.ItemsMatch(inventoryUI.consumableSlotA.CurrentItem, item)) available++;
        if (inventoryUI.consumableSlotB != null && InventoryUI.ItemsMatch(inventoryUI.consumableSlotB.CurrentItem, item)) available++;

        if (available < amount)
        {
            Debug.Log($"PlayerCollector: Not enough equipped consumables to consume {item.itemName}");
            return false;
        }

        int toConsume = amount;
        // 优先从 consumableSlotA 然后 consumableSlotB 消耗
        if (inventoryUI.consumableSlotA != null && InventoryUI.ItemsMatch(inventoryUI.consumableSlotA.CurrentItem, item) && toConsume > 0)
        {
            inventoryUI.consumableSlotA.SetItem(null);
            toConsume--;
        }
        if (inventoryUI.consumableSlotB != null && InventoryUI.ItemsMatch(inventoryUI.consumableSlotB.CurrentItem, item) && toConsume > 0)
        {
            inventoryUI.consumableSlotB.SetItem(null);
            toConsume--;
        }

        // 刷新 UI 显示（槽位已被清空）
        inventoryUI.RefreshGrid();
        Debug.Log($"PlayerCollector: Consumed equipped {item.itemName} x{amount}");
        return true;
    }
}


