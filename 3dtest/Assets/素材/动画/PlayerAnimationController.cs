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

    [Header("����ѡ��")]
    public bool showDebugInfo = false;

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

    // �������������ȼ�����
    private float lastHorizontalInput = 0f;
    private float lastVerticalInput = 0f;
    private bool preferHorizontal = true; // ˮƽ��������

    private string playerName = "Player";

    void Start()
    {
        InitializeComponents();
        lastPosition = transform.position;
        FindPlayerCamera();
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

        // ����Ƿ�ոտ�ʼ������Ʒ
        if (currentlyHolding && !isHoldingItem)
        {
            TriggerPickUpAnimation();
        }

        // ���³�����Ʒ״̬
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
                Debug.Log($"{playerName} ������������");
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

        // ��¼��ǰ����
        float currentHorizontal = inputDirection.x;
        float currentVertical = inputDirection.z;

        // ����Ƿ��жԽ�������
        bool hasDiagonalInput = Mathf.Abs(currentHorizontal) > 0.1f && Mathf.Abs(currentVertical) > 0.1f;

        if (inputDirection.magnitude > movementThreshold)
        {
            Vector3 worldDirection = GetCameraRelativeDirection(inputDirection, playerCamera);

            // ����Խ������룺����ˮƽ����
            if (hasDiagonalInput)
            {
                // ��ͬʱ����WD��WAʱ����ˮƽ����Ϊ��
                bool hasSignificantHorizontalInput = Mathf.Abs(currentHorizontal) > flipDeadZone;

                if (hasSignificantHorizontalInput)
                {
                    // ʹ��ˮƽ���������ת
                    bool shouldFlip = currentHorizontal < 0;
                    ApplyFlipWithCooldown(shouldFlip);
                    lastStableDirection = worldDirection;

                    if (showDebugInfo)
                    {
                        Debug.Log($"{playerName} �Խ������룬ʹ��ˮƽ����: {currentHorizontal}, ��ת: {shouldFlip}");
                    }
                }
            }
            else
            {
                // ���������ʹ�����緽��
                bool hasSignificantHorizontalInput = Mathf.Abs(inputDirection.x) > flipDeadZone;

                if (hasSignificantHorizontalInput)
                {
                    bool shouldFlip = worldDirection.x < 0;
                    ApplyFlipWithCooldown(shouldFlip);
                    lastStableDirection = worldDirection;
                }
            }

            // ������������¼
            lastHorizontalInput = currentHorizontal;
            lastVerticalInput = currentVertical;
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
                Debug.Log($"{playerName} ����ʰȡ����������ʱ��: {pickUpAnimationLockTime}��");
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
    }

    public void SetFlipDirection(bool flipX)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = flipX;
            lastStableDirection = flipX ? Vector3.left : Vector3.right;
        }
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
        return $"��������: {isAnimationLocked}, ����״̬: {isHoldingItem}";
    }
}