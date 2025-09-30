using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    //    [Header("Ŀ������")]
    //    public Transform target;  // Ҫ��������

    //    [Header("�����λ��")]
    //    public float height = 10f;      // ������߶�
    //    public float distance = 8f;     // ���������

    //    [Header("��ת����")]
    //    public float rotationSpeed = 2f;    // ��ת�ٶ�
    //    public KeyCode rotateLeftKey;       // ����ת����
    //    public KeyCode rotateRightKey;      // ����ת����

    //    [Header("�Զ�ת������")]
    //    public KeyCode leftTurnKey;       // ��ת����
    //    public KeyCode rightTurnKey;      // ��ת����
    //    public float turnSensitivity = 15f;    // ת��������
    //    public bool enableAutoRotation = true; // �Ƿ������Զ�ת��

    //    [Header("�ڵ�����")]
    //    [SerializeField] private LayerMask _obstructionMask = 1 << 8; // Ĭ��ʹ�õ�31��
    //    public float minDistance = 1f;     // ��С���������
    //    public float wallOffset = 0.1f;    // ǽ��ƫ�Ʒ�ֹ��ģ

    //    public float currentAngle = 45f;   // ��ǰ�Ƕȣ�45�ȸ��ǣ�
    //    [Header("��ɫ��������")]
    //    public bool controlCharacterFacing = true;
    //    public Transform characterSprite; // ��ɫ��Sprite��ģ��
    //    public float characterRotationOffset = 0f; // ��תƫ��
    //    void LateUpdate()
    //    {
    //        HandleRotation();
    //        HandleAutoRotation();
    //        UpdateCameraPosition();
    //        //UpdateCharacterFacing(); // ����
    //    }

    //    void Awake()
    //    {
    //        InitializeDefaultLayer();
    //    }

    //    void InitializeDefaultLayer()
    //    {
    //        // �Զ�����CameraCollision�㣨��������ڣ�
    //        if (LayerMask.NameToLayer("CameraCollision") == -1)
    //        {
    //            Debug.LogWarning("CameraCollision�㲻���ڣ������Զ�����...");
    //            CreateCameraCollisionLayer();
    //        }

    //        // ����Ĭ�ϲ�����
    //        _obstructionMask = 1 << LayerMask.NameToLayer("CameraCollision");
    //    }

    //    void HandleAutoRotation()
    //    {
    //        if (!enableAutoRotation || target == null) return;

    //        float rotationInput = 0f;

    //        // ��ⰴ������
    //        if (Input.GetKey(leftTurnKey)) rotationInput -= 1f;
    //        if (Input.GetKey(rightTurnKey)) rotationInput += 1f;

    //        // ������������Ƕ�
    //        if (Mathf.Abs(rotationInput) > 0.1f)
    //        {
    //            currentAngle += rotationInput * turnSensitivity * Time.deltaTime;
    //        }
    //    }
    //    void HandleRotation()
    //    {
    //        // �����������ת
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

    //        // �������������λ��
    //        float angleRad = currentAngle * Mathf.Deg2Rad;
    //        float x = -Mathf.Sin(angleRad) * distance;
    //        float z = -Mathf.Cos(angleRad) * distance;

    //        Vector3 targetPosition = target.position;
    //        Vector3 desiredPosition = new Vector3(
    //            targetPosition.x + x,
    //            targetPosition.y + height,
    //            targetPosition.z + z
    //        );

    //        // �ڵ����
    //        Vector3 checkDirection = (desiredPosition - targetPosition).normalized;
    //        float checkDistance = Vector3.Distance(targetPosition, desiredPosition);

    //        RaycastHit hit;
    //        if (Physics.Raycast(targetPosition, checkDirection, out hit, checkDistance, _obstructionMask))
    //        {
    //            // ����λ�õ���ײ��ǰ��
    //            desiredPosition = hit.point - checkDirection * wallOffset;
    //            // ���ָ߶Ȳ�����ˮƽ����
    //            desiredPosition.y = Mathf.Max(targetPosition.y + minDistance, desiredPosition.y);
    //        }

    //        transform.position = desiredPosition;
    //        transform.LookAt(target.position);
    //        //UpdateCharacterFacing();
    //    }
    //    void UpdateCharacterFacing()
    //    {
    //        if (!controlCharacterFacing || characterSprite == null) return;

    //        // �������������ɫ�ķ���
    //        Vector3 toCharacter = characterSprite.position - transform.position;
    //        toCharacter.y = 0; // ����Y��

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
    //        // ͨ��TagManager�����²�
    //        var tagManager = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
    //        var layersProp = tagManager.FindProperty("layers");

    //        for (int i = 8; i < 32; i++) // ���û����ò㿪ʼ����
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

    [Header("Ŀ������")]
    public Transform target;  // Ҫ��������

    [Header("�����λ��")]
    public float height = 10f;      // ������߶�
    public float distance = 8f;     // ���������

    [Header("��ת����")]
    public float rotationSpeed = 90f;    // ��ת�ٶȣ���/�룩
    public KeyCode rotateLeftKey;       // ����ת����
    public KeyCode rotateRightKey;      // ����ת����

    [Header("�Զ�ת������")]
    public KeyCode leftTurnKey;       // ��ת����
    public KeyCode rightTurnKey;      // ��ת����
    public float turnSensitivity = 60f;    // ת��������
    public bool enableAutoRotation = true; // �Ƿ������Զ�ת��

    [Header("ƽ������")]
    public float positionSmoothTime = 0.1f; // λ��ƽ��ʱ��
    public float rotationSmoothTime = 0.1f; // ��תƽ��ʱ��

    [Header("�ڵ�����")]
    [SerializeField] private LayerMask _obstructionMask = 1 << 8;
    public float minDistance = 1f;     // ��С���������
    public float wallOffset = 0.1f;    // ǽ��ƫ�Ʒ�ֹ��ģ

    [Header("״̬")]
    public float currentAngle = 45f;   // ��ǰ�Ƕȣ�45�ȸ��ǣ�

    // �¼�ϵͳ�����������תʱ֪ͨ�������
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

        // �������������λ��
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
            Debug.LogWarning("CameraCollision�㲻���ڣ������Զ�����...");
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

        // ʼ�տ������
        transform.LookAt(target.position);
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
        transform.LookAt(target.position);
    }

    Vector3 CalculateOrbitPosition(Vector3 center, float angle, float radius, float camHeight)
    {
        // ���Ƕ�ת��Ϊ����
        float angleRad = angle * Mathf.Deg2Rad;

        // ������XZƽ���ϵ�ƫ��
        float x = Mathf.Sin(angleRad) * radius;
        float z = Mathf.Cos(angleRad) * radius;

        // ������centerΪ���ĵ�λ��
        return new Vector3(
            center.x + x,
            center.y + camHeight,
            center.z + z
        );
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

    // ������������ȡ���������ҵķ���
    public Vector3 GetCameraToTargetDirection()
    {
        if (target == null) return Vector3.zero;
        return (target.position - transform.position).normalized;
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
                Debug.Log($"�Ѵ���CameraCollision�㵽�� {i} ��");
                return;
            }
        }
#endif
        Debug.LogError("�޷�����CameraCollision�㣬���ֶ�������");
    }
}
