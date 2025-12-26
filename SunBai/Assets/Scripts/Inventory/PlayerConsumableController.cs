using UnityEngine;

/// <summary>
/// 映射按键使用消耗品（例如小红瓶、小蓝瓶）。
/// 将对应 ItemData 拖到 Inspector（redPotion/bluePotion），运行时按键触发消耗并应用回复效果。
/// 依赖 PlayerCollector 与 InventoryUI 的接口：ConsumeItem / AddHP / AddMP。
/// </summary>
public class PlayerConsumableController : MonoBehaviour
{
    public ItemData redPotion;
    public ItemData bluePotion;

    public KeyCode redKey = KeyCode.Alpha1;
    public KeyCode blueKey = KeyCode.Alpha2;

    // 备用值：如果 ItemData.restoreHP/restoreMP 没设定，则使用这里的值
    public int redFallbackHeal = 50;
    public int blueFallbackHeal = 30;

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
        if (Input.GetKeyDown(redKey))
        {
            TryUseConsumable(redPotion, true);
        }
        if (Input.GetKeyDown(blueKey))
        {
            TryUseConsumable(bluePotion, false);
        }
    }

    private void TryUseConsumable(ItemData item, bool isRed)
    {
        if (item == null)
        {
            Debug.LogWarning(\"PlayerConsumableController: 没有绑定对应的道具 ItemData\");
            return;
        }
        if (collector == null)
        {
            Debug.LogWarning(\"PlayerConsumableController: 找不到 PlayerCollector\");
            return;
        }

        bool ok = collector.ConsumeItem(item, 1);
        if (!ok)
        {
            Debug.Log(\"PlayerConsumableController: 背包中没有足够的该道具\");
            return;
        }

        // 应用恢复效果（优先使用 ItemData 的数值）
        if (inventoryUI != null)
        {
            if (isRed)
            {
                int heal = item != null && item.restoreHP > 0 ? item.restoreHP : redFallbackHeal;
                inventoryUI.AddHP(heal);
                Debug.Log($\"Used {item.itemName}, restored HP {heal}\");
            }
            else
            {
                int heal = item != null && item.restoreMP > 0 ? item.restoreMP : blueFallbackHeal;
                inventoryUI.AddMP(heal);
                Debug.Log($\"Used {item.itemName}, restored MP {heal}\");
            }
        }
    }
}


