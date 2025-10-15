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

    [Header("调试选项")]
    public bool showDebugInfo = false;

    private Rigidbody rb1, rb2;
    private Vector3 movement1, movement2;
    private Transform player1Visual, player2Visual;
    private PlayerCamera player1CameraController, player2CameraController;

    // Sprite旋转的平滑变量
    private float player1VisualVelocity;
    private float player2VisualVelocity;

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
        GetPlayerInput();

        if (use2DSpriteMode)
        {
            UpdateSpriteRotations();
        }

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

        // 玩家2输入 (IJKL)
        float h2 = 0f, v2 = 0f;
        if (Input.GetKey(KeyCode.L)) h2 += 1f;
        if (Input.GetKey(KeyCode.J)) h2 -= 1f;
        if (Input.GetKey(KeyCode.I)) v2 += 1f;
        if (Input.GetKey(KeyCode.K)) v2 -= 1f;
        movement2 = new Vector3(h2, 0f, v2).normalized;
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

    void DebugRotationState()
    {
        Debug.Log("=== 2D Sprite旋转状态 ===");
        Debug.Log($"旋转模式: {rotationMode}");
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
    //[Header("玩家对象")]
    //public GameObject player1;
    //public GameObject player2;

    //[Header("移动设置")]
    //public float moveSpeed = 8f;

    //[Header("摄像机参考")]
    //public Camera player1Camera;
    //public Camera player2Camera;

    //[Header("2D Sprite设置")]
    //public bool use2DSpriteMode = true;
    //[Range(0f, 360f)]
    //public float player1SpriteOffset = 0f;
    //[Range(0f, 360f)]
    //public float player2SpriteOffset = 0f;

    //[Header("Sprite旋转模式")]
    //public SpriteRotationMode rotationMode = SpriteRotationMode.Billboard;

    //[Header("平滑设置")]
    //public float spriteRotationSmoothTime = 0.1f;

    //[Header("调试选项")]
    //public bool showDebugInfo = false;

    //private Rigidbody rb1, rb2;
    //private Vector3 movement1, movement2;
    //private Transform player1Visual, player2Visual;
    //private PlayerCamera player1CameraController, player2CameraController;

    //// Sprite旋转的平滑变量
    //private float player1VisualVelocity;
    //private float player2VisualVelocity;

    //// Sprite旋转模式枚举
    //public enum SpriteRotationMode
    //{
    //    Billboard,      // 始终面向摄像机
    //    FixedAngle,     // 固定角度
    //    MovementBased   // 基于移动方向
    //}

    //void Start()
    //{
    //    InitializePlayers();
    //    SetupCameraListeners();
    //    Debug.Log("双人控制器初始化完成 - 新按键配置");
    //    Debug.Log("玩家1: WS移动, AD视角转动");
    //    Debug.Log("玩家2: IK移动, JL视角转动");
    //}

    //void InitializePlayers()
    //{
    //    // 初始化玩家1
    //    if (player1 != null)
    //    {
    //        rb1 = player1.GetComponent<Rigidbody>();
    //        if (rb1 != null) rb1.freezeRotation = true;

    //        player1Visual = FindVisualTransform(player1);
    //    }

    //    // 初始化玩家2
    //    if (player2 != null)
    //    {
    //        rb2 = player2.GetComponent<Rigidbody>();
    //        if (rb2 != null) rb2.freezeRotation = true;

    //        player2Visual = FindVisualTransform(player2);
    //    }
    //}

    //void SetupCameraListeners()
    //{
    //    // 获取摄像机控制器并设置监听
    //    if (player1Camera != null)
    //    {
    //        player1CameraController = player1Camera.GetComponent<PlayerCamera>();
    //        if (player1CameraController != null)
    //        {
    //            player1CameraController.OnCameraRotated += OnPlayer1CameraRotated;
    //            //// 设置玩家1的视角控制按键
    //            //player1CameraController.rotateLeftKey = KeyCode.D;
    //            //player1CameraController.rotateRightKey = KeyCode.A;
    //            // 确保禁用自动转向
    //            player1CameraController.enableAutoRotation = false;
    //        }
    //    }

    //    if (player2Camera != null)
    //    {
    //        player2CameraController = player2Camera.GetComponent<PlayerCamera>();
    //        if (player2CameraController != null)
    //        {
    //            player2CameraController.OnCameraRotated += OnPlayer2CameraRotated;
    //            //// 设置玩家2的视角控制按键
    //            //player2CameraController.rotateLeftKey = KeyCode.L;
    //            //player2CameraController.rotateRightKey = KeyCode.J;
    //            // 确保禁用自动转向
    //            player2CameraController.enableAutoRotation = false;
    //        }
    //    }
    //}

    //Transform FindVisualTransform(GameObject playerObject)
    //{
    //    // 查找SpriteRenderer
    //    SpriteRenderer spriteRenderer = playerObject.GetComponent<SpriteRenderer>();
    //    if (spriteRenderer != null)
    //    {
    //        return spriteRenderer.transform;
    //    }

    //    // 查找子物体中的SpriteRenderer
    //    SpriteRenderer[] childSprites = playerObject.GetComponentsInChildren<SpriteRenderer>();
    //    if (childSprites.Length > 0)
    //    {
    //        return childSprites[0].transform;
    //    }

    //    // 如果都没有，返回对象本身
    //    return playerObject.transform;
    //}

    //void Update()
    //{
    //    GetPlayerInput();

    //    if (use2DSpriteMode)
    //    {
    //        UpdateSpriteRotations();
    //    }

    //    if (showDebugInfo && Input.GetKeyDown(KeyCode.F1))
    //    {
    //        DebugRotationState();
    //    }

    //    // 调试：显示当前输入状态
    //    if (showDebugInfo && Time.frameCount % 60 == 0)
    //    {
    //        Debug.Log($"玩家1移动输入: {movement1}, 玩家2移动输入: {movement2}");
    //    }
    //}

    //void FixedUpdate()
    //{
    //    MovePlayers();
    //}

    //void GetPlayerInput()
    //{
    //    // 玩家1输入 - 只检测WS键用于移动
    //    float v1 = 0f;
    //    if (Input.GetKey(KeyCode.W)) v1 += 1f;  // 前进
    //    if (Input.GetKey(KeyCode.S)) v1 -= 1f;  // 后退
    //    // 注意：AD键现在完全用于视角控制，不参与移动
    //    movement1 = new Vector3(0f, 0f, v1).normalized;

    //    // 玩家2输入 - 只检测IK键用于移动
    //    float v2 = 0f;
    //    if (Input.GetKey(KeyCode.I)) v2 += 1f;  // 前进
    //    if (Input.GetKey(KeyCode.K)) v2 -= 1f;  // 后退
    //    // 注意：JL键现在完全用于视角控制，不参与移动
    //    movement2 = new Vector3(0f, 0f, v2).normalized;

    //    // 调试：如果检测到AD/JL键被按下，记录警告
    //    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) ||
    //        Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.L))
    //    {
    //        if (showDebugInfo)
    //        {
    //            Debug.Log("检测到视角控制按键按下，这些按键不会影响移动");
    //        }
    //    }
    //}

    //void MovePlayers()
    //{
    //    // 移动玩家1
    //    if (rb1 != null)
    //    {
    //        if (movement1.magnitude > 0.1f)
    //        {
    //            Vector3 moveDirection = GetCameraRelativeDirection(movement1, player1Camera);
    //            Vector3 moveVelocity = moveDirection * moveSpeed;
    //            rb1.velocity = new Vector3(moveVelocity.x, rb1.velocity.y, moveVelocity.z);

    //            if (showDebugInfo)
    //            {
    //                Debug.Log($"玩家1移动方向: {moveDirection}, 速度: {moveVelocity}");
    //            }
    //        }
    //        else
    //        {
    //            // 只在X和Z轴归零，保持Y轴重力
    //            rb1.velocity = new Vector3(0f, rb1.velocity.y, 0f);
    //        }
    //    }

    //    // 移动玩家2
    //    if (rb2 != null)
    //    {
    //        if (movement2.magnitude > 0.1f)
    //        {
    //            Vector3 moveDirection = GetCameraRelativeDirection(movement2, player2Camera);
    //            Vector3 moveVelocity = moveDirection * moveSpeed;
    //            rb2.velocity = new Vector3(moveVelocity.x, rb2.velocity.y, moveVelocity.z);

    //            if (showDebugInfo)
    //            {
    //                Debug.Log($"玩家2移动方向: {moveDirection}, 速度: {moveVelocity}");
    //            }
    //        }
    //        else
    //        {
    //            // 只在X和Z轴归零，保持Y轴重力
    //            rb2.velocity = new Vector3(0f, rb2.velocity.y, 0f);
    //        }
    //    }
    //}

    //void UpdateSpriteRotations()
    //{
    //    if (player1Visual != null)
    //    {
    //        UpdateSingleSpriteRotation(player1Visual, player1Camera, movement1, player1SpriteOffset);
    //    }

    //    if (player2Visual != null)
    //    {
    //        UpdateSingleSpriteRotation(player2Visual, player2Camera, movement2, player2SpriteOffset);
    //    }
    //}

    //void UpdateSingleSpriteRotation(Transform sprite, Camera camera, Vector3 movement, float offset)
    //{
    //    if (sprite == null || camera == null) return;

    //    switch (rotationMode)
    //    {
    //        case SpriteRotationMode.Billboard:
    //            UpdateBillboardRotation(sprite, camera, offset);
    //            break;

    //        case SpriteRotationMode.FixedAngle:
    //            UpdateFixedRotation(sprite, offset);
    //            break;

    //        case SpriteRotationMode.MovementBased:
    //            UpdateMovementBasedRotation(sprite, camera, movement, offset);
    //            break;
    //    }
    //}

    //void UpdateBillboardRotation(Transform sprite, Camera camera, float offset)
    //{
    //    // 2D Sprite Billboard：始终面向摄像机
    //    Vector3 toCamera = camera.transform.position - sprite.position;
    //    toCamera.y = 0; // 保持水平旋转

    //    if (toCamera != Vector3.zero)
    //    {
    //        Quaternion targetRotation = Quaternion.LookRotation(-toCamera) * Quaternion.Euler(0, offset, 0);
    //        sprite.rotation = Quaternion.Slerp(sprite.rotation, targetRotation, spriteRotationSmoothTime);
    //    }
    //}

    //void UpdateFixedRotation(Transform sprite, float offset)
    //{
    //    // 固定角度旋转
    //    Quaternion targetRotation = Quaternion.Euler(0, offset, 0);
    //    sprite.rotation = Quaternion.Slerp(sprite.rotation, targetRotation, spriteRotationSmoothTime);
    //}

    //void UpdateMovementBasedRotation(Transform sprite, Camera camera, Vector3 movement, float offset)
    //{
    //    // 基于移动方向的旋转
    //    if (movement.magnitude > 0.1f)
    //    {
    //        Vector3 moveDirection = GetCameraRelativeDirection(movement, camera);
    //        if (moveDirection != Vector3.zero)
    //        {
    //            Quaternion targetRotation = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(0, offset, 0);
    //            sprite.rotation = Quaternion.Slerp(sprite.rotation, targetRotation, 10f * Time.deltaTime);
    //        }
    //    }
    //    else
    //    {
    //        // 没有移动时保持面向摄像机
    //        UpdateBillboardRotation(sprite, camera, offset);
    //    }
    //}

    //// 摄像机旋转事件处理
    //void OnPlayer1CameraRotated(float angle)
    //{
    //    if (player1Visual != null && use2DSpriteMode && rotationMode == SpriteRotationMode.Billboard)
    //    {
    //        UpdateBillboardRotation(player1Visual, player1Camera, player1SpriteOffset);
    //    }
    //}

    //void OnPlayer2CameraRotated(float angle)
    //{
    //    if (player2Visual != null && use2DSpriteMode && rotationMode == SpriteRotationMode.Billboard)
    //    {
    //        UpdateBillboardRotation(player2Visual, player2Camera, player2SpriteOffset);
    //    }
    //}

    //Vector3 GetCameraRelativeDirection(Vector3 inputDirection, Camera cam)
    //{
    //    if (cam == null) return inputDirection;

    //    Vector3 cameraForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
    //    Vector3 cameraRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;

    //    return cameraForward * inputDirection.z + cameraRight * inputDirection.x;
    //}

    //void DebugRotationState()
    //{
    //    Debug.Log("=== 2D Sprite旋转状态 ===");
    //    Debug.Log($"旋转模式: {rotationMode}");
    //    if (player1Visual != null)
    //    {
    //        Debug.Log($"玩家1 - 旋转: {player1Visual.rotation.eulerAngles}");
    //    }
    //    if (player2Visual != null)
    //    {
    //        Debug.Log($"玩家2 - 旋转: {player2Visual.rotation.eulerAngles}");
    //    }
    //    Debug.Log("=======================");
    //}

    //// 公共方法：手动检查按键配置
    //public void CheckKeyConfiguration()
    //{
    //    Debug.Log("=== 按键配置检查 ===");
    //    Debug.Log($"玩家1移动: W/S");
    //    Debug.Log($"玩家1视角: A/D");
    //    Debug.Log($"玩家2移动: I/K");
    //    Debug.Log($"玩家2视角: J/L");

    //    if (player1CameraController != null)
    //    {
    //        Debug.Log($"玩家1摄像机左转键: {player1CameraController.rotateLeftKey}");
    //        Debug.Log($"玩家1摄像机右转键: {player1CameraController.rotateRightKey}");
    //    }
    //    if (player2CameraController != null)
    //    {
    //        Debug.Log($"玩家2摄像机左转键: {player2CameraController.rotateLeftKey}");
    //        Debug.Log($"玩家2摄像机右转键: {player2CameraController.rotateRightKey}");
    //    }
    //    Debug.Log("===================");
    //}

    //// 清理事件监听
    //void OnDestroy()
    //{
    //    if (player1CameraController != null)
    //    {
    //        player1CameraController.OnCameraRotated -= OnPlayer1CameraRotated;
    //    }
    //    if (player2CameraController != null)
    //    {
    //        player2CameraController.OnCameraRotated -= OnPlayer2CameraRotated;
    //    }
    //}
}
