using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualPlayerController : MonoBehaviour
{
    [Header("玩家对象")]
    public GameObject player1;
    public GameObject player2;

    [Header("移动设置")]
    public float moveSpeed = 8f;

    [Header("摄像机参考")]
    public Camera player1Camera;
    public Camera player2Camera;

    [Header("2D Sprite设置")]
    public bool use2DSpriteMode = true;
    [Range(0f, 360f)]
    public float player1SpriteOffset = 0f;
    [Range(0f, 360f)]
    public float player2SpriteOffset = 0f;

    [Header("Sprite旋转模式")]
    public SpriteRotationMode rotationMode = SpriteRotationMode.Billboard;

    [Header("平滑设置")]
    public float spriteRotationSmoothTime = 0.1f;

    [Header("移动锁定设置")]
    public bool enableMovementLock = true; // 是否启用移动锁定功能

    [Header("移动音效设置")]
    public string player1MoveSoundGroupID = "玩家1脚步声";
    public string player2MoveSoundGroupID = "玩家2脚步声";
    public float moveSoundFadeInTime = 0.2f;
    public float moveSoundFadeOutTime = 0.3f;
    public float moveSoundInterval = 1.5f; // 音效播放间隔

    [Header("调试选项")]
    public bool showDebugInfo = false;

    private Rigidbody rb1, rb2;
    private Vector3 movement1, movement2;
    private Transform player1Visual, player2Visual;
    private PlayerCamera player1CameraController, player2CameraController;

    // 移动锁定状态
    private bool isMovementLocked = false;

    // Sprite旋转的平滑变量
    private float player1VisualVelocity;
    private float player2VisualVelocity;

    // 移动音效相关变量
    private bool isPlayer1Moving = false;
    private bool isPlayer2Moving = false;

    // 存储每个玩家实际的移动状态
    private bool player1ActuallyMoving = false;
    private bool player2ActuallyMoving = false;
    private Vector3 lastPlayer1Position;
    private Vector3 lastPlayer2Position;

    // 音效计时器
    private float player1SoundTimer = 0f;
    private float player2SoundTimer = 0f;
    private bool player1SoundReady = true;
    private bool player2SoundReady = true;

    // Sprite旋转模式枚举
    public enum SpriteRotationMode
    {
        Billboard,      // 始终面向摄像机
        FixedAngle,     // 固定角度
        MovementBased   // 基于移动方向
    }

    void Start()
    {
        InitializePlayers();
        SetupCameraListeners();

        // 初始化位置记录
        if (player1 != null) lastPlayer1Position = player1.transform.position;
        if (player2 != null) lastPlayer2Position = player2.transform.position;

        Debug.Log("双人控制器初始化完成 - 2D Sprite模式");
    }

    void InitializePlayers()
    {
        // 初始化玩家1
        if (player1 != null)
        {
            rb1 = player1.GetComponent<Rigidbody>();
            if (rb1 != null) rb1.freezeRotation = true;

            player1Visual = FindVisualTransform(player1);
        }

        // 初始化玩家2
        if (player2 != null)
        {
            rb2 = player2.GetComponent<Rigidbody>();
            if (rb2 != null) rb2.freezeRotation = true;

            player2Visual = FindVisualTransform(player2);
        }
    }

    void SetupCameraListeners()
    {
        // 获取摄像机控制器并设置监听
        if (player1Camera != null)
        {
            player1CameraController = player1Camera.GetComponent<PlayerCamera>();
            if (player1CameraController != null)
            {
                player1CameraController.OnCameraRotated += OnPlayer1CameraRotated;
            }
        }

        if (player2Camera != null)
        {
            player2CameraController = player2Camera.GetComponent<PlayerCamera>();
            if (player2CameraController != null)
            {
                player2CameraController.OnCameraRotated += OnPlayer2CameraRotated;
            }
        }
    }

    Transform FindVisualTransform(GameObject playerObject)
    {
        // 查找SpriteRenderer
        SpriteRenderer spriteRenderer = playerObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            return spriteRenderer.transform;
        }

        // 查找子物体中的SpriteRenderer
        SpriteRenderer[] childSprites = playerObject.GetComponentsInChildren<SpriteRenderer>();
        if (childSprites.Length > 0)
        {
            return childSprites[0].transform;
        }

        // 如果都没有，返回对象本身
        return playerObject.transform;
    }

    void Update()
    {
        // 如果UI禁用了输入，则停止响应移动
        var ui = FindObjectOfType<scene1UI_Manager>();
        if (ui != null && ui.noInputAnything)
        {
            // 允许F/H键单独响应
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.H))
            {
                // 可在这里触发对应UI逻辑
            }

            // 清空移动状态
            movement1 = Vector3.zero;
            movement2 = Vector3.zero;
            isPlayer1Moving = false;
            isPlayer2Moving = false;
            player1ActuallyMoving = false;
            player2ActuallyMoving = false;

            return;
        }

        // 检查移动是否被锁定
        if (!isMovementLocked)
        {
            GetPlayerInput();

            // 检测实际移动（基于位置变化）
            DetectActualMovement();
        }
        else
        {
            // 移动被锁定时清空输入和移动状态
            movement1 = Vector3.zero;
            movement2 = Vector3.zero;
            isPlayer1Moving = false;
            isPlayer2Moving = false;
            player1ActuallyMoving = false;
            player2ActuallyMoving = false;

            if (showDebugInfo && Time.frameCount % 60 == 0)
            {
                Debug.Log("移动被锁定 - 只能使用F/H键推进对话");
            }
        }

        if (use2DSpriteMode)
        {
            UpdateSpriteRotations();
        }

        // 更新移动音效
        UpdateMovementSound();

        if (showDebugInfo && Input.GetKeyDown(KeyCode.F1))
        {
            DebugRotationState();
        }
    }

    void FixedUpdate()
    {
        MovePlayers();
    }

    void GetPlayerInput()
    {
        // 玩家1输入 (WASD)
        float h1 = 0f, v1 = 0f;
        if (Input.GetKey(KeyCode.D)) h1 += 1f;
        if (Input.GetKey(KeyCode.A)) h1 -= 1f;
        if (Input.GetKey(KeyCode.W)) v1 += 1f;
        if (Input.GetKey(KeyCode.S)) v1 -= 1f;
        movement1 = new Vector3(h1, 0f, v1).normalized;
        isPlayer1Moving = movement1.magnitude > 0.1f;

        // 玩家2输入 (IJKL)
        float h2 = 0f, v2 = 0f;
        if (Input.GetKey(KeyCode.L)) h2 += 1f;
        if (Input.GetKey(KeyCode.J)) h2 -= 1f;
        if (Input.GetKey(KeyCode.I)) v2 += 1f;
        if (Input.GetKey(KeyCode.K)) v2 -= 1f;
        movement2 = new Vector3(h2, 0f, v2).normalized;
        isPlayer2Moving = movement2.magnitude > 0.1f;
    }

    void DetectActualMovement()
    {
        // 检测玩家1是否真的在移动（基于位置变化）
        if (player1 != null)
        {
            Vector3 currentPos1 = player1.transform.position;
            float distance1 = Vector3.Distance(currentPos1, lastPlayer1Position);
            player1ActuallyMoving = distance1 > 0.01f; // 微小阈值，避免浮点误差
            lastPlayer1Position = currentPos1;
        }

        // 检测玩家2是否真的在移动（基于位置变化）
        if (player2 != null)
        {
            Vector3 currentPos2 = player2.transform.position;
            float distance2 = Vector3.Distance(currentPos2, lastPlayer2Position);
            player2ActuallyMoving = distance2 > 0.01f;
            lastPlayer2Position = currentPos2;
        }

        // 调试信息
        if (showDebugInfo && Time.frameCount % 120 == 0)
        {
            Debug.Log($"移动状态 - P1输入:{isPlayer1Moving} 实际:{player1ActuallyMoving} | P2输入:{isPlayer2Moving} 实际:{player2ActuallyMoving}");
        }
    }

    void MovePlayers()
    {
        // 移动玩家1
        if (rb1 != null && movement1.magnitude > 0.1f)
        {
            Vector3 moveDirection = GetCameraRelativeDirection(movement1, player1Camera);
            Vector3 moveVelocity = moveDirection * moveSpeed;
            rb1.velocity = new Vector3(moveVelocity.x, rb1.velocity.y, moveVelocity.z);
        }
        else if (rb1 != null)
        {
            rb1.velocity = new Vector3(0f, rb1.velocity.y, 0f);
        }

        // 移动玩家2
        if (rb2 != null && movement2.magnitude > 0.1f)
        {
            Vector3 moveDirection = GetCameraRelativeDirection(movement2, player2Camera);
            Vector3 moveVelocity = moveDirection * moveSpeed;
            rb2.velocity = new Vector3(moveVelocity.x, rb2.velocity.y, moveVelocity.z);
        }
        else if (rb2 != null)
        {
            rb2.velocity = new Vector3(0f, rb2.velocity.y, 0f);
        }
    }

    void UpdateMovementSound()
    {
        // 更新音效计时器
        UpdateSoundTimers();

        // 玩家1移动音效
        if (player1ActuallyMoving && !isMovementLocked && player1SoundReady)
        {
            PlayPlayerMoveSound(true);
            player1SoundReady = false;
            player1SoundTimer = moveSoundInterval;
        }

        // 玩家2移动音效
        if (player2ActuallyMoving && !isMovementLocked && player2SoundReady)
        {
            PlayPlayerMoveSound(false);
            player2SoundReady = false;
            player2SoundTimer = moveSoundInterval;
        }
    }

    void UpdateSoundTimers()
    {
        // 更新玩家1音效计时器
        if (!player1SoundReady)
        {
            player1SoundTimer -= Time.deltaTime;
            if (player1SoundTimer <= 0f)
            {
                player1SoundReady = true;
            }
        }

        // 更新玩家2音效计时器
        if (!player2SoundReady)
        {
            player2SoundTimer -= Time.deltaTime;
            if (player2SoundTimer <= 0f)
            {
                player2SoundReady = true;
            }
        }
    }

    void PlayPlayerMoveSound(bool isPlayer1)
    {
        if (AudioManager.Instance != null)
        {
            string soundGroupID = isPlayer1 ? player1MoveSoundGroupID : player2MoveSoundGroupID;

            if (!string.IsNullOrEmpty(soundGroupID))
            {
                // 使用OneShot播放，避免循环导致的重复
                AudioManager.Instance.PlayOneShot(
                    soundGroupID,
                    -1,                    // 随机选择音效
                    true,                  // 淡入
                    moveSoundFadeInTime,
                    false,                 // 淡出
                    0f,
                    isPlayer1,             // 声道分配
                    false                  // 2D音效
                );

                if (showDebugInfo)
                {
                    Debug.Log($"播放{(isPlayer1 ? "玩家1" : "玩家2")}移动音效");
                }
            }
        }
    }

    void UpdateSpriteRotations()
    {
        if (player1Visual != null)
        {
            UpdateSingleSpriteRotation(player1Visual, player1Camera, movement1, player1SpriteOffset);
        }

        if (player2Visual != null)
        {
            UpdateSingleSpriteRotation(player2Visual, player2Camera, movement2, player2SpriteOffset);
        }
    }

    void UpdateSingleSpriteRotation(Transform sprite, Camera camera, Vector3 movement, float offset)
    {
        if (sprite == null || camera == null) return;

        switch (rotationMode)
        {
            case SpriteRotationMode.Billboard:
                UpdateBillboardRotation(sprite, camera, offset);
                break;

            case SpriteRotationMode.FixedAngle:
                UpdateFixedRotation(sprite, offset);
                break;

            case SpriteRotationMode.MovementBased:
                UpdateMovementBasedRotation(sprite, camera, movement, offset);
                break;
        }
    }

    void UpdateBillboardRotation(Transform sprite, Camera camera, float offset)
    {
        // 2D Sprite Billboard：始终面向摄像机
        Vector3 toCamera = camera.transform.position - sprite.position;
        toCamera.y = 0; // 保持水平旋转

        if (toCamera != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-toCamera) * Quaternion.Euler(0, offset, 0);
            sprite.rotation = Quaternion.Slerp(sprite.rotation, targetRotation, spriteRotationSmoothTime);
        }
    }

    void UpdateFixedRotation(Transform sprite, float offset)
    {
        // 固定角度旋转
        Quaternion targetRotation = Quaternion.Euler(0, offset, 0);
        sprite.rotation = Quaternion.Slerp(sprite.rotation, targetRotation, spriteRotationSmoothTime);
    }

    void UpdateMovementBasedRotation(Transform sprite, Camera camera, Vector3 movement, float offset)
    {
        // 基于移动方向的旋转
        if (movement.magnitude > 0.1f)
        {
            Vector3 moveDirection = GetCameraRelativeDirection(movement, camera);
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(0, offset, 0);
                sprite.rotation = Quaternion.Slerp(sprite.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }
        else
        {
            // 没有移动时保持面向摄像机
            UpdateBillboardRotation(sprite, camera, offset);
        }
    }

    // 摄像机旋转事件处理
    void OnPlayer1CameraRotated(float angle)
    {
        if (player1Visual != null && use2DSpriteMode && rotationMode == SpriteRotationMode.Billboard)
        {
            UpdateBillboardRotation(player1Visual, player1Camera, player1SpriteOffset);
        }
    }

    void OnPlayer2CameraRotated(float angle)
    {
        if (player2Visual != null && use2DSpriteMode && rotationMode == SpriteRotationMode.Billboard)
        {
            UpdateBillboardRotation(player2Visual, player2Camera, player2SpriteOffset);
        }
    }

    Vector3 GetCameraRelativeDirection(Vector3 inputDirection, Camera cam)
    {
        if (cam == null) return inputDirection;

        Vector3 cameraForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;

        return cameraForward * inputDirection.z + cameraRight * inputDirection.x;
    }

    /// <summary>
    /// 设置移动锁定状态 - 确保更新音效状态
    /// </summary>
    /// <param name="locked">true=锁定移动，false=解锁移动</param>
    public void SetMovementLock(bool locked)
    {
        if (!enableMovementLock) return;

        isMovementLocked = locked;

        // 锁定移动时立即停止玩家速度
        if (locked)
        {
            if (rb1 != null)
            {
                rb1.velocity = new Vector3(0f, rb1.velocity.y, 0f);
                isPlayer1Moving = false;
                player1ActuallyMoving = false;
            }
            if (rb2 != null)
            {
                rb2.velocity = new Vector3(0f, rb2.velocity.y, 0f);
                isPlayer2Moving = false;
                player2ActuallyMoving = false;
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"DualPlayerController 移动 {(locked ? "锁定" : "解锁")}");
        }
    }

    /// <summary>
    /// 获取当前移动锁定状态
    /// </summary>
    /// <returns>true=移动被锁定，false=移动正常</returns>
    public bool IsMovementLocked()
    {
        return isMovementLocked;
    }

    /// <summary>
    /// 获取玩家1的输入方向（供动画控制器使用）
    /// </summary>
    public Vector3 GetPlayer1InputDirection()
    {
        return movement1;
    }

    /// <summary>
    /// 获取玩家2的输入方向（供动画控制器使用）
    /// </summary>
    public Vector3 GetPlayer2InputDirection()
    {
        return movement2;
    }

    /// <summary>
    /// 获取玩家1的摄像机
    /// </summary>
    public Camera GetPlayer1Camera()
    {
        return player1Camera;
    }

    /// <summary>
    /// 获取玩家2的摄像机
    /// </summary>
    public Camera GetPlayer2Camera()
    {
        return player2Camera;
    }

    void DebugRotationState()
    {
        Debug.Log("=== 2D Sprite旋转状态 ===");
        Debug.Log($"旋转模式: {rotationMode}");
        Debug.Log($"移动锁定状态: {isMovementLocked}");
        if (player1Visual != null)
        {
            Debug.Log($"玩家1 - 旋转: {player1Visual.rotation.eulerAngles}");
        }
        if (player2Visual != null)
        {
            Debug.Log($"玩家2 - 旋转: {player2Visual.rotation.eulerAngles}");
        }
        Debug.Log("=======================");
    }

    // 清理事件监听
    void OnDestroy()
    {
        if (player1CameraController != null)
        {
            player1CameraController.OnCameraRotated -= OnPlayer1CameraRotated;
        }
        if (player2CameraController != null)
        {
            player2CameraController.OnCameraRotated -= OnPlayer2CameraRotated;
        }
    }
}