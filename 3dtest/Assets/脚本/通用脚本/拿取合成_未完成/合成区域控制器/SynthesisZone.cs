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

    [Header("合成冷却时间")]
    public float synthesisCooldown = 2f;

    [Header("合成失败弹出设置")]
    public bool enableFailEjection = true; // 是否启用失败弹出
    public float failEjectionForce = 8f; // 弹出力度
    public float failEjectionHeight = 2f; // 弹出高度
    public float failEjectionRandomness = 0.5f; // 弹出随机性
    public ParticleSystem failEffect; // 失败效果
    public AudioClip failSound; // 失败音效

    private List<InteractableItem> itemsInZone = new List<InteractableItem>();
    private AudioSource audioSource;
    private bool isCombining = false;
    private bool hasCheckedThisFrame = false;
    private float lastSynthesisTime = 0f;
    private Dictionary<InteractableItem, float> itemEnterTimes = new Dictionary<InteractableItem, float>();
    private Dictionary<InteractableItem, float> failedItems = new Dictionary<InteractableItem, float>();

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
            CleanupFailedItems();

            if (Time.time - lastSynthesisTime >= synthesisCooldown)
            {
                CheckForSynthesis();
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
                itemEnterTimes[item] = Time.time;

                if (Time.time - lastSynthesisTime >= synthesisCooldown)
                {
                    CheckForSynthesis();
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
            itemEnterTimes.Remove(item);
        }
    }

    bool ShouldProcessItem(InteractableItem item)
    {
        if (item == null) return false;
        if (item.isBeingHeld) return false;
        if (item.isInExchangeProcess) return false;

        if (failedItems.ContainsKey(item))
        {
            float timeSinceFail = Time.time - failedItems[item];
            if (timeSinceFail < synthesisCooldown)
            {
                return false;
            }
            else
            {
                failedItems.Remove(item);
            }
        }

        return true;
    }

    void CleanupFailedItems()
    {
        List<InteractableItem> toRemove = new List<InteractableItem>();
        foreach (var kvp in failedItems)
        {
            if (Time.time - kvp.Value >= synthesisCooldown)
            {
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var item in toRemove)
        {
            failedItems.Remove(item);
        }
    }

    void CheckForSynthesis()
    {
        if (isCombining) return;
        if (Time.time - lastSynthesisTime < synthesisCooldown) return;

        itemsInZone.RemoveAll(item => item == null || item.isBeingHeld);

        List<InteractableItem> validItems = new List<InteractableItem>();
        foreach (var item in itemsInZone)
        {
            if (item != null && !item.isBeingHeld && !item.isInExchangeProcess)
            {
                validItems.Add(item);
            }
        }

        if (validItems.Count >= 2)
        {
            StartCoroutine(PerformSynthesis());
        }
    }

    IEnumerator PerformSynthesis()
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

        if (itemsToCombine.Count < 2)
        {
            isCombining = false;
            yield break;
        }

        if (itemsToCombine.Count > 2)
        {
            itemsToCombine = itemsToCombine.GetRange(0, 2);
        }

        // 锁定物品
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
        if (synthesisEffect != null) synthesisEffect.Play();
        if (synthesisSound != null) audioSource.PlayOneShot(synthesisSound);

        yield return new WaitForSeconds(synthesisDelay);

        CraftingRecipe matchedRecipe = CraftingManager.Instance.CombineItems(itemsToCombine);

        if (matchedRecipe != null && matchedRecipe.resultItemPrefab != null)
        {
            // 合成成功
            foreach (var item in itemsToCombine)
            {
                if (item != null)
                {
                    itemsInZone.Remove(item);
                    itemEnterTimes.Remove(item);
                    Destroy(item.gameObject);
                }
            }

            Vector3 spawnPosition = GetRecipeSpawnPosition(matchedRecipe);
            yield return StartCoroutine(SpawnResultItemWithPosition(matchedRecipe.resultItemPrefab, spawnPosition));
        }
        else
        {
            // 合成失败 - 弹出物品
            Debug.LogWarning($"合成失败，将弹出 {itemsToCombine.Count} 个物品");

            // 播放失败效果
            if (failEffect != null) failEffect.Play();
            if (failSound != null) audioSource.PlayOneShot(failSound);

            // 弹出所有参与合成的物品
            yield return StartCoroutine(EjectFailedItems(itemsToCombine));
        }

        isCombining = false;
    }

    // 新增：弹出失败物品的协程
    IEnumerator EjectFailedItems(List<InteractableItem> itemsToEject)
    {
        Debug.Log($"开始弹出 {itemsToEject.Count} 个失败物品");

        foreach (var item in itemsToEject)
        {
            if (item != null)
            {
                // 标记为失败物品
                failedItems[item] = Time.time;

                // 从区域列表中移除
                itemsInZone.Remove(item);
                itemEnterTimes.Remove(item);

                // 解锁物品状态
                item.isInExchangeProcess = false;

                // 启用物理
                if (item.Rb != null)
                {
                    item.Rb.isKinematic = false;
                    item.Rb.useGravity = true;

                    // 计算弹出方向（从区域中心向外，带随机性）
                    Vector3 ejectionDirection = CalculateEjectionDirection(item.transform.position);

                    // 应用弹出力
                    item.Rb.AddForce(ejectionDirection * failEjectionForce, ForceMode.Impulse);

                    // 添加一些随机旋转
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

    // 计算弹出方向
    Vector3 CalculateEjectionDirection(Vector3 itemPosition)
    {
        // 基本方向：从区域中心指向物品位置
        Vector3 baseDirection = (itemPosition - transform.position).normalized;

        // 确保有向上的分量
        baseDirection.y = Mathf.Max(baseDirection.y, 0.3f);

        // 添加随机性
        Vector3 randomVariation = new Vector3(
            Random.Range(-failEjectionRandomness, failEjectionRandomness),
            Random.Range(0, failEjectionRandomness * 0.5f), // 减少向下的随机性
            Random.Range(-failEjectionRandomness, failEjectionRandomness)
        );

        Vector3 finalDirection = (baseDirection + randomVariation).normalized;
        finalDirection.y += failEjectionHeight * 0.1f; // 添加高度因子

        return finalDirection;
    }

    private Vector3 GetRecipeSpawnPosition(CraftingRecipe recipe)
    {
        if (GlobalSynthesisManager.Instance != null)
        {
            return GlobalSynthesisManager.Instance.GetGlobalSpawnPosition();
        }
        return itemSpawnPoint != null ? itemSpawnPoint.position : throwTarget.position + Vector3.up * 2f;
    }

    public IEnumerator SpawnResultItemWithPosition(GameObject resultPrefab, Vector3 spawnPosition)
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

        if (!itemsInZone.Contains(item))
        {
            itemsInZone.Add(item);
            itemEnterTimes[item] = Time.time;
        }

        item.isInExchangeProcess = false;
        if (item.Rb != null) item.Rb.isKinematic = false;
    }

    public List<InteractableItem> GetItemsInZone()
    {
        itemsInZone.RemoveAll(item =>
            item == null ||
            item.isBeingHeld ||
            item.isInExchangeProcess
        );
        return new List<InteractableItem>(itemsInZone);
    }

    public void RemoveItemFromZone(InteractableItem item)
    {
        if (itemsInZone.Contains(item))
        {
            itemsInZone.Remove(item);
            itemEnterTimes.Remove(item);
            failedItems.Remove(item);
        }
    }

    [ContextMenu("测试弹出效果")]
    public void TestEjection()
    {
        List<InteractableItem> testItems = new List<InteractableItem>();
        foreach (var item in itemsInZone)
        {
            if (item != null && testItems.Count < 2)
            {
                testItems.Add(item);
            }
        }

        if (testItems.Count > 0)
        {
            StartCoroutine(EjectFailedItems(testItems));
            Debug.Log($"测试弹出 {testItems.Count} 个物品");
        }
        else
        {
            Debug.LogWarning("没有物品可用于测试弹出");
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
        itemEnterTimes.Clear();
        failedItems.Clear();
        isCombining = false;
    }
}