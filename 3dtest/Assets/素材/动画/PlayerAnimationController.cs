using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("动画参数")]
    public string walkParameter = "IsWalking";
    public string holdParameter = "IsHolding";
    public string pickUpParameter = "PickUp";

    [Header("动画组件")]
    public Animator playerAnimator;
    public SpriteRenderer spriteRenderer;

    [Header("玩家引用")]
    public PlayerController playerController;
    public Rigidbody playerRigidbody;
    public DualPlayerController dualPlayerController;

    [Header("动画设置")]
    public float movementThreshold = 0.1f;

    [Header("方向设置")]
    public bool invertFlipDirection = false;
    public bool useCameraRelativeFlip = true;
    public bool useInputBasedFlip = true;
    public float flipDeadZone = 0.3f;

    [Header("拾取动画设置")]
    public float pickUpAnimationLockTime = 0.8f;
    public bool enableAnimationLock = true;

    [Header("状态恢复设置")]
    public bool enableStateRecovery = true;
    public float stateRecoveryCheckInterval = 1f; // 延长检查间隔
    public float stuckStateThreshold = 10f; // 增加卡死阈值

    private Vector3 lastPosition;
    private bool wasMoving = false;
    private bool isHoldingItem = false;
    private bool forceStateUpdate = false;
    private Camera playerCamera;
    private Vector3 lastStableDirection = Vector3.forward;
    private float lastFlipTime = 0f;
    private float flipCooldown = 0.2f;

    // 动画锁定相关变量
    private bool isAnimationLocked = false;
    private float animationLockEndTime = 0f;
    private Coroutine animationLockCoroutine;

    // 状态恢复相关变量
    private float lastStateCheckTime = 0f;
    private string lastAnimatorState = "";
    private float stateStartTime = 0f;
    private bool isInNormalHoldingState = false;

    void Start()
    {
        InitializeComponents();
        lastPosition = transform.position;
        FindPlayerCamera();
        stateStartTime = Time.time;
    }

    void InitializeComponents()
    {
        if (playerAnimator == null)
            playerAnimator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (playerRigidbody == null)
            playerRigidbody = GetComponent<Rigidbody>();

        if (dualPlayerController == null)
            dualPlayerController = FindObjectOfType<DualPlayerController>();
    }

    void FindPlayerCamera()
    {
        if (dualPlayerController != null)
        {
            if (gameObject.CompareTag("Player1") || gameObject.name.Contains("Player1"))
            {
                playerCamera = dualPlayerController.player1Camera;
            }
            else if (gameObject.CompareTag("Player2") || gameObject.name.Contains("Player2"))
            {
                playerCamera = dualPlayerController.player2Camera;
            }
        }

        if (playerCamera == null)
        {
            Camera[] cameras = FindObjectsOfType<Camera>();
            foreach (Camera cam in cameras)
            {
                if (cam.name.Contains("Player1") && (gameObject.CompareTag("Player1") || gameObject.name.Contains("Player1")))
                {
                    playerCamera = cam;
                    break;
                }
                else if (cam.name.Contains("Player2") && (gameObject.CompareTag("Player2") || gameObject.name.Contains("Player2")))
                {
                    playerCamera = cam;
                    break;
                }
            }
        }
    }

    void Update()
    {
        if (playerAnimator == null) return;

        UpdateAnimationStates();

        // 状态恢复检查 - 只在启用且不在正常持物状态时检查
        if (enableStateRecovery && Time.time - lastStateCheckTime >= stateRecoveryCheckInterval && !IsInNormalHoldingState())
        {
            CheckStateRecovery();
            lastStateCheckTime = Time.time;
        }

        // 只有在没有动画锁定时才更新方向
        if (!isAnimationLocked)
        {
            UpdateSpriteDirection();
        }
    }

    void UpdateAnimationStates()
    {
        // 检查动画锁定状态
        CheckAnimationLock();

        bool isMoving = CheckMovement();
        bool currentlyHolding = playerController != null && playerController.IsHoldingItem();

        // 更新正常持物状态标志
        UpdateNormalHoldingState(currentlyHolding, isMoving);

        // 如果处于动画锁定状态，不更新行走状态
        if (!isAnimationLocked)
        {
            playerAnimator.SetBool(walkParameter, isMoving);
        }

        // 检查是否刚刚开始持有物品
        if (currentlyHolding && !isHoldingItem)
        {
            // 触发拾取动画
            TriggerPickUpAnimation();
        }

        // 更新持有物品状态
        if (isHoldingItem != currentlyHolding || forceStateUpdate)
        {
            isHoldingItem = currentlyHolding;

            // 如果没有动画锁定，立即更新Animator
            if (!isAnimationLocked)
            {
                playerAnimator.SetBool(holdParameter, isHoldingItem);
            }

            if (forceStateUpdate)
            {
                playerAnimator.Update(0f);
                forceStateUpdate = false;
            }
        }

        wasMoving = isMoving;
    }

    // 新增：更新正常持物状态标志
    void UpdateNormalHoldingState(bool currentlyHolding, bool isMoving)
    {
        // 正常持物状态：持有物品且处于HoldIdle或HoldWalk状态
        isInNormalHoldingState = currentlyHolding && (GetCurrentStateName() == "HoldIdle" || GetCurrentStateName() == "HoldWalk");

        // 如果处于正常持物状态，重置状态开始时间
        if (isInNormalHoldingState)
        {
            stateStartTime = Time.time;
        }
    }

    // 新增：检查是否处于正常持物状态
    bool IsInNormalHoldingState()
    {
        return isInNormalHoldingState;
    }

    void CheckAnimationLock()
    {
        if (isAnimationLocked && Time.time >= animationLockEndTime)
        {
            isAnimationLocked = false;
            forceStateUpdate = true;

            if (showDebugInfo)
            {
                Debug.Log($"{playerName} 动画锁定结束");
            }
        }
    }

    // 修改：状态恢复检查，只针对真正卡死的状态
    void CheckStateRecovery()
    {
        if (playerAnimator == null) return;

        string currentState = GetCurrentStateName();
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

        // 只在状态改变时重置计时器
        if (currentState != lastAnimatorState)
        {
            stateStartTime = Time.time;
            lastAnimatorState = currentState;
        }

        // 检查是否真正卡死（排除正常持物状态）
        if (!IsInNormalHoldingState() && Time.time - stateStartTime > stuckStateThreshold)
        {
            // 只对特定状态进行恢复检查
            if (ShouldRecoverFromState(currentState, stateInfo))
            {
                Debug.LogWarning($"{playerName} 检测到卡死状态: {currentState}，持续 {Time.time - stateStartTime:F1}秒");
                ForceRecoveryFromStuckState();
            }
        }

        lastAnimatorState = currentState;
    }

    // 新增：判断是否应该从某个状态恢复
    bool ShouldRecoverFromState(string stateName, AnimatorStateInfo stateInfo)
    {
        // 永远不需要从这些正常状态恢复
        if (stateName == "Idle" || stateName == "Walk" || stateName == "HoldIdle" || stateName == "HoldWalk")
            return false;

        // 对于PickUp状态，只在动画播放多次后仍不切换时恢复
        if (stateName == "PickUp" && stateInfo.normalizedTime > 2.0f)
            return true;

        // 对于其他状态，使用默认阈值
        return stateInfo.normalizedTime > 3.0f;
    }

    bool CheckMovement()
    {
        if (playerRigidbody != null)
        {
            Vector3 horizontalVelocity = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
            return horizontalVelocity.magnitude > movementThreshold;
        }
        else
        {
            Vector3 positionChange = transform.position - lastPosition;
            lastPosition = transform.position;
            return positionChange.magnitude > movementThreshold * Time.deltaTime;
        }
    }

    void UpdateSpriteDirection()
    {
        if (spriteRenderer == null) return;

        if (useInputBasedFlip && dualPlayerController != null)
        {
            UpdateInputBasedFlipStable();
        }
        else if (useCameraRelativeFlip && playerCamera != null)
        {
            UpdateCameraRelativeFlipStable();
        }
        else if (playerRigidbody != null)
        {
            UpdateVelocityBasedFlipStable();
        }
    }

    void UpdateInputBasedFlipStable()
    {
        Vector3 inputDirection = GetPlayerInputDirection();

        if (inputDirection.magnitude > movementThreshold)
        {
            Vector3 worldDirection = GetCameraRelativeDirection(inputDirection, playerCamera);

            bool hasSignificantHorizontalInput = Mathf.Abs(inputDirection.x) > flipDeadZone;

            if (hasSignificantHorizontalInput)
            {
                bool shouldFlip = worldDirection.x < 0;
                ApplyFlipWithCooldown(shouldFlip);
                lastStableDirection = worldDirection;
            }
        }
    }

    void UpdateCameraRelativeFlipStable()
    {
        Vector3 moveDirection = GetMoveDirection();

        if (moveDirection.magnitude > movementThreshold)
        {
            Vector3 screenPos = playerCamera.WorldToViewportPoint(transform.position);
            Vector3 worldTarget = transform.position + moveDirection;
            Vector3 screenTarget = playerCamera.WorldToViewportPoint(worldTarget);

            bool hasSignificantHorizontalMovement = Mathf.Abs(screenTarget.x - screenPos.x) > flipDeadZone * 0.1f;

            if (hasSignificantHorizontalMovement)
            {
                bool shouldFlip = screenTarget.x < screenPos.x;
                ApplyFlipWithCooldown(shouldFlip);
                lastStableDirection = moveDirection;
            }
        }
    }

    void UpdateVelocityBasedFlipStable()
    {
        Vector3 horizontalVelocity = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);

        if (horizontalVelocity.magnitude > movementThreshold)
        {
            bool hasSignificantHorizontalVelocity = Mathf.Abs(horizontalVelocity.x) > flipDeadZone;

            if (hasSignificantHorizontalVelocity)
            {
                bool shouldFlip = horizontalVelocity.x < 0;
                ApplyFlipWithCooldown(shouldFlip);
                lastStableDirection = horizontalVelocity.normalized;
            }
        }
    }

    void ApplyFlipWithCooldown(bool shouldFlip)
    {
        if (Time.time - lastFlipTime < flipCooldown)
        {
            return;
        }

        bool newFlipState = invertFlipDirection ? !shouldFlip : shouldFlip;

        if (spriteRenderer.flipX != newFlipState)
        {
            spriteRenderer.flipX = newFlipState;
            lastFlipTime = Time.time;

            if (showDebugInfo)
            {
                Debug.Log($"{playerName} 翻转方向: {newFlipState}");
            }
        }
    }

    Vector3 GetMoveDirection()
    {
        if (playerRigidbody != null)
        {
            return new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
        }
        return (transform.position - lastPosition).normalized;
    }

    Vector3 GetPlayerInputDirection()
    {
        if (dualPlayerController == null) return Vector3.zero;

        if (gameObject.CompareTag("Player1") || gameObject.name.Contains("Player1"))
        {
            return dualPlayerController.GetPlayer1InputDirection();
        }
        else if (gameObject.CompareTag("Player2") || gameObject.name.Contains("Player2"))
        {
            return dualPlayerController.GetPlayer2InputDirection();
        }

        return Vector3.zero;
    }

    Vector3 GetCameraRelativeDirection(Vector3 inputDirection, Camera cam)
    {
        if (cam == null) return inputDirection;

        Vector3 cameraForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;

        return cameraForward * inputDirection.z + cameraRight * inputDirection.x;
    }

    // 强制设置动画状态
    public void SetHoldingState(bool holding)
    {
        isHoldingItem = holding;
        if (playerAnimator != null && !isAnimationLocked)
        {
            playerAnimator.SetBool(holdParameter, holding);
            forceStateUpdate = true;
        }
    }

    // 触发特殊动画
    public void TriggerPickUpAnimation()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(pickUpParameter);

            if (enableAnimationLock)
            {
                StartAnimationLock(pickUpAnimationLockTime);
            }

            if (showDebugInfo)
            {
                Debug.Log($"{playerName} 触发拾取动画，锁定时间: {pickUpAnimationLockTime}秒");
            }
        }
    }

    // 开始动画锁定
    public void StartAnimationLock(float lockTime)
    {
        if (animationLockCoroutine != null)
        {
            StopCoroutine(animationLockCoroutine);
        }
        animationLockCoroutine = StartCoroutine(AnimationLockCoroutine(lockTime));
    }

    // 动画锁定协程
    private System.Collections.IEnumerator AnimationLockCoroutine(float lockTime)
    {
        isAnimationLocked = true;
        animationLockEndTime = Time.time + lockTime;

        playerAnimator.SetBool(walkParameter, false);

        if (showDebugInfo)
        {
            Debug.Log($"{playerName} 开始动画锁定，持续 {lockTime}秒");
        }

        yield return new WaitForSeconds(lockTime);

        isAnimationLocked = false;
        forceStateUpdate = true;

        if (showDebugInfo)
        {
            Debug.Log($"{playerName} 动画锁定结束");
        }
    }

    // 强制结束动画锁定
    public void ForceEndAnimationLock()
    {
        if (animationLockCoroutine != null)
        {
            StopCoroutine(animationLockCoroutine);
        }
        isAnimationLocked = false;
        forceStateUpdate = true;

        if (showDebugInfo)
        {
            Debug.Log($"{playerName} 强制结束动画锁定");
        }
    }

    // 检查是否处于动画锁定状态
    public bool IsAnimationLocked()
    {
        return isAnimationLocked;
    }

    // 修改：强制从卡死状态恢复，保留持物状态
    void ForceRecoveryFromStuckState()
    {
        if (playerAnimator == null) return;

        // 获取当前实际状态
        bool shouldHold = playerController != null && playerController.IsHoldingItem();
        bool isMoving = CheckMovement();

        // 重置触发器
        playerAnimator.ResetTrigger(pickUpParameter);

        // 设置正确的参数
        playerAnimator.SetBool(holdParameter, shouldHold);
        playerAnimator.SetBool(walkParameter, isMoving);

        // 强制结束动画锁定
        ForceEndAnimationLock();

        // 根据状态播放正确的动画
        if (shouldHold)
        {
            if (isMoving)
            {
                playerAnimator.Play("HoldWalk", 0, 0f);
            }
            else
            {
                playerAnimator.Play("HoldIdle", 0, 0f);
            }
        }
        else
        {
            if (isMoving)
            {
                playerAnimator.Play("Walk", 0, 0f);
            }
            else
            {
                playerAnimator.Play("Idle", 0, 0f);
            }
        }

        // 重置状态计时器
        stateStartTime = Time.time;

        Debug.Log($"{playerName} 执行状态强制恢复，状态: {(shouldHold ? (isMoving ? "HoldWalk" : "HoldIdle") : (isMoving ? "Walk" : "Idle"))}");
    }

    // 获取当前状态名称
    string GetCurrentStateName()
    {
        if (playerAnimator == null) return "Unknown";

        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Idle")) return "Idle";
        if (stateInfo.IsName("Walk")) return "Walk";
        if (stateInfo.IsName("PickUp")) return "PickUp";
        if (stateInfo.IsName("HoldIdle")) return "HoldIdle";
        if (stateInfo.IsName("HoldWalk")) return "HoldWalk";

        return "Other";
    }

    // 重置所有动画状态
    public void ResetAllAnimations()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool(walkParameter, false);
            playerAnimator.SetBool(holdParameter, false);
            playerAnimator.ResetTrigger(pickUpParameter);
        }

        ForceEndAnimationLock();
    }

    // 手动设置翻转方向
    public void SetFlipDirection(bool flipX)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = flipX;
            lastStableDirection = flipX ? Vector3.left : Vector3.right;
        }
    }

    // 调试信息
    [Header("调试选项")]
    public bool showDebugInfo = false;
    private string playerName = "Player";

    void OnEnable()
    {
        if (playerController != null)
        {
            playerName = playerController.playerName;
        }
    }

    // 新增：获取状态恢复信息（用于调试）
    public string GetStateRecoveryInfo()
    {
        return $"状态: {GetCurrentStateName()}, 正常持物: {IsInNormalHoldingState()}, 状态持续时间: {Time.time - stateStartTime:F1}s";
    }
}