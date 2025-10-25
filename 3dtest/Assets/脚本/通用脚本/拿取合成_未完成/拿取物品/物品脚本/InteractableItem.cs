using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractableItem : MonoBehaviour
{
    [Header("物品设置")]
    public string itemName = "物品";
    [TextArea(3, 5)] // 添加多行文本区域，方便输入更长的描述
    public string itemDescription = ""; // 物品描述
    public bool canBePickedUp = true;
    public Vector3 holdOffset = new Vector3(0, 1, 1);

    [Header("关键道具设置")]
    public bool isKeyItem = false; // 是否为关键道具
    public bool showPickupMessage = true; // 是否显示拾取提示

    [Header("UI提示设置")]
    public GameObject pickupUIPrefab; // UI预制体
    public float displayTime = 3f; // 显示时间
    public float fadeDuration = 0.5f; // 淡出时间

    [Header("UI位置设置")]
    [Range(0f, 1f)]
    public float uiVerticalPosition = 0.3f; // UI垂直位置（屏幕高度的比例，0=底部，1=顶部）
    public float uiHorizontalOffset = 0f; // 水平微调偏移

    [Header("提示内容设置")]
    public string pickupMessageFormat = "找到了 {0}"; // 提示消息格式，{0}会被itemName替换
    [TextArea(2, 4)]
    public string customPickupMessage = ""; // 自定义提示消息，如果为空则使用默认格式

    [Header("持握角度设置")]
    public Vector3 holdRotationOffset = Vector3.zero; // 持握时的角度偏移

    [Header("传送设置")]
    public bool canBeExchanged = true;
    public bool isExchangeLocked = false;
    public string lastExchangeZone = "";

    [Header("交换次数限制")]
    public bool limitExchangeTimes = true;  // 是否限制交换次数
    public int maxExchangeTimes = 1;        // 最大交换次数
    public int currentExchangeTimes = 0;    // 当前交换次数

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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;

        // 初始化交换次数
        currentExchangeTimes = 0;

        // 如果没有设置UI预制体，尝试动态创建一个简单的UI
        if (pickupUIPrefab == null)
        {
            CreateDefaultUIPrefab();
        }
    }

    void Update()
    {
        if (isBeingHeld && currentHolder != null && !isInExchangeProcess)
        {
            FollowHolder();
        }

        CheckIfLeftZone();
    }

    void LateUpdate()
    {
        // 在每帧的最后强制保持缩放，确保覆盖任何其他可能的缩放修改
        if (isBeingHeld)
        {
            transform.localScale = originalScale;
        }
    }

    // 在 Interact 方法中添加检查
    public void Interact(GameObject player)
    {
        if (!canBePickedUp || isInExchangeProcess) return; // 添加 canBePickedUp 检查

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
        if (!canBePickedUp || isTransitioning || player == null) return;

        // 立即设置基础状态，避免逻辑错误
        isBeingHeld = true;
        currentHolder = player;
        originalScale = transform.localScale;

        ResetExchangeLock();

        // 禁用物理效果
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }

        // 显示UI提示
        if (isKeyItem && showPickupMessage)
        {
            ShowPickupUI(player);
        }

        // 开始过渡动画
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(PickUpTransition(player));

        Debug.Log($"{player.name} 拿起了 {itemName}");
    }

    // 显示拾取UI提示
    private void ShowPickupUI(GameObject player)
    {
        // 如果已经有UI在显示，先停止之前的协程
        if (playerUICoroutines.ContainsKey(player) && playerUICoroutines[player] != null)
        {
            StopCoroutine(playerUICoroutines[player]);
        }

        // 创建或获取UI实例
        GameObject uiInstance = GetOrCreateUIInstance(player);

        if (uiInstance != null)
        {
            // 开始显示UI的协程
            playerUICoroutines[player] = StartCoroutine(ShowPickupMessageRoutine(player, uiInstance));
        }
    }

    // 获取或创建UI实例
    private GameObject GetOrCreateUIInstance(GameObject player)
    {
        // 清理已销毁的引用
        if (playerUIInstances.ContainsKey(player) && playerUIInstances[player] == null)
        {
            playerUIInstances.Remove(player);
        }

        if (playerUICoroutines.ContainsKey(player) && playerUICoroutines[player] == null)
        {
            playerUICoroutines.Remove(player);
        }

        if (!playerUIInstances.ContainsKey(player) || playerUIInstances[player] == null)
        {
            if (pickupUIPrefab != null)
            {
                // 创建新的UI实例
                GameObject uiInstance = Instantiate(pickupUIPrefab);

                // 设置UI的父对象为Canvas
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    uiInstance.transform.SetParent(canvas.transform, false);
                }

                // 设置UI位置和锚点
                RectTransform rectTransform = uiInstance.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    int playerIndex = GetPlayerIndex(player);

                    if (playerIndex == 0) // 玩家1 - 左屏
                    {
                        // 左屏底部居中：水平0-50%，垂直底部
                        rectTransform.anchorMin = new Vector2(0f, 0f);
                        rectTransform.anchorMax = new Vector2(0.5f, 0f);
                        rectTransform.pivot = new Vector2(0.5f, 0f);
                        rectTransform.anchoredPosition = new Vector2(0, uiVerticalPosition * 100);
                    }
                    else // 玩家2 - 右屏
                    {
                        // 右屏底部居中：水平50%-100%，垂直底部
                        rectTransform.anchorMin = new Vector2(0.5f, 0f);
                        rectTransform.anchorMax = new Vector2(1f, 0f);
                        rectTransform.pivot = new Vector2(0.5f, 0f);
                        rectTransform.anchoredPosition = new Vector2(0, uiVerticalPosition * 100);
                    }
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

    // 获取玩家索引
    private int GetPlayerIndex(GameObject player)
    {
        // 根据玩家对象的名称或标签判断是哪个玩家
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
        // 如果有自定义消息，优先使用
        if (!string.IsNullOrEmpty(customPickupMessage))
        {
            return customPickupMessage;
        }

        // 否则使用格式化消息
        if (!string.IsNullOrEmpty(pickupMessageFormat))
        {
            return string.Format(pickupMessageFormat, itemName);
        }

        // 默认消息
        return $"找到了 {itemName}";
    }

    // 显示拾取消息的协程
    private IEnumerator ShowPickupMessageRoutine(GameObject player, GameObject uiInstance)
    {
        // 立即检查对象是否有效
        if (uiInstance == null || player == null)
            yield break;

        // 获取文本组件
        TextMeshProUGUI textComponent = uiInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent == null)
        {
            Debug.LogWarning("UI实例中未找到TextMeshProUGUI组件！");
            yield break;
        }

        // 设置文本内容
        textComponent.text = GeneratePickupMessage();

        // 立即显示（不透明）
        textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 1f);
        uiInstance.SetActive(true);

        // 等待显示时间
        yield return new WaitForSeconds(displayTime);

        // 淡出效果
        float elapsedTime = 0f;
        Color startColor = textComponent.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsedTime < fadeDuration)
        {
            // 每帧都检查对象是否仍然有效
            if (uiInstance == null || textComponent == null)
                yield break;

            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            textComponent.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // 完全透明后隐藏UI，检查对象是否仍然有效
        if (uiInstance != null && textComponent != null)
        {
            textComponent.color = targetColor;
            uiInstance.SetActive(false);
        }

        // 清理协程引用
        if (playerUICoroutines.ContainsKey(player))
        {
            playerUICoroutines[player] = null;
        }
    }

    // 创建默认UI预制体（如果没有设置的话）
    private void CreateDefaultUIPrefab()
    {
        // 这里可以创建一个简单的UI预制体
        // 在实际项目中，建议在编辑器中设置好UI预制体
        Debug.Log("请设置Pickup UI Prefab或在编辑器中创建UI元素");
    }

    IEnumerator PickUpTransition(GameObject player)
    {
        isTransitioning = true;

        // 保存初始状态
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < pickUpTransitionTime && currentHolder != null)
        {
            elapsedTime += Time.deltaTime;
            float t = pickUpCurve.Evaluate(elapsedTime / pickUpTransitionTime);

            // 计算当前帧的目标位置
            Vector3 targetPosition = GetTargetHoldPosition();
            Quaternion targetRotation = GetTargetHoldRotation();

            // 平滑插值
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        // 确保最终位置准确
        if (currentHolder != null)
        {
            transform.position = GetTargetHoldPosition();
            transform.rotation = GetTargetHoldRotation();
        }

        isTransitioning = false;
    }

    public void ResetItemState()
    {
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
        if (!isBeingHeld) return; // 如果已经不是持有状态，直接返回

        isBeingHeld = false;

        // 恢复物理效果
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // 恢复碰撞体
        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }

        // 最终确认缩放
        transform.localScale = originalScale;

        Debug.Log($"{currentHolder?.name} 放下了 {itemName}");
        currentHolder = null;
    }

    void FollowHolder()
    {
        if (currentHolder == null || isTransitioning) return;

        // 直接设置位置，没有延迟
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

    // 检查是否离开了交换区域
    void CheckIfLeftZone()
    {
        if (currentZone != null && !isBeingHeld)
        {
            float distance = Vector3.Distance(transform.position, currentZone.transform.position);
            if (distance > currentZone.detectionRadius * 1.2f)
            {
                OnLeftZone();
            }
        }
    }

    // 当物品离开区域时调用
    void OnLeftZone()
    {
        if (currentZone != null)
        {
            Debug.Log($"物品 {itemName} 离开了区域 {currentZone.zoneID}");
            currentZone.OnItemLeft(this.gameObject);
            currentZone = null;
            ResetExchangeLock();
        }
    }

    // 标记物品已被交换
    public void MarkAsExchanged(string fromZoneID)
    {
        isExchangeLocked = true;
        lastExchangeZone = fromZoneID;

        // 增加交换次数
        if (limitExchangeTimes)
        {
            currentExchangeTimes++;
            Debug.Log($"物品 {itemName} 被标记为已交换，来自区域 {fromZoneID}，交换次数: {currentExchangeTimes}/{maxExchangeTimes}");

            // 检查是否达到交换次数限制
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

    // 重置交换锁定
    public void ResetExchangeLock()
    {
        if (isExchangeLocked)
        {
            isExchangeLocked = false;
            lastExchangeZone = "";
            Debug.Log($"物品 {itemName} 的交换锁定已重置");
        }
    }

    // 检查是否可以交换到指定区域
    public bool CanExchangeTo(string targetZoneID)
    {
        // 检查交换次数限制
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

    // 设置当前所在的区域
    public void SetCurrentZone(ExchangeZone zone)
    {
        currentZone = zone;
    }

    // 重置物品到原始位置
    public void ResetItem()
    {
        PutDown();
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;
        isInExchangeProcess = false;
        ResetExchangeLock();
    }

    // 重置交换次数（用于特殊情况下重置物品状态）
    public void ResetExchangeTimes()
    {
        currentExchangeTimes = 0;
        canBeExchanged = true;
        Debug.Log($"物品 {itemName} 的交换次数已重置");
    }

    // 设置交换次数限制
    public void SetExchangeLimit(int maxTimes)
    {
        maxExchangeTimes = maxTimes;
        limitExchangeTimes = true;
        Debug.Log($"物品 {itemName} 的交换次数限制设置为: {maxTimes}");
    }

    // 移除交换次数限制
    public void RemoveExchangeLimit()
    {
        limitExchangeTimes = false;
        Debug.Log($"物品 {itemName} 的交换次数限制已移除");
    }

    // 重置物理状态
    public void ResetPhysics()
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void ForceRelease()
    {
        if (isBeingHeld)
        {
            // 停止过渡动画
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

    void OnDestroy()
    {
        // 停止所有运行的协程
        foreach (var pair in playerUICoroutines)
        {
            if (pair.Value != null)
            {
                StopCoroutine(pair.Value);
            }
        }

        // 清理所有UI实例
        foreach (var uiInstance in playerUIInstances.Values)
        {
            if (uiInstance != null)
            {
                Destroy(uiInstance);
            }
        }
        playerUIInstances.Clear();
        playerUICoroutines.Clear();
    }

    // 添加物品被禁用时的清理
    void OnDisable()
    {
        // 停止所有协程但不销毁UI，因为物品可能只是暂时禁用
        foreach (var pair in playerUICoroutines)
        {
            if (pair.Value != null)
            {
                StopCoroutine(pair.Value);
                playerUICoroutines[pair.Key] = null;
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

        // 显示交换次数信息
        if (limitExchangeTimes && showDebugInfo)
        {
#if UNITY_EDITOR
            string exchangeInfo = $"{itemName}\n交换: {currentExchangeTimes}/{maxExchangeTimes}";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, exchangeInfo);
#endif
        }
    }
}