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

    [Header("玩家引用")]
    public PlayerController playerController;
    public Rigidbody playerRigidbody;
    public DualPlayerController dualPlayerController;

    [Header("动画设置")]
    public float movementThreshold = 0.1f;

    [Header("方向设置")]
    public bool invertFlipDirection = false;
    public float flipDeadZone = 0.1f;

    [Header("拾取动画设置")]
    public float pickUpAnimationLockTime = 0.8f;
    public bool enableAnimationLock = true;

    [Header("调试选项")]
    public bool showDebugInfo = false;

    private Vector3 lastPosition;
    private bool wasMoving = false;
    private bool isHoldingItem = false;
    private bool forceStateUpdate = false;
    private float lastFlipTime = 0f;
    private float flipCooldown = 0.1f;

    // 动画锁定相关变量
    private bool isAnimationLocked = false;
    private float animationLockEndTime = 0f;
    private Coroutine animationLockCoroutine;

    private string playerName = "Player";

    // 存储原始缩放值
    private Vector3 originalScale;

    void Start()
    {
        InitializeComponents();
        lastPosition = transform.position;

        // 记录原始缩放
        originalScale = transform.localScale;
    }

    void InitializeComponents()
    {
        if (playerAnimator == null)
            playerAnimator = GetComponent<Animator>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (playerRigidbody == null)
            playerRigidbody = GetComponent<Rigidbody>();

        if (dualPlayerController == null)
            dualPlayerController = FindObjectOfType<DualPlayerController>();
    }

    void Update()
    {
        if (playerAnimator == null) return;

        UpdateAnimationStates();

        if (!isAnimationLocked)
        {
            UpdateSpriteDirection();
        }
    }

    void UpdateAnimationStates()
    {
        CheckAnimationLock();

        bool isMoving = CheckMovement();
        bool currentlyHolding = playerController != null && playerController.IsHoldingItem();

        if (!isAnimationLocked)
        {
            playerAnimator.SetBool(walkParameter, isMoving);
        }

        // 检查是否刚刚开始持有物品
        if (currentlyHolding && !isHoldingItem)
        {
            TriggerPickUpAnimation();
        }

        // 更新持有物品状态
        if (isHoldingItem != currentlyHolding || forceStateUpdate)
        {
            isHoldingItem = currentlyHolding;

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
        if (dualPlayerController != null)
        {
            UpdateInputBasedFlip();
        }
    }

    void UpdateInputBasedFlip()
    {
        Vector3 inputDirection = GetPlayerInputDirection();
        float currentHorizontal = inputDirection.x;

        // 只有当有有效水平输入时才翻转
        if (Mathf.Abs(currentHorizontal) > flipDeadZone)
        {
            // 直接根据水平输入决定翻转方向
            bool shouldFlip = currentHorizontal < 0;
            ApplyFlipWithCooldown(shouldFlip);

            if (showDebugInfo && inputDirection.magnitude > movementThreshold)
            {
                string diagonalInfo = (Mathf.Abs(inputDirection.z) > 0.1f) ? "对角线" : "水平";
                Debug.Log($"{playerName} {diagonalInfo}输入 - 水平: {currentHorizontal}, 翻转: {shouldFlip}");
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

        // 使用缩放来实现翻转
        Vector3 newScale = originalScale;
        newScale.x = newFlipState ? -originalScale.x : originalScale.x;

        if (transform.localScale != newScale)
        {
            transform.localScale = newScale;
            lastFlipTime = Time.time;

            if (showDebugInfo)
            {
                Debug.Log($"{playerName} 翻转方向: {newFlipState}, 新缩放: {newScale}");
            }
        }
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

    public void SetHoldingState(bool holding)
    {
        isHoldingItem = holding;
        if (playerAnimator != null && !isAnimationLocked)
        {
            playerAnimator.SetBool(holdParameter, holding);
            forceStateUpdate = true;
        }
    }

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

    public void StartAnimationLock(float lockTime)
    {
        if (animationLockCoroutine != null)
        {
            StopCoroutine(animationLockCoroutine);
        }
        animationLockCoroutine = StartCoroutine(AnimationLockCoroutine(lockTime));
    }

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

    public bool IsAnimationLocked()
    {
        return isAnimationLocked;
    }

    public void ResetAllAnimations()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool(walkParameter, false);
            playerAnimator.SetBool(holdParameter, false);
            playerAnimator.ResetTrigger(pickUpParameter);
        }

        ForceEndAnimationLock();

        // 重置为原始缩放
        transform.localScale = originalScale;
    }

    public void SetFlipDirection(bool flipX)
    {
        // 使用缩放来实现翻转
        Vector3 newScale = originalScale;
        newScale.x = flipX ? -originalScale.x : originalScale.x;
        transform.localScale = newScale;
    }

    void OnEnable()
    {
        if (playerController != null)
        {
            playerName = playerController.playerName;
        }
    }

    public string GetStateRecoveryInfo()
    {
        return $"动画锁定: {isAnimationLocked}, 持有状态: {isHoldingItem}, 当前缩放: {transform.localScale}";
    }
}