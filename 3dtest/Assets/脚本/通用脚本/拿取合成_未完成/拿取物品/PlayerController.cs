using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Video; // 新增引用，用于播放视频

public class PlayerController : MonoBehaviour
{
    [Header("交互设置")]
    public KeyCode interactKey = KeyCode.F;
    public float interactionRange = 2f;
    public float throwRange = 10f;

    [Header("交互冷却设置")]
    public float interactionCooldown = 0.5f; // 交互冷却时间
    public bool enableInteractionCooldown = true; // 启用交互冷却

    [Header("特殊物品设置")]
    public string wateringCanItemName = "水壶"; // 水壶的物品名称

    [Header("音效设置")]
    public string pickupSoundGroupID = "拾取物品"; // 拾取音效组ID
    public string dropSoundGroupID = "放下物品";   // 放下音效组ID
    public string throwSoundGroupID = "抛掷物品";  // 抛掷音效组ID

    [Header("调试选项")]
    public bool showInputDebug = false;
    public bool showInteractionDebug = true;

    [Header("张奕忻：是否是scene5")]
    public bool isScene5 = false;

    // 张奕忻：新增 UI & Video 引用
    [Header("Scene5 结局 UI & 视频")]
    public GameObject UI_mask1; // Yes选项UI
    public GameObject UI_mask2; // No选项UI
    public VideoPlayer video_Ending1; // Yes 结局
    public VideoPlayer video_Ending2; // No 结局
    private bool isHoldingSpace = false;
    private float spaceHoldTime = 0f;
    private bool isPlayingEnding = false;
    private bool UI_mask1WasActive = false;
    private bool UI_mask2WasActive = false;

    private Rigidbody rb;
    private Vector3 movement;
    private InteractableItem heldItem;
    private bool isTemporarilyLocked = false; // 临时锁定状态（用于浇花等特殊动作）

    // 玩家属性
    public string playerName = "玩家";
    public Color playerColor = Color.white;

    [Header("手电筒设置")]
    public string flashLight = "手电筒"; // 手电筒的物品名称
    public bool isHoldFlashLight = false;
    public event Action OnFlashlightPickedUp;// 新增事件
    private FlashlightController currentFlashlight; // 当前持有的手电筒

    [Header("动画控制")]
    public PlayerAnimationController animationController;

