using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractableItem : MonoBehaviour
{
    [Header("物品设置")]
    public string itemName = "物品";
    [TextArea(3, 5)]
    public string itemDescription = "";
    public bool canBePickedUp = true;
    public Vector3 holdOffset = new Vector3(0, 1, 1);

    [Header("关键道具设置")]
    public bool isKeyItem = false;
    public bool showPickupMessage = true;

    [Header("UI提示设置")]
    public GameObject pickupUIPrefab;
    public float displayTime = 3f;
    public float fadeDuration = 0.5f;

    [Header("UI位置设置")]
    [Range(0f, 1f)]
    public float uiVerticalPosition = 0.3f;
    public float uiHorizontalOffset = 0f;

    [Header("UI垂直位置微调")]
    [Tooltip("垂直位置的基准偏移（屏幕高度的百分比）")]
    [Range(0f, 0.5f)]
    public float verticalBaseOffset = 0.1f;

    [Tooltip("垂直位置的最大调整范围（屏幕高度的百分比）")]
    [Range(0f, 0.3f)]
    public float verticalAdjustRange = 0.2f;

    [Tooltip("根据玩家位置自动微调垂直位置")]
    public bool autoAdjustByPlayer = true;

    [Tooltip("自动微调的强度")]
    [Range(0f, 0.1f)]
    public float autoAdjustStrength = 0.05f;

    [Header("提示内容设置")]
    public string pickupMessageFormat = "找到了 {0}";
    [TextArea(2, 4)]
    public string customPickupMessage = "";

    [Header("持握角度设置")]
    public Vector3 holdRotationOffset = Vector3.zero;

    [Header("传送设置")]
    public bool canBeExchanged = true;
    public bool isExchangeLocked = false;
    public string lastExchangeZone = "";

    [Header("交换次数限制")]
    public bool limitExchangeTimes = true;
    public int maxExchangeTimes = 1;
    public int currentExchangeTimes = 0;

    [Header("状态")]
    public bool isBeingHeld = false;
    public bool isInExchangeProcess = false;
    public GameObject currentHolder = null;

    [Header("拾取动画设置")]
    public float pickUpTransitionTime = 0.3f;
    public AnimationCurve pickUpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine transitionCoroutine;
    private bool isTransitioning = false;

    private Rigidbody rb;
    private Collider itemCollider;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private ExchangeZone currentZone;

    // UI相关变量
    private Dictionary<GameObject, GameObject> playerUIInstances = new Dictionary<GameObject, GameObject>();
    private Dictionary<GameObject, Coroutine> playerUICoroutines = new Dictionary<GameObject, Coroutine>();

    // 添加公共属性来访问私有字段
    public Rigidbody Rb => rb;
    public Collider ItemCollider => itemCollider;
    public Vector3 OriginalPosition => originalPosition;
    public Quaternion OriginalRotation => originalRotation;
    public Vector3 OriginalScale => originalScale;

    // 添加销毁标记 - 核心修复
    private bool isBeingDestroyed = false;

    void Start()
    {
        // 更安全的初始化
        try
        {
            rb = GetComponent<Rigidbody>();
            itemCollider = GetComponent<Collider>();

            // 检查必要的组件
            if (rb == null)
            {
                Debug.LogWarning($"物品 {itemName} 缺少 Rigidbody 组件", this);
            }

            if (itemCollider == null)
            {
                Debug.LogWarning($"物品 {itemName} 缺少 Collider 组件", this);
            }

            originalPosition = transform.position;
            originalRotation = transform.rotation;
            originalScale = transform.localScale;

            currentExchangeTimes = 0;

            // 更安全的 UI 预制体检查
            if (pickupUIPrefab == null)
            {
                Debug.LogWarning($"物品 {itemName} 的 pickupUIPrefab 未设置！请在 Inspector 中分配 UI 预制体。", this);
                // 禁用 UI 相关功能但不影响其他功能
                showPickupMessage = false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"物品 {itemName} 初始化失败: {e.Message}", this);
        }
    }

    void Update()
    {
        if (isBeingHeld && currentHolder != null && !isInExchangeProcess && !isBeingDestroyed)
        {
            FollowHolder();
        }

        CheckIfLeftZone();
    }

    void LateUpdate()
    {
        if (isBeingHeld && !isBeingDestroyed)
        {
            transform.localScale = originalScale;
        }
    }

    public void Interact(GameObject player)
    {
        if (!canBePickedUp || isInExchangeProcess || isBeingDestroyed) return;

        if (!isBeingHeld)
        {
            PickUp(player);
        }
        else
        {
            PutDown();
        }
    }

    void PickUp(GameObject player)
    {
        if (!canBePickedUp || isTransitioning || player == null || isBeingDestroyed) return;

        isBeingHeld = true;
        currentHolder = player;
        originalScale = transform.localScale;

        ResetExchangeLock();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }

        if (isKeyItem && showPickupMessage)
        {
            ShowPickupUI(player);
        }

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(PickUpTransition(player));

        Debug.Log($"{player.name} 拿起了 {itemName}");
    }

    // 显示拾取UI提示
    private void ShowPickupUI(GameObject player)
    {
        if (isBeingDestroyed || player == null) return;

        if (playerUICoroutines.ContainsKey(player) && playerUICoroutines[player] != null)
        {
            StopCoroutine(playerUICoroutines[player]);
        }

        GameObject uiInstance = GetOrCreateUIInstance(player);

        if (uiInstance != null)
        {
            playerUICoroutines[player] = StartCoroutine(ShowPickupMessageRoutine(player, uiInstance));
        }
    }

    // 获取或创建UI实例 - 添加更多安全检查
    private GameObject GetOrCreateUIInstance(GameObject player)
    {
        if (isBeingDestroyed || player == null)
        {
            return null;
        }

        // 安全的字典清理
        SafeCleanupDictionaries();

        if (!playerUIInstances.ContainsKey(player) || playerUIInstances[player] == null)
        {
            if (pickupUIPrefab != null)
            {
                GameObject uiInstance = Instantiate(pickupUIPrefab);

                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    uiInstance.transform.SetParent(canvas.transform, false);
                }
                else
                {
                    Debug.LogWarning("场景中未找到 Canvas，UI 提示可能无法正确显示");
                }

                RectTransform rectTransform = uiInstance.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    SetupUIPosition(player, rectTransform);
                }

                playerUIInstances[player] = uiInstance;
            }
            else
            {
                Debug.LogWarning("Pickup UI Prefab 未设置！");
                return null;
            }
        }

        return playerUIInstances[player];
    }

    // 安全的字典清理方法
    private void SafeCleanupDictionaries()
    {
        // 使用临时列表记录需要移除的键
        List<GameObject> keysToRemove = new List<GameObject>();

        // 清理 UI 实例字典
        foreach (var pair in playerUIInstances)
        {
            if (pair.Value == null)
                keysToRemove.Add(pair.Key);
        }
        foreach (var key in keysToRemove)
        {
            playerUIInstances.Remove(key);
        }

        keysToRemove.Clear();

        // 清理协程字典
        foreach (var pair in playerUICoroutines)
        {
            if (pair.Value == null)
                keysToRemove.Add(pair.Key);
        }
        foreach (var key in keysToRemove)
        {
            playerUICoroutines.Remove(key);
        }
    }

    // 设置UI位置的方法
    private void SetupUIPosition(GameObject player, RectTransform rectTransform)
    {
        int playerIndex = GetPlayerIndex(player);
        Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
        float canvasHeight = canvas != null ? canvas.GetComponent<RectTransform>().rect.height : Screen.height;

        // 计算基础垂直位置
        float baseVerticalPos = verticalBaseOffset * canvasHeight;

        // 计算调整范围
        float adjustRange = verticalAdjustRange * canvasHeight;

        // 计算最终垂直位置
        float finalVerticalPos = baseVerticalPos + (uiVerticalPosition * adjustRange);

        // 自动根据玩家位置微调
        if (autoAdjustByPlayer && player != null)
        {
            float playerScreenY = GetPlayerScreenPosition(player).y;
            float screenAdjust = (playerScreenY - 0.5f) * autoAdjustStrength * canvasHeight;
            finalVerticalPos += screenAdjust;

            // 限制在合理范围内
            finalVerticalPos = Mathf.Clamp(finalVerticalPos, 50f, canvasHeight * 0.8f);
        }

        if (playerIndex == 0) // 玩家1 - 左屏
        {
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(uiHorizontalOffset, finalVerticalPos);
        }
        else // 玩家2 - 右屏
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(uiHorizontalOffset, finalVerticalPos);
        }
    }

    // 获取玩家屏幕位置
    private Vector2 GetPlayerScreenPosition(GameObject player)
    {
        if (player == null) return new Vector2(0.5f, 0.5f);

        Camera camera = Camera.main;
        if (camera != null)
        {
            Vector3 screenPos = camera.WorldToViewportPoint(player.transform.position);
            return new Vector2(screenPos.x, screenPos.y);
        }

        return new Vector2(0.5f, 0.5f);
    }

    // 获取玩家索引
    private int GetPlayerIndex(GameObject player)
    {
        if (player.name.Contains("Player1") || player.CompareTag("Player1"))
            return 0;
        else if (player.name.Contains("Player2") || player.CompareTag("Player2"))
            return 1;
        else
        {
            Debug.LogWarning($"无法确定玩家索引: {player.name}");
            return 0;
        }
    }

    // 生成提示消息
    private string GeneratePickupMessage()
    {
        if (!string.IsNullOrEmpty(customPickupMessage))
        {
            return customPickupMessage;
        }

        if (!string.IsNullOrEmpty(pickupMessageFormat))
        {
            return string.Format(pickupMessageFormat, itemName);
        }

        return $"找到了 {itemName}";
    }

    // 显示拾取消息的协程 - 核心修复：添加销毁检查
    private IEnumerator ShowPickupMessageRoutine(GameObject player, GameObject uiInstance)
    {
        if (uiInstance == null || player == null || isBeingDestroyed)
            yield break;

        TextMeshProUGUI textComponent = uiInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent == null)
        {
            Debug.LogWarning("UI实例中未找到TextMeshProUGUI组件！");
            yield break;
        }

        textComponent.text = GeneratePickupMessage();
        textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 1f);
        uiInstance.SetActive(true);

        // 使用计时器而不是 WaitForSeconds，以便每帧检查销毁状态
        float displayTimer = 0f;
        while (displayTimer < displayTime)
        {
            if (uiInstance == null || textComponent == null || isBeingDestroyed)
                yield break;

            displayTimer += Time.deltaTime;
            yield return null;
        }

        float elapsedTime = 0f;
        Color startColor = textComponent.color;

        while (elapsedTime < fadeDuration)
        {
            if (uiInstance == null || textComponent == null || isBeingDestroyed)
                yield break;

            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            textComponent.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        if (uiInstance != null && textComponent != null && !isBeingDestroyed)
        {
            textComponent.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
            uiInstance.SetActive(false);
        }

        // 只有在物品没有被销毁时才修改字典
        if (!isBeingDestroyed && playerUICoroutines.ContainsKey(player))
        {
            playerUICoroutines[player] = null;
        }
    }

    // 创建默认UI预制体
    private void CreateDefaultUIPrefab()
    {
        Debug.Log("请设置Pickup UI Prefab或在编辑器中创建UI元素");
    }

    IEnumerator PickUpTransition(GameObject player)
    {
        isTransitioning = true;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < pickUpTransitionTime && currentHolder != null && !isBeingDestroyed)
        {
            elapsedTime += Time.deltaTime;
            float t = pickUpCurve.Evaluate(elapsedTime / pickUpTransitionTime);

            Vector3 targetPosition = GetTargetHoldPosition();
            Quaternion targetRotation = GetTargetHoldRotation();

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        if (currentHolder != null && !isBeingDestroyed)
        {
            transform.position = GetTargetHoldPosition();
            transform.rotation = GetTargetHoldRotation();
        }

        isTransitioning = false;
    }

    public void ResetItemState()
    {
        if (isBeingDestroyed) return;

        isBeingHeld = false;
        isInExchangeProcess = false;
        canBePickedUp = true;
        currentHolder = null;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }
    }

    public void PutDown()
    {
        if (!isBeingHeld || isBeingDestroyed) return;

        isBeingHeld = false;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }

        transform.localScale = originalScale;

        Debug.Log($"{currentHolder?.name} 放下了 {itemName}");
        currentHolder = null;
    }

    void FollowHolder()
    {
        if (currentHolder == null || isTransitioning || isBeingDestroyed) return;

        transform.position = GetTargetHoldPosition();
        transform.rotation = GetTargetHoldRotation();
        transform.localScale = originalScale;
    }

    Vector3 GetTargetHoldPosition()
    {
        return currentHolder.transform.position +
               currentHolder.transform.forward * holdOffset.z +
               currentHolder.transform.up * holdOffset.y +
               currentHolder.transform.right * holdOffset.x;
    }

    Quaternion GetTargetHoldRotation()
    {
        return currentHolder.transform.rotation * Quaternion.Euler(holdRotationOffset);
    }

    [Header("调试选项")]
    public bool showDebugInfo = false;

    void CheckIfLeftZone()
    {
        if (currentZone != null && !isBeingHeld && !isBeingDestroyed)
        {
            float distance = Vector3.Distance(transform.position, currentZone.transform.position);
            if (distance > currentZone.detectionRadius * 1.2f)
            {
                OnLeftZone();
            }
        }
    }

    void OnLeftZone()
    {
        if (currentZone != null && !isBeingDestroyed)
        {
            Debug.Log($"物品 {itemName} 离开了区域 {currentZone.zoneID}");
            currentZone.OnItemLeft(this.gameObject);
            currentZone = null;
            ResetExchangeLock();
        }
    }

    public void MarkAsExchanged(string fromZoneID)
    {
        if (isBeingDestroyed) return;

        isExchangeLocked = true;
        lastExchangeZone = fromZoneID;

        if (limitExchangeTimes)
        {
            currentExchangeTimes++;
            Debug.Log($"物品 {itemName} 被标记为已交换，来自区域 {fromZoneID}，交换次数: {currentExchangeTimes}/{maxExchangeTimes}");

            if (currentExchangeTimes >= maxExchangeTimes)
            {
                canBeExchanged = false;
                Debug.Log($"物品 {itemName} 已达到最大交换次数 ({maxExchangeTimes})，禁止再次交换");
            }
        }
        else
        {
            Debug.Log($"物品 {itemName} 被标记为已交换，来自区域 {fromZoneID}");
        }
    }

    public void ResetExchangeLock()
    {
        if (isExchangeLocked && !isBeingDestroyed)
        {
            isExchangeLocked = false;
            lastExchangeZone = "";
            Debug.Log($"物品 {itemName} 的交换锁定已重置");
        }
    }

    public bool CanExchangeTo(string targetZoneID)
    {
        if (isBeingDestroyed) return false;

        if (limitExchangeTimes && currentExchangeTimes >= maxExchangeTimes)
        {
            if (showDebugInfo) Debug.Log($"物品 {itemName} 已达到最大交换次数，无法交换");
            return false;
        }

        if (!canBeExchanged || isExchangeLocked || isBeingHeld || isInExchangeProcess)
            return false;

        if (lastExchangeZone == targetZoneID)
            return false;

        return true;
    }

    public void SetCurrentZone(ExchangeZone zone)
    {
        if (isBeingDestroyed) return;
        currentZone = zone;
    }

    public void ResetItem()
    {
        if (isBeingDestroyed) return;

        PutDown();
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;
        isInExchangeProcess = false;
        ResetExchangeLock();
    }

    public void ResetExchangeTimes()
    {
        if (isBeingDestroyed) return;

        currentExchangeTimes = 0;
        canBeExchanged = true;
        Debug.Log($"物品 {itemName} 的交换次数已重置");
    }

    public void SetExchangeLimit(int maxTimes)
    {
        if (isBeingDestroyed) return;

        maxExchangeTimes = maxTimes;
        limitExchangeTimes = true;
        Debug.Log($"物品 {itemName} 的交换次数限制设置为: {maxTimes}");
    }

    public void RemoveExchangeLimit()
    {
        if (isBeingDestroyed) return;

        limitExchangeTimes = false;
        Debug.Log($"物品 {itemName} 的交换次数限制已移除");
    }

    public void ResetPhysics()
    {
        if (rb != null && !isBeingDestroyed)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void ForceRelease()
    {
        if (isBeingHeld && !isBeingDestroyed)
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                isTransitioning = false;
            }

            isBeingHeld = false;
            currentHolder = null;

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            if (itemCollider != null)
            {
                itemCollider.enabled = true;
            }

            Debug.Log($"强制释放物品: {itemName}");
        }
    }

    // 核心修复：安全的销毁方法
    void OnDestroy()
    {
        isBeingDestroyed = true;
        SafeStopAllCoroutines();
        SafeDestroyAllUI();

        playerUIInstances.Clear();
        playerUICoroutines.Clear();
    }

    // 修复 OnDisable 方法 - 使用副本遍历字典
    void OnDisable()
    {
        if (isBeingDestroyed) return;

        // 使用副本遍历避免修改字典时的异常
        var coroutineEntries = new List<KeyValuePair<GameObject, Coroutine>>(playerUICoroutines);
        foreach (var entry in coroutineEntries)
        {
            if (entry.Value != null)
            {
                StopCoroutine(entry.Value);
                playerUICoroutines[entry.Key] = null;
            }
        }
    }

    // 安全的协程停止方法
    private void SafeStopAllCoroutines()
    {
        // 使用副本遍历避免修改字典时的异常
        var coroutineEntries = new List<KeyValuePair<GameObject, Coroutine>>(playerUICoroutines);

        foreach (var entry in coroutineEntries)
        {
            if (entry.Value != null)
            {
                StopCoroutine(entry.Value);
            }
        }

        // 停止物品自身的过渡协程
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }
    }

    // 安全的UI清理方法
    private void SafeDestroyAllUI()
    {
        // 使用副本遍历避免修改字典时的异常
        var uiEntries = new List<KeyValuePair<GameObject, GameObject>>(playerUIInstances);

        foreach (var entry in uiEntries)
        {
            if (entry.Value != null)
            {
                Destroy(entry.Value);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (isBeingHeld && currentHolder != null)
        {
            Gizmos.color = isExchangeLocked ? Color.red : (isInExchangeProcess ? Color.yellow : Color.green);
            Vector3 holdPosition = currentHolder.transform.position +
                                  currentHolder.transform.forward * holdOffset.z +
                                  currentHolder.transform.up * holdOffset.y +
                                  currentHolder.transform.right * holdOffset.x;
            Gizmos.DrawWireSphere(holdPosition, 0.2f);
        }

        if (isExchangeLocked)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.2f);
        }

        if (limitExchangeTimes && showDebugInfo)
        {
#if UNITY_EDITOR
            string exchangeInfo = $"{itemName}\n交换: {currentExchangeTimes}/{maxExchangeTimes}";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, exchangeInfo);
#endif
        }
    }
}