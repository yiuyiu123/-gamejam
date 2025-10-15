using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialSynthesisZone : MonoBehaviour
{
    [Header("教学关卡设置")]
    public string zoneID = "TutorialSynthesisZone";
    public Transform throwTarget;
    public float detectionRadius = 3f;

    [Header("抛掷设置")]
    public float throwHeight = 3f;
    public float throwDuration = 0.8f;

    [Header("场景跳转设置")]
    public string targetSceneName = "NextScene";
    public float sceneTransitionDelay = 2f;

    [Header("完成效果")]
    public ParticleSystem completionEffect;
    public AudioClip completionSound;
    public Light completionLight;

    [Header("教学关卡特定物品")]
    public List<string> requiredItemNames = new List<string>();

    [Header("调试选项")]
    public bool showDebugGUI = false;

    // 事件：当区域物品状态改变时触发
    public System.Action<bool> OnItemStateChanged;

    private List<InteractableItem> itemsInZone = new List<InteractableItem>();
    private AudioSource audioSource;
    private bool isTutorialCompleted = false;
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

        Debug.Log($"教学关卡合成区域 {zoneID} 已初始化");
        Debug.Log($"目标场景: {targetSceneName}");
        Debug.Log($"需要物品: {string.Join(", ", requiredItemNames)}");
    }

    void EnsureColliderSize()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(5, 3, 5);
            Debug.Log($"为教学区域 {zoneID} 添加了触发碰撞体");
        }
        else if (!collider.isTrigger)
        {
            collider.isTrigger = true;
            Debug.Log($"已将教学区域 {zoneID} 的碰撞体设置为触发器");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTutorialCompleted) return;

        InteractableItem item = other.GetComponent<InteractableItem>();
        if (item != null)
        {
            if (ShouldIgnoreItem(item))
            {
                Debug.Log($"忽略物品 {item.itemName} - 被持有: {item.isBeingHeld}, 交换中: {item.isInExchangeProcess}");
                return;
            }

            if (!itemsInZone.Contains(item))
            {
                itemsInZone.Add(item);
                itemEnterTimes[item] = Time.time;

                Debug.Log($"物品 {item.itemName} 进入教学区域 {zoneID}，当前区域物品数: {itemsInZone.Count}");
                DebugItemsInZone();

                // 立即检查是否满足条件
                CheckAndUpdateZoneState();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (isTutorialCompleted) return;

        InteractableItem item = other.GetComponent<InteractableItem>();
        if (item != null && itemsInZone.Contains(item))
        {
            itemsInZone.Remove(item);
            itemEnterTimes.Remove(item);

            Debug.Log($"物品 {item.itemName} 离开教学区域 {zoneID}，剩余物品数: {itemsInZone.Count}");
            DebugItemsInZone();

            // 检查状态并更新
            CheckAndUpdateZoneState();
        }
    }

    bool ShouldIgnoreItem(InteractableItem item)
    {
        if (item == null) return true;
        if (item.isBeingHeld) return true;
        if (item.isInExchangeProcess) return true;

        return false;
    }

    void DebugItemsInZone()
    {
        string debugInfo = $"教学区域 {zoneID} 物品列表 ({itemsInZone.Count} 个): ";
        foreach (var item in itemsInZone)
        {
            if (item != null)
            {
                debugInfo += $"{item.itemName} ";
            }
        }
        Debug.Log(debugInfo);
    }

    void CheckAndUpdateZoneState()
    {
        bool hasRequiredItem = HasRequiredItem();

        // 触发事件
        OnItemStateChanged?.Invoke(hasRequiredItem);
    }

    bool HasRequiredItem()
    {
        List<InteractableItem> validItems = new List<InteractableItem>();
        foreach (var item in itemsInZone)
        {
            if (item != null && !item.isBeingHeld && !item.isInExchangeProcess)
            {
                validItems.Add(item);
            }
        }

        if (requiredItemNames.Count == 0)
        {
            return validItems.Count >= 1; // 至少有一个物品
        }

        List<string> currentItemNames = new List<string>();
        foreach (var item in validItems)
        {
            if (item != null)
            {
                currentItemNames.Add(item.itemName);
            }
        }

        // 检查是否包含所有必需物品
        foreach (string requiredName in requiredItemNames)
        {
            if (!currentItemNames.Contains(requiredName))
            {
                return false;
            }
        }

        return true;
    }

    // 抛掷物品到区域 - 完全按照 SynthesisZone 的实现
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

        // 使用与 SynthesisZone 完全相同的抛掷动画
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

        // 检查状态并更新
        CheckAndUpdateZoneState();
    }

    // 调试方法
    [ContextMenu("显示区域状态")]
    public void ShowZoneStatus()
    {
        Debug.Log($"=== 教学区域 {zoneID} 状态 ===");
        Debug.Log($"物品数量: {itemsInZone.Count}");
        Debug.Log($"教学关卡完成: {isTutorialCompleted}");
        Debug.Log($"需要物品: {string.Join(", ", requiredItemNames)}");
        Debug.Log($"是否满足条件: {HasRequiredItem()}");

        foreach (var item in itemsInZone)
        {
            if (item != null)
            {
                Debug.Log($"- {item.itemName} (持有: {item.isBeingHeld}, 交换: {item.isInExchangeProcess})");
            }
        }
    }

    [ContextMenu("测试抛掷动画")]
    public void TestThrowAnimation()
    {
        // 查找附近的物品进行测试
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider collider in colliders)
        {
            InteractableItem item = collider.GetComponent<InteractableItem>();
            if (item != null && !item.isBeingHeld)
            {
                StartCoroutine(ThrowItemCoroutine(item));
                break;
            }
        }
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
}