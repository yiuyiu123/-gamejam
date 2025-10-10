using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExchangeZone : MonoBehaviour
{
    [Header("交换区域设置")]
    public string zoneID = "Zone1";
    public ExchangeZone targetZone;
    public float detectionRadius = 2f;
    public Color gizmoColor = Color.green;

    [Header("交换设置")]
    public bool enableAutoExchange = true;

    [Header("状态")]
    public bool hasItem = false;
    public GameObject currentItem = null;

    private void Update()
    {
        if (enableAutoExchange)
        {
            CheckForItems();
        }
    }

    void CheckForItems()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        bool itemFound = false;
        GameObject foundItem = null;

        foreach (var collider in hitColliders)
        {
            InteractableItem item = collider.GetComponent<InteractableItem>();
            if (item != null && item.CanExchangeTo(targetZone.zoneID))
            {
                itemFound = true;
                foundItem = item.gameObject;
                break;
            }
        }

        // 如果状态发生变化
        if (itemFound != hasItem)
        {
            hasItem = itemFound;
            currentItem = foundItem;

            if (hasItem && foundItem != null)
            {
                // 设置物品的当前区域
                InteractableItem itemComponent = foundItem.GetComponent<InteractableItem>();
                if (itemComponent != null)
                {
                    itemComponent.SetCurrentZone(this);
                }

                OnItemPlaced(foundItem);
            }
            else
            {
                OnItemRemoved();
            }
        }
    }

    void OnItemPlaced(GameObject item)
    {
        if (!enableAutoExchange) return;

        Debug.Log($"物品 {item.name} 被放置到交换区域 {zoneID}");

        // 触发交换逻辑
        if (targetZone != null)
        {
            StartCoroutine(ExchangeItem(item));
        }
        else
        {
            Debug.LogWarning($"交换区域 {zoneID} 没有设置目标区域！");
        }
    }

    void OnItemRemoved()
    {
        Debug.Log($"物品从交换区域 {zoneID} 移除");
        currentItem = null;
    }

    // 当物品离开区域时调用（由物品脚本调用）
    public void OnItemLeft(GameObject item)
    {
        if (currentItem == item)
        {
            hasItem = false;
            currentItem = null;
        }
    }

    IEnumerator ExchangeItem(GameObject item)
    {
        InteractableItem itemComponent = item.GetComponent<InteractableItem>();
        if (itemComponent == null) yield break;

        // 标记物品正在交换过程中
        itemComponent.isInExchangeProcess = true;

        Debug.Log($"开始交换物品: {item.name} 从 {zoneID} 到 {targetZone.zoneID}");

        // 标记物品已被交换（防止重复交换）
        itemComponent.MarkAsExchanged(zoneID);

        // 等待短暂时间，让玩家看到物品被放置
        yield return new WaitForSeconds(0.5f);

        // 传送物品到目标区域
        Vector3 targetPosition = targetZone.transform.position;
        item.transform.position = targetPosition;

        // 重置物品的物理状态
        itemComponent.ResetPhysics();

        Debug.Log($"物品 {item.name} 已传送到区域 {targetZone.zoneID}");

        // 通知目标区域有物品到达
        targetZone.OnItemArrived(item);

        // 重置当前区域状态
        hasItem = false;
        currentItem = null;

        // 等待后取消物品交换状态标记
        yield return new WaitForSeconds(0.5f);
        itemComponent.isInExchangeProcess = false;
    }

    // 当物品从其他区域传送过来时调用
    public void OnItemArrived(GameObject item)
    {
        // 设置当前物品
        currentItem = item;
        hasItem = true;

        // 设置物品的当前区域
        InteractableItem itemComponent = item.GetComponent<InteractableItem>();
        if (itemComponent != null)
        {
            itemComponent.SetCurrentZone(this);
        }

        Debug.Log($"物品 {item.name} 到达区域 {zoneID}");
    }

    // 在Scene视图中显示区域范围
    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 显示到目标区域的连线
        if (targetZone != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetZone.transform.position);
        }

        // 显示区域标识
        GUIStyle style = new GUIStyle();
        style.normal.textColor = gizmoColor;
#if UNITY_EDITOR
        string statusText = $"{zoneID}\n{(hasItem ? "有物品" : "空")}";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, statusText, style);
#endif
    }
}
