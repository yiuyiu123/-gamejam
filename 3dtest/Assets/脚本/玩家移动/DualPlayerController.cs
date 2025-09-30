using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualPlayerController : MonoBehaviour
{
    //[Header("玩家对象")]
    //public GameObject player1;
    //public GameObject player2;

    //[Header("移动设置")]
    //public float moveSpeed = 8f;

    //[Header("摄像机参考")]
    //public Camera player1Camera;
    //public Camera player2Camera;

    //[Header("调试选项")]
    //public bool showInputDebug = false;

    //private Rigidbody rb1, rb2;
    //private Vector3 movement1, movement2;

    //void Start()
    //{
    //    // 初始化玩家1
    //    if (player1 != null)
    //    {
    //        rb1 = player1.GetComponent<Rigidbody>();
    //        if (rb1 != null) rb1.freezeRotation = true;
    //    }

    //    // 初始化玩家2
    //    if (player2 != null)
    //    {
    //        rb2 = player2.GetComponent<Rigidbody>();
    //        if (rb2 != null) rb2.freezeRotation = true;
    //    }

    //    Debug.Log("双人控制器初始化完成");
    //    Debug.Log("玩家1控制: WASD");
    //    Debug.Log("玩家2控制: IJKL");
    //}

    //void Update()
    //{
    //    GetPlayerInput();
    //}

    //void FixedUpdate()
    //{
    //    MovePlayers();
    //}

    //void GetPlayerInput()
    //{
    //    // 玩家1输入 (WASD)
    //    float h1 = 0f, v1 = 0f;
    //    if (Input.GetKey(KeyCode.D)) h1 += 1f;
    //    if (Input.GetKey(KeyCode.A)) h1 -= 1f;
    //    if (Input.GetKey(KeyCode.W)) v1 += 1f;
    //    if (Input.GetKey(KeyCode.S)) v1 -= 1f;
    //    movement1 = new Vector3(h1, 0f, v1).normalized;

    //    // 玩家2输入 (IJKL)
    //    float h2 = 0f, v2 = 0f;
    //    if (Input.GetKey(KeyCode.L)) h2 += 1f;
    //    if (Input.GetKey(KeyCode.J)) h2 -= 1f;
    //    if (Input.GetKey(KeyCode.I)) v2 += 1f;
    //    if (Input.GetKey(KeyCode.K)) v2 -= 1f;
    //    movement2 = new Vector3(h2, 0f, v2).normalized;

    //    // 调试信息
    //    if (showInputDebug)
    //    {
    //        if (movement1.magnitude > 0.1f) Debug.Log($"玩家1输入: {movement1}");
    //        if (movement2.magnitude > 0.1f) Debug.Log($"玩家2输入: {movement2}");
    //    }
    //}

    //void MovePlayers()
    //{
    //    // 移动玩家1
    //    if (rb1 != null && movement1.magnitude > 0.1f)
    //    {
    //        Vector3 moveDirection = GetCameraRelativeDirection(movement1, player1Camera);
    //        Vector3 moveVelocity = moveDirection * moveSpeed;
    //        rb1.velocity = new Vector3(moveVelocity.x, rb1.velocity.y, moveVelocity.z);

    //        // 面向移动方向
    //        if (moveDirection != Vector3.zero)
    //        {
    //            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
    //            rb1.rotation = Quaternion.Slerp(rb1.rotation, targetRotation, 10f * Time.deltaTime);
    //        }
    //    }
    //    else if (rb1 != null)
    //    {
    //        rb1.velocity = new Vector3(0f, rb1.velocity.y, 0f);
    //    }

    //    // 移动玩家2
    //    if (rb2 != null && movement2.magnitude > 0.1f)
    //    {
    //        Vector3 moveDirection = GetCameraRelativeDirection(movement2, player2Camera);
    //        Vector3 moveVelocity = moveDirection * moveSpeed;
    //        rb2.velocity = new Vector3(moveVelocity.x, rb2.velocity.y, moveVelocity.z);

    //        // 面向移动方向
    //        if (moveDirection != Vector3.zero)
    //        {
    //            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
    //            rb2.rotation = Quaternion.Slerp(rb2.rotation, targetRotation, 10f * Time.deltaTime);
    //        }
    //    }
    //    else if (rb2 != null)
    //    {
    //        rb2.velocity = new Vector3(0f, rb2.velocity.y, 0f);
    //    }
    //}

    //Vector3 GetCameraRelativeDirection(Vector3 inputDirection, Camera cam)
    //{
    //    if (cam == null) return inputDirection;

    //    // 获取摄像机的前方向和右方向（忽略Y轴）
    //    Vector3 cameraForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
    //    Vector3 cameraRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;

    //    // 将输入方向转换为摄像机空间方向
    //    return cameraForward * inputDirection.z + cameraRight * inputDirection.x;
    //}

    //[Header("玩家对象")]
    //public GameObject player1;
    //public GameObject player2;

    //[Header("移动设置")]
    //public float moveSpeed = 8f;

    //[Header("摄像机参考")]
    //public Camera player1Camera;
    //public Camera player2Camera;

    //[Header("2.5D贴图设置")]
    //public bool useBillboardFacing = true;
    //[Range(0f, 360f)]
    //public float player1RotationOffset = 0f;
    //[Range(0f, 360f)]
    //public float player2RotationOffset = 0f;

    //[Header("贴图立起设置")]
    //public float standUpXAngle = 90f; // X轴立起角度，默认为90度

    //[Header("调试选项")]
    //public bool showDebugInfo = false;
    //public KeyCode debugKey = KeyCode.F1;

    //private Rigidbody rb1, rb2;
    //private Vector3 movement1, movement2;
    //private Transform player1Visual, player2Visual;

    //void Start()
    //{
    //    InitializePlayers();
    //    Debug.Log("双人控制器初始化完成 - 贴图立起模式");
    //}

    //void InitializePlayers()
    //{
    //    // 初始化玩家1
    //    if (player1 != null)
    //    {
    //        rb1 = player1.GetComponent<Rigidbody>();
    //        if (rb1 != null) rb1.freezeRotation = true;

    //        player1Visual = FindVisualTransform(player1);
    //        if (player1Visual != null)
    //        {
    //            ResetPlayerRotation(player1Visual, player1Camera, player1RotationOffset);
    //            Debug.Log($"玩家1视觉部分找到: {player1Visual.name}");
    //        }
    //    }

    //    // 初始化玩家2
    //    if (player2 != null)
    //    {
    //        rb2 = player2.GetComponent<Rigidbody>();
    //        if (rb2 != null) rb2.freezeRotation = true;

    //        player2Visual = FindVisualTransform(player2);
    //        if (player2Visual != null)
    //        {
    //            ResetPlayerRotation(player2Visual, player2Camera, player2RotationOffset);
    //            Debug.Log($"玩家2视觉部分找到: {player2Visual.name}");
    //        }
    //    }
    //}

    //Transform FindVisualTransform(GameObject playerObject)
    //{
    //    if (playerObject.GetComponent<MeshRenderer>() != null)
    //    {
    //        return playerObject.transform;
    //    }

    //    MeshRenderer[] renderers = playerObject.GetComponentsInChildren<MeshRenderer>();
    //    if (renderers.Length > 0)
    //    {
    //        return renderers[0].transform;
    //    }

    //    if (playerObject.transform.childCount > 0)
    //    {
    //        return playerObject.transform.GetChild(0);
    //    }

    //    return playerObject.transform;
    //}

    //void ResetPlayerRotation(Transform visual, Camera camera, float offset)
    //{
    //    if (visual == null || camera == null) return;

    //    // 首先让贴图立起来（X轴旋转90度）
    //    visual.rotation = Quaternion.Euler(standUpXAngle, 0, 0);

    //    // 然后计算面向摄像机的方向
    //    Vector3 toCamera = camera.transform.position - visual.position;
    //    toCamera.y = 0;

    //    if (toCamera != Vector3.zero)
    //    {
    //        // 组合旋转：先立起来，再面向摄像机
    //        Quaternion standUpRotation = Quaternion.Euler(standUpXAngle, 0, 0);
    //        Quaternion facingRotation = Quaternion.LookRotation(-toCamera) * Quaternion.Euler(0, offset, 0);

    //        // 应用组合旋转
    //        visual.rotation = facingRotation * standUpRotation;

    //        if (showDebugInfo)
    //        {
    //            Debug.Log($"重置旋转: {visual.name}, 最终旋转={visual.rotation.eulerAngles}");
    //        }
    //    }
    //}

    //void Update()
    //{
    //    GetPlayerInput();

    //    if (useBillboardFacing)
    //    {
    //        UpdateBillboardFacing();
    //    }

    //    if (Input.GetKeyDown(debugKey))
    //    {
    //        DebugRotationState();
    //    }

    //    // 测试功能：实时调整立起角度
    //    if (showDebugInfo)
    //    {
    //        if (Input.GetKeyDown(KeyCode.Alpha1))
    //        {
    //            standUpXAngle += 15f;
    //            ResetAllRotations();
    //        }
    //        if (Input.GetKeyDown(KeyCode.Alpha2))
    //        {
    //            standUpXAngle -= 15f;
    //            ResetAllRotations();
    //        }
    //    }
    //}

    //void FixedUpdate()
    //{
    //    MovePlayers();
    //}

    //void GetPlayerInput()
    //{
    //    // 玩家1输入 (WASD)
    //    float h1 = 0f, v1 = 0f;
    //    if (Input.GetKey(KeyCode.D)) h1 += 1f;
    //    if (Input.GetKey(KeyCode.A)) h1 -= 1f;
    //    if (Input.GetKey(KeyCode.W)) v1 += 1f;
    //    if (Input.GetKey(KeyCode.S)) v1 -= 1f;
    //    movement1 = new Vector3(h1, 0f, v1).normalized;

    //    // 玩家2输入 (IJKL)
    //    float h2 = 0f, v2 = 0f;
    //    if (Input.GetKey(KeyCode.L)) h2 += 1f;
    //    if (Input.GetKey(KeyCode.J)) h2 -= 1f;
    //    if (Input.GetKey(KeyCode.I)) v2 += 1f;
    //    if (Input.GetKey(KeyCode.K)) v2 -= 1f;
    //    movement2 = new Vector3(h2, 0f, v2).normalized;
    //}

    //void MovePlayers()
    //{
    //    // 移动玩家1
    //    if (rb1 != null && movement1.magnitude > 0.1f)
    //    {
    //        Vector3 moveDirection = GetCameraRelativeDirection(movement1, player1Camera);
    //        Vector3 moveVelocity = moveDirection * moveSpeed;
    //        rb1.velocity = new Vector3(moveVelocity.x, rb1.velocity.y, moveVelocity.z);
    //    }
    //    else if (rb1 != null)
    //    {
    //        rb1.velocity = new Vector3(0f, rb1.velocity.y, 0f);
    //    }

    //    // 移动玩家2
    //    if (rb2 != null && movement2.magnitude > 0.1f)
    //    {
    //        Vector3 moveDirection = GetCameraRelativeDirection(movement2, player2Camera);
    //        Vector3 moveVelocity = moveDirection * moveSpeed;
    //        rb2.velocity = new Vector3(moveVelocity.x, rb2.velocity.y, moveVelocity.z);
    //    }
    //    else if (rb2 != null)
    //    {
    //        rb2.velocity = new Vector3(0f, rb2.velocity.y, 0f);
    //    }
    //}

    //void UpdateBillboardFacing()
    //{
    //    if (player1Visual != null && player1Camera != null)
    //    {
    //        UpdateSingleBillboard(player1Visual, player1Camera, player1RotationOffset);
    //    }

    //    if (player2Visual != null && player2Camera != null)
    //    {
    //        UpdateSingleBillboard(player2Visual, player2Camera, player2RotationOffset);
    //    }
    //}

    //void UpdateSingleBillboard(Transform visual, Camera camera, float offset)
    //{
    //    Vector3 toCamera = camera.transform.position - visual.position;
    //    toCamera.y = 0;

    //    if (toCamera != Vector3.zero)
    //    {
    //        // 组合旋转：立起 + 面向摄像机
    //        Quaternion standUpRotation = Quaternion.Euler(standUpXAngle, 0, 0);
    //        Quaternion facingRotation = Quaternion.LookRotation(-toCamera) * Quaternion.Euler(0, offset, 0);

    //        visual.rotation = facingRotation * standUpRotation;
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
    //    Debug.Log("=== 旋转状态调试 ===");
    //    Debug.Log($"立起角度: {standUpXAngle}度");
    //    if (player1Visual != null)
    //    {
    //        Debug.Log($"玩家1 - 旋转: {player1Visual.rotation.eulerAngles}");
    //    }
    //    if (player2Visual != null)
    //    {
    //        Debug.Log($"玩家2 - 旋转: {player2Visual.rotation.eulerAngles}");
    //    }
    //    Debug.Log("===================");
    //}

    //void ResetAllRotations()
    //{
    //    if (player1Visual != null && player1Camera != null)
    //    {
    //        ResetPlayerRotation(player1Visual, player1Camera, player1RotationOffset);
    //    }
    //    if (player2Visual != null && player2Camera != null)
    //    {
    //        ResetPlayerRotation(player2Visual, player2Camera, player2RotationOffset);
    //    }
    //    Debug.Log($"所有贴图立起角度更新为: {standUpXAngle}度");
    //}

    //// 公共方法：手动修正旋转
    //public void FixAllRotations()
    //{
    //    ResetAllRotations();
    //}
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
}
