using UnityEngine;

/// <summary>
/// 挂在 Player 上：检测触发器内的 ItemPickup 并拾取到 InventoryUI。
/// 需要玩家对象有 Collider2D (非 isTrigger) 和 Rigidbody2D (通常 kinematic)。
/// </summary>
public class PlayerCollector : MonoBehaviour
{
    public InventoryUI inventoryUI;
    // 近距离拾取备选：当触发器事件未触发时使用 OverlapCircle 检测附近的 ItemPickup
    public float proximityPickupRadius = 0.5f;
    public LayerMask pickupLayerMask;
    public float proximityCheckInterval = 0.15f;
    public bool debugProximity = false;
    float proximityTimer = 0f;

    void Start()
    {
        if (inventoryUI == null)
        {
            inventoryUI = FindObjectOfType<InventoryUI>();
        }
        // 默认只检测 Default 层（如果没有显式设置）
        if (pickupLayerMask == 0)
        {
            pickupLayerMask = LayerMask.GetMask("Default");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Debug: 记录进入触发器的对象信息，便于诊断碰撞/图层问题
        string otherLayerName = LayerMask.LayerToName(other.gameObject.layer);
        string myLayerName = LayerMask.LayerToName(gameObject.layer);
        Debug.Log($"PlayerCollector: OnTriggerEnter2D other.name={other.gameObject.name} other.isTrigger={other.isTrigger} other.layer={otherLayerName} myLayer={myLayerName} other.attachedRigidbody={(other.attachedRigidbody!=null?other.attachedRigidbody.bodyType.ToString():\"null\")}");

        var pickup = other.GetComponent<ItemPickup>();
        if (pickup != null && pickup.item != null)
        {
        inventoryUI.AddItemToInventory(pickup.item, pickup.amount);
        Debug.Log($"PlayerCollector: Picked up {pickup.item.itemName} x{pickup.amount}");
        // 物品直接添加到背包，不自动装备到道具槽
        // 玩家需要手动从背包中装备消耗品到道具槽
            Destroy(pickup.gameObject);
        }
    }

    void Update()
    {
        // 定时执行近距离检测，避免每帧都查询
        proximityTimer -= Time.deltaTime;
        if (proximityTimer > 0f) return;
        proximityTimer = proximityCheckInterval;

        // 如果 inventoryUI 尚未绑定，尽量尝试获取
        if (inventoryUI == null)
        {
            inventoryUI = FindObjectOfType<InventoryUI>();
            if (inventoryUI == null) return;
        }

        // 在玩家位置附近查找可能的拾取物（只查指定层）
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, proximityPickupRadius, pickupLayerMask);
        if (debugProximity)
        {
            // 打印 LayerMask 包含的层
            string layers = "";
            for (int i = 0; i < 32; i++)
            {
                if ((pickupLayerMask.value & (1 << i)) != 0)
                {
                    layers += (layers.Length == 0 ? "" : ",") + LayerMask.LayerToName(i);
                }
            }
            Debug.Log($"PlayerCollector[Debug]: proximity check layers=[{layers}] radius={proximityPickupRadius} hits={(hits!=null?hits.Length:0)}");
        }
        if (hits == null || hits.Length == 0)
        {
            if (debugProximity)
            {
                // 如果没有命中，列出所有场景中的 ItemPickup 及其距离，帮助定位是否在检测范围之外
                var allPickups = FindObjectsOfType<ItemPickup>();
                if (allPickups != null && allPickups.Length > 0)
                {
                    foreach (var p in allPickups)
                    {
                        float d = Vector2.Distance(transform.position, p.transform.position);
                        Debug.Log($"PlayerCollector[Debug]: scene pickup {p.gameObject.name} at {p.transform.position} dist={d} layer={LayerMask.LayerToName(p.gameObject.layer)} isTrigger={p.GetComponent<Collider2D>()?.isTrigger}");
                    }
                }
                else
                {
                    Debug.Log("PlayerCollector[Debug]: no ItemPickup instances found in scene");
                }
            }
            return;
        }
        foreach (var c in hits)
        {
            if (c == null) continue;
            var pickup = c.GetComponent<ItemPickup>();
            if (pickup == null) continue;
            // 如果发现 ItemPickup，执行拾取（与 OnTriggerEnter2D 相同的逻辑）
            TryProcessPickup(pickup);
        }
    }

    // 将拾取处理抽成方法以便 OnTriggerEnter2D 和近距离检测复用
    void TryProcessPickup(ItemPickup pickup)
    {
        if (pickup == null || pickup.item == null) return;
        // 防止重复拾取：如果对象已经被其他逻辑标记为不可用则跳过
        if (!pickup.gameObject.activeInHierarchy) return;

        inventoryUI.AddItemToInventory(pickup.item, pickup.amount);
        Debug.Log($"PlayerCollector: Proximity picked up {pickup.item.itemName} x{pickup.amount}");
        // 物品直接添加到背包，不自动装备到道具槽
        // 先将对象设为不可见/不可交互，避免在同一帧内重复处理
        pickup.gameObject.SetActive(false);
        Destroy(pickup.gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // 在编辑器中绘制近距离检测范围，便于调试
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, proximityPickupRadius);
    }

}