    // 新增：交互状态控制
    private bool isInteractionInProgress = false;
    private float lastInteractionTime = 0f;
    private Coroutine interactionCooldownCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
        // 张奕忻初始化scene5遮罩选项
        if (UI_mask1 != null) UI_mask1.SetActive(false);
        if (UI_mask2 != null) UI_mask2.SetActive(false);
    }

    void Update()
    {
        if (isTemporarilyLocked) return; // 如果被临时锁定，不处理输入
        //张奕忻
        /*if (isScene5)
        {
            HandleScene5Input(); // ✅ 替换为Scene5逻辑
        }
        else
        {
            HandleInteraction(); // ✅ 其他场景维持原逻辑
        }*/
        HandleInteraction();
        if (showInputDebug && movement.magnitude > 0.1f)
        {
            Debug.Log($"{playerName} 移动输入: {movement}");
        }
    }

    void HandleInteraction()
    {
        // 检查交互冷却和进行中的交互
        if (IsInteractionBlocked())
        {
            if (showInteractionDebug && Input.GetKeyDown(interactKey))
            {
                Debug.Log($"{playerName} 交互被阻止 - 冷却中或进行中");
            }
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            if (showInteractionDebug)
                Debug.Log($"{playerName} 按下交互键，持有物品: {heldItem?.itemName}");

            if (heldItem != null)
            {
                // 如果持有水壶，优先尝试浇花
                if (heldItem.itemName == wateringCanItemName && TryWateringFlowers())
                {
                    Debug.Log($"{playerName} 成功开始浇花");
                    return;
                }

                // 如果持有物品，尝试抛掷到合成区域
                if (TryThrowToSynthesisZone())
                {
                    Debug.Log($"{playerName} 成功抛掷物品到合成区域");
                    return;
                }
                else
                {
                    // 否则放下物品
                    DropItem();
                }
            }
            else
            {
                // 尝试捡起物品
                TryPickUpItem();
            }
        }
    }

    // 新增：检查交互是否被阻止
    bool IsInteractionBlocked()
    {
        // 检查临时锁定
        if (isTemporarilyLocked) return true;

        // 检查交互冷却
        if (enableInteractionCooldown && Time.time - lastInteractionTime < interactionCooldown)
            return true;

        // 检查是否有进行中的交互
        if (isInteractionInProgress) return true;

        return false;
    }

    // 新增：开始交互流程
    void StartInteraction()
    {
        isInteractionInProgress = true;
        lastInteractionTime = Time.time;

        try
        {
            if (heldItem != null)
            {
                // 如果持有水壶，优先尝试浇花
                if (heldItem.itemName == wateringCanItemName && TryWateringFlowers())
                {
                    Debug.Log($"{playerName} 成功开始浇花");
                    EndInteraction();
                    return;
                }

                // 如果持有物品，尝试抛掷到合成区域
                if (TryThrowToSynthesisZone())
                {
                    Debug.Log($"{playerName} 成功抛掷物品到合成区域");
                    EndInteraction();
                    return;
                }
                else
                {
                    // 否则放下物品
                    DropItem();
                }
            }
            else
            {
                // 尝试捡起物品
                TryPickUpItem();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{playerName} 交互过程中发生错误: {e.Message}");
            ForceEndInteraction();
        }
    }

    // 新增：强制结束交互（用于错误恢复）
    void ForceEndInteraction()
    {
        isInteractionInProgress = false;
        heldItem = null;

        // 重置动画状态
        if (animationController != null)
        {
            animationController.ForceEndAnimationLock();
            animationController.SetHoldingState(false);
        }

        // 启动冷却协程
        if (interactionCooldownCoroutine != null)
            StopCoroutine(interactionCooldownCoroutine);

        interactionCooldownCoroutine = StartCoroutine(InteractionCooldownCoroutine());

        Debug.LogWarning($"{playerName} 强制结束交互流程");
    }

    // 新增：交互冷却协程
    System.Collections.IEnumerator InteractionCooldownCoroutine()
    {
        yield return new WaitForSeconds(interactionCooldown);
        // 冷却结束后，isInteractionInProgress 已经在 EndInteraction 中设置为 false
    }

    // 新增：正常结束交互
    void EndInteraction()
    {
        isInteractionInProgress = false;

        // 启动冷却协程
        if (interactionCooldownCoroutine != null)
            StopCoroutine(interactionCooldownCoroutine);

        interactionCooldownCoroutine = StartCoroutine(InteractionCooldownCoroutine());
    }

    // 拾取物品音效
    void PlayPickupSound()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(pickupSoundGroupID))
        {
            bool isPlayer1 = gameObject.CompareTag("Player1");
            AudioManager.Instance.PlayOneShot(
                pickupSoundGroupID,
                -1,           // 随机选择音效
                false,        // 不淡入
                0f,
                false,        // 不淡出
                0f,
                isPlayer1,    // 声道分配
                false         // 2D音效
            );

            if (showInteractionDebug)
            {
                Debug.Log($"{playerName} 播放拾取音效");
            }
        }
    }

    // 放下物品音效
    void PlayDropSound()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(dropSoundGroupID))
        {
            bool isPlayer1 = gameObject.CompareTag("Player1");
            AudioManager.Instance.PlayOneShot(
                dropSoundGroupID,
                -1,           // 随机选择音效
                false,        // 不淡入
                0f,
                false,        // 不淡出
                0f,
                isPlayer1,    // 声道分配
                false         // 2D音效
            );

            if (showInteractionDebug)
            {
                Debug.Log($"{playerName} 播放放下音效");
            }
        }
    }

    // 抛掷物品音效
    void PlayThrowSound()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(throwSoundGroupID))
        {
            bool isPlayer1 = gameObject.CompareTag("Player1");
            AudioManager.Instance.PlayOneShot(
                throwSoundGroupID,
                -1,           // 随机选择音效
                false,        // 不淡入
                0f,
                false,        // 不淡出
                0f,
                isPlayer1,    // 声道分配
                false         // 2D音效
            );

            if (showInteractionDebug)
            {
                Debug.Log($"{playerName} 播放抛掷音效");
            }
        }
    }


    #region 张奕忻scene5输入
    /*
    // 新增：Scene5 专属交互逻辑
    void HandleScene5Input()
    {
        if (isPlayingEnding) return; // 防止重复触发播放

        // 玩家1选Yes（F键）
        if (CompareTag("Player1") && Input.GetKeyDown(KeyCode.F))
        {
            if (UI_mask1 != null) UI_mask1.SetActive(true);
            if (UI_mask2 != null) UI_mask2.SetActive(false);
            Debug.Log("玩家1选择了 YES");
        }

        // 玩家2选No（H键）
        if (CompareTag("Player2") && Input.GetKeyDown(KeyCode.H))
        {
            if (UI_mask2 != null) UI_mask2.SetActive(true);
            if (UI_mask1 != null) UI_mask1.SetActive(false);
            Debug.Log("玩家2选择了 NO");
        }

        // 如果有任意UI激活，则可以触发按空格倒计时
        if ((UI_mask1 != null && UI_mask1.activeSelf) || (UI_mask2 != null && UI_mask2.activeSelf))
        {
            if (Input.GetKey(KeyCode.Space))
            {
                spaceHoldTime += Time.deltaTime;

                if (!isHoldingSpace && spaceHoldTime > 0.5f)
                {
                    Debug.Log("正在按住空格...");
                    isHoldingSpace = true;
                }

                if (spaceHoldTime >= 5f && !isPlayingEnding)
                {
                    isPlayingEnding = true;
                    StartCoroutine(PlayEndingVideo());
                }
            }
            else
            {
                if (isHoldingSpace)
                    Debug.Log("松开空格，重置计时");

                isHoldingSpace = false;
                spaceHoldTime = 0f;
            }
        }
    }

    // 新增：播放结局视频
    IEnumerator PlayEndingVideo()
    {
        Debug.Log("检测到空格按住5秒，开始播放结局视频");
        // 隐藏选择UI
        if (UI_mask1 != null) UI_mask1.SetActive(false);
        if (UI_mask2 != null) UI_mask2.SetActive(false);

        // 等待一点时间以避免UI隐藏和视频激活冲突
        yield return new WaitForSeconds(0.2f);

        // 判断播放哪一个结局
        bool isYesSelected = (UI_mask1 != null && UI_mask1WasActive);
        bool isNoSelected = (UI_mask2 != null && UI_mask2WasActive);

        if (isYesSelected && video_Ending1 != null)
        {
            video_Ending1.gameObject.SetActive(true);
            video_Ending1.Play();
            Debug.Log("播放 YES 结局视频");
        }
        else if (isNoSelected && video_Ending2 != null)
        {
            video_Ending2.gameObject.SetActive(true);
            video_Ending2.Play();
            Debug.Log("播放 NO 结局视频");
        }
        else
        {
            Debug.LogWarning("未检测到结局选项，默认播放 YES 结局");
            if (video_Ending1 != null)
            {
                video_Ending1.gameObject.SetActive(true);
                video_Ending1.Play();
            }
        }

        yield return null;
    }
    public void OnSelectYes()
    {
        UI_mask1WasActive = true;
        UI_mask2WasActive = false;
        UI_mask1.SetActive(true);
        UI_mask2.SetActive(false);
        Debug.Log("玩家选择 YES 结局");
    }
    public void OnSelectNo()
    {
        UI_mask1WasActive = false;
        UI_mask2WasActive = true;
        UI_mask1.SetActive(false);
        UI_mask2.SetActive(true);
        Debug.Log("玩家选择 NO 结局");
    }*/
    #endregion

    // 尝试浇花 - 修复逻辑
    bool TryWateringFlowers()
    {
        // 检查是否持有水壶
        if (heldItem == null || heldItem.itemName != wateringCanItemName)
        {
            if (showInteractionDebug)
                Debug.Log($"{playerName} 没有持有水壶或水壶名称不匹配");
            return false;
        }

        // 查找附近的花盆区域
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRange);
        if (showInteractionDebug)
            Debug.Log($"{playerName} 检测到 {hitColliders.Length} 个碰撞体");

        foreach (var hitCollider in hitColliders)
        {
            FlowerPotZone flowerPot = hitCollider.GetComponent<FlowerPotZone>();
            if (flowerPot != null)
            {
                if (showInteractionDebug)
                    Debug.Log($"{playerName} 找到花盆区域: {flowerPot.zoneID}");

                // 检查玩家是否在花盆区域内
                if (flowerPot.IsPlayerInZone(gameObject))
                {
                    if (showInteractionDebug)
                        Debug.Log($"{playerName} 在花盆区域内，开始浇花");

                    // 开始浇花
                    bool wateringStarted = flowerPot.StartWatering(gameObject, heldItem.gameObject);
                    if (wateringStarted)
                    {
                        // 浇花成功后，立即释放水壶引用，因为花盆会处理水壶的销毁
                        heldItem = null;
                        return true;
                    }
                    else
                    {
                        Debug.Log($"{playerName} 花盆拒绝开始浇花");
                    }
                }
                else
                {
                    if (showInteractionDebug)
                        Debug.Log($"{playerName} 在花盆附近但不在区域内");
                }
            }
        }

        if (showInteractionDebug)
            Debug.Log($"{playerName} 没有找到可用的花盆区域");
        return false;
    }

    bool TryThrowToSynthesisZone()
    {
        // 查找前方的合成区域 - 同时检测 SynthesisZone 和 TutorialSynthesisZone
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, throwRange))
        {
            // 先尝试普通合成区域
            SynthesisZone zone = hit.collider.GetComponent<SynthesisZone>();
            if (zone != null && heldItem != null)
            {
                ThrowToZone(zone, heldItem);
                return true;
            }

            // 再尝试教学合成区域
            TutorialSynthesisZone tutorialZone = hit.collider.GetComponent<TutorialSynthesisZone>();
            if (tutorialZone != null && heldItem != null)
            {
                ThrowToTutorialZone(tutorialZone, heldItem);
                return true;
            }

            // 新增：尝试三合成区域
            ThreeItemSynthesisZone threeItemZone = hit.collider.GetComponent<ThreeItemSynthesisZone>();
            if (threeItemZone != null && heldItem != null)
            {
                ThrowToThreeItemZone(threeItemZone, heldItem);
                return true;
            }
        }

        return false;
    }

    // 新增：向三合成区域抛掷
    void ThrowToThreeItemZone(ThreeItemSynthesisZone zone, InteractableItem item)
    {
        // 从玩家手中移除物品引用
        InteractableItem itemToThrow = heldItem;
        heldItem = null;

        // 重要：在抛掷前强制清除物品的持有状态
        itemToThrow.ForceRelease();

        // 调用三合成区域的抛掷方法
        zone.ThrowItemToZone(itemToThrow);
        // 播放抛掷音效
        PlayThrowSound();

        Debug.Log($"{playerName} 向三合成区域抛掷 {itemToThrow.itemName}");
    }
    void ThrowToZone(SynthesisZone zone, InteractableItem item)
    {
        // 从玩家手中移除物品引用
        InteractableItem itemToThrow = heldItem;
        heldItem = null;

        // 重要：在抛掷前强制清除物品的持有状态
        itemToThrow.ForceRelease();

        // 调用区域的抛掷方法
        zone.ThrowItemToZone(itemToThrow);
        // 播放抛掷音效
        PlayThrowSound();

        Debug.Log($"{playerName} 向合成区域抛掷 {itemToThrow.itemName}");
    }

    void ThrowToTutorialZone(TutorialSynthesisZone zone, InteractableItem item)
    {
        // 从玩家手中移除物品引用
        InteractableItem itemToThrow = heldItem;
        heldItem = null;

        // 重要：在抛掷前强制清除物品的持有状态
        itemToThrow.ForceRelease();

        // 调用区域的抛掷方法
        zone.ThrowItemToZone(itemToThrow);
        // 播放抛掷音效
        PlayThrowSound();
        Debug.Log($"{playerName} 向教学区域抛掷 {itemToThrow.itemName}");
    }

    void TryPickUpItem()
    {
        // 安全检查：确保没有进行中的交互
        if (isInteractionInProgress && heldItem != null)
        {
            Debug.LogWarning($"{playerName} 尝试捡起物品时检测到状态冲突");
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRange);
        InteractableItem closestItem = null;
        float closestDistance = Mathf.Infinity;

        foreach (var hitCollider in hitColliders)
        {
            InteractableItem item = hitCollider.GetComponent<InteractableItem>();
            if (item != null && item.canBePickedUp && !item.isBeingHeld && !item.isInExchangeProcess)
            {
                float distance = Vector3.Distance(transform.position, item.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = item;
                }
            }
        }

        if (closestItem != null)
        {
            PickUpItem(closestItem);
        }
        else if (showInteractionDebug)
        {
            Debug.Log($"{playerName} 附近没有可拾取的物品");
        }
        EndInteraction(); // 没有找到物品，结束交互
    }


    void PickUpItem(InteractableItem item)
    {
        // 状态验证
        if (heldItem != null)
        {
            Debug.LogError($"{playerName} 尝试捡起物品时已经持有物品: {heldItem.itemName}");
            ForceEndInteraction();
            return;
        }

        if (!item.canBePickedUp || item.isBeingHeld)
        {
            Debug.LogError($"{playerName} 尝试捡起不可拾取或已被持有的物品: {item.itemName}");
            ForceEndInteraction();
            return;
        }

        heldItem = item;

        try
        {
            item.Interact(gameObject);

            // 播放拾取音效
            PlayPickupSound();
            // 触发拾取动画
            if (animationController != null)
            {
                animationController.TriggerPickUpAnimation();
            }

            // 检查是否是手电筒
            if (item.itemName == flashLight)
            {
                currentFlashlight = item.GetComponent<FlashlightController>();
                isHoldFlashLight = true;
                OnFlashlightPickedUp?.Invoke();

                if (gameObject.CompareTag("Player2") && currentFlashlight != null)
                {
                    currentFlashlight.TurnOn();
                }
            }

            if (showInteractionDebug)
            {
                Debug.Log($"{playerName} 捡起了 {item.itemName}");
            }

            EndInteraction(); // 成功捡起，结束交互
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{playerName} 捡起物品时发生错误: {e.Message}");
            heldItem = null;
            ForceEndInteraction();
        }
    }

    // 修改：DropItem 方法，添加安全机制
    void DropItem()
    {
        if (heldItem == null)
        {
            Debug.LogWarning($"{playerName} 尝试放下空物品");
            ForceEndInteraction();
            return;
        }

        // 保存引用，避免空引用
        InteractableItem itemToDrop = heldItem;
        string itemName = itemToDrop.itemName;

        try
        {
            // 放下前如果是手电筒，关闭灯光
            if (itemName == flashLight && currentFlashlight != null)
            {
                currentFlashlight.TurnOff();
                currentFlashlight = null;
                isHoldFlashLight = false;
            }

            itemToDrop.Interact(gameObject);

            // 播放放下音效
            PlayDropSound();
            // 立即清除引用
            heldItem = null;

            // 更新动画状态
            if (animationController != null)
            {
                animationController.SetHoldingState(false);
                animationController.ForceEndAnimationLock(); // 强制结束任何可能的动画锁定
            }

            if (showInteractionDebug)
            {
                Debug.Log($"{playerName} 放下了 {itemName}");
            }

            EndInteraction(); // 成功放下，结束交互
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{playerName} 放下物品时发生错误: {e.Message}");
            heldItem = null;
            ForceEndInteraction();
        }
    }

    //void DropItem()
    //{
    //    if (heldItem != null)
    //    {

    //        // 放下前如果是手电筒，关闭灯光
    //        if (heldItem.itemName == flashLight && currentFlashlight != null)
    //        {
    //            currentFlashlight.TurnOff();
    //            currentFlashlight = null;
    //            isHoldFlashLight = false;
    //        }

    //        heldItem.Interact(gameObject);

    //        // 更新动画状态 - 确保在放下物品后立即更新
    //        if (animationController != null)
    //        {
    //            animationController.SetHoldingState(false);

    //            // 强制立即检查移动状态
    //            StartCoroutine(ForceAnimationUpdateNextFrame());
    //        }

    //        if (showInteractionDebug)
    //        {
    //            Debug.Log($"{playerName} 放下了 {heldItem.itemName}");
    //        }

    //        heldItem = null;
    //    }
    //}

    // 新增协程：在下一帧强制更新动画状态
    System.Collections.IEnumerator ForceAnimationUpdateNextFrame()
    {
        yield return null;

        if (animationController != null && !isInteractionInProgress)
        {
            animationController.SetHoldingState(false);
        }
    }
    // 新增：设置临时锁定状态
    public void SetTemporaryLock(bool locked)
    {
        isTemporarilyLocked = locked;
        Debug.Log($"{playerName} 临时锁定状态: {locked}");
    }

    // 检查是否持有物品
    public bool IsHoldingItem()
    {
        return heldItem != null;
    }

    // 获取当前持有的物品
    public InteractableItem GetHeldItem()
    {
        return heldItem;
    }

    // 强制释放持有的物品（用于浇花等特殊操作后）
    public void ForceReleaseItem()
    {
        if (heldItem != null)
        {
            // 释放前如果是手电筒，关闭灯光
            if (heldItem.itemName == flashLight && currentFlashlight != null)
            {
                currentFlashlight.TurnOff();
                currentFlashlight = null;
                isHoldFlashLight = false;
            }

            heldItem.ForceRelease();
            heldItem = null;
            Debug.Log($"{playerName} 强制释放了物品");
        }
    }
    // 新增：获取交互状态（用于调试）
    public string GetInteractionState()
    {
        return $"持有物品: {heldItem?.itemName ?? "无"}, 交互进行中: {isInteractionInProgress}, 冷却剩余: {Mathf.Max(0, interactionCooldown - (Time.time - lastInteractionTime)):F2}s";
    }

    // 新增：强制重置交互状态（用于调试和恢复）
    [ContextMenu("强制重置交互状态")]
    public void ForceResetInteractionState()
    {
        Debug.Log($"{playerName} 强制重置交互状态");
        ForceEndInteraction();

        if (animationController != null)
        {
            animationController.ResetAllAnimations();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        Gizmos.color = Color.blue;
        Vector3 throwEnd = transform.position + transform.forward * throwRange;
        Gizmos.DrawLine(transform.position, throwEnd);
        Gizmos.DrawWireSphere(throwEnd, 0.3f);
    }
}