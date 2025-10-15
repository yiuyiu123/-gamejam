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
    //[Header("��Ҷ���")]
    //public GameObject player1;
    //public GameObject player2;

    //[Header("�ƶ�����")]
    //public float moveSpeed = 8f;

    //[Header("������ο�")]
    //public Camera player1Camera;
    //public Camera player2Camera;

    //[Header("2D Sprite����")]
    //public bool use2DSpriteMode = true;
    //[Range(0f, 360f)]
    //public float player1SpriteOffset = 0f;
    //[Range(0f, 360f)]
    //public float player2SpriteOffset = 0f;

    //[Header("Sprite��תģʽ")]
    //public SpriteRotationMode rotationMode = SpriteRotationMode.Billboard;

    //[Header("ƽ������")]
    //public float spriteRotationSmoothTime = 0.1f;

    //[Header("����ѡ��")]
    //public bool showDebugInfo = false;

    //private Rigidbody rb1, rb2;
    //private Vector3 movement1, movement2;
    //private Transform player1Visual, player2Visual;
    //private PlayerCamera player1CameraController, player2CameraController;

    //// Sprite��ת��ƽ������
    //private float player1VisualVelocity;
    //private float player2VisualVelocity;

    //// Sprite��תģʽö��
    //public enum SpriteRotationMode
    //{
    //    Billboard,      // ʼ�����������
    //    FixedAngle,     // �̶��Ƕ�
    //    MovementBased   // �����ƶ�����
    //}

    //void Start()
    //{
    //    InitializePlayers();
    //    SetupCameraListeners();
    //    Debug.Log("˫�˿�������ʼ����� - �°�������");
    //    Debug.Log("���1: WS�ƶ�, AD�ӽ�ת��");
    //    Debug.Log("���2: IK�ƶ�, JL�ӽ�ת��");
    //}

    //void InitializePlayers()
    //{
    //    // ��ʼ�����1
    //    if (player1 != null)
    //    {
    //        rb1 = player1.GetComponent<Rigidbody>();
    //        if (rb1 != null) rb1.freezeRotation = true;

    //        player1Visual = FindVisualTransform(player1);
    //    }

    //    // ��ʼ�����2
    //    if (player2 != null)
    //    {
    //        rb2 = player2.GetComponent<Rigidbody>();
    //        if (rb2 != null) rb2.freezeRotation = true;

    //        player2Visual = FindVisualTransform(player2);
    //    }
    //}

    //void SetupCameraListeners()
    //{
    //    // ��ȡ����������������ü���
    //    if (player1Camera != null)
    //    {
    //        player1CameraController = player1Camera.GetComponent<PlayerCamera>();
    //        if (player1CameraController != null)
    //        {
    //            player1CameraController.OnCameraRotated += OnPlayer1CameraRotated;
    //            //// �������1���ӽǿ��ư���
    //            //player1CameraController.rotateLeftKey = KeyCode.D;
    //            //player1CameraController.rotateRightKey = KeyCode.A;
    //            // ȷ�������Զ�ת��
    //            player1CameraController.enableAutoRotation = false;
    //        }
    //    }

    //    if (player2Camera != null)
    //    {
    //        player2CameraController = player2Camera.GetComponent<PlayerCamera>();
    //        if (player2CameraController != null)
    //        {
    //            player2CameraController.OnCameraRotated += OnPlayer2CameraRotated;
    //            //// �������2���ӽǿ��ư���
    //            //player2CameraController.rotateLeftKey = KeyCode.L;
    //            //player2CameraController.rotateRightKey = KeyCode.J;
    //            // ȷ�������Զ�ת��
    //            player2CameraController.enableAutoRotation = false;
    //        }
    //    }
    //}

    //Transform FindVisualTransform(GameObject playerObject)
    //{
    //    // ����SpriteRenderer
    //    SpriteRenderer spriteRenderer = playerObject.GetComponent<SpriteRenderer>();
    //    if (spriteRenderer != null)
    //    {
    //        return spriteRenderer.transform;
    //    }

    //    // �����������е�SpriteRenderer
    //    SpriteRenderer[] childSprites = playerObject.GetComponentsInChildren<SpriteRenderer>();
    //    if (childSprites.Length > 0)
    //    {
    //        return childSprites[0].transform;
    //    }

    //    // �����û�У����ض�����
    //    return playerObject.transform;
    //}

    //void Update()
    //{
    //    GetPlayerInput();

    //    if (use2DSpriteMode)
    //    {
    //        UpdateSpriteRotations();
    //    }

    //    if (showDebugInfo && Input.GetKeyDown(KeyCode.F1))
    //    {
    //        DebugRotationState();
    //    }

    //    // ���ԣ���ʾ��ǰ����״̬
    //    if (showDebugInfo && Time.frameCount % 60 == 0)
    //    {
    //        Debug.Log($"���1�ƶ�����: {movement1}, ���2�ƶ�����: {movement2}");
    //    }
    //}

    //void FixedUpdate()
    //{
    //    MovePlayers();
    //}

    //void GetPlayerInput()
    //{
    //    // ���1���� - ֻ���WS�������ƶ�
    //    float v1 = 0f;
    //    if (Input.GetKey(KeyCode.W)) v1 += 1f;  // ǰ��
    //    if (Input.GetKey(KeyCode.S)) v1 -= 1f;  // ����
    //    // ע�⣺AD��������ȫ�����ӽǿ��ƣ��������ƶ�
    //    movement1 = new Vector3(0f, 0f, v1).normalized;

    //    // ���2���� - ֻ���IK�������ƶ�
    //    float v2 = 0f;
    //    if (Input.GetKey(KeyCode.I)) v2 += 1f;  // ǰ��
    //    if (Input.GetKey(KeyCode.K)) v2 -= 1f;  // ����
    //    // ע�⣺JL��������ȫ�����ӽǿ��ƣ��������ƶ�
    //    movement2 = new Vector3(0f, 0f, v2).normalized;

    //    // ���ԣ������⵽AD/JL�������£���¼����
    //    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) ||
    //        Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.L))
    //    {
    //        if (showDebugInfo)
    //        {
    //            Debug.Log("��⵽�ӽǿ��ư������£���Щ��������Ӱ���ƶ�");
    //        }
    //    }
    //}

    //void MovePlayers()
    //{
    //    // �ƶ����1
    //    if (rb1 != null)
    //    {
    //        if (movement1.magnitude > 0.1f)
    //        {
    //            Vector3 moveDirection = GetCameraRelativeDirection(movement1, player1Camera);
    //            Vector3 moveVelocity = moveDirection * moveSpeed;
    //            rb1.velocity = new Vector3(moveVelocity.x, rb1.velocity.y, moveVelocity.z);

    //            if (showDebugInfo)
    //            {
    //                Debug.Log($"���1�ƶ�����: {moveDirection}, �ٶ�: {moveVelocity}");
    //            }
    //        }
    //        else
    //        {
    //            // ֻ��X��Z����㣬����Y������
    //            rb1.velocity = new Vector3(0f, rb1.velocity.y, 0f);
    //        }
    //    }

    //    // �ƶ����2
    //    if (rb2 != null)
    //    {
    //        if (movement2.magnitude > 0.1f)
    //        {
    //            Vector3 moveDirection = GetCameraRelativeDirection(movement2, player2Camera);
    //            Vector3 moveVelocity = moveDirection * moveSpeed;
    //            rb2.velocity = new Vector3(moveVelocity.x, rb2.velocity.y, moveVelocity.z);

    //            if (showDebugInfo)
    //            {
    //                Debug.Log($"���2�ƶ�����: {moveDirection}, �ٶ�: {moveVelocity}");
    //            }
    //        }
    //        else
    //        {
    //            // ֻ��X��Z����㣬����Y������
    //            rb2.velocity = new Vector3(0f, rb2.velocity.y, 0f);
    //        }
    //    }
    //}

    //void UpdateSpriteRotations()
    //{
    //    if (player1Visual != null)
    //    {
    //        UpdateSingleSpriteRotation(player1Visual, player1Camera, movement1, player1SpriteOffset);
    //    }

    //    if (player2Visual != null)
    //    {
    //        UpdateSingleSpriteRotation(player2Visual, player2Camera, movement2, player2SpriteOffset);
    //    }
    //}

    //void UpdateSingleSpriteRotation(Transform sprite, Camera camera, Vector3 movement, float offset)
    //{
    //    if (sprite == null || camera == null) return;

    //    switch (rotationMode)
    //    {
    //        case SpriteRotationMode.Billboard:
    //            UpdateBillboardRotation(sprite, camera, offset);
    //            break;

    //        case SpriteRotationMode.FixedAngle:
    //            UpdateFixedRotation(sprite, offset);
    //            break;

    //        case SpriteRotationMode.MovementBased:
    //            UpdateMovementBasedRotation(sprite, camera, movement, offset);
    //            break;
    //    }
    //}

    //void UpdateBillboardRotation(Transform sprite, Camera camera, float offset)
    //{
    //    // 2D Sprite Billboard��ʼ�����������
    //    Vector3 toCamera = camera.transform.position - sprite.position;
    //    toCamera.y = 0; // ����ˮƽ��ת

    //    if (toCamera != Vector3.zero)
    //    {
    //        Quaternion targetRotation = Quaternion.LookRotation(-toCamera) * Quaternion.Euler(0, offset, 0);
    //        sprite.rotation = Quaternion.Slerp(sprite.rotation, targetRotation, spriteRotationSmoothTime);
    //    }
    //}

    //void UpdateFixedRotation(Transform sprite, float offset)
    //{
    //    // �̶��Ƕ���ת
    //    Quaternion targetRotation = Quaternion.Euler(0, offset, 0);
    //    sprite.rotation = Quaternion.Slerp(sprite.rotation, targetRotation, spriteRotationSmoothTime);
    //}

    //void UpdateMovementBasedRotation(Transform sprite, Camera camera, Vector3 movement, float offset)
    //{
    //    // �����ƶ��������ת
    //    if (movement.magnitude > 0.1f)
    //    {
    //        Vector3 moveDirection = GetCameraRelativeDirection(movement, camera);
    //        if (moveDirection != Vector3.zero)
    //        {
    //            Quaternion targetRotation = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(0, offset, 0);
    //            sprite.rotation = Quaternion.Slerp(sprite.rotation, targetRotation, 10f * Time.deltaTime);
    //        }
    //    }
    //    else
    //    {
    //        // û���ƶ�ʱ�������������
    //        UpdateBillboardRotation(sprite, camera, offset);
    //    }
    //}

    //// �������ת�¼�����
    //void OnPlayer1CameraRotated(float angle)
    //{
    //    if (player1Visual != null && use2DSpriteMode && rotationMode == SpriteRotationMode.Billboard)
    //    {
    //        UpdateBillboardRotation(player1Visual, player1Camera, player1SpriteOffset);
    //    }
    //}

    //void OnPlayer2CameraRotated(float angle)
    //{
    //    if (player2Visual != null && use2DSpriteMode && rotationMode == SpriteRotationMode.Billboard)
    //    {
    //        UpdateBillboardRotation(player2Visual, player2Camera, player2SpriteOffset);
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
    //    Debug.Log("=== 2D Sprite��ת״̬ ===");
    //    Debug.Log($"��תģʽ: {rotationMode}");
    //    if (player1Visual != null)
    //    {
    //        Debug.Log($"���1 - ��ת: {player1Visual.rotation.eulerAngles}");
    //    }
    //    if (player2Visual != null)
    //    {
    //        Debug.Log($"���2 - ��ת: {player2Visual.rotation.eulerAngles}");
    //    }
    //    Debug.Log("=======================");
    //}

    //// �����������ֶ���鰴������
    //public void CheckKeyConfiguration()
    //{
    //    Debug.Log("=== �������ü�� ===");
    //    Debug.Log($"���1�ƶ�: W/S");
    //    Debug.Log($"���1�ӽ�: A/D");
    //    Debug.Log($"���2�ƶ�: I/K");
    //    Debug.Log($"���2�ӽ�: J/L");

    //    if (player1CameraController != null)
    //    {
    //        Debug.Log($"���1�������ת��: {player1CameraController.rotateLeftKey}");
    //        Debug.Log($"���1�������ת��: {player1CameraController.rotateRightKey}");
    //    }
    //    if (player2CameraController != null)
    //    {
    //        Debug.Log($"���2�������ת��: {player2CameraController.rotateLeftKey}");
    //        Debug.Log($"���2�������ת��: {player2CameraController.rotateRightKey}");
    //    }
    //    Debug.Log("===================");
    //}

    //// �����¼�����
    //void OnDestroy()
    //{
    //    if (player1CameraController != null)
    //    {
    //        player1CameraController.OnCameraRotated -= OnPlayer1CameraRotated;
    //    }
    //    if (player2CameraController != null)
    //    {
    //        player2CameraController.OnCameraRotated -= OnPlayer2CameraRotated;
    //    }
    //}
}
