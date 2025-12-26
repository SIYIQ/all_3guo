using UnityEngine;

/// <summary>
/// 背包测试辅助脚本
/// 用于验证背包功能是否正常工作
/// </summary>
public class InventoryTestHelper : MonoBehaviour
{
    public InventoryUI inventoryUI;
    public ItemData testItem; // 在Inspector中赋值一个测试物品

    void Start()
    {
        if (inventoryUI == null)
        {
            inventoryUI = FindObjectOfType<InventoryUI>();
        }

        Debug.Log("InventoryTestHelper: Initialized. Press T to add test item, I to toggle inventory");
    }

    void Update()
    {
        // 按T键添加测试物品到背包
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (inventoryUI != null && testItem != null)
            {
                inventoryUI.AddItemToInventory(testItem, 1);
                Debug.Log($"InventoryTestHelper: Added {testItem.itemName} to inventory");
            }
            else
            {
                Debug.LogError("InventoryTestHelper: inventoryUI or testItem is null");
            }
        }

        // 按Y键强制显示背包
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (inventoryUI != null && inventoryUI.inventoryRoot != null)
            {
                inventoryUI.inventoryRoot.SetActive(true);
                Debug.Log("InventoryTestHelper: Force showed inventory");
            }
        }

        // 按U键强制隐藏背包
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (inventoryUI != null && inventoryUI.inventoryRoot != null)
            {
                inventoryUI.inventoryRoot.SetActive(false);
                Debug.Log("InventoryTestHelper: Force hid inventory");
            }
        }
    }
}
