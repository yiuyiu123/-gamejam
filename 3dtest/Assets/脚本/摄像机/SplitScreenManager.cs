using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitScreenManager : MonoBehaviour
{
    //[Header("玩家设置")]
    //public Transform player1;
    //public Transform player2;

    //[Header("摄像机设置")]
    //public Camera camera1;
    //public Camera camera2;

    //[Header("分屏模式")]
    //public bool horizontalSplit = true; // 水平分屏

    //[Header("摄像机参数")]
    //public float cameraHeight1 = 10f;
    //public float cameraDistance1 = 8f;
    //public float cameraHeight2 = 10f;
    //public float cameraDistance2 = 8f;
    //public float currentAngle1 = 8f;
    //public float currentAngle2 = 8f;
    //public float rotationSpeed = 2f;



    //private PlayerCamera player1Camera;
    //private PlayerCamera player2Camera;

    //void Start()
    //{
    //    SetupCameras();
    //    SetupSplitScreen();
    //}

    //void SetupCameras()
    //{
    //    // 创建或获取摄像机组件
    //    if (camera1 == null || camera2 == null)
    //    {
    //        CreateCameras();
    //    }

    //    //// 添加摄像机控制器
    //    //player1Camera = camera1.gameObject.AddComponent<PlayerCamera>();
    //    //player2Camera = camera2.gameObject.AddComponent<PlayerCamera>();

    //    // 设置摄像机参数
    //    SetCameraParameters();

    //    // 分配目标
    //    player1Camera.SetTarget(player1);
    //    player2Camera.SetTarget(player2);
    //}

    ////创建相机
    //void CreateCameras()
    //{
    //    // 创建玩家1摄像机
    //    GameObject cam1Obj = new GameObject("Player1Camera");
    //    camera1 = cam1Obj.AddComponent<Camera>();

    //    // 创建玩家2摄像机
    //    GameObject cam2Obj = new GameObject("Player2Camera");
    //    camera2 = cam2Obj.AddComponent<Camera>();
    //}

    //void SetCameraParameters()
    //{
    //    // 玩家1摄像机设置
    //    player1Camera.height = cameraHeight1;
    //    player1Camera.distance = cameraDistance1;
    //    player1Camera.rotationSpeed = rotationSpeed;
    //    player1Camera.currentAngle = currentAngle1;
    //    player1Camera.rotateLeftKey = KeyCode.E;  // e键旋转
    //    player1Camera.rotateRightKey = KeyCode.Q; // q键旋转

    //    // 玩家2摄像机设置
    //    player2Camera.height = cameraHeight2;
    //    player2Camera.distance = cameraDistance2;
    //    player2Camera.rotationSpeed = rotationSpeed;
    //    player2Camera.currentAngle = currentAngle2;
    //    player2Camera.rotateLeftKey = KeyCode.O;  // o键旋转
    //    player2Camera.rotateRightKey = KeyCode.U; // u键旋转
    //}

    ////分屏
    //void SetupSplitScreen()
    //{
    //    if (horizontalSplit)
    //    {
    //        //水平分屏：左屏玩家1，右屏玩家2
    //        camera1.rect = new Rect(0f, 0f, 0.5f, 1f);
    //        camera2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
    //    }
    //    else
    //    {
    //        //垂直分屏：上屏玩家1，下屏玩家2
    //        camera1.rect = new Rect(0f, 0.5f, 1f, 0.5f);
    //        camera2.rect = new Rect(0f, 0f, 1f, 0.5f);
    //    }
    //}
    [Header("玩家设置")]
    public Transform player1;
    public Transform player2;

    [Header("摄像机设置")]
    public Camera camera1;
    public Camera camera2;

    [Header("分屏模式")]
    public bool horizontalSplit = true; // 水平分屏

    [Header("摄像机参数")]
    public float cameraHeight = 10f;
    public float cameraDistance = 8f;
    public float rotationSpeed = 90f; // 增加到90

    [Header("初始角度设置")]
    public float player1StartAngle = 45f;
    public float player2StartAngle = 45f;

    private PlayerCamera player1Camera;
    private PlayerCamera player2Camera;

    void Start()
    {
        SetupCameras();
        SetupSplitScreen();
        InitializeCameras();
    }

    void SetupCameras()
    {
        // 确保摄像机存在
        if (camera1 == null || camera2 == null)
        {
            CreateCameras();
        }

        // 获取或添加摄像机控制器
        player1Camera = camera1.GetComponent<PlayerCamera>();
        if (player1Camera == null)
        {
            player1Camera = camera1.gameObject.AddComponent<PlayerCamera>();
        }

        player2Camera = camera2.GetComponent<PlayerCamera>();
        if (player2Camera == null)
        {
            player2Camera = camera2.gameObject.AddComponent<PlayerCamera>();
        }
    }

    void CreateCameras()
    {
        // 创建玩家1摄像机
        if (camera1 == null)
        {
            GameObject cam1Obj = new GameObject("Player1Camera");
            camera1 = cam1Obj.AddComponent<Camera>();
            camera1.tag = "Untagged"; // 避免成为主摄像机
        }

        // 创建玩家2摄像机
        if (camera2 == null)
        {
            GameObject cam2Obj = new GameObject("Player2Camera");
            camera2 = cam2Obj.AddComponent<Camera>();
            camera2.tag = "Untagged"; // 避免成为主摄像机
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
            player1Camera.rotateLeftKey = KeyCode.Q;
            player1Camera.rotateRightKey = KeyCode.E;
            player1Camera.currentAngle = player1StartAngle;

            // 设置目标并立即对准
            if (player1 != null)
            {
                player1Camera.SetTarget(player1);
                player1Camera.SnapToTarget(); // 立即对准
            }
        }

        // 设置玩家2摄像机参数
        if (player2Camera != null)
        {
            player2Camera.height = cameraHeight;
            player2Camera.distance = cameraDistance;
            player2Camera.rotationSpeed = rotationSpeed;
            player2Camera.rotateLeftKey = KeyCode.U;
            player2Camera.rotateRightKey = KeyCode.O;
            player2Camera.currentAngle = player2StartAngle;

            // 设置目标并立即对准
            if (player2 != null)
            {
                player2Camera.SetTarget(player2);
                player2Camera.SnapToTarget(); // 立即对准
            }
        }

        Debug.Log($"分屏摄像机初始化完成 - 玩家1角度: {player1StartAngle}°, 玩家2角度: {player2StartAngle}°");
    }

    void SetupSplitScreen()
    {
        if (horizontalSplit)
        {
            // 水平分屏：左屏玩家1，右屏玩家2
            camera1.rect = new Rect(0f, 0f, 0.5f, 1f);
            camera2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        }
        else
        {
            // 垂直分屏：上屏玩家1，下屏玩家2
            camera1.rect = new Rect(0f, 0.5f, 1f, 0.5f);
            camera2.rect = new Rect(0f, 0f, 1f, 0.5f);
        }

        // 确保两个摄像机都启用
        camera1.enabled = true;
        camera2.enabled = true;
    }

    void Update()
    {
        // 调试功能：按R键重置摄像机位置
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCameraPositions();
        }
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

    // 公共方法：手动设置摄像机角度
    public void SetCameraAngles(float angle1, float angle2)
    {
        if (player1Camera != null)
        {
            player1Camera.SetCameraAngle(angle1);
        }

        if (player2Camera != null)
        {
            player2Camera.SetCameraAngle(angle2);
        }
    }

    // 公共方法：检查摄像机状态
    public void CheckCameraStatus()
    {
        Debug.Log("=== 摄像机状态检查 ===");
        Debug.Log($"玩家1摄像机: {(player1Camera != null ? "正常" : "缺失")}, 目标: {(player1Camera?.target != null ? player1Camera.target.name : "无")}");
        Debug.Log($"玩家2摄像机: {(player2Camera != null ? "正常" : "缺失")}, 目标: {(player2Camera?.target != null ? player2Camera.target.name : "无")}");
        Debug.Log($"玩家1角度: {player1Camera?.GetCurrentAngle()}, 玩家2角度: {player2Camera?.GetCurrentAngle()}");
        Debug.Log("=====================");
    }
}
