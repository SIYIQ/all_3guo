using UnityEngine;

/// <summary>
/// 简化的消耗品使用系统。
/// 按键1使用红药水类消耗品，按键2使用蓝药水类消耗品。
/// 直接从背包中查找并消耗匹配的消耗品，不需要装备到道具槽。
/// </summary>
public class PlayerConsumableController : MonoBehaviour
{
    [Header("消耗品类型配置")]
    public string hpPotionKeyword = "HP"; // 按键1使用的HP药水关键词
    public string mpPotionKeyword = "MP"; // 按键2使用的MP药水关键词

    public KeyCode hpPotionKey = KeyCode.Alpha1;
    public KeyCode mpPotionKey = KeyCode.Alpha2;

    private InventoryUI inventoryUI;

    void Start()
    {
        inventoryUI = FindObjectOfType<InventoryUI>();
    }

    void Update()
    {
        if (Input.GetKeyDown(hpPotionKey))
        {
            UseConsumableByKeyword(hpPotionKeyword);
        }
        if (Input.GetKeyDown(mpPotionKey))
        {
            UseConsumableByKeyword(mpPotionKeyword);
        }
    }

    private void UseConsumableByKeyword(string keyword)
    {
        if (inventoryUI == null || string.IsNullOrEmpty(keyword))
        {
            return;
        }

        // 在背包中查找匹配的消耗品
        ItemData foundItem = null;
        foreach (var item in inventoryUI.inventoryItems)
        {
            if (item != null && item.itemType == ItemType.Consumable &&
                item.itemName.ToLower().Contains(keyword.ToLower()))
            {
                foundItem = item;
                break;
            }
        }

        if (foundItem == null)
        {
            Debug.Log($"背包中没有找到包含 '{keyword}' 的消耗品");
            return;
        }

        // 使用消耗品
        if (foundItem.restoreHP > 0)
        {
            inventoryUI.AddHP(foundItem.restoreHP);
            Debug.Log($"使用 {foundItem.itemName}，恢复 HP {foundItem.restoreHP}");
        }
        if (foundItem.restoreMP > 0)
        {
            inventoryUI.AddMP(foundItem.restoreMP);
            Debug.Log($"使用 {foundItem.itemName}，恢复 MP {foundItem.restoreMP}");
        }

        // 从背包中移除消耗品
        inventoryUI.RemoveItemFromInventory(foundItem, 1);
        Debug.Log($"从背包中消耗了 {foundItem.itemName}");
    }
}


