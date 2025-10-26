using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class item1: MonoBehaviour
{
    [Header("教学关卡设置")]
    public string zoneID = "TutorialSynthesisZone";
    public Transform throwTarget;
    public float detectionRadius = 3f;

    [Header("抛掷设置")]
    public float throwHeight = 3f;
    public float throwDuration = 0.8f;

    [Header("UI设置")]
    public Image completionUIImage; // 合成成功后显示的UI图片
    public float uiDisplayDelay = 1f; // UI显示延迟 

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

        // 初始化UI图片为隐藏状态
        if (completionUIImage != null)
        {
            completionUIImage.gameObject.SetActive(false);
        }

        EnsureColliderSize();

        Debug.Log($"教学关卡合成区域 {zoneID} 已初始化");
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
                Debug.Log($"忽略物品 {item.itemName}  - 被持有: {item.isBeingHeld},  交换中: {item.isInExchangeProcess}");
                return;
            }

            if (!itemsInZone.Contains(item))
            {
                itemsInZone.Add(item);
                itemEnterTimes[item] = Time.time;

                Debug.Log($"物品 {item.itemName}  进入教学区域 {zoneID}，当前区域物品数: {itemsInZone.Count}");
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

            Debug.Log($"物品 {item.itemName}  离开教学区域 {zoneID}，剩余物品数: {itemsInZone.Count}");
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
                debugInfo += $"{item.itemName}  ";
            }
        }
        Debug.Log(debugInfo);
    }

    void CheckAndUpdateZoneState()
    {
        bool hasRequiredItem = HasRequiredItem();

        // 如果满足条件且尚未完成，触发完成事件 
        if (hasRequiredItem && !isTutorialCompleted)
        {
            StartCoroutine(OnTutorialCompleted());
        }

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

    // 教学关卡完成时的处理
    IEnumerator OnTutorialCompleted()
    {
        isTutorialCompleted = true;
        Debug.Log($"教学关卡 {zoneID} 完成！");

        // 播放完成效果
        PlayCompletionEffects();

        // 延迟显示UI
        yield return new WaitForSeconds(uiDisplayDelay);

        // 显示UI图片 
        ShowCompletionUI();

        // 可选：禁用区域功能 
        // DisableZoneFunctionality();
    }

    void PlayCompletionEffects()
    {
        // 播放粒子效果
        if (completionEffect != null)
        {
            completionEffect.Play();
        }

        // 播放音效
        if (completionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completionSound);
        }

        // 灯光效果
        if (completionLight != null)
        {
            completionLight.enabled = true;
            StartCoroutine(FadeLight());
        }
    }

    IEnumerator FadeLight()
    {
        if (completionLight == null) yield break;

        float duration = 2f;
        float elapsed = 0f;
        float startIntensity = completionLight.intensity;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            completionLight.intensity = Mathf.Lerp(startIntensity, 0f, elapsed / duration);
            yield return null;
        }

        completionLight.enabled = false;
    }

    void ShowCompletionUI()
    {
        if (completionUIImage != null)
        {
            completionUIImage.gameObject.SetActive(true);
            Debug.Log($"显示完成UI图片: {completionUIImage.name}");

            // 可选：添加淡入效果
            StartCoroutine(FadeInUI());
        }
        else
        {
            Debug.LogWarning("未分配完成UI图片！");
        }
    }

    IEnumerator FadeInUI()
    {
        if (completionUIImage == null) yield break;

        float duration = 1f;
        float elapsed = 0f;
        Color startColor = completionUIImage.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 1f);

        completionUIImage.color = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            completionUIImage.color = Color.Lerp(
                new Color(startColor.r, startColor.g, startColor.b, 0f),
                targetColor,
                elapsed / duration
            );
            yield return null;
        }

        completionUIImage.color = targetColor;
    }

    void DisableZoneFunctionality()
    {
        // 禁用碰撞体
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // 清空物品列表
        itemsInZone.Clear();
        itemEnterTimes.Clear();
    }

    // 抛掷物品到区域 
    public void ThrowItemToZone(InteractableItem item)
    {
        StartCoroutine(ThrowItemCoroutine(item));
    }

    IEnumerator ThrowItemCoroutine(InteractableItem item)
    {
        Debug.Log($"开始抛掷物品: {item.itemName}  到区域 {zoneID}");

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

        if (!itemsInZone.Contains(item))
        {
            itemsInZone.Add(item);
            itemEnterTimes[item] = Time.time;
            Debug.Log($"抛掷完成后添加物品: {item.itemName}  到区域 {zoneID}");
        }

        item.isInExchangeProcess = false;
        if (item.Rb != null) item.Rb.isKinematic = false;

        Debug.Log($"物品 {item.itemName}  抛掷完成到区域 {zoneID}");
        DebugItemsInZone();

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
                Debug.Log($"- {item.itemName}  (持有: {item.isBeingHeld},  交换: {item.isInExchangeProcess})");
            }
        }
    }

    [ContextMenu("测试抛掷动画")]
    public void TestThrowAnimation()
    {
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