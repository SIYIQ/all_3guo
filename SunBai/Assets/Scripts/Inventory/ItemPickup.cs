using UnityEngine;

/// <summary>
/// 放在场景中的可拾取物体（带碰撞体, isTrigger=true）。
/// 当玩家进入触发器时发送拾取事件并自毁。
/// </summary>
public class ItemPickup : MonoBehaviour
{
    public ItemData item;
    public int amount = 1;

    void Reset()
    {
        // 尝试自动添加 Collider2D 如果没有
        if (GetComponent<Collider2D>() == null)
        {
            var col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
        }
    }
}


