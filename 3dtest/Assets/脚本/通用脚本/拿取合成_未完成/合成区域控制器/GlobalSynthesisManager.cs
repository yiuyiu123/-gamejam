using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSynthesisManager : MonoBehaviour
{
    public static GlobalSynthesisManager Instance;

    [Header("全局合成设置")]
    public float synthesisCheckInterval = 0.5f;

    [Header("合成物品出生位置设置")]
    public SynthesisResultSpawnMode spawnMode = SynthesisResultSpawnMode.FirstZone;
    public SynthesisZone specificSpawnZone; // 当选择特定区域时使用
    public Transform customSpawnPoint; // 当选择自定义位置时使用

    public enum SynthesisResultSpawnMode
    {
        FirstZone,      // 在第一个区域生成
        LastZone,       // 在最后一个区域生成  
        SpecificZone,   // 在指定区域生成
        CustomPosition, // 在自定义位置生成
        RandomZone      // 在随机区域生成
    }

    private List<SynthesisZone> allZones = new List<SynthesisZone>();
    private List<InteractableItem> allItemsInAllZones = new List<InteractableItem>();
    private bool isGlobalCombining = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 定期检查全局合成
        StartCoroutine(GlobalSynthesisCheckRoutine());
    }

    // 注册合成区域
    public void RegisterZone(SynthesisZone zone)
    {
        if (!allZones.Contains(zone))
        {
            allZones.Add(zone);
            Debug.Log($"注册合成区域: {zone.zoneID}");
        }
    }

    // 取消注册合成区域
    public void UnregisterZone(SynthesisZone zone)
    {
        if (allZones.Contains(zone))
        {
            allZones.Remove(zone);
            Debug.Log($"取消注册合成区域: {zone.zoneID}");
        }
    }

    // 获取所有区域中的所有物品
    public List<InteractableItem> GetAllItemsInAllZones()
    {
        allItemsInAllZones.Clear();

        foreach (SynthesisZone zone in allZones)
        {
            List<InteractableItem> zoneItems = zone.GetItemsInZone();
            foreach (InteractableItem item in zoneItems)
            {
                if (item != null && !allItemsInAllZones.Contains(item))
                {
                    allItemsInAllZones.Add(item);
                }
            }
        }

        Debug.Log($"全局物品检测: {allItemsInAllZones.Count} 个物品在 {allZones.Count} 个区域中");

        // 显示每个物品的详细状态
        foreach (var item in allItemsInAllZones)
        {
            if (item != null)
            {
                Debug.Log($"物品 {item.itemName} - 可拾取: {item.canBePickedUp}, 交换中: {item.isInExchangeProcess}, 被持有: {item.isBeingHeld}");
            }
        }

        return new List<InteractableItem>(allItemsInAllZones);
    }

    // 获取合成物品的出生位置
    private Vector3 GetSpawnPosition()
    {
        switch (spawnMode)
        {
            case SynthesisResultSpawnMode.FirstZone:
                if (allZones.Count > 0)
                {
                    SynthesisZone zone = allZones[0];
                    return zone.itemSpawnPoint != null ? zone.itemSpawnPoint.position : zone.throwTarget.position + Vector3.up * 2f;
                }
                break;

            case SynthesisResultSpawnMode.LastZone:
                if (allZones.Count > 0)
                {
                    SynthesisZone zone = allZones[allZones.Count - 1];
                    return zone.itemSpawnPoint != null ? zone.itemSpawnPoint.position : zone.throwTarget.position + Vector3.up * 2f;
                }
                break;

            case SynthesisResultSpawnMode.SpecificZone:
                if (specificSpawnZone != null)
                {
                    return specificSpawnZone.itemSpawnPoint != null ? specificSpawnZone.itemSpawnPoint.position : specificSpawnZone.throwTarget.position + Vector3.up * 2f;
                }
                break;

            case SynthesisResultSpawnMode.CustomPosition:
                if (customSpawnPoint != null)
                {
                    return customSpawnPoint.position;
                }
                break;

            case SynthesisResultSpawnMode.RandomZone:
                if (allZones.Count > 0)
                {
                    SynthesisZone randomZone = allZones[Random.Range(0, allZones.Count)];
                    return randomZone.itemSpawnPoint != null ? randomZone.itemSpawnPoint.position : randomZone.throwTarget.position + Vector3.up * 2f;
                }
                break;
        }

        // 默认回退到第一个区域
        if (allZones.Count > 0)
        {
            SynthesisZone defaultZone = allZones[0];
            return defaultZone.itemSpawnPoint != null ? defaultZone.itemSpawnPoint.position : defaultZone.throwTarget.position + Vector3.up * 2f;
        }

        return Vector3.zero;
    }

    // 获取用于生成物品的合成区域（用于播放效果等）
    private SynthesisZone GetSpawnZone()
    {
        switch (spawnMode)
        {
            case SynthesisResultSpawnMode.FirstZone:
                return allZones.Count > 0 ? allZones[0] : null;

            case SynthesisResultSpawnMode.LastZone:
                return allZones.Count > 0 ? allZones[allZones.Count - 1] : null;

            case SynthesisResultSpawnMode.SpecificZone:
                return specificSpawnZone;

            case SynthesisResultSpawnMode.RandomZone:
                return allZones.Count > 0 ? allZones[Random.Range(0, allZones.Count)] : null;

            default:
                return allZones.Count > 0 ? allZones[0] : null;
        }
    }

    // 全局合成检查协程
    IEnumerator GlobalSynthesisCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(synthesisCheckInterval);

            if (!isGlobalCombining)
            {
                CheckGlobalSynthesis();
            }
        }
    }

    // 检查全局合成
    public void CheckGlobalSynthesis()
    {
        List<InteractableItem> allItems = GetAllItemsInAllZones();

        // 清理无效物品 - 只过滤被持有和null的物品
        // 不要过滤 isInExchangeProcess，因为合成失败后需要重新检测
        allItems.RemoveAll(item => item == null || item.isBeingHeld);

        Debug.Log($"全局合成检查: {allItems.Count} 个有效物品");

        if (allItems.Count >= 2 && !isGlobalCombining)
        {
            Debug.Log($"=== 满足全局合成条件，开始全局合成 ===");
            StartCoroutine(PerformGlobalSynthesis(allItems));
        }
    }

    // 执行全局合成
    IEnumerator PerformGlobalSynthesis(List<InteractableItem> itemsToCombine)
    {
        isGlobalCombining = true;
        Debug.Log("=== 开始全局合成过程 ===");

        // 只取前2个物品进行合成
        if (itemsToCombine.Count > 2)
        {
            itemsToCombine = itemsToCombine.GetRange(0, 2);
        }

        // 显示要合成的具体物品
        string combineItems = "";
        foreach (var item in itemsToCombine)
        {
            combineItems += $"{item.itemName} ";

            // 重要：不要在这里锁定物品的物理状态！
            // 只设置状态标记，不修改物理属性
            item.isInExchangeProcess = true;
            item.canBePickedUp = false;

            // 注释掉物理锁定，让物品保持物理状态
            // if (item.Rb != null)
            // {
            //     item.Rb.isKinematic = true;
            //     item.Rb.velocity = Vector3.zero;
            // }

            Debug.Log($"设置合成状态: {item.itemName} (交换中: {item.isInExchangeProcess}, 可拾取: {item.canBePickedUp})");
        }
        Debug.Log($"将要全局合成的物品: {combineItems}");

        // 获取生成区域和位置
        SynthesisZone spawnZone = GetSpawnZone();
        Vector3 spawnPosition = GetSpawnPosition();

        Debug.Log($"合成物品将在 {spawnMode} 模式生成，位置: {spawnPosition}");

        // 播放合成效果
        if (spawnZone != null && spawnZone.synthesisEffect != null)
        {
            spawnZone.synthesisEffect.Play();
        }

        float delay = spawnZone != null ? spawnZone.synthesisDelay : 1f;
        Debug.Log($"等待合成延迟: {delay}秒");
        yield return new WaitForSeconds(delay);

        // 使用 CraftingManager 获取合成结果
        Debug.Log($"向 CraftingManager 请求合成...");
        GameObject resultPrefab = CraftingManager.Instance.CombineItems(itemsToCombine);

        if (resultPrefab != null)
        {
            Debug.Log("=== 全局合成成功！ ===");

            // 从所有区域中移除要销毁的物品
            foreach (var item in itemsToCombine)
            {
                if (item != null)
                {
                    // 从所有区域中移除这个物品
                    foreach (SynthesisZone zone in allZones)
                    {
                        zone.RemoveItemFromZone(item);
                    }
                    Debug.Log($"销毁物品: {item.itemName}");
                    Destroy(item.gameObject);
                }
            }

            // 生成新物品
            Debug.Log($"在位置 {spawnPosition} 生成新物品...");
            yield return StartCoroutine(SpawnResultItem(resultPrefab, spawnPosition, spawnZone));

            Debug.Log("=== 全局合成过程完成 ===");
        }
        else
        {
            Debug.LogWarning("=== 全局合成失败：没有匹配的配方 ===");

            // 重要：完全解锁物品
            foreach (var item in itemsToCombine)
            {
                if (item != null)
                {
                    // 重置所有状态
                    item.isInExchangeProcess = false;
                    item.canBePickedUp = true;

                    // 确保物理状态正常（即使之前没有修改）
                    if (item.Rb != null)
                    {
                        item.Rb.isKinematic = false;
                        item.Rb.useGravity = true;
                    }

                    // 确保碰撞体正常
                    Collider itemCollider = item.ItemCollider;
                    if (itemCollider != null)
                    {
                        itemCollider.enabled = true;
                    }

                    Debug.Log($"完全解锁物品: {item.itemName} (交换中: {item.isInExchangeProcess}, 可拾取: {item.canBePickedUp})");
                }
            }
        }

        isGlobalCombining = false;

        // 合成完成后再次检查
        yield return new WaitForSeconds(0.5f);
        CheckGlobalSynthesis();
    }

    // 修改生成方法，接受位置参数
    IEnumerator SpawnResultItem(GameObject resultPrefab, Vector3 spawnPosition, SynthesisZone spawnZone)
    {
        GameObject newItemObj = Instantiate(resultPrefab, spawnPosition, Quaternion.identity);

        // 添加弹出效果
        Rigidbody rb = newItemObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 popDirection = new Vector3(Random.Range(-0.5f, 0.5f), 1f, Random.Range(-0.5f, 0.5f)).normalized;
            rb.AddForce(popDirection * 5f, ForceMode.Impulse);
        }

        InteractableItem newItem = newItemObj.GetComponent<InteractableItem>();
        if (newItem != null)
        {
            Debug.Log($"新道具已生成: {newItem.itemName} 在位置: {spawnPosition}");

            // 可选：将新物品添加到生成区域中
            if (spawnZone != null && !spawnZone.GetItemsInZone().Contains(newItem))
            {
                // 使用反射或其他方法添加到区域，或者不添加
                Debug.Log($"新物品 {newItem.itemName} 已生成在区域 {spawnZone.zoneID} 附近");
            }
        }
        else
        {
            Debug.LogWarning("新生成的物品没有 InteractableItem 组件");
        }

        yield return new WaitForSeconds(0.5f);
    }

    // 强制合成检测方法
    [ContextMenu("强制全局合成检测")]
    public void ForceGlobalSynthesisCheck()
    {
        Debug.Log("=== 强制全局合成检测 ===");
        CheckGlobalSynthesis();
    }

    [ContextMenu("显示全局状态")]
    public void ShowGlobalStatus()
    {
        List<InteractableItem> allItems = GetAllItemsInAllZones();
        Debug.Log($"=== 全局合成状态 ===");
        Debug.Log($"区域数量: {allZones.Count}");
        Debug.Log($"总物品数量: {allItems.Count}");
        Debug.Log($"全局合成中: {isGlobalCombining}");
        Debug.Log($"出生模式: {spawnMode}");
        Debug.Log($"指定出生区域: {specificSpawnZone?.zoneID ?? "未设置"}");
        Debug.Log($"自定义出生点: {customSpawnPoint?.name ?? "未设置"}");

        foreach (var item in allItems)
        {
            if (item != null)
            {
                Debug.Log($"- {item.itemName} (持有: {item.isBeingHeld}, 交换: {item.isInExchangeProcess})");
            }
        }

        // 显示每个区域的详细状态
        foreach (SynthesisZone zone in allZones)
        {
            Debug.Log($"区域 {zone.zoneID}: {zone.GetItemsInZone().Count} 个物品");
        }
    }

    [ContextMenu("紧急修复：解锁所有物品")]
    public void EmergencyUnlockAllItems()
    {
        Debug.Log("=== 紧急修复：解锁所有物品 ===");

        // 解锁所有区域的物品
        foreach (SynthesisZone zone in allZones)
        {
            zone.EmergencyUnlockAllItems();
        }

        // 解锁全局管理器中的物品
        foreach (var item in allItemsInAllZones)
        {
            if (item != null)
            {
                item.isInExchangeProcess = false;
                item.canBePickedUp = true;
                if (item.Rb != null) item.Rb.isKinematic = false;
            }
        }
        allItemsInAllZones.Clear();

        isGlobalCombining = false;
        Debug.Log("所有物品已解锁");
    }

    // 设置出生模式的方法（可以在运行时调用）
    public void SetSpawnMode(SynthesisResultSpawnMode mode)
    {
        spawnMode = mode;
        Debug.Log($"设置合成物品出生模式为: {mode}");
    }

    public void SetSpecificSpawnZone(SynthesisZone zone)
    {
        specificSpawnZone = zone;
        Debug.Log($"设置指定出生区域为: {zone?.zoneID ?? "null"}");
    }

    public void SetCustomSpawnPoint(Transform point)
    {
        customSpawnPoint = point;
        Debug.Log($"设置自定义出生点为: {point?.name ?? "null"}");
    }
}