using UnityEngine;

/// <summary>
/// 映射按键使用装备在消耗品槽中的道具。
/// 按键1使用消耗品槽A，按键2使用消耗品槽B。
/// 只有装备在道具槽中的消耗品才能被使用，使用后自动从槽位消失。
/// 依赖 PlayerCollector 与 InventoryUI 的接口：ConsumeItem / AddHP / AddMP。
/// </summary>
public class PlayerConsumableController : MonoBehaviour
{
    public KeyCode slotAKey = KeyCode.Alpha1;
    public KeyCode slotBKey = KeyCode.Alpha2;

    private PlayerCollector collector;
    private InventoryUI inventoryUI;

    void Start()
    {
        collector = GetComponent<PlayerCollector>();
        if (collector == null)
        {
            collector = FindObjectOfType<PlayerCollector>();
        }
        inventoryUI = collector != null ? collector.inventoryUI : FindObjectOfType<InventoryUI>();
    }

    void Update()
    {
        if (Input.GetKeyDown(slotAKey))
        {
            TryUseConsumableSlot(inventoryUI.consumableSlotA);
        }
        if (Input.GetKeyDown(slotBKey))
        {
            TryUseConsumableSlot(inventoryUI.consumableSlotB);
        }
    }

    private void TryUseConsumableSlot(EquipSlot slot)
    {
        if (slot == null || slot.CurrentItem == null)
        {
            Debug.Log("PlayerConsumableController: 道具槽为空或不存在");
            return;
        }

        ItemData item = slot.CurrentItem;
        if (item.itemType != ItemType.Consumable)
        {
            Debug.LogWarning("PlayerConsumableController: 道具槽中不是消耗品");
            return;
        }

        // 使用道具并应用效果
        if (inventoryUI != null)
        {
            // 应用恢复效果
            if (item.restoreHP > 0)
            {
                inventoryUI.AddHP(item.restoreHP);
                Debug.Log($"Used {item.itemName}, restored HP {item.restoreHP}");
            }
            if (item.restoreMP > 0)
            {
                inventoryUI.AddMP(item.restoreMP);
                Debug.Log($"Used {item.itemName}, restored MP {item.restoreMP}");
            }

            // 消耗道具：清空槽位
            slot.SetItem(null);
            Debug.Log($"PlayerConsumableController: 消耗了装备在道具槽中的 {item.itemName}，槽位已清空");
        }
    }
}


