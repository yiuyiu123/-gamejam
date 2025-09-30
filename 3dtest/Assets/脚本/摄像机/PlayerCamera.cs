using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    //    [Header("目标设置")]
    //    public Transform target;  // 要跟随的玩家

    //    [Header("摄像机位置")]
    //    public float height = 10f;      // 摄像机高度
    //    public float distance = 8f;     // 摄像机距离

    //    [Header("旋转设置")]
    //    public float rotationSpeed = 2f;    // 旋转速度
    //    public KeyCode rotateLeftKey;       // 左旋转按键
    //    public KeyCode rotateRightKey;      // 右旋转按键

    //    [Header("自动转向设置")]
    //    public KeyCode leftTurnKey;       // 左转按键
    //    public KeyCode rightTurnKey;      // 右转按键
    //    public float turnSensitivity = 15f;    // 转向灵敏度
    //    public bool enableAutoRotation = true; // 是否启用自动转向

    //    [Header("遮挡处理")]
    //    [SerializeField] private LayerMask _obstructionMask = 1 << 8; // 默认使用第31层
    //    public float minDistance = 1f;     // 最小摄像机距离
    //    public float wallOffset = 0.1f;    // 墙体偏移防止穿模

    //    public float currentAngle = 45f;   // 当前角度（45度俯角）
    //    [Header("角色朝向设置")]
    //    public bool controlCharacterFacing = true;
    //    public Transform characterSprite; // 角色的Sprite或模型
    //    public float characterRotationOffset = 0f; // 旋转偏移
    //    void LateUpdate()
    //    {
    //        HandleRotation();
    //        HandleAutoRotation();
    //        UpdateCameraPosition();
    //        //UpdateCharacterFacing(); // 新增
    //    }

    //    void Awake()
    //    {
    //        InitializeDefaultLayer();
    //    }

    //    void InitializeDefaultLayer()
    //    {
    //        // 自动创建CameraCollision层（如果不存在）
    //        if (LayerMask.NameToLayer("CameraCollision") == -1)
    //        {
    //            Debug.LogWarning("CameraCollision层不存在，正在自动创建...");
    //            CreateCameraCollisionLayer();
    //        }

    //        // 设置默认层掩码
    //        _obstructionMask = 1 << LayerMask.NameToLayer("CameraCollision");
    //    }

    //    void HandleAutoRotation()
    //    {
    //        if (!enableAutoRotation || target == null) return;

    //        float rotationInput = 0f;

    //        // 检测按键输入
    //        if (Input.GetKey(leftTurnKey)) rotationInput -= 1f;
    //        if (Input.GetKey(rightTurnKey)) rotationInput += 1f;

    //        // 根据输入调整角度
    //        if (Mathf.Abs(rotationInput) > 0.1f)
    //        {
    //            currentAngle += rotationInput * turnSensitivity * Time.deltaTime;
    //        }
    //    }
    //    void HandleRotation()
    //    {
    //        // 处理摄像机旋转
    //        if (Input.GetKey(rotateLeftKey))
    //        {
    //            currentAngle += rotationSpeed;
    //        }
    //        if (Input.GetKey(rotateRightKey))
    //        {
    //            currentAngle -= rotationSpeed;
    //        }
    //    }

    //    void UpdateCameraPosition()
    //    {
    //        if (target == null) return;

    //        // 计算理想摄像机位置
    //        float angleRad = currentAngle * Mathf.Deg2Rad;
    //        float x = -Mathf.Sin(angleRad) * distance;
    //        float z = -Mathf.Cos(angleRad) * distance;

    //        Vector3 targetPosition = target.position;
    //        Vector3 desiredPosition = new Vector3(
    //            targetPosition.x + x,
    //            targetPosition.y + height,
    //            targetPosition.z + z
    //        );

    //        // 遮挡检测
    //        Vector3 checkDirection = (desiredPosition - targetPosition).normalized;
    //        float checkDistance = Vector3.Distance(targetPosition, desiredPosition);

    //        RaycastHit hit;
    //        if (Physics.Raycast(targetPosition, checkDirection, out hit, checkDistance, _obstructionMask))
    //        {
    //            // 调整位置到碰撞点前方
    //            desiredPosition = hit.point - checkDirection * wallOffset;
    //            // 保持高度并计算水平距离
    //            desiredPosition.y = Mathf.Max(targetPosition.y + minDistance, desiredPosition.y);
    //        }

    //        transform.position = desiredPosition;
    //        transform.LookAt(target.position);
    //        //UpdateCharacterFacing();
    //    }
    //    void UpdateCharacterFacing()
    //    {
    //        if (!controlCharacterFacing || characterSprite == null) return;

    //        // 计算摄像机到角色的方向
    //        Vector3 toCharacter = characterSprite.position - transform.position;
    //        toCharacter.y = 0; // 锁定Y轴

    //        if (toCharacter != Vector3.zero)
    //        {
    //            Quaternion targetRotation = Quaternion.LookRotation(toCharacter);
    //            characterSprite.rotation = targetRotation * Quaternion.Euler(0, characterRotationOffset, 0);
    //        }
    //    }
    //    public void SetTarget(Transform newTarget)
    //    {
    //        target = newTarget;
    //    }


    //    void CreateCameraCollisionLayer()
    //    {
    //#if UNITY_EDITOR
    //        // 通过TagManager创建新层
    //        var tagManager = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
    //        var layersProp = tagManager.FindProperty("layers");

    //        for (int i = 8; i < 32; i++) // 从用户可用层开始查找
    //        {
    //            var layer = layersProp.GetArrayElementAtIndex(i);
    //            if (layer.stringValue == "CameraCollision") return;

    //            if (layer.stringValue == "")
    //            {
    //                layer.stringValue = "CameraCollision";
    //                tagManager.ApplyModifiedProperties();
    //                Debug.Log($"已创建CameraCollision层到第 {i} 层");
    //                return;
    //            }
    //        }
    //#endif
    //        Debug.LogError("无法创建CameraCollision层，请手动创建！");
    //    }

    [Header("目标设置")]
    public Transform target;  // 要跟随的玩家

    [Header("摄像机位置")]
    public float height = 10f;      // 摄像机高度
    public float distance = 8f;     // 摄像机距离

    [Header("旋转设置")]
    public float rotationSpeed = 90f;    // 旋转速度（度/秒）
    public KeyCode rotateLeftKey;       // 左旋转按键
    public KeyCode rotateRightKey;      // 右旋转按键

    [Header("自动转向设置")]
    public KeyCode leftTurnKey;       // 左转按键
    public KeyCode rightTurnKey;      // 右转按键
    public float turnSensitivity = 60f;    // 转向灵敏度
    public bool enableAutoRotation = true; // 是否启用自动转向

    [Header("平滑设置")]
    public float positionSmoothTime = 0.1f; // 位置平滑时间
    public float rotationSmoothTime = 0.1f; // 旋转平滑时间

    [Header("遮挡处理")]
    [SerializeField] private LayerMask _obstructionMask = 1 << 8;
    public float minDistance = 1f;     // 最小摄像机距离
    public float wallOffset = 0.1f;    // 墙体偏移防止穿模

    [Header("状态")]
    public float currentAngle = 45f;   // 当前角度（45度俯角）

    // 事件系统：当摄像机旋转时通知其他组件
    public System.Action<float> OnCameraRotated;

    private Vector3 currentVelocity;
    private float lastNotifiedAngle;
    private bool isInitialized = false;

    void Start()
    {
        InitializeCamera();
    }

    void InitializeCamera()
    {
        InitializeDefaultLayer();
        lastNotifiedAngle = currentAngle;
        isInitialized = true;

        // 立即更新摄像机位置
        if (target != null)
        {
            UpdateCameraPositionImmediate();
        }
    }

    void LateUpdate()
    {
        if (!isInitialized || target == null) return;

        HandleRotation();
        HandleAutoRotation();
        UpdateCameraPosition();
    }

    void InitializeDefaultLayer()
    {
        if (LayerMask.NameToLayer("CameraCollision") == -1)
        {
            Debug.LogWarning("CameraCollision层不存在，正在自动创建...");
            CreateCameraCollisionLayer();
        }
        _obstructionMask = 1 << LayerMask.NameToLayer("CameraCollision");
    }

    void HandleAutoRotation()
    {
        if (!enableAutoRotation || target == null) return;

        float rotationInput = 0f;
        if (Input.GetKey(leftTurnKey)) rotationInput -= 1f;
        if (Input.GetKey(rightTurnKey)) rotationInput += 1f;

        if (Mathf.Abs(rotationInput) > 0.1f)
        {
            float newAngle = currentAngle + rotationInput * turnSensitivity * Time.deltaTime;
            SetCameraAngle(newAngle);
        }
    }

    void HandleRotation()
    {
        bool rotated = false;
        float newAngle = currentAngle;

        if (Input.GetKey(rotateLeftKey))
        {
            newAngle += rotationSpeed * Time.deltaTime;
            rotated = true;
        }
        if (Input.GetKey(rotateRightKey))
        {
            newAngle -= rotationSpeed * Time.deltaTime;
            rotated = true;
        }

        if (rotated)
        {
            SetCameraAngle(newAngle);
        }
    }

    void UpdateCameraPosition()
    {
        if (target == null) return;

        // 计算以玩家为中心的摄像机位置
        Vector3 desiredPosition = CalculateOrbitPosition(target.position, currentAngle, distance, height);

        // 遮挡检测
        Vector3 checkDirection = (desiredPosition - target.position).normalized;
        float checkDistance = Vector3.Distance(target.position, desiredPosition);

        RaycastHit hit;
        if (Physics.Raycast(target.position, checkDirection, out hit, checkDistance, _obstructionMask))
        {
            // 调整到碰撞点前方
            desiredPosition = hit.point - checkDirection * wallOffset;

            // 保持最小高度
            float adjustedHeight = Mathf.Max(target.position.y + minDistance, desiredPosition.y);
            desiredPosition.y = adjustedHeight;
        }

        // 使用平滑移动
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, positionSmoothTime);

        // 始终看向玩家
        transform.LookAt(target.position);
    }

    void UpdateCameraPositionImmediate()
    {
        if (target == null) return;

        // 立即更新摄像机位置（无平滑）
        Vector3 desiredPosition = CalculateOrbitPosition(target.position, currentAngle, distance, height);

        // 遮挡检测
        Vector3 checkDirection = (desiredPosition - target.position).normalized;
        float checkDistance = Vector3.Distance(target.position, desiredPosition);

        RaycastHit hit;
        if (Physics.Raycast(target.position, checkDirection, out hit, checkDistance, _obstructionMask))
        {
            desiredPosition = hit.point - checkDirection * wallOffset;
            desiredPosition.y = Mathf.Max(target.position.y + minDistance, desiredPosition.y);
        }

        transform.position = desiredPosition;
        transform.LookAt(target.position);
    }

    Vector3 CalculateOrbitPosition(Vector3 center, float angle, float radius, float camHeight)
    {
        // 将角度转换为弧度
        float angleRad = angle * Mathf.Deg2Rad;

        // 计算在XZ平面上的偏移
        float x = Mathf.Sin(angleRad) * radius;
        float z = Mathf.Cos(angleRad) * radius;

        // 返回以center为中心的位置
        return new Vector3(
            center.x + x,
            center.y + camHeight,
            center.z + z
        );
    }

    // 公共方法：设置当前角度
    public void SetCameraAngle(float angle)
    {
        float newAngle = Mathf.Repeat(angle, 360f); // 保持角度在0-360度范围内

        if (Mathf.Abs(newAngle - currentAngle) > 0.1f)
        {
            currentAngle = newAngle;

            // 只有当角度确实发生变化时才通知
            if (Mathf.Abs(currentAngle - lastNotifiedAngle) > 0.1f)
            {
                lastNotifiedAngle = currentAngle;
                OnCameraRotated?.Invoke(currentAngle);
            }
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (isInitialized && target != null)
        {
            UpdateCameraPositionImmediate();
        }
    }

    // 公共方法：获取当前角度
    public float GetCurrentAngle()
    {
        return currentAngle;
    }

    // 公共方法：立即对准目标
    public void SnapToTarget()
    {
        if (target != null)
        {
            UpdateCameraPositionImmediate();
        }
    }

    // 公共方法：获取摄像机到玩家的方向
    public Vector3 GetCameraToTargetDirection()
    {
        if (target == null) return Vector3.zero;
        return (target.position - transform.position).normalized;
    }

    // 在Scene视图中显示调试信息
    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // 显示摄像机轨道
        Gizmos.color = Color.blue;
        Vector3 orbitCenter = target.position;

        // 绘制圆形轨道
        const int segments = 36;
        float segmentAngle = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * segmentAngle * Mathf.Deg2Rad;
            float angle2 = (i + 1) * segmentAngle * Mathf.Deg2Rad;

            Vector3 pos1 = new Vector3(
                orbitCenter.x + Mathf.Sin(angle1) * distance,
                orbitCenter.y + height,
                orbitCenter.z + Mathf.Cos(angle1) * distance
            );

            Vector3 pos2 = new Vector3(
                orbitCenter.x + Mathf.Sin(angle2) * distance,
                orbitCenter.y + height,
                orbitCenter.z + Mathf.Cos(angle2) * distance
            );

            Gizmos.DrawLine(pos1, pos2);
        }

        // 显示当前摄像机位置
        Gizmos.color = Color.red;
        Vector3 currentPos = CalculateOrbitPosition(orbitCenter, currentAngle, distance, height);
        Gizmos.DrawWireSphere(currentPos, 0.3f);
        Gizmos.DrawLine(orbitCenter, currentPos);

        // 显示看向玩家的线
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, target.position);
    }

    void CreateCameraCollisionLayer()
    {
#if UNITY_EDITOR
        var tagManager = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        var layersProp = tagManager.FindProperty("layers");

        for (int i = 8; i < 32; i++)
        {
            var layer = layersProp.GetArrayElementAtIndex(i);
            if (layer.stringValue == "CameraCollision") return;

            if (layer.stringValue == "")
            {
                layer.stringValue = "CameraCollision";
                tagManager.ApplyModifiedProperties();
                Debug.Log($"已创建CameraCollision层到第 {i} 层");
                return;
            }
        }
#endif
        Debug.LogError("无法创建CameraCollision层，请手动创建！");
    }
}
