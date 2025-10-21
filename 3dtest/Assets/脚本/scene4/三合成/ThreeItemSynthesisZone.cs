using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeItemSynthesisZone : MonoBehaviour
{
    [Header("三合成区域设置")]
    public string zoneID = "ThreeItemSynthesisZone";

    [Header("抛掷目标位置 - 三个物品分别飞向不同位置")]
    public Transform throwTarget1;
    public Transform throwTarget2;
    public Transform throwTarget3;

    public float detectionRadius = 3f;

    [Header("抛掷设置")]
    public float throwHeight = 3f;
    public float throwDuration = 0.8f;

    [Header("合成冷却时间")]
    public float synthesisCooldown = 2f;

    [Header("合成失败弹出设置")]
    public bool enableFailEjection = true;
    public float failEjectionForce = 10f;
    public float failEjectionHeight = 2.5f;
    public float failEjectionRandomness = 0.8f;

    private List<InteractableItem> itemsInZone = new List<InteractableItem>();
    private bool isCombining = false;
    private bool hasCheckedThisFrame = false;
    private float lastSynthesisTime = 0f;

    // 跟踪每个物品的目标位置
    private Dictionary<InteractableItem, Transform> itemTargetMap = new Dictionary<InteractableItem, Transform>();

    // 新增：目标位置使用队列，确保公平分配
    private Queue<Transform> availableTargets = new Queue<Transform>();

    void Start()
    {
        // 如果没有设置目标位置，使用默认位置
        if (throwTarget1 == null) throwTarget1 = CreateDefaultTarget("Target1", new Vector3(-1, 0, 0));
        if (throwTarget2 == null) throwTarget2 = CreateDefaultTarget("Target2", new Vector3(0, 0, 0));
        if (throwTarget3 == null) throwTarget3 = CreateDefaultTarget("Target3", new Vector3(1, 0, 0));

        // 初始化可用目标队列
        InitializeTargetQueue();

        EnsureColliderSize();

        Debug.Log($"三合成区域初始化完成，三个目标位置: {throwTarget1.position}, {throwTarget2.position}, {throwTarget3.position}");
    }

    // 初始化目标队列
    void InitializeTargetQueue()
    {
        availableTargets.Clear();
        availableTargets.Enqueue(throwTarget1);
        availableTargets.Enqueue(throwTarget2);
        availableTargets.Enqueue(throwTarget3);
    }

    // 创建默认目标位置
    Transform CreateDefaultTarget(string name, Vector3 offset)
    {
        GameObject targetObj = new GameObject($"{zoneID}_{name}");
        targetObj.transform.SetParent(transform);
        targetObj.transform.localPosition = offset;
        return targetObj.transform;
    }

    void EnsureColliderSize()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(5, 3, 5);
        }
        else if (!collider.isTrigger)
        {
            collider.isTrigger = true;
        }
    }

    void Update()
    {
        if (!hasCheckedThisFrame)
        {
            if (Time.time - lastSynthesisTime >= synthesisCooldown)
            {
                CheckForThreeItemSynthesis();
            }
            hasCheckedThisFrame = true;
        }
    }

    void LateUpdate()
    {
        hasCheckedThisFrame = false;
    }

    void OnTriggerEnter(Collider other)
    {
        InteractableItem item = other.GetComponent<InteractableItem>();
        if (item != null && ShouldProcessItem(item))
        {
            if (!itemsInZone.Contains(item))
            {
                itemsInZone.Add(item);

                if (Time.time - lastSynthesisTime >= synthesisCooldown)
                {
                    CheckForThreeItemSynthesis();
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (isCombining) return;

        InteractableItem item = other.GetComponent<InteractableItem>();
        if (item != null && itemsInZone.Contains(item))
        {
            itemsInZone.Remove(item);
            // 从目标映射中移除，并将目标位置重新加入可用队列
            if (itemTargetMap.ContainsKey(item))
            {
                Transform freedTarget = itemTargetMap[item];
                itemTargetMap.Remove(item);

                // 只有当这个目标不在当前可用队列中时才重新加入
                if (!availableTargets.Contains(freedTarget))
                {
                    availableTargets.Enqueue(freedTarget);
                    Debug.Log($"目标位置 {freedTarget.name} 已释放并重新加入队列");
                }
            }
        }
    }

    bool ShouldProcessItem(InteractableItem item)
    {
        if (item == null) return false;
        if (item.isBeingHeld) return false;
        if (item.isInExchangeProcess) return false;
        return true;
    }

    void CheckForThreeItemSynthesis()
    {
        if (isCombining) return;
        if (Time.time - lastSynthesisTime < synthesisCooldown) return;

        // 清理无效物品
        itemsInZone.RemoveAll(item => item == null || item.isBeingHeld);

        // 只有恰好3个物品时才尝试三合成
        if (itemsInZone.Count == 3)
        {
            StartCoroutine(PerformThreeItemSynthesis());
        }
    }

    IEnumerator PerformThreeItemSynthesis()
    {
        isCombining = true;
        lastSynthesisTime = Time.time;

        List<InteractableItem> itemsToCombine = new List<InteractableItem>();
        foreach (var item in itemsInZone)
        {
            if (item != null && !item.isBeingHeld && !item.isInExchangeProcess)
            {
                itemsToCombine.Add(item);
            }
        }

        // 确保还是3个物品
        if (itemsToCombine.Count != 3)
        {
            isCombining = false;
            yield break;
        }

        // 锁定所有物品
        foreach (var item in itemsToCombine)
        {
            item.isInExchangeProcess = true;
            if (item.Rb != null)
            {
                item.Rb.isKinematic = true;
                item.Rb.velocity = Vector3.zero;
            }
        }

        // 播放合成效果
        if (ThreeItemCraftingManager.Instance.synthesisEffect != null)
            Instantiate(ThreeItemCraftingManager.Instance.synthesisEffect, transform.position, Quaternion.identity);

        if (ThreeItemCraftingManager.Instance.synthesisSound != null)
            AudioSource.PlayClipAtPoint(ThreeItemCraftingManager.Instance.synthesisSound, transform.position);

        yield return new WaitForSeconds(ThreeItemCraftingManager.Instance.synthesisDelay);

        // 尝试三合成
        ThreeItemRecipe matchedRecipe = ThreeItemCraftingManager.Instance.CombineThreeItems(itemsToCombine);

        if (matchedRecipe != null && matchedRecipe.resultItemPrefab != null)
        {
            // 三合成成功
            foreach (var item in itemsToCombine)
            {
                if (item != null)
                {
                    itemsInZone.Remove(item);
                    // 从目标映射中移除，并将目标位置重新加入可用队列
                    if (itemTargetMap.ContainsKey(item))
                    {
                        Transform freedTarget = itemTargetMap[item];
                        itemTargetMap.Remove(item);

                        // 只有当这个目标不在当前可用队列中时才重新加入
                        if (!availableTargets.Contains(freedTarget))
                        {
                            availableTargets.Enqueue(freedTarget);
                            Debug.Log($"合成成功，目标位置 {freedTarget.name} 已释放");
                        }
                    }
                    Destroy(item.gameObject);
                }
            }

            // 生成结果物品
            Vector3 spawnPosition = ThreeItemCraftingManager.Instance.GetSpawnPosition(matchedRecipe);
            yield return StartCoroutine(SpawnResultItem(matchedRecipe.resultItemPrefab, spawnPosition));

            Debug.Log($"三合成成功！生成了 {matchedRecipe.recipeName}");
        }
        else
        {
            // 三合成失败 - 弹出所有物品
            Debug.LogWarning($"三合成失败，将弹出 {itemsToCombine.Count} 个物品");
            yield return StartCoroutine(EjectFailedItems(itemsToCombine));
        }

        isCombining = false;
    }

    IEnumerator EjectFailedItems(List<InteractableItem> itemsToEject)
    {
        Debug.Log($"开始弹出 {itemsToEject.Count} 个失败物品");

        foreach (var item in itemsToEject)
        {
            if (item != null)
            {
                // 从区域列表中移除
                itemsInZone.Remove(item);
                // 从目标映射中移除，并将目标位置重新加入可用队列
                if (itemTargetMap.ContainsKey(item))
                {
                    Transform freedTarget = itemTargetMap[item];
                    itemTargetMap.Remove(item);

                    // 只有当这个目标不在当前可用队列中时才重新加入
                    if (!availableTargets.Contains(freedTarget))
                    {
                        availableTargets.Enqueue(freedTarget);
                        Debug.Log($"合成失败，目标位置 {freedTarget.name} 已释放");
                    }
                }

                // 解锁物品状态
                item.isInExchangeProcess = false;

                // 启用物理
                if (item.Rb != null)
                {
                    item.Rb.isKinematic = false;
                    item.Rb.useGravity = true;

                    // 计算弹出方向
                    Vector3 ejectionDirection = CalculateEjectionDirection(item.transform.position);

                    // 应用弹出力
                    item.Rb.AddForce(ejectionDirection * failEjectionForce, ForceMode.Impulse);

                    // 添加随机旋转
                    Vector3 randomTorque = new Vector3(
                        Random.Range(-failEjectionRandomness, failEjectionRandomness),
                        Random.Range(-failEjectionRandomness, failEjectionRandomness),
                        Random.Range(-failEjectionRandomness, failEjectionRandomness)
                    ) * failEjectionForce;
                    item.Rb.AddTorque(randomTorque, ForceMode.Impulse);
                }

                Debug.Log($"弹出物品: {item.itemName}");
            }
        }

        yield return new WaitForSeconds(0.5f);
    }

    Vector3 CalculateEjectionDirection(Vector3 itemPosition)
    {
        // 基本方向：从区域中心向外
        Vector3 baseDirection = (itemPosition - transform.position).normalized;

        // 确保有向上的分量
        baseDirection.y = Mathf.Max(baseDirection.y, 0.3f);

        // 添加随机性
        Vector3 randomVariation = new Vector3(
            Random.Range(-failEjectionRandomness, failEjectionRandomness),
            Random.Range(0, failEjectionRandomness * 0.5f),
            Random.Range(-failEjectionRandomness, failEjectionRandomness)
        );

        Vector3 finalDirection = (baseDirection + randomVariation).normalized;
        finalDirection.y += failEjectionHeight * 0.1f;

        return finalDirection;
    }

    IEnumerator SpawnResultItem(GameObject resultPrefab, Vector3 spawnPosition)
    {
        GameObject newItemObj = Instantiate(resultPrefab, spawnPosition, Quaternion.identity);

        Rigidbody rb = newItemObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 popDirection = new Vector3(Random.Range(-0.5f, 0.5f), 1f, Random.Range(-0.5f, 0.5f)).normalized;
            rb.AddForce(popDirection * 5f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(0.5f);
    }

    public void ThrowItemToZone(InteractableItem item)
    {
        StartCoroutine(ThrowItemCoroutine(item));
    }

    IEnumerator ThrowItemCoroutine(InteractableItem item)
    {
        item.isInExchangeProcess = true;
        if (item.Rb != null)
        {
            item.Rb.isKinematic = true;
            item.Rb.velocity = Vector3.zero;
        }

        // 为物品分配目标位置 - 使用队列确保公平分配
        Transform target = GetNextAvailableTarget();
        if (target == null)
        {
            Debug.LogWarning("没有可用的目标位置，使用默认位置");
            target = throwTarget1;
        }

        // 记录物品的目标位置
        itemTargetMap[item] = target;
        Debug.Log($"物品 {item.itemName} 将抛掷到位置: {target.name}");

        Vector3 startPosition = item.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < throwDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / throwDuration;
            Vector3 currentPos = Vector3.Lerp(startPosition, target.position, progress);
            currentPos.y += Mathf.Sin(progress * Mathf.PI) * throwHeight;
            item.transform.position = currentPos;
            yield return null;
        }

        item.transform.position = target.position;

        if (!itemsInZone.Contains(item))
        {
            itemsInZone.Add(item);
        }

        item.isInExchangeProcess = false;
        if (item.Rb != null) item.Rb.isKinematic = false;
    }

    // 修改：使用队列获取下一个可用目标
    Transform GetNextAvailableTarget()
    {
        if (availableTargets.Count == 0)
        {
            Debug.LogWarning("所有目标位置都被占用，无法分配新目标");
            return null;
        }

        Transform nextTarget = availableTargets.Dequeue();
        Debug.Log($"分配目标位置: {nextTarget.name}，剩余可用目标: {availableTargets.Count}");
        return nextTarget;
    }

    [ContextMenu("显示区域状态")]
    public void ShowZoneStatus()
    {
        Debug.Log($"=== 三合成区域 {zoneID} 状态 ===");
        Debug.Log($"物品数量: {itemsInZone.Count}");
        Debug.Log($"合成中: {isCombining}");
        Debug.Log($"可用目标位置数量: {availableTargets.Count}");

        foreach (var item in itemsInZone)
        {
            if (item != null)
            {
                string targetInfo = itemTargetMap.ContainsKey(item) ?
                    $"目标: {itemTargetMap[item].name}" : "目标: 未分配";
                Debug.Log($"- {item.itemName} ({targetInfo})");
            }
        }

        // 显示队列中的目标位置
        Debug.Log("可用目标队列:");
        foreach (var target in availableTargets)
        {
            Debug.Log($"  - {target.name}");
        }
    }

    [ContextMenu("紧急解锁所有物品")]
    public void EmergencyUnlockAllItems()
    {
        foreach (var item in itemsInZone)
        {
            if (item != null)
            {
                item.isInExchangeProcess = false;
                if (item.Rb != null) item.Rb.isKinematic = false;
            }
        }
        itemsInZone.Clear();
        itemTargetMap.Clear();

        // 重新初始化目标队列
        InitializeTargetQueue();

        isCombining = false;
        Debug.Log("所有物品已解锁，目标队列已重置");
    }

    void OnDrawGizmosSelected()
    {
        // 绘制三个目标位置的Gizmos
        Gizmos.color = Color.red;
        if (throwTarget1 != null) Gizmos.DrawWireSphere(throwTarget1.position, 0.3f);

        Gizmos.color = Color.green;
        if (throwTarget2 != null) Gizmos.DrawWireSphere(throwTarget2.position, 0.3f);

        Gizmos.color = Color.blue;
        if (throwTarget3 != null) Gizmos.DrawWireSphere(throwTarget3.position, 0.3f);

        // 绘制连接线
        Gizmos.color = Color.white;
        if (throwTarget1 != null && throwTarget2 != null)
            Gizmos.DrawLine(throwTarget1.position, throwTarget2.position);
        if (throwTarget2 != null && throwTarget3 != null)
            Gizmos.DrawLine(throwTarget2.position, throwTarget3.position);
        if (throwTarget3 != null && throwTarget1 != null)
            Gizmos.DrawLine(throwTarget3.position, throwTarget1.position);
    }
}