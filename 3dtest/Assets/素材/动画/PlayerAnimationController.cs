using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("��������")]
    public string walkParameter = "IsWalking";
    public string holdParameter = "IsHolding";
    public string pickUpParameter = "PickUp";

    [Header("�������")]
    public Animator playerAnimator;
    public SpriteRenderer spriteRenderer;

    [Header("�������")]
    public PlayerController playerController;
    public Rigidbody playerRigidbody;
    public DualPlayerController dualPlayerController;

    [Header("��������")]
    public float movementThreshold = 0.1f;

    [Header("��������")]
    public bool invertFlipDirection = false;
    public bool useCameraRelativeFlip = true;
    public bool useInputBasedFlip = true;
    public float flipDeadZone = 0.3f;

    [Header("ʰȡ��������")]
    public float pickUpAnimationLockTime = 0.8f;
    public bool enableAnimationLock = true;

    [Header("״̬�ָ�����")]
    public bool enableStateRecovery = true;
    public float stateRecoveryCheckInterval = 1f; // �ӳ������
    public float stuckStateThreshold = 10f; // ���ӿ�����ֵ

    private Vector3 lastPosition;
    private bool wasMoving = false;
    private bool isHoldingItem = false;
    private bool forceStateUpdate = false;
    private Camera playerCamera;
    private Vector3 lastStableDirection = Vector3.forward;
    private float lastFlipTime = 0f;
    private float flipCooldown = 0.2f;

    // ����������ر���
    private bool isAnimationLocked = false;
    private float animationLockEndTime = 0f;
    private Coroutine animationLockCoroutine;

    // ״̬�ָ���ر���
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

        // ״̬�ָ���� - ֻ�������Ҳ�����������״̬ʱ���
        if (enableStateRecovery && Time.time - lastStateCheckTime >= stateRecoveryCheckInterval && !IsInNormalHoldingState())
        {
            CheckStateRecovery();
            lastStateCheckTime = Time.time;
        }

        // ֻ����û�ж�������ʱ�Ÿ��·���
        if (!isAnimationLocked)
        {
            UpdateSpriteDirection();
        }
    }

    void UpdateAnimationStates()
    {
        // ��鶯������״̬
        CheckAnimationLock();

        bool isMoving = CheckMovement();
        bool currentlyHolding = playerController != null && playerController.IsHoldingItem();

        // ������������״̬��־
        UpdateNormalHoldingState(currentlyHolding, isMoving);

        // ������ڶ�������״̬������������״̬
        if (!isAnimationLocked)
        {
            playerAnimator.SetBool(walkParameter, isMoving);
        }

        // ����Ƿ�ոտ�ʼ������Ʒ
        if (currentlyHolding && !isHoldingItem)
        {
            // ����ʰȡ����
            TriggerPickUpAnimation();
        }

        // ���³�����Ʒ״̬
        if (isHoldingItem != currentlyHolding || forceStateUpdate)
        {
            isHoldingItem = currentlyHolding;

            // ���û�ж�����������������Animator
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

    // ������������������״̬��־
    void UpdateNormalHoldingState(bool currentlyHolding, bool isMoving)
    {
        // ��������״̬��������Ʒ�Ҵ���HoldIdle��HoldWalk״̬
        isInNormalHoldingState = currentlyHolding && (GetCurrentStateName() == "HoldIdle" || GetCurrentStateName() == "HoldWalk");

        // ���������������״̬������״̬��ʼʱ��
        if (isInNormalHoldingState)
        {
            stateStartTime = Time.time;
        }
    }

    // ����������Ƿ�����������״̬
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
                Debug.Log($"{playerName} ������������");
            }
        }
    }

    // �޸ģ�״̬�ָ���飬ֻ�������������״̬
    void CheckStateRecovery()
    {
        if (playerAnimator == null) return;

        string currentState = GetCurrentStateName();
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

        // ֻ��״̬�ı�ʱ���ü�ʱ��
        if (currentState != lastAnimatorState)
        {
            stateStartTime = Time.time;
            lastAnimatorState = currentState;
        }

        // ����Ƿ������������ų���������״̬��
        if (!IsInNormalHoldingState() && Time.time - stateStartTime > stuckStateThreshold)
        {
            // ֻ���ض�״̬���лָ����
            if (ShouldRecoverFromState(currentState, stateInfo))
            {
                Debug.LogWarning($"{playerName} ��⵽����״̬: {currentState}������ {Time.time - stateStartTime:F1}��");
                ForceRecoveryFromStuckState();
            }
        }

        lastAnimatorState = currentState;
    }

    // �������ж��Ƿ�Ӧ�ô�ĳ��״̬�ָ�
    bool ShouldRecoverFromState(string stateName, AnimatorStateInfo stateInfo)
    {
        // ��Զ����Ҫ����Щ����״̬�ָ�
        if (stateName == "Idle" || stateName == "Walk" || stateName == "HoldIdle" || stateName == "HoldWalk")
            return false;

        // ����PickUp״̬��ֻ�ڶ������Ŷ�κ��Բ��л�ʱ�ָ�
        if (stateName == "PickUp" && stateInfo.normalizedTime > 2.0f)
            return true;

        // ��������״̬��ʹ��Ĭ����ֵ
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
                Debug.Log($"{playerName} ��ת����: {newFlipState}");
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

    // ǿ�����ö���״̬
    public void SetHoldingState(bool holding)
    {
        isHoldingItem = holding;
        if (playerAnimator != null && !isAnimationLocked)
        {
            playerAnimator.SetBool(holdParameter, holding);
            forceStateUpdate = true;
        }
    }

    // �������⶯��
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
                Debug.Log($"{playerName} ����ʰȡ����������ʱ��: {pickUpAnimationLockTime}��");
            }
        }
    }

    // ��ʼ��������
    public void StartAnimationLock(float lockTime)
    {
        if (animationLockCoroutine != null)
        {
            StopCoroutine(animationLockCoroutine);
        }
        animationLockCoroutine = StartCoroutine(AnimationLockCoroutine(lockTime));
    }

    // ��������Э��
    private System.Collections.IEnumerator AnimationLockCoroutine(float lockTime)
    {
        isAnimationLocked = true;
        animationLockEndTime = Time.time + lockTime;

        playerAnimator.SetBool(walkParameter, false);

        if (showDebugInfo)
        {
            Debug.Log($"{playerName} ��ʼ�������������� {lockTime}��");
        }

        yield return new WaitForSeconds(lockTime);

        isAnimationLocked = false;
        forceStateUpdate = true;

        if (showDebugInfo)
        {
            Debug.Log($"{playerName} ������������");
        }
    }

    // ǿ�ƽ�����������
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
            Debug.Log($"{playerName} ǿ�ƽ�����������");
        }
    }

    // ����Ƿ��ڶ�������״̬
    public bool IsAnimationLocked()
    {
        return isAnimationLocked;
    }

    // �޸ģ�ǿ�ƴӿ���״̬�ָ�����������״̬
    void ForceRecoveryFromStuckState()
    {
        if (playerAnimator == null) return;

        // ��ȡ��ǰʵ��״̬
        bool shouldHold = playerController != null && playerController.IsHoldingItem();
        bool isMoving = CheckMovement();

        // ���ô�����
        playerAnimator.ResetTrigger(pickUpParameter);

        // ������ȷ�Ĳ���
        playerAnimator.SetBool(holdParameter, shouldHold);
        playerAnimator.SetBool(walkParameter, isMoving);

        // ǿ�ƽ�����������
        ForceEndAnimationLock();

        // ����״̬������ȷ�Ķ���
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

        // ����״̬��ʱ��
        stateStartTime = Time.time;

        Debug.Log($"{playerName} ִ��״̬ǿ�ƻָ���״̬: {(shouldHold ? (isMoving ? "HoldWalk" : "HoldIdle") : (isMoving ? "Walk" : "Idle"))}");
    }

    // ��ȡ��ǰ״̬����
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

    // �������ж���״̬
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

    // �ֶ����÷�ת����
    public void SetFlipDirection(bool flipX)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = flipX;
            lastStableDirection = flipX ? Vector3.left : Vector3.right;
        }
    }

    // ������Ϣ
    [Header("����ѡ��")]
    public bool showDebugInfo = false;
    private string playerName = "Player";

    void OnEnable()
    {
        if (playerController != null)
        {
            playerName = playerController.playerName;
        }
    }

    // ��������ȡ״̬�ָ���Ϣ�����ڵ��ԣ�
    public string GetStateRecoveryInfo()
    {
        return $"״̬: {GetCurrentStateName()}, ��������: {IsInNormalHoldingState()}, ״̬����ʱ��: {Time.time - stateStartTime:F1}s";
    }
}