using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualPlayerController : MonoBehaviour
{
    //[Header("��Ҷ���")]
    //public GameObject player1;
    //public GameObject player2;

    //[Header("�ƶ�����")]
    //public float moveSpeed = 8f;

    //[Header("������ο�")]
    //public Camera player1Camera;
    //public Camera player2Camera;

    //[Header("����ѡ��")]
    //public bool showInputDebug = false;

    //private Rigidbody rb1, rb2;
    //private Vector3 movement1, movement2;

    //void Start()
    //{
    //    // ��ʼ�����1
    //    if (player1 != null)
    //    {
    //        rb1 = player1.GetComponent<Rigidbody>();
    //        if (rb1 != null) rb1.freezeRotation = true;
    //    }

    //    // ��ʼ�����2
    //    if (player2 != null)
    //    {
    //        rb2 = player2.GetComponent<Rigidbody>();
    //        if (rb2 != null) rb2.freezeRotation = true;
    //    }

    //    Debug.Log("˫�˿�������ʼ�����");
    //    Debug.Log("���1����: WASD");
    //    Debug.Log("���2����: IJKL");
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
    //    // ���1���� (WASD)
    //    float h1 = 0f, v1 = 0f;
    //    if (Input.GetKey(KeyCode.D)) h1 += 1f;
    //    if (Input.GetKey(KeyCode.A)) h1 -= 1f;
    //    if (Input.GetKey(KeyCode.W)) v1 += 1f;
    //    if (Input.GetKey(KeyCode.S)) v1 -= 1f;
    //    movement1 = new Vector3(h1, 0f, v1).normalized;

    //    // ���2���� (IJKL)
    //    float h2 = 0f, v2 = 0f;
    //    if (Input.GetKey(KeyCode.L)) h2 += 1f;
    //    if (Input.GetKey(KeyCode.J)) h2 -= 1f;
    //    if (Input.GetKey(KeyCode.I)) v2 += 1f;
    //    if (Input.GetKey(KeyCode.K)) v2 -= 1f;
    //    movement2 = new Vector3(h2, 0f, v2).normalized;

    //    // ������Ϣ
    //    if (showInputDebug)
    //    {
    //        if (movement1.magnitude > 0.1f) Debug.Log($"���1����: {movement1}");
    //        if (movement2.magnitude > 0.1f) Debug.Log($"���2����: {movement2}");
    //    }
    //}

    //void MovePlayers()
    //{
    //    // �ƶ����1
    //    if (rb1 != null && movement1.magnitude > 0.1f)
    //    {
    //        Vector3 moveDirection = GetCameraRelativeDirection(movement1, player1Camera);
    //        Vector3 moveVelocity = moveDirection * moveSpeed;
    //        rb1.velocity = new Vector3(moveVelocity.x, rb1.velocity.y, moveVelocity.z);

    //        // �����ƶ�����
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

    //    // �ƶ����2
    //    if (rb2 != null && movement2.magnitude > 0.1f)
    //    {
    //        Vector3 moveDirection = GetCameraRelativeDirection(movement2, player2Camera);
    //        Vector3 moveVelocity = moveDirection * moveSpeed;
    //        rb2.velocity = new Vector3(moveVelocity.x, rb2.velocity.y, moveVelocity.z);

    //        // �����ƶ�����
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

    //    // ��ȡ�������ǰ������ҷ��򣨺���Y�ᣩ
    //    Vector3 cameraForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
    //    Vector3 cameraRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;

    //    // �����뷽��ת��Ϊ������ռ䷽��
    //    return cameraForward * inputDirection.z + cameraRight * inputDirection.x;
    //}

    //[Header("��Ҷ���")]
    //public GameObject player1;
    //public GameObject player2;

    //[Header("�ƶ�����")]
    //public float moveSpeed = 8f;

    //[Header("������ο�")]
    //public Camera player1Camera;
    //public Camera player2Camera;

    //[Header("2.5D��ͼ����")]
    //public bool useBillboardFacing = true;
    //[Range(0f, 360f)]
    //public float player1RotationOffset = 0f;
    //[Range(0f, 360f)]
    //public float player2RotationOffset = 0f;

    //[Header("��ͼ��������")]
    //public float standUpXAngle = 90f; // X������Ƕȣ�Ĭ��Ϊ90��

    //[Header("����ѡ��")]
    //public bool showDebugInfo = false;
    //public KeyCode debugKey = KeyCode.F1;

    //private Rigidbody rb1, rb2;
    //private Vector3 movement1, movement2;
    //private Transform player1Visual, player2Visual;

    //void Start()
    //{
    //    InitializePlayers();
    //    Debug.Log("˫�˿�������ʼ����� - ��ͼ����ģʽ");
    //}

    //void InitializePlayers()
    //{
    //    // ��ʼ�����1
    //    if (player1 != null)
    //    {
    //        rb1 = player1.GetComponent<Rigidbody>();
    //        if (rb1 != null) rb1.freezeRotation = true;

    //        player1Visual = FindVisualTransform(player1);
    //        if (player1Visual != null)
    //        {
    //            ResetPlayerRotation(player1Visual, player1Camera, player1RotationOffset);
    //            Debug.Log($"���1�Ӿ������ҵ�: {player1Visual.name}");
    //        }
    //    }

    //    // ��ʼ�����2
    //    if (player2 != null)
    //    {
    //        rb2 = player2.GetComponent<Rigidbody>();
    //        if (rb2 != null) rb2.freezeRotation = true;

    //        player2Visual = FindVisualTransform(player2);
    //        if (player2Visual != null)
    //        {
    //            ResetPlayerRotation(player2Visual, player2Camera, player2RotationOffset);
    //            Debug.Log($"���2�Ӿ������ҵ�: {player2Visual.name}");
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

    //    // ��������ͼ��������X����ת90�ȣ�
    //    visual.rotation = Quaternion.Euler(standUpXAngle, 0, 0);

    //    // Ȼ���������������ķ���
    //    Vector3 toCamera = camera.transform.position - visual.position;
    //    toCamera.y = 0;

    //    if (toCamera != Vector3.zero)
    //    {
    //        // �����ת�����������������������
    //        Quaternion standUpRotation = Quaternion.Euler(standUpXAngle, 0, 0);
    //        Quaternion facingRotation = Quaternion.LookRotation(-toCamera) * Quaternion.Euler(0, offset, 0);

    //        // Ӧ�������ת
    //        visual.rotation = facingRotation * standUpRotation;

    //        if (showDebugInfo)
    //        {
    //            Debug.Log($"������ת: {visual.name}, ������ת={visual.rotation.eulerAngles}");
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

    //    // ���Թ��ܣ�ʵʱ��������Ƕ�
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
    //    // ���1���� (WASD)
    //    float h1 = 0f, v1 = 0f;
    //    if (Input.GetKey(KeyCode.D)) h1 += 1f;
    //    if (Input.GetKey(KeyCode.A)) h1 -= 1f;
    //    if (Input.GetKey(KeyCode.W)) v1 += 1f;
    //    if (Input.GetKey(KeyCode.S)) v1 -= 1f;
    //    movement1 = new Vector3(h1, 0f, v1).normalized;

    //    // ���2���� (IJKL)
    //    float h2 = 0f, v2 = 0f;
    //    if (Input.GetKey(KeyCode.L)) h2 += 1f;
    //    if (Input.GetKey(KeyCode.J)) h2 -= 1f;
    //    if (Input.GetKey(KeyCode.I)) v2 += 1f;
    //    if (Input.GetKey(KeyCode.K)) v2 -= 1f;
    //    movement2 = new Vector3(h2, 0f, v2).normalized;
    //}

    //void MovePlayers()
    //{
    //    // �ƶ����1
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

    //    // �ƶ����2
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
    //        // �����ת������ + ���������
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
    //    Debug.Log("=== ��ת״̬���� ===");
    //    Debug.Log($"����Ƕ�: {standUpXAngle}��");
    //    if (player1Visual != null)
    //    {
    //        Debug.Log($"���1 - ��ת: {player1Visual.rotation.eulerAngles}");
    //    }
    //    if (player2Visual != null)
    //    {
    //        Debug.Log($"���2 - ��ת: {player2Visual.rotation.eulerAngles}");
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
    //    Debug.Log($"������ͼ����Ƕȸ���Ϊ: {standUpXAngle}��");
    //}

    //// �����������ֶ�������ת
    //public void FixAllRotations()
    //{
    //    ResetAllRotations();
    //}
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

    [Header("����ѡ��")]
    public bool showDebugInfo = false;

    private Rigidbody rb1, rb2;
    private Vector3 movement1, movement2;
    private Transform player1Visual, player2Visual;
    private PlayerCamera player1CameraController, player2CameraController;

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

    void DebugRotationState()
    {
        Debug.Log("=== 2D Sprite��ת״̬ ===");
        Debug.Log($"��תģʽ: {rotationMode}");
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
