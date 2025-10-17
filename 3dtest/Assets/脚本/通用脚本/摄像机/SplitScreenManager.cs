using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplitScreenManager : MonoBehaviour
{
    //#region 绿色的注释
    ////[Header("玩家设置")]
    ////public Transform player1;
    ////public Transform player2;

    ////[Header("摄像机设置")]
    ////public Camera camera1;
    ////public Camera camera2;

    ////[Header("分屏模式")]
    ////public bool horizontalSplit = true; // 水平分屏

    ////[Header("摄像机参数")]
    ////public float cameraHeight = 10f;
    ////public float cameraDistance = 8f;
    ////public float rotationSpeed = 90f; // 增加到90

    ////[Header("初始角度设置")]
    ////public float player1StartAngle = 45f;
    ////public float player2StartAngle = 45f;

    ////private PlayerCamera player1Camera;
    ////private PlayerCamera player2Camera;

    ////void Start()
    ////{
    ////    SetupCameras();
    ////    SetupSplitScreen();
    ////    InitializeCameras();
    ////}

    ////void SetupCameras()
    ////{
    ////    // 确保摄像机存在
    ////    if (camera1 == null || camera2 == null)
    ////    {
    ////        CreateCameras();
    ////    }

    ////    // 获取或添加摄像机控制器
    ////    player1Camera = camera1.GetComponent<PlayerCamera>();
    ////    if (player1Camera == null)
    ////    {
    ////        player1Camera = camera1.gameObject.AddComponent<PlayerCamera>();
    ////    }

    ////    player2Camera = camera2.GetComponent<PlayerCamera>();
    ////    if (player2Camera == null)
    ////    {
    ////        player2Camera = camera2.gameObject.AddComponent<PlayerCamera>();
    ////    }
    ////}

    ////void CreateCameras()
    ////{
    ////    // 创建玩家1摄像机
    ////    if (camera1 == null)
    ////    {
    ////        GameObject cam1Obj = new GameObject("Player1Camera");
    ////        camera1 = cam1Obj.AddComponent<Camera>();
    ////        camera1.tag = "Untagged"; // 避免成为主摄像机
    ////    }

    ////    // 创建玩家2摄像机
    ////    if (camera2 == null)
    ////    {
    ////        GameObject cam2Obj = new GameObject("Player2Camera");
    ////        camera2 = cam2Obj.AddComponent<Camera>();
    ////        camera2.tag = "Untagged"; // 避免成为主摄像机
    ////    }
    ////}

    ////void InitializeCameras()
    ////{
    ////    // 设置玩家1摄像机参数
    ////    if (player1Camera != null)
    ////    {
    ////        player1Camera.height = cameraHeight;
    ////        player1Camera.distance = cameraDistance;
    ////        player1Camera.rotationSpeed = rotationSpeed;
    ////        player1Camera.rotateLeftKey = KeyCode.Q;
    ////        player1Camera.rotateRightKey = KeyCode.E;
    ////        player1Camera.currentAngle = player1StartAngle;

    ////        // 设置目标并立即对准
    ////        if (player1 != null)
    ////        {
    ////            player1Camera.SetTarget(player1);
    ////            player1Camera.SnapToTarget(); // 立即对准
    ////        }
    ////    }

    ////    // 设置玩家2摄像机参数
    ////    if (player2Camera != null)
    ////    {
    ////        player2Camera.height = cameraHeight;
    ////        player2Camera.distance = cameraDistance;
    ////        player2Camera.rotationSpeed = rotationSpeed;
    ////        player2Camera.rotateLeftKey = KeyCode.U;
    ////        player2Camera.rotateRightKey = KeyCode.O;
    ////        player2Camera.currentAngle = player2StartAngle;

    ////        // 设置目标并立即对准
    ////        if (player2 != null)
    ////        {
    ////            player2Camera.SetTarget(player2);
    ////            player2Camera.SnapToTarget(); // 立即对准
    ////        }
    ////    }

    ////    Debug.Log($"分屏摄像机初始化完成 - 玩家1角度: {player1StartAngle}°, 玩家2角度: {player2StartAngle}°");
    ////}

    ////void SetupSplitScreen()
    ////{
    ////    if (horizontalSplit)
    ////    {
    ////        // 水平分屏：左屏玩家1，右屏玩家2
    ////        camera1.rect = new Rect(0f, 0f, 0.5f, 1f);
    ////        camera2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
    ////    }
    ////    else
    ////    {
    ////        // 垂直分屏：上屏玩家1，下屏玩家2
    ////        camera1.rect = new Rect(0f, 0.5f, 1f, 0.5f);
    ////        camera2.rect = new Rect(0f, 0f, 1f, 0.5f);
    ////    }

    ////    // 确保两个摄像机都启用
    ////    camera1.enabled = true;
    ////    camera2.enabled = true;
    ////}

    ////void Update()
    ////{
    ////    // 调试功能：按R键重置摄像机位置
    ////    if (Input.GetKeyDown(KeyCode.R))
    ////    {
    ////        ResetCameraPositions();
    ////    }
    ////}

    ////// 公共方法：重置摄像机位置
    ////public void ResetCameraPositions()
    ////{
    ////    if (player1Camera != null && player1 != null)
    ////    {
    ////        player1Camera.SetCameraAngle(player1StartAngle);
    ////        player1Camera.SnapToTarget();
    ////    }

    ////    if (player2Camera != null && player2 != null)
    ////    {
    ////        player2Camera.SetCameraAngle(player2StartAngle);
    ////        player2Camera.SnapToTarget();
    ////    }

    ////    Debug.Log("摄像机位置已重置");
    ////}

    ////// 公共方法：手动设置摄像机角度
    ////public void SetCameraAngles(float angle1, float angle2)
    ////{
    ////    if (player1Camera != null)
    ////    {
    ////        player1Camera.SetCameraAngle(angle1);
    ////    }

    ////    if (player2Camera != null)
    ////    {
    ////        player2Camera.SetCameraAngle(angle2);
    ////    }
    ////}

    ////// 公共方法：检查摄像机状态
    ////public void CheckCameraStatus()
    ////{
    ////    Debug.Log("=== 摄像机状态检查 ===");
    ////    Debug.Log($"玩家1摄像机: {(player1Camera != null ? "正常" : "缺失")}, 目标: {(player1Camera?.target != null ? player1Camera.target.name : "无")}");
    ////    Debug.Log($"玩家2摄像机: {(player2Camera != null ? "正常" : "缺失")}, 目标: {(player2Camera?.target != null ? player2Camera.target.name : "无")}");
    ////    Debug.Log($"玩家1角度: {player1Camera?.GetCurrentAngle()}, 玩家2角度: {player2Camera?.GetCurrentAngle()}");
    ////    Debug.Log("=====================");
    ////}
    //#endregion
    //[Header("玩家设置")]
    //public Transform player1;
    //public Transform player2;

    //[Header("摄像机设置")]
    //public Camera camera1;
    //public Camera camera2;

    //[Header("分屏模式")]
    //public bool horizontalSplit = true; // 水平分屏

    //[Header("摄像机参数")]
    //public float cameraHeight = 10f;
    //public float cameraDistance = 8f;
    //public float rotationSpeed = 90f;

    //[Header("初始角度设置")]
    //public float player1StartAngle = 45f;
    //public float player2StartAngle = 45f;

    //private PlayerCamera player1Camera;
    //private PlayerCamera player2Camera;

    //void Start()
    //{
    //    SetupCameras();
    //    SetupSplitScreen();
    //    InitializeCameras();
    //}

    //void SetupCameras()
    //{
    //    // 确保摄像机存在
    //    if (camera1 == null || camera2 == null)
    //    {
    //        CreateCameras();
    //    }

    //    // 获取或添加摄像机控制器
    //    player1Camera = camera1.GetComponent<PlayerCamera>();
    //    if (player1Camera == null)
    //    {
    //        player1Camera = camera1.gameObject.AddComponent<PlayerCamera>();
    //    }

    //    player2Camera = camera2.GetComponent<PlayerCamera>();
    //    if (player2Camera == null)
    //    {
    //        player2Camera = camera2.gameObject.AddComponent<PlayerCamera>();
    //    }
    //}

    //void CreateCameras()
    //{
    //    // 创建玩家1摄像机
    //    if (camera1 == null)
    //    {
    //        GameObject cam1Obj = new GameObject("Player1Camera");
    //        camera1 = cam1Obj.AddComponent<Camera>();
    //        camera1.tag = "Untagged";
    //    }

    //    // 创建玩家2摄像机
    //    if (camera2 == null)
    //    {
    //        GameObject cam2Obj = new GameObject("Player2Camera");
    //        camera2 = cam2Obj.AddComponent<Camera>();
    //        camera2.tag = "Untagged";
    //    }
    //}

    //void InitializeCameras()
    //{
    //    // 设置玩家1摄像机参数 - 新按键配置
    //    if (player1Camera != null)
    //    {
    //        player1Camera.height = cameraHeight;
    //        player1Camera.distance = cameraDistance;
    //        player1Camera.rotationSpeed = rotationSpeed;
    //        //player1Camera.rotateLeftKey = KeyCode.D;  // 改为A键
    //        //player1Camera.rotateRightKey = KeyCode.A; // 改为D键
    //        player1Camera.currentAngle = player1StartAngle;

    //        // 禁用自动转向，因为我们使用AD键专门控制视角
    //        player1Camera.enableAutoRotation = false;

    //        // 设置目标并立即对准
    //        if (player1 != null)
    //        {
    //            player1Camera.SetTarget(player1);
    //            player1Camera.SnapToTarget();
    //        }
    //    }

    //    // 设置玩家2摄像机参数 - 新按键配置
    //    if (player2Camera != null)
    //    {
    //        player2Camera.height = cameraHeight;
    //        player2Camera.distance = cameraDistance;
    //        player2Camera.rotationSpeed = rotationSpeed;
    //        //player2Camera.rotateLeftKey = KeyCode.L;  // 改为J键
    //        //player2Camera.rotateRightKey = KeyCode.J; // 改为L键
    //        player2Camera.currentAngle = player2StartAngle;

    //        // 禁用自动转向
    //        player2Camera.enableAutoRotation = false;

    //        // 设置目标并立即对准
    //        if (player2 != null)
    //        {
    //            player2Camera.SetTarget(player2);
    //            player2Camera.SnapToTarget();
    //        }
    //    }

    //    Debug.Log($"分屏摄像机初始化完成 - 新按键配置");
    //    Debug.Log($"玩家1: AD控制视角转动");
    //    Debug.Log($"玩家2: JL控制视角转动");
    //}

    //void SetupSplitScreen()
    //{
    //    if (horizontalSplit)
    //    {
    //        // 水平分屏：左屏玩家1，右屏玩家2
    //        camera1.rect = new Rect(0f, 0f, 0.5f, 1f);
    //        camera2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
    //    }
    //    else
    //    {
    //        // 垂直分屏：上屏玩家1，下屏玩家2
    //        camera1.rect = new Rect(0f, 0.5f, 1f, 0.5f);
    //        camera2.rect = new Rect(0f, 0f, 1f, 0.5f);
    //    }

    //    // 确保两个摄像机都启用
    //    camera1.enabled = true;
    //    camera2.enabled = true;
    //}

    //void Update()
    //{
    //    // 调试功能：按R键重置摄像机位置
    //    if (Input.GetKeyDown(KeyCode.R))
    //    {
    //        ResetCameraPositions();
    //    }
    //}

    //// 公共方法：重置摄像机位置
    //public void ResetCameraPositions()
    //{
    //    if (player1Camera != null && player1 != null)
    //    {
    //        player1Camera.SetCameraAngle(player1StartAngle);
    //        player1Camera.SnapToTarget();
    //    }

    //    if (player2Camera != null && player2 != null)
    //    {
    //        player2Camera.SetCameraAngle(player2StartAngle);
    //        player2Camera.SnapToTarget();
    //    }

    //    Debug.Log("摄像机位置已重置");
    //}

    //// 公共方法：手动设置摄像机角度
    //public void SetCameraAngles(float angle1, float angle2)
    //{
    //    if (player1Camera != null)
    //    {
    //        player1Camera.SetCameraAngle(angle1);
    //    }

    //    if (player2Camera != null)
    //    {
    //        player2Camera.SetCameraAngle(angle2);
    //    }
    //}

    //// 公共方法：检查摄像机状态
    //public void CheckCameraStatus()
    //{
    //    Debug.Log("=== 摄像机状态检查 ===");
    //    Debug.Log($"玩家1摄像机: {(player1Camera != null ? "正常" : "缺失")}, 目标: {(player1Camera?.target != null ? player1Camera.target.name : "无")}");
    //    Debug.Log($"玩家2摄像机: {(player2Camera != null ? "正常" : "缺失")}, 目标: {(player2Camera?.target != null ? player2Camera.target.name : "无")}");
    //    Debug.Log($"玩家1角度: {player1Camera?.GetCurrentAngle()}, 玩家2角度: {player2Camera?.GetCurrentAngle()}");
    //    Debug.Log("=====================");
    //}
    [Header("玩家设置")]
    public Transform player1;
    public Transform player2;

    [Header("摄像机设置 - 将在每个场景中自动查找")]
    public Camera camera1;
    public Camera camera2;

    [Header("分屏模式")]
    public bool horizontalSplit = true;

    [Header("摄像机参数")]
    public float cameraHeight = 10f;
    public float cameraDistance = 8f;
    public float rotationSpeed = 90f;

    [Header("初始角度设置")]
    public float player1StartAngle = 45f;
    public float player2StartAngle = 45f;

    [Header("阴影修复设置")]
    public float shadowDistance = 100f;
    public float shadowBias = 0.05f;
    public float shadowNormalBias = 1.0f;
    public bool enableShadowFix = true;

    [Header("场景管理")]
    public bool destroyOtherCameras = true; // 销毁场景中的其他摄像机

    private PlayerCamera player1Camera;
    private PlayerCamera player2Camera;
    private bool isInitialized = false;

    // 单例模式
    public static SplitScreenManager Instance { get; private set; }

    void Awake()
    {
        // 单例模式确保只有一个分屏管理器
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeSystem();
    }

    void InitializeSystem()
    {
        Debug.Log("开始初始化分屏系统...");

        // 重置摄像机引用，强制使用当前场景的摄像机
        camera1 = null;
        camera2 = null;
        player1Camera = null;
        player2Camera = null;

        SetupCameras();
        SetupSplitScreen();
        InitializeCameras();
        ApplyShadowSettings();

        isInitialized = true;
        Debug.Log("分屏管理器初始化完成");
    }

    void SetupCameras()
    {
        // 强制查找当前场景中的摄像机
        FindCamerasInCurrentScene();

        // 如果找不到摄像机，给出错误提示
        if (camera1 == null)
        {
            Debug.LogError("在当前场景中找不到玩家1摄像机！请确保场景中有名为'Player1Camera'的摄像机");
        }

        if (camera2 == null)
        {
            Debug.LogError("在当前场景中找不到玩家2摄像机！请确保场景中有名为'Player2Camera'的摄像机");
        }

        // 设置摄像机组件
        SetupCameraComponents();
    }

    void FindCamerasInCurrentScene()
    {
        // 通过名称查找当前场景中的摄像机
        GameObject cam1Obj = GameObject.Find("Player1Camera");
        GameObject cam2Obj = GameObject.Find("Player2Camera");

        if (cam1Obj != null)
        {
            camera1 = cam1Obj.GetComponent<Camera>();
            if (camera1 == null)
            {
                Debug.LogWarning("Player1Camera对象没有Camera组件，正在添加...");
                camera1 = cam1Obj.AddComponent<Camera>();
            }
            Debug.Log($"找到玩家1摄像机: {cam1Obj.name}");
        }

        if (cam2Obj != null)
        {
            camera2 = cam2Obj.GetComponent<Camera>();
            if (camera2 == null)
            {
                Debug.LogWarning("Player2Camera对象没有Camera组件，正在添加...");
                camera2 = cam2Obj.AddComponent<Camera>();
            }
            Debug.Log($"找到玩家2摄像机: {cam2Obj.name}");
        }

        // 如果通过名称找不到，尝试通过标签查找
        if (camera1 == null)
        {
            GameObject taggedCam1 = GameObject.FindGameObjectWithTag("Player1Camera");
            if (taggedCam1 != null)
            {
                camera1 = taggedCam1.GetComponent<Camera>();
                Debug.Log($"通过标签找到玩家1摄像机: {taggedCam1.name}");
            }
        }

        if (camera2 == null)
        {
            GameObject taggedCam2 = GameObject.FindGameObjectWithTag("Player2Camera");
            if (taggedCam2 != null)
            {
                camera2 = taggedCam2.GetComponent<Camera>();
                Debug.Log($"通过标签找到玩家2摄像机: {taggedCam2.name}");
            }
        }
    }

    void SetupCameraComponents()
    {
        if (camera1 != null)
        {
            player1Camera = camera1.GetComponent<PlayerCamera>();
            if (player1Camera == null)
            {
                player1Camera = camera1.gameObject.AddComponent<PlayerCamera>();
                Debug.Log("为玩家1摄像机添加PlayerCamera组件");
            }
        }

        if (camera2 != null)
        {
            player2Camera = camera2.GetComponent<PlayerCamera>();
            if (player2Camera == null)
            {
                player2Camera = camera2.gameObject.AddComponent<PlayerCamera>();
                Debug.Log("为玩家2摄像机添加PlayerCamera组件");
            }
        }
    }

    void InitializeCameras()
    {
        // 设置玩家1摄像机参数
        if (player1Camera != null)
        {
            player1Camera.height = cameraHeight;
            player1Camera.distance = cameraDistance;
            player1Camera.rotationSpeed = rotationSpeed;
            player1Camera.currentAngle = player1StartAngle;
            player1Camera.enableAutoRotation = false;

            // 设置目标并立即对准
            if (player1 != null)
            {
                player1Camera.SetTarget(player1);
                player1Camera.SnapToTarget();
            }
            else
            {
                Debug.LogWarning("玩家1引用为空");
            }
        }

        // 设置玩家2摄像机参数
        if (player2Camera != null)
        {
            player2Camera.height = cameraHeight;
            player2Camera.distance = cameraDistance;
            player2Camera.rotationSpeed = rotationSpeed;
            player2Camera.currentAngle = player2StartAngle;
            player2Camera.enableAutoRotation = false;

            // 设置目标并立即对准
            if (player2 != null)
            {
                player2Camera.SetTarget(player2);
                player2Camera.SnapToTarget();
            }
            else
            {
                Debug.LogWarning("玩家2引用为空");
            }
        }

        SetupCameraRendering();
    }

    void SetupCameraRendering()
    {
        if (camera1 != null)
        {
            camera1.allowHDR = false;
            camera1.allowMSAA = false;
            camera1.depthTextureMode = DepthTextureMode.Depth;
            camera1.depth = 0; // 设置深度确保渲染顺序
        }

        if (camera2 != null)
        {
            camera2.allowHDR = false;
            camera2.allowMSAA = false;
            camera2.depthTextureMode = DepthTextureMode.Depth;
            camera2.depth = 1; // 设置深度确保渲染顺序
        }
    }

    void SetupSplitScreen()
    {
        // 检查摄像机引用是否有效
        if (camera1 == null || camera2 == null)
        {
            Debug.LogWarning("摄像机引用丢失，重新查找...");
            SetupCameras();

            if (camera1 == null || camera2 == null)
            {
                Debug.LogError("无法找到有效的摄像机引用！");
                return;
            }
        }

        // 禁用所有其他摄像机
        DisableOtherCameras();

        if (horizontalSplit)
        {
            camera1.rect = new Rect(0f, 0f, 0.5f, 1f);
            camera2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        }
        else
        {
            camera1.rect = new Rect(0f, 0.5f, 1f, 0.5f);
            camera2.rect = new Rect(0f, 0f, 1f, 0.5f);
        }

        camera1.enabled = true;
        camera2.enabled = true;

        Debug.Log($"分屏设置完成: 水平分屏={horizontalSplit}");
    }

    void DisableOtherCameras()
    {
        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam != camera1 && cam != camera2 && cam.enabled)
            {
                cam.enabled = false;
                Debug.Log($"禁用其他摄像机: {cam.name}");
            }
        }
    }

    void ApplyShadowSettings()
    {
        if (!enableShadowFix) return;

        StartCoroutine(DelayedShadowFix());
    }

    IEnumerator DelayedShadowFix()
    {
        yield return null;

        QualitySettings.shadowDistance = shadowDistance;
        FixDirectionalLightShadows();

        if (camera1 != null) camera1.farClipPlane = Mathf.Max(camera1.farClipPlane, shadowDistance * 0.8f);
        if (camera2 != null) camera2.farClipPlane = Mathf.Max(camera2.farClipPlane, shadowDistance * 0.8f);

        DynamicGI.UpdateEnvironment();

        Debug.Log($"阴影设置已应用 - 阴影距离: {shadowDistance}, 偏置: {shadowBias}");
    }

    void FixDirectionalLightShadows()
    {
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional && light.shadows != LightShadows.None)
            {
                light.shadowBias = shadowBias;
                light.shadowNormalBias = shadowNormalBias;
                light.shadowStrength = 1.0f;

                Debug.Log($"修复方向光阴影: {light.name}, 偏置: {light.shadowBias}");
            }
        }
    }

    void Update()
    {
        // 调试功能
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCameraPositions();
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            ApplyShadowSettings();
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            ReinitializeSystem();
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isInitialized)
        {
            Debug.Log($"场景加载完成: {scene.name}");
            Invoke("DelayedSceneFix", 0.2f);
        }
    }

    void DelayedSceneFix()
    {
        Debug.Log("执行场景修复...");

        // 重置所有引用，强制使用新场景的资源
        camera1 = null;
        camera2 = null;
        player1Camera = null;
        player2Camera = null;
        player1 = null;
        player2 = null;

        // 重新初始化系统
        InitializeSystem();

        Debug.Log("场景修复完成");
    }

    void FindPlayers()
    {
        bool foundPlayer1 = false;
        bool foundPlayer2 = false;

        // 尝试通过名称查找玩家
        if (player1 == null)
        {
            GameObject p1 = GameObject.Find("Player1");
            if (p1 != null)
            {
                player1 = p1.transform;
                foundPlayer1 = true;
                Debug.Log($"找到玩家1: {p1.name}");
            }
        }

        if (player2 == null)
        {
            GameObject p2 = GameObject.Find("Player2");
            if (p2 != null)
            {
                player2 = p2.transform;
                foundPlayer2 = true;
                Debug.Log($"找到玩家2: {p2.name}");
            }
        }

        // 重新设置摄像机目标
        if (player1Camera != null && player1 != null)
            player1Camera.SetTarget(player1);
        if (player2Camera != null && player2 != null)
            player2Camera.SetTarget(player2);

        Debug.Log($"玩家查找结果: 玩家1={foundPlayer1}, 玩家2={foundPlayer2}");
    }

    // 公共方法：重置摄像机位置
    public void ResetCameraPositions()
    {
        if (player1Camera != null && player1 != null)
        {
            player1Camera.SetCameraAngle(player1StartAngle);
            player1Camera.SnapToTarget();
        }

        if (player2Camera != null && player2 != null)
        {
            player2Camera.SetCameraAngle(player2StartAngle);
            player2Camera.SnapToTarget();
        }

        Debug.Log("摄像机位置已重置");
    }

    // 公共方法：重新初始化整个系统
    [ContextMenu("重新初始化系统")]
    public void ReinitializeSystem()
    {
        InitializeSystem();
    }

    // 公共方法：检查摄像机状态
    [ContextMenu("检查摄像机状态")]
    public void CheckCameraStatus()
    {
        Debug.Log("=== 摄像机状态检查 ===");
        Debug.Log($"玩家1摄像机: {(camera1 != null ? camera1.name : "缺失")}");
        Debug.Log($"玩家2摄像机: {(camera2 != null ? camera2.name : "缺失")}");

        Camera[] allCameras = FindObjectsOfType<Camera>();
        Debug.Log($"场景中总摄像机数量: {allCameras.Length}");
        foreach (Camera cam in allCameras)
        {
            Debug.Log($"摄像机: {cam.name}, 启用: {cam.enabled}, Rect: {cam.rect}");
        }
        Debug.Log("=====================");
    }

    // 公共方法：强制修复阴影
    [ContextMenu("强制修复阴影")]
    public void ForceFixShadows()
    {
        ApplyShadowSettings();
    }

    void OnDestroy()
    {
        if (player1Camera != null)
            player1Camera.OnCameraRotated = null;
        if (player2Camera != null)
            player2Camera.OnCameraRotated = null;
    }
}
