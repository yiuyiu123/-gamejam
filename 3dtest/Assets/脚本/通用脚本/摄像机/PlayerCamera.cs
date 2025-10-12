using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    #region ע�Ͳ���
    //    [Header("Ŀ������")]
    //    public Transform target;  // Ҫ��������

    //    [Header("�����λ��")]
    //    public float height = 10f;      // ������߶�
    //    public float distance = 8f;     // ���������

    //    [Header("��ת����")]
    //    public float rotationSpeed = 90f;    // ��ת�ٶȣ���/�룩
    //    public KeyCode rotateLeftKey;       // ����ת����
    //    public KeyCode rotateRightKey;      // ����ת����

    //    [Header("�Զ�ת������")]
    //    public KeyCode leftTurnKey;       // ��ת����
    //    public KeyCode rightTurnKey;      // ��ת����
    //    public float turnSensitivity = 60f;    // ת��������
    //    public bool enableAutoRotation = true; // �Ƿ������Զ�ת��

    //    [Header("ƽ������")]
    //    public float positionSmoothTime = 0.1f; // λ��ƽ��ʱ��
    //    public float rotationSmoothTime = 0.1f; // ��תƽ��ʱ��

    //    [Header("�ڵ�����")]
    //    [SerializeField] private LayerMask _obstructionMask = 1 << 8;
    //    public float minDistance = 1f;     // ��С���������
    //    public float wallOffset = 0.1f;    // ǽ��ƫ�Ʒ�ֹ��ģ

    //    [Header("״̬")]
    //    public float currentAngle = 45f;   // ��ǰ�Ƕȣ�45�ȸ��ǣ�

    //    // �¼�ϵͳ�����������תʱ֪ͨ�������
    //    public System.Action<float> OnCameraRotated;

    //    private Vector3 currentVelocity;
    //    private float lastNotifiedAngle;
    //    private bool isInitialized = false;

    //    void Start()
    //    {
    //        InitializeCamera();
    //    }

    //    void InitializeCamera()
    //    {
    //        InitializeDefaultLayer();
    //        lastNotifiedAngle = currentAngle;
    //        isInitialized = true;

    //        // �������������λ��
    //        if (target != null)
    //        {
    //            UpdateCameraPositionImmediate();
    //        }
    //    }

    //    void LateUpdate()
    //    {
    //        if (!isInitialized || target == null) return;

    //        HandleRotation();
    //        HandleAutoRotation();
    //        UpdateCameraPosition();
    //    }

    //    void InitializeDefaultLayer()
    //    {
    //        if (LayerMask.NameToLayer("CameraCollision") == -1)
    //        {
    //            Debug.LogWarning("CameraCollision�㲻���ڣ������Զ�����...");
    //            CreateCameraCollisionLayer();
    //        }
    //        _obstructionMask = 1 << LayerMask.NameToLayer("CameraCollision");
    //    }

    //    void HandleAutoRotation()
    //    {
    //        if (!enableAutoRotation || target == null) return;

    //        float rotationInput = 0f;
    //        if (Input.GetKey(leftTurnKey)) rotationInput -= 1f;
    //        if (Input.GetKey(rightTurnKey)) rotationInput += 1f;

    //        if (Mathf.Abs(rotationInput) > 0.1f)
    //        {
    //            float newAngle = currentAngle + rotationInput * turnSensitivity * Time.deltaTime;
    //            SetCameraAngle(newAngle);
    //        }
    //    }

    //    void HandleRotation()
    //    {
    //        bool rotated = false;
    //        float newAngle = currentAngle;

    //        if (Input.GetKey(rotateLeftKey))
    //        {
    //            newAngle += rotationSpeed * Time.deltaTime;
    //            rotated = true;
    //        }
    //        if (Input.GetKey(rotateRightKey))
    //        {
    //            newAngle -= rotationSpeed * Time.deltaTime;
    //            rotated = true;
    //        }

    //        if (rotated)
    //        {
    //            SetCameraAngle(newAngle);
    //        }
    //    }

    //    void UpdateCameraPosition()
    //    {
    //        if (target == null) return;

    //        // ���������Ϊ���ĵ������λ��
    //        Vector3 desiredPosition = CalculateOrbitPosition(target.position, currentAngle, distance, height);

    //        // �ڵ����
    //        Vector3 checkDirection = (desiredPosition - target.position).normalized;
    //        float checkDistance = Vector3.Distance(target.position, desiredPosition);

    //        RaycastHit hit;
    //        if (Physics.Raycast(target.position, checkDirection, out hit, checkDistance, _obstructionMask))
    //        {
    //            // ��������ײ��ǰ��
    //            desiredPosition = hit.point - checkDirection * wallOffset;

    //            // ������С�߶�
    //            float adjustedHeight = Mathf.Max(target.position.y + minDistance, desiredPosition.y);
    //            desiredPosition.y = adjustedHeight;
    //        }

    //        // ʹ��ƽ���ƶ�
    //        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, positionSmoothTime);

    //        // ʼ�տ������
    //        transform.LookAt(target.position);
    //    }

    //    void UpdateCameraPositionImmediate()
    //    {
    //        if (target == null) return;

    //        // �������������λ�ã���ƽ����
    //        Vector3 desiredPosition = CalculateOrbitPosition(target.position, currentAngle, distance, height);

    //        // �ڵ����
    //        Vector3 checkDirection = (desiredPosition - target.position).normalized;
    //        float checkDistance = Vector3.Distance(target.position, desiredPosition);

    //        RaycastHit hit;
    //        if (Physics.Raycast(target.position, checkDirection, out hit, checkDistance, _obstructionMask))
    //        {
    //            desiredPosition = hit.point - checkDirection * wallOffset;
    //            desiredPosition.y = Mathf.Max(target.position.y + minDistance, desiredPosition.y);
    //        }

    //        transform.position = desiredPosition;
    //        transform.LookAt(target.position);
    //    }

    //    Vector3 CalculateOrbitPosition(Vector3 center, float angle, float radius, float camHeight)
    //    {
    //        // ���Ƕ�ת��Ϊ����
    //        float angleRad = angle * Mathf.Deg2Rad;

    //        // ������XZƽ���ϵ�ƫ��
    //        float x = Mathf.Sin(angleRad) * radius;
    //        float z = Mathf.Cos(angleRad) * radius;

    //        // ������centerΪ���ĵ�λ��
    //        return new Vector3(
    //            center.x + x,
    //            center.y + camHeight,
    //            center.z + z
    //        );
    //    }

    //    // �������������õ�ǰ�Ƕ�
    //    public void SetCameraAngle(float angle)
    //    {
    //        float newAngle = Mathf.Repeat(angle, 360f); // ���ֽǶ���0-360�ȷ�Χ��

    //        if (Mathf.Abs(newAngle - currentAngle) > 0.1f)
    //        {
    //            currentAngle = newAngle;

    //            // ֻ�е��Ƕ�ȷʵ�����仯ʱ��֪ͨ
    //            if (Mathf.Abs(currentAngle - lastNotifiedAngle) > 0.1f)
    //            {
    //                lastNotifiedAngle = currentAngle;
    //                OnCameraRotated?.Invoke(currentAngle);
    //            }
    //        }
    //    }

    //    public void SetTarget(Transform newTarget)
    //    {
    //        target = newTarget;
    //        if (isInitialized && target != null)
    //        {
    //            UpdateCameraPositionImmediate();
    //        }
    //    }

    //    // ������������ȡ��ǰ�Ƕ�
    //    public float GetCurrentAngle()
    //    {
    //        return currentAngle;
    //    }

    //    // ����������������׼Ŀ��
    //    public void SnapToTarget()
    //    {
    //        if (target != null)
    //        {
    //            UpdateCameraPositionImmediate();
    //        }
    //    }

    //    // ������������ȡ���������ҵķ���
    //    public Vector3 GetCameraToTargetDirection()
    //    {
    //        if (target == null) return Vector3.zero;
    //        return (target.position - transform.position).normalized;
    //    }

    //    // ��Scene��ͼ����ʾ������Ϣ
    //    void OnDrawGizmosSelected()
    //    {
    //        if (target == null) return;

    //        // ��ʾ��������
    //        Gizmos.color = Color.blue;
    //        Vector3 orbitCenter = target.position;

    //        // ����Բ�ι��
    //        const int segments = 36;
    //        float segmentAngle = 360f / segments;

    //        for (int i = 0; i < segments; i++)
    //        {
    //            float angle1 = i * segmentAngle * Mathf.Deg2Rad;
    //            float angle2 = (i + 1) * segmentAngle * Mathf.Deg2Rad;

    //            Vector3 pos1 = new Vector3(
    //                orbitCenter.x + Mathf.Sin(angle1) * distance,
    //                orbitCenter.y + height,
    //                orbitCenter.z + Mathf.Cos(angle1) * distance
    //            );

    //            Vector3 pos2 = new Vector3(
    //                orbitCenter.x + Mathf.Sin(angle2) * distance,
    //                orbitCenter.y + height,
    //                orbitCenter.z + Mathf.Cos(angle2) * distance
    //            );

    //            Gizmos.DrawLine(pos1, pos2);
    //        }

    //        // ��ʾ��ǰ�����λ��
    //        Gizmos.color = Color.red;
    //        Vector3 currentPos = CalculateOrbitPosition(orbitCenter, currentAngle, distance, height);
    //        Gizmos.DrawWireSphere(currentPos, 0.3f);
    //        Gizmos.DrawLine(orbitCenter, currentPos);

    //        // ��ʾ������ҵ���
    //        Gizmos.color = Color.green;
    //        Gizmos.DrawLine(transform.position, target.position);
    //    }

    //    void CreateCameraCollisionLayer()
    //    {
    //#if UNITY_EDITOR
    //        var tagManager = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
    //        var layersProp = tagManager.FindProperty("layers");

    //        for (int i = 8; i < 32; i++)
    //        {
    //            var layer = layersProp.GetArrayElementAtIndex(i);
    //            if (layer.stringValue == "CameraCollision") return;

    //            if (layer.stringValue == "")
    //            {
    //                layer.stringValue = "CameraCollision";
    //                tagManager.ApplyModifiedProperties();
    //                Debug.Log($"�Ѵ���CameraCollision�㵽�� {i} ��");
    //                return;
    //            }
    //        }
    //#endif
    //        Debug.LogError("�޷�����CameraCollision�㣬���ֶ�������");
    //    }
    #endregion 
    [Header("Ŀ������")]
    public Transform target;  // Ҫ��������

    [Header("�����λ��")]
    public float height = 10f;      // ������߶�
    public float distance = 8f;     // ���������

    [Header("��ת����")]
    public float rotationSpeed = 90f;    // ��ת�ٶȣ���/�룩
    public KeyCode rotateLeftKey;       // ����ת����
    public KeyCode rotateRightKey;      // ����ת����

    [Header("ƽ������")]
    public float positionSmoothTime = 0.1f; // λ��ƽ��ʱ��
    public float rotationSmoothTime = 0.3f; // ������תƽ��ʱ��

    [Header("�ڵ�����")]
    [SerializeField] private LayerMask _obstructionMask = 1 << 8;
    public float minDistance = 1f;     // ��С���������
    public float wallOffset = 0.1f;    // ǽ��ƫ�Ʒ�ֹ��ģ

    [Header("״̬")]
    public float currentAngle = 45f;   // ��ǰ�Ƕȣ�45�ȸ��ǣ�

    [Header("��������")]
    public bool enableAutoRotation = false; // �����Զ�ת��ʹ��ר�ð���

    // �¼�ϵͳ�����������תʱ֪ͨ�������
    public System.Action<float> OnCameraRotated;

    private Vector3 currentVelocity;
    private Quaternion currentRotation;
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
        currentRotation = transform.rotation;
        isInitialized = true;

        // �������������λ��
        if (target != null)
        {
            UpdateCameraPositionImmediate();
        }

        Debug.Log($"�������ʼ����� - ��ת����: {rotateLeftKey}/{rotateRightKey}");
    }

    void LateUpdate()
    {
        if (!isInitialized || target == null) return;

        HandleRotation();
        UpdateCameraPosition();
    }

    void HandleRotation()
    {
        bool rotated = false;
        float newAngle = currentAngle;

        // ʹ��ר�ð��������ӽ���ת
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

        // ���������Ϊ���ĵ������λ��
        Vector3 desiredPosition = CalculateOrbitPosition(target.position, currentAngle, distance, height);

        // �ڵ����
        Vector3 checkDirection = (desiredPosition - target.position).normalized;
        float checkDistance = Vector3.Distance(target.position, desiredPosition);

        RaycastHit hit;
        if (Physics.Raycast(target.position, checkDirection, out hit, checkDistance, _obstructionMask))
        {
            // ��������ײ��ǰ��
            desiredPosition = hit.point - checkDirection * wallOffset;

            // ������С�߶�
            float adjustedHeight = Mathf.Max(target.position.y + minDistance, desiredPosition.y);
            desiredPosition.y = adjustedHeight;
        }

        // ʹ��ƽ���ƶ�
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, positionSmoothTime);

        // ʼ�տ������ - �޸���ķ���
        LookAtTarget();
    }

    void LookAtTarget()
    {
        if (target == null) return;

        // ֱ��ʹ�� LookAt ������������ɿ��ķ�ʽ
        transform.LookAt(target.position);

        // ������Ϣ
        if (Time.frameCount % 120 == 0 && showDebugInfo)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToTarget);
            Debug.Log($"���������Ŀ�� - �ǶȲ�: {angle:F2}��, ����: {Vector3.Distance(transform.position, target.position):F2}");
        }
    }

    void UpdateCameraPositionImmediate()
    {
        if (target == null) return;

        // �������������λ�ã���ƽ����
        Vector3 desiredPosition = CalculateOrbitPosition(target.position, currentAngle, distance, height);

        // �ڵ����
        Vector3 checkDirection = (desiredPosition - target.position).normalized;
        float checkDistance = Vector3.Distance(target.position, desiredPosition);

        RaycastHit hit;
        if (Physics.Raycast(target.position, checkDirection, out hit, checkDistance, _obstructionMask))
        {
            desiredPosition = hit.point - checkDirection * wallOffset;
            desiredPosition.y = Mathf.Max(target.position.y + minDistance, desiredPosition.y);
        }

        transform.position = desiredPosition;

        // �����������
        LookAtTarget();
    }

    Vector3 CalculateOrbitPosition(Vector3 center, float angle, float radius, float camHeight)
    {
        // ���Ƕ�ת��Ϊ����
        float angleRad = angle * Mathf.Deg2Rad;

        // ������XZƽ���ϵ�ƫ��
        float x = Mathf.Sin(angleRad) * radius;
        float z = Mathf.Cos(angleRad) * radius;

        // ������centerΪ���ĵ�λ��
        Vector3 orbitPosition = new Vector3(
            center.x + x,
            center.y + camHeight,
            center.z + z
        );

        return orbitPosition;
    }

    // �������������õ�ǰ�Ƕ�
    public void SetCameraAngle(float angle)
    {
        float newAngle = Mathf.Repeat(angle, 360f); // ���ֽǶ���0-360�ȷ�Χ��

        if (Mathf.Abs(newAngle - currentAngle) > 0.1f)
        {
            currentAngle = newAngle;

            // ֻ�е��Ƕ�ȷʵ�����仯ʱ��֪ͨ
            if (Mathf.Abs(currentAngle - lastNotifiedAngle) > 0.1f)
            {
                lastNotifiedAngle = currentAngle;
                OnCameraRotated?.Invoke(currentAngle);
            }
        }
    }

    // ��������������Ŀ��
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (isInitialized && target != null)
        {
            UpdateCameraPositionImmediate();
        }
    }

    // ������������ȡ��ǰ�Ƕ�
    public float GetCurrentAngle()
    {
        return currentAngle;
    }

    // ����������������׼Ŀ��
    public void SnapToTarget()
    {
        if (target != null)
        {
            UpdateCameraPositionImmediate();
        }
    }

    // �����������������������
    public void SetCameraParameters(float newHeight, float newDistance, float newRotationSpeed)
    {
        height = newHeight;
        distance = newDistance;
        rotationSpeed = newRotationSpeed;
    }

    // ������������ȡ���������ҵķ���
    public Vector3 GetCameraToTargetDirection()
    {
        if (target == null) return Vector3.zero;
        return (target.position - transform.position).normalized;
    }

    void InitializeDefaultLayer()
    {
        if (LayerMask.NameToLayer("CameraCollision") == -1)
        {
            Debug.LogWarning("CameraCollision�㲻���ڣ�ʹ��Ĭ���ڵ���");
            _obstructionMask = 1; // ʹ��Ĭ�ϲ�
        }
        else
        {
            _obstructionMask = 1 << LayerMask.NameToLayer("CameraCollision");
        }
    }

    // ��Scene��ͼ����ʾ������Ϣ
    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // ��ʾ��������
        Gizmos.color = Color.blue;
        Vector3 orbitCenter = target.position;

        // ����Բ�ι��
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

        // ��ʾ��ǰ�����λ��
        Gizmos.color = Color.red;
        Vector3 currentPos = CalculateOrbitPosition(orbitCenter, currentAngle, distance, height);
        Gizmos.DrawWireSphere(currentPos, 0.3f);
        Gizmos.DrawLine(orbitCenter, currentPos);

        // ��ʾ������ҵ���
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, target.position);

        // ��ʾ�������Ұ����
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);
    }

    // ������Ϣ
    private bool showDebugInfo = true;

    //void OnGUI()
    //{
    //    if (!Application.isPlaying) return;

    //    // ����Ļ���Ͻ���ʾ�������Ϣ
    //    GUIStyle style = new GUIStyle();
    //    style.normal.textColor = Color.white;
    //    style.fontSize = 12;

    //    Vector3 toTarget = target != null ? (target.position - transform.position) : Vector3.zero;
    //    float lookAngle = Vector3.Angle(transform.forward, toTarget.normalized);

    //    string info = $"������Ƕ�: {currentAngle:F1}��\n" +
    //                 $"Ŀ��: {(target != null ? target.name : "��")}\n" +
    //                 $"����Ŀ��ǶȲ�: {lookAngle:F1}��\n" +
    //                 $"����: {toTarget.magnitude:F2}";

    //    GUI.Label(new Rect(10, 10, 300, 100), info, style);
    //}

    // ������Դ
    void OnDestroy()
    {
        // �����¼�����
        OnCameraRotated = null;
    }

    // ���������״̬
    public void ResetCamera()
    {
        currentAngle = 45f;
        lastNotifiedAngle = currentAngle;
        currentVelocity = Vector3.zero;

        if (target != null)
        {
            UpdateCameraPositionImmediate();
        }
    }

    // ǿ���������������
    public void ForceUpdate()
    {
        if (target != null)
        {
            UpdateCameraPositionImmediate();
        }
    }
}
