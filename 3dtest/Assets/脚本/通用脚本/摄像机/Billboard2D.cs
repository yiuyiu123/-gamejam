using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard2D : MonoBehaviour
{
    [Header("���������")]
    public Camera targetCamera;
    public bool useMainCameraIfNull = true;

    [Header("��������")]
    public BillboardMode billboardMode = BillboardMode.FaceCamera;
    public bool lockUpAxis = true; // �����Ϸ��򣬷�ֹ��б
    public float yOffset = 0f; // Y����תƫ��

    [Header("ƽ������")]
    public bool useSmoothRotation = true;
    public float rotationSmoothness = 5f;

    [Header("����")]
    public bool showDebugInfo = false;

    // Billboardģʽö��
    public enum BillboardMode
    {
        FaceCamera,     // ��ȫ���������
        YAxisOnly,      // ֻ��Y����ת
        CameraForward   // �������ǰ����һ��
    }

    private Transform targetTransform;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        InitializeComponents();
        FindTargetCamera();

        if (targetTransform == null)
        {
            Debug.LogError("Billboard2D: û���ҵ���Ч��Transform�����", this);
            enabled = false;
            return;
        }

        if (targetCamera == null)
        {
            Debug.LogError("Billboard2D: û���ҵ�Ŀ���������", this);
            enabled = false;
            return;
        }

        if (showDebugInfo)
        {
            Debug.Log($"Billboard2D��ʼ��: {gameObject.name} -> {targetCamera.name}, ģʽ: {billboardMode}");
        }
    }

    void InitializeComponents()
    {
        targetTransform = transform;

        // ����Ƿ���SpriteRenderer��2D���飩
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && showDebugInfo)
        {
            Debug.Log($"�ҵ�SpriteRenderer: {spriteRenderer.sprite?.name}");
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
            // ���ݶ������Ʋ²������
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
            // ����ʹ������������һ���ҵ��������
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
        // ����Ӷ���ָ��������ķ���
        Vector3 toCamera = targetCamera.transform.position - targetTransform.position;

        if (lockUpAxis)
        {
            toCamera.y = 0; // ����Y�ᣬ����ˮƽ
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
        // ֻ��Y����ת������ֱ��
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
        // �������ǰ����һ��
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

    // ��������������Ŀ�������
    public void SetTargetCamera(Camera newCamera)
    {
        targetCamera = newCamera;
        if (showDebugInfo && targetCamera != null)
        {
            Debug.Log($"����Ŀ�������: {gameObject.name} -> {targetCamera.name}");
        }
    }

    // �����������������³���
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

    // ��������������Billboardģʽ
    public void SetBillboardMode(BillboardMode newMode)
    {
        billboardMode = newMode;
        if (showDebugInfo)
        {
            Debug.Log($"����Billboardģʽ: {gameObject.name} -> {newMode}");
        }
    }

    // ��Scene��ͼ����ʾ������Ϣ
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying && showDebugInfo)
        {
            // ��ʾ����ָʾ��
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.forward * 1f);

            // ��ʾ���������
            if (targetCamera != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, targetCamera.transform.position);
            }
        }
    }
}
