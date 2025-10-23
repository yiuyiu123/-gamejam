using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualPlayerController : MonoBehaviour
{
    [Header("��Ҷ���")]
    public GameObject player1;
    public GameObject player2;

    [Header("�ƶ�����")]
    public float moveSpeed = 8f;

    [Header("������ο�")]
    public Camera player1Camera;
    public Camera player2Camera;

    [Header("2D Sprite����")]
    public bool use2DSpriteMode = true;
    [Range(0f, 360f)]
    public float player1SpriteOffset = 0f;
    [Range(0f, 360f)]
    public float player2SpriteOffset = 0f;

    [Header("Sprite��תģʽ")]
    public SpriteRotationMode rotationMode = SpriteRotationMode.Billboard;

    [Header("ƽ������")]
    public float spriteRotationSmoothTime = 0.1f;

    [Header("�ƶ���������")]
    public bool enableMovementLock = true; // �Ƿ������ƶ���������

    [Header("����ѡ��")]
    public bool showDebugInfo = false;

    private Rigidbody rb1, rb2;
    private Vector3 movement1, movement2;
    private Transform player1Visual, player2Visual;
    private PlayerCamera player1CameraController, player2CameraController;

    // �ƶ�����״̬
    private bool isMovementLocked = false;

    // Sprite��ת��ƽ������
    private float player1VisualVelocity;
    private float player2VisualVelocity;

    // Sprite��תģʽö��
    public enum SpriteRotationMode
    {
        Billboard,      // ʼ�����������
        FixedAngle,     // �̶��Ƕ�
        MovementBased   // �����ƶ�����
    }

    void Start()
    {
        InitializePlayers();
        SetupCameraListeners();
        Debug.Log("˫�˿�������ʼ����� - 2D Spriteģʽ");
    }

    void InitializePlayers()
    {
        // ��ʼ�����1
        if (player1 != null)
        {
            rb1 = player1.GetComponent<Rigidbody>();
            if (rb1 != null) rb1.freezeRotation = true;

            player1Visual = FindVisualTransform(player1);
        }

        // ��ʼ�����2
        if (player2 != null)
        {
            rb2 = player2.GetComponent<Rigidbody>();
            if (rb2 != null) rb2.freezeRotation = true;

            player2Visual = FindVisualTransform(player2);
        }
    }

    void SetupCameraListeners()
    {
        // ��ȡ����������������ü���
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
        // ����SpriteRenderer
        SpriteRenderer spriteRenderer = playerObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            return spriteRenderer.transform;
        }

        // �����������е�SpriteRenderer
        SpriteRenderer[] childSprites = playerObject.GetComponentsInChildren<SpriteRenderer>();
        if (childSprites.Length > 0)
        {
            return childSprites[0].transform;
        }

        // �����û�У����ض�����
        return playerObject.transform;
    }

    void Update()
    {
        // ���UI���������룬��ֹͣ��Ӧ�ƶ�
        var ui = FindObjectOfType<scene1UI_Manager>();
        if (ui != null && ui.noInputAnything)
        {
            // ����F/H��������Ӧ
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.H))
            {
                // �������ﴥ����ӦUI�߼�
            }
            return;
        }

        // ����ƶ��Ƿ�����
        if (!isMovementLocked)
        {
            GetPlayerInput();
        }
        else
        {
            // �ƶ�������ʱ�������
            movement1 = Vector3.zero;
            movement2 = Vector3.zero;

            if (showDebugInfo && Time.frameCount % 60 == 0)
            {
                Debug.Log("�ƶ������� - ֻ��ʹ��F/H���ƽ��Ի�");
            }
        }

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
        // ���1���� (WASD)
        float h1 = 0f, v1 = 0f;
        if (Input.GetKey(KeyCode.D)) h1 += 1f;
        if (Input.GetKey(KeyCode.A)) h1 -= 1f;
        if (Input.GetKey(KeyCode.W)) v1 += 1f;
        if (Input.GetKey(KeyCode.S)) v1 -= 1f;
        movement1 = new Vector3(h1, 0f, v1).normalized;

        // ���2���� (IJKL)
        float h2 = 0f, v2 = 0f;
        if (Input.GetKey(KeyCode.L)) h2 += 1f;
        if (Input.GetKey(KeyCode.J)) h2 -= 1f;
        if (Input.GetKey(KeyCode.I)) v2 += 1f;
        if (Input.GetKey(KeyCode.K)) v2 -= 1f;
        movement2 = new Vector3(h2, 0f, v2).normalized;
    }

    void MovePlayers()
    {
        // �ƶ����1
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

        // �ƶ����2
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
        // 2D Sprite Billboard��ʼ�����������
        Vector3 toCamera = camera.transform.position - sprite.position;
        toCamera.y = 0; // ����ˮƽ��ת

        if (toCamera != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-toCamera) * Quaternion.Euler(0, offset, 0);
            sprite.rotation = Quaternion.Slerp(sprite.rotation, targetRotation, spriteRotationSmoothTime);
        }
    }

    void UpdateFixedRotation(Transform sprite, float offset)
    {
        // �̶��Ƕ���ת
        Quaternion targetRotation = Quaternion.Euler(0, offset, 0);
        sprite.rotation = Quaternion.Slerp(sprite.rotation, targetRotation, spriteRotationSmoothTime);
    }

    void UpdateMovementBasedRotation(Transform sprite, Camera camera, Vector3 movement, float offset)
    {
        // �����ƶ��������ת
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
            // û���ƶ�ʱ�������������
            UpdateBillboardRotation(sprite, camera, offset);
        }
    }

    // �������ת�¼�����
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

    /// <summary>
    /// �����ƶ�����״̬
    /// ���Ի�����ʱ���ô˷��������ƶ������������������
    /// </summary>
    /// <param name="locked">true=�����ƶ���false=�����ƶ�</param>
    public void SetMovementLock(bool locked)
    {
        if (!enableMovementLock) return;

        isMovementLocked = locked;

        // �����ƶ�ʱ����ֹͣ����ٶ�
        if (locked)
        {
            if (rb1 != null)
            {
                rb1.velocity = new Vector3(0f, rb1.velocity.y, 0f);
            }
            if (rb2 != null)
            {
                rb2.velocity = new Vector3(0f, rb2.velocity.y, 0f);
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"DualPlayerController �ƶ� {(locked ? "����" : "����")}");
        }
    }

    /// <summary>
    /// ��ȡ��ǰ�ƶ�����״̬
    /// </summary>
    /// <returns>true=�ƶ���������false=�ƶ�����</returns>
    public bool IsMovementLocked()
    {
        return isMovementLocked;
    }

    /// <summary>
    /// ��ȡ���1�����뷽�򣨹�����������ʹ�ã�
    /// </summary>
    public Vector3 GetPlayer1InputDirection()
    {
        return movement1;
    }
 
    /// ��ȡ���2�����뷽�򣨹�����������ʹ�ã�

    public Vector3 GetPlayer2InputDirection()
    {
        return movement2;
    }
    /// <summary>
    /// ��ȡ���1�������
    /// </summary>
    public Camera GetPlayer1Camera()
    {
        return player1Camera;
    }

    /// <summary>
    /// ��ȡ���2�������
    /// </summary>
    public Camera GetPlayer2Camera()
    {
        return player2Camera;
    }
    void DebugRotationState()
    {
        Debug.Log("=== 2D Sprite��ת״̬ ===");
        Debug.Log($"��תģʽ: {rotationMode}");
        Debug.Log($"�ƶ�����״̬: {isMovementLocked}");
        if (player1Visual != null)
        {
            Debug.Log($"���1 - ��ת: {player1Visual.rotation.eulerAngles}");
        }
        if (player2Visual != null)
        {
            Debug.Log($"���2 - ��ת: {player2Visual.rotation.eulerAngles}");
        }
        Debug.Log("=======================");
    }

    // �����¼�����
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