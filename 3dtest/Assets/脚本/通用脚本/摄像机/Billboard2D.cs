using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard2D : MonoBehaviour
{
    [Header("摄像机设置")]
    public Camera targetCamera;
    public bool useMainCameraIfNull = true;

    [Header("朝向设置")]
    public BillboardMode billboardMode = BillboardMode.FaceCamera;
    public bool lockUpAxis = true; // 锁定上方向，防止倾斜
    public float yOffset = 0f; // Y轴旋转偏移

    [Header("平滑设置")]
    public bool useSmoothRotation = true;
    public float rotationSmoothness = 5f;

    [Header("调试")]
    public bool showDebugInfo = false;

    // Billboard模式枚举
    public enum BillboardMode
    {
        FaceCamera,     // 完全面向摄像机
        YAxisOnly,      // 只绕Y轴旋转
        CameraForward   // 与摄像机前方向一致
    }

    private Transform targetTransform;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        InitializeComponents();
        FindTargetCamera();

        if (targetTransform == null)
        {
            Debug.LogError("Billboard2D: 没有找到有效的Transform组件！", this);
            enabled = false;
            return;
        }

        if (targetCamera == null)
        {
            Debug.LogError("Billboard2D: 没有找到目标摄像机！", this);
            enabled = false;
            return;
        }

        if (showDebugInfo)
        {
            Debug.Log($"Billboard2D初始化: {gameObject.name} -> {targetCamera.name}, 模式: {billboardMode}");
        }
    }

    void InitializeComponents()
    {
        targetTransform = transform;

        // 检查是否有SpriteRenderer（2D精灵）
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && showDebugInfo)
        {
            Debug.Log($"找到SpriteRenderer: {spriteRenderer.sprite?.name}");
        }
    }

    void FindTargetCamera()
    {
        if (targetCamera != null) return;

        if (useMainCameraIfNull)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            // 根据对象名称猜测摄像机
            targetCamera = GuessTargetCamera();
        }

        if (targetCamera == null && Camera.allCamerasCount > 0)
        {
            targetCamera = Camera.allCameras[0];
        }
    }

    Camera GuessTargetCamera()
    {
        string objName = gameObject.name.ToLower();

        if (objName.Contains("player1") || objName.Contains("p1"))
        {
            return GameObject.Find("Player1Camera")?.GetComponent<Camera>() ??
                   GameObject.Find("Camera1")?.GetComponent<Camera>();
        }
        else if (objName.Contains("player2") || objName.Contains("p2"))
        {
            return GameObject.Find("Player2Camera")?.GetComponent<Camera>() ??
                   GameObject.Find("Camera2")?.GetComponent<Camera>();
        }
        else if (objName.Contains("item") || objName.Contains("prop"))
        {
            // 道具使用主摄像机或第一个找到的摄像机
            return Camera.main;
        }

        return null;
    }

    void LateUpdate()
    {
        if (targetCamera == null || targetTransform == null) return;

        UpdateBillboard();
    }

    void UpdateBillboard()
    {
        switch (billboardMode)
        {
            case BillboardMode.FaceCamera:
                UpdateFaceCamera();
                break;
            case BillboardMode.YAxisOnly:
                UpdateYAxisOnly();
                break;
            case BillboardMode.CameraForward:
                UpdateCameraForward();
                break;
        }
    }

    void UpdateFaceCamera()
    {
        // 计算从对象指向摄像机的方向
        Vector3 toCamera = targetCamera.transform.position - targetTransform.position;

        if (lockUpAxis)
        {
            toCamera.y = 0; // 锁定Y轴，保持水平
        }

        if (toCamera != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-toCamera) * Quaternion.Euler(0, yOffset, 0);

            if (useSmoothRotation)
            {
                targetTransform.rotation = Quaternion.Slerp(
                    targetTransform.rotation,
                    targetRotation,
                    rotationSmoothness * Time.deltaTime
                );
            }
            else
            {
                targetTransform.rotation = targetRotation;
            }
        }
    }

    void UpdateYAxisOnly()
    {
        // 只绕Y轴旋转，保持直立
        Vector3 toCamera = targetCamera.transform.position - targetTransform.position;
        toCamera.y = 0;

        if (toCamera != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-toCamera) * Quaternion.Euler(0, yOffset, 0);

            if (useSmoothRotation)
            {
                targetTransform.rotation = Quaternion.Slerp(
                    targetTransform.rotation,
                    targetRotation,
                    rotationSmoothness * Time.deltaTime
                );
            }
            else
            {
                targetTransform.rotation = targetRotation;
            }
        }
    }

    void UpdateCameraForward()
    {
        // 与摄像机前方向一致
        Quaternion targetRotation = targetCamera.transform.rotation * Quaternion.Euler(0, yOffset, 0);

        if (useSmoothRotation)
        {
            targetTransform.rotation = Quaternion.Slerp(
                targetTransform.rotation,
                targetRotation,
                rotationSmoothness * Time.deltaTime
            );
        }
        else
        {
            targetTransform.rotation = targetRotation;
        }
    }

    // 公共方法：设置目标摄像机
    public void SetTargetCamera(Camera newCamera)
    {
        targetCamera = newCamera;
        if (showDebugInfo && targetCamera != null)
        {
            Debug.Log($"设置目标摄像机: {gameObject.name} -> {targetCamera.name}");
        }
    }

    // 公共方法：立即更新朝向
    public void SnapToCamera()
    {
        if (targetCamera == null) return;

        switch (billboardMode)
        {
            case BillboardMode.FaceCamera:
                Vector3 toCamera = targetCamera.transform.position - targetTransform.position;
                if (lockUpAxis) toCamera.y = 0;
                if (toCamera != Vector3.zero)
                {
                    targetTransform.rotation = Quaternion.LookRotation(-toCamera) * Quaternion.Euler(0, yOffset, 0);
                }
                break;

            case BillboardMode.YAxisOnly:
                Vector3 toCameraY = targetCamera.transform.position - targetTransform.position;
                toCameraY.y = 0;
                if (toCameraY != Vector3.zero)
                {
                    targetTransform.rotation = Quaternion.LookRotation(-toCameraY) * Quaternion.Euler(0, yOffset, 0);
                }
                break;

            case BillboardMode.CameraForward:
                targetTransform.rotation = targetCamera.transform.rotation * Quaternion.Euler(0, yOffset, 0);
                break;
        }
    }

    // 公共方法：更改Billboard模式
    public void SetBillboardMode(BillboardMode newMode)
    {
        billboardMode = newMode;
        if (showDebugInfo)
        {
            Debug.Log($"更改Billboard模式: {gameObject.name} -> {newMode}");
        }
    }

    // 在Scene视图中显示调试信息
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying && showDebugInfo)
        {
            // 显示朝向指示器
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.forward * 1f);

            // 显示摄像机方向
            if (targetCamera != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, targetCamera.transform.position);
            }
        }
    }
}
