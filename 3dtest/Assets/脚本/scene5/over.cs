using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class over : MonoBehaviour
{
    

    [Header("碰撞器区域引用")]
    [Tooltip("第一个碰撞器区域的空物体引用")]
    public GameObject colliderArea1;

    [Tooltip("第二个碰撞器区域的空物体引用")]
    public GameObject colliderArea2;

    [Header("碰撞器大小设置")]
    [Tooltip("第一个碰撞器区域的大小")]
    public Vector3 collider1Size = new Vector3(2f, 2f, 2f);

    [Tooltip("第二个碰撞器区域的大小")]
    public Vector3 collider2Size = new Vector3(2f, 2f, 2f);

    [Header("Tag检测设置")]
    [Tooltip("需要在第一个区域检测的Tag")]
    public string targetTag1 = "Item1";

    [Tooltip("需要在第二个区域检测的Tag")]
    public string targetTag2 = "Item2";

    [Header("UI设置")]
    [Tooltip("需要控制的UI Image GameObject")]
    public GameObject targetImageObject; // 改为GameObject引用 

    [Header("调试设置")]
    public bool showDebugGizmos = true;
    public Color gizmoColor1 = Color.green;
    public Color gizmoColor2 = Color.blue;

    private BoxCollider collider1;
    private BoxCollider collider2;
    private bool isObject1InArea = false;
    private bool isObject2InArea = false;

    void Start()
    {
        InitializeColliders();
        InitializeUI();
    }

    void InitializeColliders()
    {
        // 检查空物体引用 
        if (colliderArea1 == null || colliderArea2 == null)
        {
            Debug.LogError("碰撞器区域引用未设置！请在Inspector中指定两个空物体。");
            return;
        }

        // 为第一个空物体添加碰撞器 
        collider1 = colliderArea1.GetComponent<BoxCollider>();
        if (collider1 == null)
        {
            collider1 = colliderArea1.AddComponent<BoxCollider>();
        }
        collider1.center = Vector3.zero;
        collider1.size = collider1Size;
        collider1.isTrigger = true;

        // 为第二个空物体添加碰撞器 
        collider2 = colliderArea2.GetComponent<BoxCollider>();
        if (collider2 == null)
        {
            collider2 = colliderArea2.AddComponent<BoxCollider>();
        }
        collider2.center = Vector3.zero;
        collider2.size = collider2Size;
        collider2.isTrigger = true;

        Debug.Log("碰撞器初始化完成");
    }

    void InitializeUI()
    {
        if (targetImageObject != null)
        {
            // 直接隐藏整个GameObject 
            targetImageObject.SetActive(false);
            Debug.Log("UI Image GameObject已初始化为隐藏状态");
        }
        else
        {
            Debug.LogWarning("未设置目标UI Image GameObject，请在Inspector中指定。");
        }
    }

    void Update()
    {
        // 每帧检查物体是否在区域内（更可靠的方法）
        CheckObjectsInAreas();
    }

    void CheckObjectsInAreas()
    {
        bool previousState1 = isObject1InArea;
        bool previousState2 = isObject2InArea;

        // 重置状态 
        isObject1InArea = false;
        isObject2InArea = false;

        // 查找所有带有目标Tag的物体 
        GameObject[] objectsWithTag1 = GameObject.FindGameObjectsWithTag(targetTag1);
        GameObject[] objectsWithTag2 = GameObject.FindGameObjectsWithTag(targetTag2);

        // 检查第一个区域的物体 
        foreach (GameObject obj in objectsWithTag1)
        {
            Collider objCollider = obj.GetComponent<Collider>();
            if (objCollider != null && collider1 != null && IsColliderInBoxCollider(objCollider, collider1))
            {
                isObject1InArea = true;
                break;
            }
        }

        // 检查第二个区域的物体 
        foreach (GameObject obj in objectsWithTag2)
        {
            Collider objCollider = obj.GetComponent<Collider>();
            if (objCollider != null && collider2 != null && IsColliderInBoxCollider(objCollider, collider2))
            {
                isObject2InArea = true;
                break;
            }
        }

        // 如果状态改变，更新UI 
        if (previousState1 != isObject1InArea || previousState2 != isObject2InArea)
        {
            UpdateImageVisibility();
        }
    }

    bool IsColliderInBoxCollider(Collider targetCollider, BoxCollider boxCollider)
    {
        if (targetCollider == null || boxCollider == null) return false;

        // 获取目标碰撞器的边界 
        Bounds targetBounds = targetCollider.bounds;

        // 获取BoxCollider的世界空间边界 
        Bounds boxBounds = new Bounds(
            boxCollider.transform.TransformPoint(boxCollider.center),
            Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale)
        );

        return boxBounds.Intersects(targetBounds);
    }

    void OnTriggerEnter(Collider other)
    {
        ProcessTriggerEvent(other, true);
    }

    void OnTriggerExit(Collider other)
    {
        ProcessTriggerEvent(other, false);
    }

    void ProcessTriggerEvent(Collider other, bool isEntering)
    {
        // 检测第一个区域 
        if (collider1 != null && IsColliderInBoxCollider(other, collider1))
        {
            if (other.CompareTag(targetTag1))
            {
                isObject1InArea = isEntering;
                Debug.Log($"物体 {other.name}    {(isEntering ? "进入" : "离开")}区域1");
                UpdateImageVisibility();
            }
        }

        // 检测第二个区域 
        if (collider2 != null && IsColliderInBoxCollider(other, collider2))
        {
            if (other.CompareTag(targetTag2))
            {
                isObject2InArea = isEntering;
                Debug.Log($"物体 {other.name}    {(isEntering ? "进入" : "离开")}区域2");
                UpdateImageVisibility();
            }
        }
    }

    void UpdateImageVisibility()
    {
        
        if (targetImageObject == null)
        {
            Debug.LogWarning("targetImageObject为null，无法更新显示状态");
            return;
        }

        bool shouldShow = isObject1InArea && isObject2InArea;

        // 使用SetActive来控制整个GameObject的显示/隐藏 
        targetImageObject.SetActive(shouldShow);

        if (shouldShow)
        {
            Debug.Log("两个目标物体都在区域内，显示UI Image");
        }
        else
        {
            Debug.Log($"至少有一个目标物体不在区域内，隐藏UI Image。区域1: {isObject1InArea}, 区域2: {isObject2InArea}");
        }
    }

    // 在Scene视图中绘制碰撞器区域的Gizmos 
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        if (colliderArea1 != null)
        {
            DrawAreaGizmo(colliderArea1.transform, collider1Size, gizmoColor1, "区域1");
        }

        if (colliderArea2 != null)
        {
            DrawAreaGizmo(colliderArea2.transform, collider2Size, gizmoColor2, "区域2");
        }
    }

    void DrawAreaGizmo(Transform areaTransform, Vector3 size, Color color, string label)
    {
        Gizmos.color = color;

        // 保存当前矩阵 
        Matrix4x4 originalMatrix = Gizmos.matrix;

        // 设置Gizmos矩阵以匹配区域变换 
        Gizmos.matrix = areaTransform.localToWorldMatrix;

        // 绘制实心立方体（半透明）
        Color translucentColor = color;
        translucentColor.a = 0.3f;
        Gizmos.color = translucentColor;
        Gizmos.DrawCube(Vector3.zero, size);

        // 绘制边框 
        Gizmos.color = color;
        Gizmos.DrawWireCube(Vector3.zero, size);

        // 恢复原始矩阵 
        Gizmos.matrix = originalMatrix;

        // 绘制标签 
#if UNITY_EDITOR
        UnityEditor.Handles.color   = color;
        UnityEditor.Handles.Label(areaTransform.position   + Vector3.up   * size.y * 0.6f, label);
#endif
    }

    // 公共方法用于手动检查状态（可选）
    public bool IsConditionMet()
    {
        return isObject1InArea && isObject2InArea;
    }

    public string GetStatus()
    {
        return $"区域1: {isObject1InArea}, 区域2: {isObject2InArea}, UI显示: {(targetImageObject != null ? targetImageObject.activeSelf.ToString() : "null")}";
    }

    // 在Inspector中验证输入 
    void OnValidate()
    {
        // 确保大小不为负 
        collider1Size = new Vector3(
            Mathf.Max(0.1f, collider1Size.x),
            Mathf.Max(0.1f, collider1Size.y),
            Mathf.Max(0.1f, collider1Size.z)
        );

        collider2Size = new Vector3(
            Mathf.Max(0.1f, collider2Size.x),
            Mathf.Max(0.1f, collider2Size.y),
            Mathf.Max(0.1f, collider2Size.z)
        );
    }

    // 清理方法 
    void OnDestroy()
    {
        // 移除添加的碰撞器组件（可选）
        if (colliderArea1 != null && collider1 != null)
        {
            if (Application.isPlaying)
            {
                Destroy(collider1);
            }
            else
            {
                DestroyImmediate(collider1);
            }
        }

        if (colliderArea2 != null && collider2 != null)
        {
            if (Application.isPlaying)
            {
                Destroy(collider2);
            }
            else
            {
                DestroyImmediate(collider2);
            }
        }
    }
}