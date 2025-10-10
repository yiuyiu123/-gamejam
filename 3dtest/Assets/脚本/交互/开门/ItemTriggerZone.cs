using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTriggerZone : MonoBehaviour
{
    [Header("触发区域设置")]
    public string zoneName = "门锁区域";
    public float triggerRadius = 3f;
    public Color gizmoColor = Color.cyan;

    [Header("所需物品")]
    public string requiredItemName = "钥匙"; // 需要匹配的物品名称
    public bool requireExactMatch = true;   // 是否需要精确匹配名称

    [Header("触发效果")]
    public bool destroyItemOnTrigger = true; // 触发后是否销毁物品
    public float triggerDelay = 0.5f;       // 触发延迟

    [Header("事件响应")]
    public UnityEngine.Events.UnityEvent onTriggerSuccess; // 触发成功事件
    public UnityEngine.Events.UnityEvent onTriggerFail;    // 触发失败事件

    [Header("状态")]
    public bool isTriggered = false;
    public bool isPlayerInZone = false;

    private GameObject currentPlayerInZone;
    private List<InteractableItem> detectedItems = new List<InteractableItem>();

    void Update()
    {
        if (isTriggered) return; // 如果已经触发过，不再检测

        CheckForPlayers();

        if (isPlayerInZone && currentPlayerInZone != null)
        {
            CheckPlayerForRequiredItem(currentPlayerInZone);
        }
    }

    void CheckForPlayers()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, triggerRadius);
        bool playerFound = false;
        GameObject foundPlayer = null;

        foreach (var collider in hitColliders)
        {
            // 通过标签检测玩家，或者通过玩家控制器组件
            if (collider.CompareTag("Player") || collider.GetComponent<PlayerController>() != null)
            {
                playerFound = true;
                foundPlayer = collider.gameObject;
                break;
            }
        }

        if (playerFound != isPlayerInZone)
        {
            isPlayerInZone = playerFound;
            currentPlayerInZone = foundPlayer;

            if (isPlayerInZone)
            {
                OnPlayerEnterZone(foundPlayer);
            }
            else
            {
                OnPlayerExitZone();
            }
        }
    }

    void CheckPlayerForRequiredItem(GameObject player)
    {
        // 获取玩家持有的物品
        InteractableItem heldItem = GetPlayerHeldItem(player);

        if (heldItem != null && IsItemMatch(heldItem))
        {
            OnRequiredItemDetected(player, heldItem);
        }
    }

    InteractableItem GetPlayerHeldItem(GameObject player)
    {
        // 方法1: 通过玩家控制器获取
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null && playerController.IsHoldingItem())
        {
            return playerController.GetHeldItem();
        }

        // 方法2: 通过物品的持有者信息获取
        InteractableItem[] allItems = FindObjectsOfType<InteractableItem>();
        foreach (InteractableItem item in allItems)
        {
            if (item.isBeingHeld && item.currentHolder == player)
            {
                return item;
            }
        }

        return null;
    }

    bool IsItemMatch(InteractableItem item)
    {
        if (requireExactMatch)
        {
            return item.itemName == requiredItemName;
        }
        else
        {
            return item.itemName.Contains(requiredItemName);
        }
    }

    void OnPlayerEnterZone(GameObject player)
    {
        Debug.Log($"玩家进入 {zoneName}");

        // 立即检查玩家是否持有所需物品
        CheckPlayerForRequiredItem(player);
    }

    void OnPlayerExitZone()
    {
        Debug.Log($"玩家离开 {zoneName}");
        currentPlayerInZone = null;
        detectedItems.Clear();
    }

    void OnRequiredItemDetected(GameObject player, InteractableItem item)
    {
        if (isTriggered) return;

        Debug.Log($"检测到所需物品: {item.itemName} 在 {zoneName}");
        StartCoroutine(TriggerSequence(player, item));
    }

    System.Collections.IEnumerator TriggerSequence(GameObject player, InteractableItem item)
    {
        isTriggered = true;

        Debug.Log($"触发开始: {zoneName}");

        // 触发延迟
        yield return new WaitForSeconds(triggerDelay);

        // 触发成功事件
        onTriggerSuccess?.Invoke();

        // 处理物品
        if (destroyItemOnTrigger)
        {
            DestroyItem(item);
        }
        else
        {
            // 如果不销毁物品，可以将其放下
            item.PutDown();
        }

        Debug.Log($"触发完成: {zoneName}");
    }

    void DestroyItem(InteractableItem item)
    {
        if (item != null)
        {
            Debug.Log($"销毁物品: {item.itemName}");
            Destroy(item.gameObject);
        }
    }

    // 公共方法：手动触发（用于测试或特殊情况）
    public void ManualTrigger()
    {
        if (!isTriggered)
        {
            onTriggerSuccess?.Invoke();
            isTriggered = true;
        }
    }

    // 公共方法：重置触发状态
    public void ResetTrigger()
    {
        isTriggered = false;
        Debug.Log($"重置触发区域: {zoneName}");
    }

    // 在Scene视图中显示触发区域
    void OnDrawGizmos()
    {
        Gizmos.color = isTriggered ? Color.green : gizmoColor;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);

        // 显示区域名称
        GUIStyle style = new GUIStyle();
        style.normal.textColor = gizmoColor;
#if UNITY_EDITOR
        string statusText = $"{zoneName}\n所需: {requiredItemName}\n状态: {(isTriggered ? "已触发" : "等待中")}";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.3f, statusText, style);
#endif
    }
}
