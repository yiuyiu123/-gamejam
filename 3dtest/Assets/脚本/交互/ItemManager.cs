using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [Header("物品管理")]
    public List<InteractableItem> allItems = new List<InteractableItem>();

    [Header("调试选项")]
    public bool showItemStatus = true;

    void Start()
    {
        // 自动查找场景中的所有物品
        if (allItems.Count == 0)
        {
            InteractableItem[] foundItems = FindObjectsOfType<InteractableItem>();
            allItems.AddRange(foundItems);
        }

        Debug.Log($"物品管理器初始化完成，找到 {allItems.Count} 个物品");
    }

    void Update()
    {
        if (showItemStatus)
        {
            DisplayItemStatus();
        }
    }

    void DisplayItemStatus()
    {
        foreach (InteractableItem item in allItems)
        {
            if (item != null)
            {
                string status = $"物品: {item.itemName} | ";
                status += item.isBeingHeld ? "被持有" : "未被持有";
                status += item.isExchangeLocked ? " | 交换锁定" : " | 可交换";

                if (item.isExchangeLocked && !string.IsNullOrEmpty(item.lastExchangeZone))
                {
                    status += $" (来自 {item.lastExchangeZone})";
                }

                // 这里可以显示在UI上或控制台
                // Debug.Log(status);
            }
        }
    }

    // 重置所有物品的交换锁定
    public void ResetAllItemLocks()
    {
        foreach (InteractableItem item in allItems)
        {
            if (item != null)
            {
                item.ResetExchangeLock();
            }
        }

        Debug.Log("所有物品的交换锁定已重置");
    }

    // 查找特定状态的物品
    public List<InteractableItem> FindItemsByStatus(bool isLocked, bool isHeld)
    {
        List<InteractableItem> result = new List<InteractableItem>();

        foreach (InteractableItem item in allItems)
        {
            if (item != null && item.isExchangeLocked == isLocked && item.isBeingHeld == isHeld)
            {
                result.Add(item);
            }
        }

        return result;
    }
}
