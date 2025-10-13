using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynthesisZone : MonoBehaviour
{
    [Header("合成区域设置")]
    public string zoneID = "SynthesisZone";
    public Transform throwTarget;
    public float detectionRadius = 3f;
    public Transform itemSpawnPoint;

    [Header("合成效果")]
    public ParticleSystem synthesisEffect;
    public AudioClip synthesisSound;
    public float synthesisDelay = 1f;

    [Header("抛掷设置")]
    public float throwHeight = 3f;
    public float throwDuration = 0.8f;

    [Header("调试选项")]
    public bool showDebugGUI = false;

    private List<InteractableItem> itemsInZone = new List<InteractableItem>();
    private AudioSource audioSource;
    private bool isCombining = false;
    private bool hasCheckedThisFrame = false;

    // 添加物品进入区域的跟踪
    private Dictionary<InteractableItem, float> itemEnterTimes = new Dictionary<InteractableItem, float>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (throwTarget == null)
            throwTarget = transform;

        EnsureColliderSize();

        // 注册到全局管理器
        if (GlobalSynthesisManager.Instance != null)
        {
            GlobalSynthesisManager.Instance.RegisterZone(this);
        }
    }


    void EnsureColliderSize()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(5, 3, 5);
            Debug.Log($"为合成区域 {zoneID} 添加了触发碰撞体");
        }
        else if (!collider.isTrigger)
        {
            collider.isTrigger = true;
            Debug.Log($"已将合成区域 {zoneID} 的碰撞体设置为触发器");
        }
    }

    void Update()
    {
        if (!hasCheckedThisFrame)
        {
            CheckForSynthesis();
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
        if (item != null)
        {
            // 更严格的忽略条件检查
            if (ShouldIgnoreItem(item))
            {
                Debug.Log($"忽略物品 {item.itemName} - 被持有: {item.isBeingHeld}, 交换中: {item.isInExchangeProcess}");
                return;
            }

            if (!itemsInZone.Contains(item))
            {
                itemsInZone.Add(item);
                // 记录进入时间
                itemEnterTimes[item] = Time.time;

                Debug.Log($"物品 {item.itemName} 进入合成区域 {zoneID}，当前区域物品数: {itemsInZone.Count}");
                DebugItemsInZone();

                // 立即检查合成
                CheckForSynthesis();
            }
        }
    }
    void OnDestroy()
    {
        // 取消注册
        if (GlobalSynthesisManager.Instance != null)
        {
            GlobalSynthesisManager.Instance.UnregisterZone(this);
        }
    }
    // 添加公共方法供全局管理器调用
    public List<InteractableItem> GetItemsInZone()
    {
        // 清理无效物品，但不要过滤掉 canBePickedUp = false 的物品
        // 因为合成失败后物品需要被重新检测
        itemsInZone.RemoveAll(item =>
            item == null ||
            item.isBeingHeld//||
            //item.isInExchangeProcess || 
            //!item.canBePickedUp         
        );

        // 返回所有在区域内的物品，让全局管理器自己处理状态检查
        return new List<InteractableItem>(itemsInZone);
    }

    // 从区域中移除指定物品
    public void RemoveItemFromZone(InteractableItem item)
    {
        if (itemsInZone.Contains(item))
        {
            itemsInZone.Remove(item);
            itemEnterTimes.Remove(item);
            Debug.Log($"从区域 {zoneID} 移除物品: {item.itemName}");
        }
    }
    void OnTriggerStay(Collider other)
    {
        InteractableItem item = other.GetComponent<InteractableItem>();
        if (item != null && !itemsInZone.Contains(item))
        {
            if (ShouldIgnoreItem(item))
            {
                return;
            }

            itemsInZone.Add(item);
            itemEnterTimes[item] = Time.time;

            Debug.Log($"物品 {item.itemName} 在合成区域内被检测到，当前区域物品数: {itemsInZone.Count}");
            DebugItemsInZone();

            CheckForSynthesis();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (isCombining) return;

        InteractableItem item = other.GetComponent<InteractableItem>();
        if (item != null && itemsInZone.Contains(item))
        {
            itemsInZone.Remove(item);
            itemEnterTimes.Remove(item);

            Debug.Log($"物品 {item.itemName} 离开合成区域 {zoneID}，剩余物品数: {itemsInZone.Count}");
            DebugItemsInZone();
        }
    }

    // 改进的物品忽略检查
    bool ShouldIgnoreItem(InteractableItem item)
    {
        if (item == null) return true;
        if (item.isBeingHeld) return true;
        if (item.isInExchangeProcess) return true;

        // 检查物品是否刚被抛掷（避免重复检测）
        if (itemEnterTimes.ContainsKey(item))
        {
            float timeSinceEnter = Time.time - itemEnterTimes[item];
            if (timeSinceEnter < 0.1f) // 100ms内刚进入的物品
            {
                return false; // 允许新进入的物品
            }
        }

        return false;
    }

    void DebugItemsInZone()
    {
        string debugInfo = $"区域 {zoneID} 物品列表 ({itemsInZone.Count} 个): ";
        foreach (var item in itemsInZone)
        {
            if (item != null)
            {
                debugInfo += $"{item.itemName} ";
            }
        }
        Debug.Log(debugInfo);
    }

    void CheckForSynthesis()
    {
        // ... 现有的本地合成检查代码 ...

        // 同时触发全局合成检查
        if (GlobalSynthesisManager.Instance != null)
        {
            GlobalSynthesisManager.Instance.CheckGlobalSynthesis();
        }
    }

    IEnumerator PerformSynthesis()
    {
        isCombining = true;
        Debug.Log("=== 开始合成过程 ===");

        // 使用当前有效的物品列表
        List<InteractableItem> itemsToCombine = new List<InteractableItem>();
        foreach (var item in itemsInZone)
        {
            if (item != null && !item.isBeingHeld && !item.isInExchangeProcess)
            {
                itemsToCombine.Add(item);
            }
        }

        Debug.Log($"准备合成的物品数量: {itemsToCombine.Count}");

        if (itemsToCombine.Count < 2)
        {
            Debug.LogWarning("合成失败：有效物品不足2个");
            isCombining = false;
            yield break;
        }

        // 只取前2个物品
        if (itemsToCombine.Count > 2)
        {
            itemsToCombine = itemsToCombine.GetRange(0, 2);
        }

        string combineItems = "";
        foreach (var item in itemsToCombine)
        {
            combineItems += $"{item.itemName} ";
            item.isInExchangeProcess = true;
            if (item.Rb != null)
            {
                item.Rb.isKinematic = true;
                item.Rb.velocity = Vector3.zero;
            }
            Debug.Log($"锁定合成物品: {item.itemName}");
        }
        Debug.Log($"将要合成的物品: {combineItems}");

        if (synthesisEffect != null) synthesisEffect.Play();
        if (synthesisSound != null) audioSource.PlayOneShot(synthesisSound);

        yield return new WaitForSeconds(synthesisDelay);

        GameObject resultPrefab = CraftingManager.Instance.CombineItems(itemsToCombine);

        if (resultPrefab != null)
        {
            Debug.Log("=== 合成成功！ ===");

            // 从所有相关列表中移除物品
            foreach (var item in itemsToCombine)
            {
                if (item != null)
                {
                    if (itemsInZone.Contains(item)) itemsInZone.Remove(item);
                    itemEnterTimes.Remove(item);
                    Destroy(item.gameObject);
                }
            }

            yield return StartCoroutine(SpawnResultItem(resultPrefab));
            Debug.Log("=== 合成过程完成 ===");
        }
        else
        {
            Debug.LogWarning("=== 合成失败：没有匹配的配方 ===");
            foreach (var item in itemsToCombine)
            {
                if (item != null)
                {
                    item.isInExchangeProcess = false;
                    if (item.Rb != null) item.Rb.isKinematic = false;
                }
            }
        }

        isCombining = false;
        yield return new WaitForSeconds(0.5f);
        CheckForSynthesis();
    }

    public IEnumerator SpawnResultItem(GameObject resultPrefab)
    {
        Vector3 spawnPosition = itemSpawnPoint != null ? itemSpawnPoint.position : throwTarget.position + Vector3.up * 2f;
        GameObject newItemObj = Instantiate(resultPrefab, spawnPosition, Quaternion.identity);

        Rigidbody rb = newItemObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 popDirection = new Vector3(Random.Range(-0.5f, 0.5f), 1f, Random.Range(-0.5f, 0.5f)).normalized;
            rb.AddForce(popDirection * 5f, ForceMode.Impulse);
        }

        InteractableItem newItem = newItemObj.GetComponent<InteractableItem>();
        if (newItem != null)
        {
            Debug.Log($"新道具已生成: {newItem.itemName}");
        }

        yield return new WaitForSeconds(0.5f);
    }

    public void ThrowItemToZone(InteractableItem item)
    {
        StartCoroutine(ThrowItemCoroutine(item));
    }

    IEnumerator ThrowItemCoroutine(InteractableItem item)
    {
        Debug.Log($"开始抛掷物品: {item.itemName} 到区域 {zoneID}");

        item.isInExchangeProcess = true;
        if (item.Rb != null)
        {
            item.Rb.isKinematic = true;
            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
        }

        Vector3 startPosition = item.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < throwDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / throwDuration;
            Vector3 currentPos = Vector3.Lerp(startPosition, throwTarget.position, progress);
            currentPos.y += Mathf.Sin(progress * Mathf.PI) * throwHeight;
            item.transform.position = currentPos;
            yield return null;
        }

        item.transform.position = throwTarget.position;
        yield return null;

        // 确保物品被添加到区域
        if (!itemsInZone.Contains(item))
        {
            itemsInZone.Add(item);
            itemEnterTimes[item] = Time.time;
            Debug.Log($"抛掷完成后添加物品: {item.itemName} 到区域 {zoneID}");
        }

        item.isInExchangeProcess = false;
        if (item.Rb != null) item.Rb.isKinematic = false;

        Debug.Log($"物品 {item.itemName} 抛掷完成到区域 {zoneID}");
        DebugItemsInZone();
        CheckForSynthesis();
    }

    // 调试方法
    [ContextMenu("强制触发合成检测")]
    public void ForceSynthesisCheck()
    {
        Debug.Log("=== 强制触发合成检测 ===");
        ManualCheckItemsInZone();
        CheckForSynthesis();
    }

    [ContextMenu("显示区域状态")]
    public void ShowZoneStatus()
    {
        Debug.Log($"=== 合成区域 {zoneID} 状态 ===");
        Debug.Log($"物品数量: {itemsInZone.Count}");
        Debug.Log($"合成中: {isCombining}");

        foreach (var item in itemsInZone)
        {
            if (item != null)
            {
                Debug.Log($"- {item.itemName} (持有: {item.isBeingHeld}, 交换: {item.isInExchangeProcess})");
            }
        }
    }

    public void ManualCheckItemsInZone()
    {
        Debug.Log("=== 开始手动检查区域物品 ===");
        itemsInZone.RemoveAll(item => item == null);

        Collider zoneCollider = GetComponent<Collider>();
        if (zoneCollider != null)
        {
            Collider[] collidersInZone = Physics.OverlapBox(zoneCollider.bounds.center, zoneCollider.bounds.extents);
            Debug.Log($"物理检测到 {collidersInZone.Length} 个碰撞体在区域内");

            foreach (Collider collider in collidersInZone)
            {
                InteractableItem item = collider.GetComponent<InteractableItem>();
                if (item != null && !itemsInZone.Contains(item) && !ShouldIgnoreItem(item))
                {
                    itemsInZone.Add(item);
                    itemEnterTimes[item] = Time.time;
                    Debug.Log($"手动添加物品: {item.itemName}");
                }
            }
        }

        Debug.Log($"手动检查完成，区域物品数: {itemsInZone.Count}");
        DebugItemsInZone();
        CheckForSynthesis();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        if (throwTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(throwTarget.position, 0.5f);
        }
    }
    // 添加紧急解锁所有物品的方法
    [ContextMenu("紧急解锁所有物品")]
    public void EmergencyUnlockAllItems()
    {
        Debug.Log("=== 紧急解锁所有物品 ===");

        // 解锁区域内的物品
        foreach (var item in itemsInZone)
        {
            if (item != null)
            {
                item.isInExchangeProcess = false;
                item.canBePickedUp = true;
                if (item.Rb != null) item.Rb.isKinematic = false;
                Debug.Log($"解锁物品: {item.itemName}");
            }
        }

        // 清理区域列表
        itemsInZone.Clear();
        itemEnterTimes.Clear();
    }
}