using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplitScreenManager : MonoBehaviour
{
    //#region ��ɫ��ע��
    ////[Header("�������")]
    ////public Transform player1;
    ////public Transform player2;

    ////[Header("���������")]
    ////public Camera camera1;
    ////public Camera camera2;

    ////[Header("����ģʽ")]
    ////public bool horizontalSplit = true; // ˮƽ����

    ////[Header("���������")]
    ////public float cameraHeight = 10f;
    ////public float cameraDistance = 8f;
    ////public float rotationSpeed = 90f; // ���ӵ�90

    ////[Header("��ʼ�Ƕ�����")]
    ////public float player1StartAngle = 45f;
    ////public float player2StartAngle = 45f;

    ////private PlayerCamera player1Camera;
    ////private PlayerCamera player2Camera;

    ////void Start()
    ////{
    ////    SetupCameras();
    ////    SetupSplitScreen();
    ////    InitializeCameras();
    ////}

    ////void SetupCameras()
    ////{
    ////    // ȷ�����������
    ////    if (camera1 == null || camera2 == null)
    ////    {
    ////        CreateCameras();
    ////    }

    ////    // ��ȡ����������������
    ////    player1Camera = camera1.GetComponent<PlayerCamera>();
    ////    if (player1Camera == null)
    ////    {
    ////        player1Camera = camera1.gameObject.AddComponent<PlayerCamera>();
    ////    }

    ////    player2Camera = camera2.GetComponent<PlayerCamera>();
    ////    if (player2Camera == null)
    ////    {
    ////        player2Camera = camera2.gameObject.AddComponent<PlayerCamera>();
    ////    }
    ////}

    ////void CreateCameras()
    ////{
    ////    // �������1�����
    ////    if (camera1 == null)
    ////    {
    ////        GameObject cam1Obj = new GameObject("Player1Camera");
    ////        camera1 = cam1Obj.AddComponent<Camera>();
    ////        camera1.tag = "Untagged"; // �����Ϊ�������
    ////    }

    ////    // �������2�����
    ////    if (camera2 == null)
    ////    {
    ////        GameObject cam2Obj = new GameObject("Player2Camera");
    ////        camera2 = cam2Obj.AddComponent<Camera>();
    ////        camera2.tag = "Untagged"; // �����Ϊ�������
    ////    }
    ////}

    ////void InitializeCameras()
    ////{
    ////    // �������1���������
    ////    if (player1Camera != null)
    ////    {
    ////        player1Camera.height = cameraHeight;
    ////        player1Camera.distance = cameraDistance;
    ////        player1Camera.rotationSpeed = rotationSpeed;
    ////        player1Camera.rotateLeftKey = KeyCode.Q;
    ////        player1Camera.rotateRightKey = KeyCode.E;
    ////        player1Camera.currentAngle = player1StartAngle;

    ////        // ����Ŀ�겢������׼
    ////        if (player1 != null)
    ////        {
    ////            player1Camera.SetTarget(player1);
    ////            player1Camera.SnapToTarget(); // ������׼
    ////        }
    ////    }

    ////    // �������2���������
    ////    if (player2Camera != null)
    ////    {
    ////        player2Camera.height = cameraHeight;
    ////        player2Camera.distance = cameraDistance;
    ////        player2Camera.rotationSpeed = rotationSpeed;
    ////        player2Camera.rotateLeftKey = KeyCode.U;
    ////        player2Camera.rotateRightKey = KeyCode.O;
    ////        player2Camera.currentAngle = player2StartAngle;

    ////        // ����Ŀ�겢������׼
    ////        if (player2 != null)
    ////        {
    ////            player2Camera.SetTarget(player2);
    ////            player2Camera.SnapToTarget(); // ������׼
    ////        }
    ////    }

    ////    Debug.Log($"�����������ʼ����� - ���1�Ƕ�: {player1StartAngle}��, ���2�Ƕ�: {player2StartAngle}��");
    ////}

    ////void SetupSplitScreen()
    ////{
    ////    if (horizontalSplit)
    ////    {
    ////        // ˮƽ�������������1���������2
    ////        camera1.rect = new Rect(0f, 0f, 0.5f, 1f);
    ////        camera2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
    ////    }
    ////    else
    ////    {
    ////        // ��ֱ�������������1���������2
    ////        camera1.rect = new Rect(0f, 0.5f, 1f, 0.5f);
    ////        camera2.rect = new Rect(0f, 0f, 1f, 0.5f);
    ////    }

    ////    // ȷ�����������������
    ////    camera1.enabled = true;
    ////    camera2.enabled = true;
    ////}

    ////void Update()
    ////{
    ////    // ���Թ��ܣ���R�����������λ��
    ////    if (Input.GetKeyDown(KeyCode.R))
    ////    {
    ////        ResetCameraPositions();
    ////    }
    ////}

    ////// �������������������λ��
    ////public void ResetCameraPositions()
    ////{
    ////    if (player1Camera != null && player1 != null)
    ////    {
    ////        player1Camera.SetCameraAngle(player1StartAngle);
    ////        player1Camera.SnapToTarget();
    ////    }

    ////    if (player2Camera != null && player2 != null)
    ////    {
    ////        player2Camera.SetCameraAngle(player2StartAngle);
    ////        player2Camera.SnapToTarget();
    ////    }

    ////    Debug.Log("�����λ��������");
    ////}

    ////// �����������ֶ�����������Ƕ�
    ////public void SetCameraAngles(float angle1, float angle2)
    ////{
    ////    if (player1Camera != null)
    ////    {
    ////        player1Camera.SetCameraAngle(angle1);
    ////    }

    ////    if (player2Camera != null)
    ////    {
    ////        player2Camera.SetCameraAngle(angle2);
    ////    }
    ////}

    ////// ������������������״̬
    ////public void CheckCameraStatus()
    ////{
    ////    Debug.Log("=== �����״̬��� ===");
    ////    Debug.Log($"���1�����: {(player1Camera != null ? "����" : "ȱʧ")}, Ŀ��: {(player1Camera?.target != null ? player1Camera.target.name : "��")}");
    ////    Debug.Log($"���2�����: {(player2Camera != null ? "����" : "ȱʧ")}, Ŀ��: {(player2Camera?.target != null ? player2Camera.target.name : "��")}");
    ////    Debug.Log($"���1�Ƕ�: {player1Camera?.GetCurrentAngle()}, ���2�Ƕ�: {player2Camera?.GetCurrentAngle()}");
    ////    Debug.Log("=====================");
    ////}
    //#endregion
    //[Header("�������")]
    //public Transform player1;
    //public Transform player2;

    //[Header("���������")]
    //public Camera camera1;
    //public Camera camera2;

    //[Header("����ģʽ")]
    //public bool horizontalSplit = true; // ˮƽ����

    //[Header("���������")]
    //public float cameraHeight = 10f;
    //public float cameraDistance = 8f;
    //public float rotationSpeed = 90f;

    //[Header("��ʼ�Ƕ�����")]
    //public float player1StartAngle = 45f;
    //public float player2StartAngle = 45f;

    //private PlayerCamera player1Camera;
    //private PlayerCamera player2Camera;

    //void Start()
    //{
    //    SetupCameras();
    //    SetupSplitScreen();
    //    InitializeCameras();
    //}

    //void SetupCameras()
    //{
    //    // ȷ�����������
    //    if (camera1 == null || camera2 == null)
    //    {
    //        CreateCameras();
    //    }

    //    // ��ȡ����������������
    //    player1Camera = camera1.GetComponent<PlayerCamera>();
    //    if (player1Camera == null)
    //    {
    //        player1Camera = camera1.gameObject.AddComponent<PlayerCamera>();
    //    }

    //    player2Camera = camera2.GetComponent<PlayerCamera>();
    //    if (player2Camera == null)
    //    {
    //        player2Camera = camera2.gameObject.AddComponent<PlayerCamera>();
    //    }
    //}

    //void CreateCameras()
    //{
    //    // �������1�����
    //    if (camera1 == null)
    //    {
    //        GameObject cam1Obj = new GameObject("Player1Camera");
    //        camera1 = cam1Obj.AddComponent<Camera>();
    //        camera1.tag = "Untagged";
    //    }

    //    // �������2�����
    //    if (camera2 == null)
    //    {
    //        GameObject cam2Obj = new GameObject("Player2Camera");
    //        camera2 = cam2Obj.AddComponent<Camera>();
    //        camera2.tag = "Untagged";
    //    }
    //}

    //void InitializeCameras()
    //{
    //    // �������1��������� - �°�������
    //    if (player1Camera != null)
    //    {
    //        player1Camera.height = cameraHeight;
    //        player1Camera.distance = cameraDistance;
    //        player1Camera.rotationSpeed = rotationSpeed;
    //        //player1Camera.rotateLeftKey = KeyCode.D;  // ��ΪA��
    //        //player1Camera.rotateRightKey = KeyCode.A; // ��ΪD��
    //        player1Camera.currentAngle = player1StartAngle;

    //        // �����Զ�ת����Ϊ����ʹ��AD��ר�ſ����ӽ�
    //        player1Camera.enableAutoRotation = false;

    //        // ����Ŀ�겢������׼
    //        if (player1 != null)
    //        {
    //            player1Camera.SetTarget(player1);
    //            player1Camera.SnapToTarget();
    //        }
    //    }

    //    // �������2��������� - �°�������
    //    if (player2Camera != null)
    //    {
    //        player2Camera.height = cameraHeight;
    //        player2Camera.distance = cameraDistance;
    //        player2Camera.rotationSpeed = rotationSpeed;
    //        //player2Camera.rotateLeftKey = KeyCode.L;  // ��ΪJ��
    //        //player2Camera.rotateRightKey = KeyCode.J; // ��ΪL��
    //        player2Camera.currentAngle = player2StartAngle;

    //        // �����Զ�ת��
    //        player2Camera.enableAutoRotation = false;

    //        // ����Ŀ�겢������׼
    //        if (player2 != null)
    //        {
    //            player2Camera.SetTarget(player2);
    //            player2Camera.SnapToTarget();
    //        }
    //    }

    //    Debug.Log($"�����������ʼ����� - �°�������");
    //    Debug.Log($"���1: AD�����ӽ�ת��");
    //    Debug.Log($"���2: JL�����ӽ�ת��");
    //}

    //void SetupSplitScreen()
    //{
    //    if (horizontalSplit)
    //    {
    //        // ˮƽ�������������1���������2
    //        camera1.rect = new Rect(0f, 0f, 0.5f, 1f);
    //        camera2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
    //    }
    //    else
    //    {
    //        // ��ֱ�������������1���������2
    //        camera1.rect = new Rect(0f, 0.5f, 1f, 0.5f);
    //        camera2.rect = new Rect(0f, 0f, 1f, 0.5f);
    //    }

    //    // ȷ�����������������
    //    camera1.enabled = true;
    //    camera2.enabled = true;
    //}

    //void Update()
    //{
    //    // ���Թ��ܣ���R�����������λ��
    //    if (Input.GetKeyDown(KeyCode.R))
    //    {
    //        ResetCameraPositions();
    //    }
    //}

    //// �������������������λ��
    //public void ResetCameraPositions()
    //{
    //    if (player1Camera != null && player1 != null)
    //    {
    //        player1Camera.SetCameraAngle(player1StartAngle);
    //        player1Camera.SnapToTarget();
    //    }

    //    if (player2Camera != null && player2 != null)
    //    {
    //        player2Camera.SetCameraAngle(player2StartAngle);
    //        player2Camera.SnapToTarget();
    //    }

    //    Debug.Log("�����λ��������");
    //}

    //// �����������ֶ�����������Ƕ�
    //public void SetCameraAngles(float angle1, float angle2)
    //{
    //    if (player1Camera != null)
    //    {
    //        player1Camera.SetCameraAngle(angle1);
    //    }

    //    if (player2Camera != null)
    //    {
    //        player2Camera.SetCameraAngle(angle2);
    //    }
    //}

    //// ������������������״̬
    //public void CheckCameraStatus()
    //{
    //    Debug.Log("=== �����״̬��� ===");
    //    Debug.Log($"���1�����: {(player1Camera != null ? "����" : "ȱʧ")}, Ŀ��: {(player1Camera?.target != null ? player1Camera.target.name : "��")}");
    //    Debug.Log($"���2�����: {(player2Camera != null ? "����" : "ȱʧ")}, Ŀ��: {(player2Camera?.target != null ? player2Camera.target.name : "��")}");
    //    Debug.Log($"���1�Ƕ�: {player1Camera?.GetCurrentAngle()}, ���2�Ƕ�: {player2Camera?.GetCurrentAngle()}");
    //    Debug.Log("=====================");
    //}
    [Header("�������")]
    public Transform player1;
    public Transform player2;

    [Header("��������� - ����ÿ���������Զ�����")]
    public Camera camera1;
    public Camera camera2;

    [Header("����ģʽ")]
    public bool horizontalSplit = true;

    [Header("���������")]
    public float cameraHeight = 10f;
    public float cameraDistance = 8f;
    public float rotationSpeed = 90f;

    [Header("��ʼ�Ƕ�����")]
    public float player1StartAngle = 45f;
    public float player2StartAngle = 45f;

    [Header("��Ӱ�޸�����")]
    public float shadowDistance = 100f;
    public float shadowBias = 0.05f;
    public float shadowNormalBias = 1.0f;
    public bool enableShadowFix = true;

    [Header("��������")]
    public bool destroyOtherCameras = true; // ���ٳ����е����������

    private PlayerCamera player1Camera;
    private PlayerCamera player2Camera;
    private bool isInitialized = false;

    // ����ģʽ
    public static SplitScreenManager Instance { get; private set; }

    void Awake()
    {
        // ����ģʽȷ��ֻ��һ������������
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeSystem();
    }

    void InitializeSystem()
    {
        Debug.Log("��ʼ��ʼ������ϵͳ...");

        // ������������ã�ǿ��ʹ�õ�ǰ�����������
        camera1 = null;
        camera2 = null;
        player1Camera = null;
        player2Camera = null;

        SetupCameras();
        SetupSplitScreen();
        InitializeCameras();
        ApplyShadowSettings();

        isInitialized = true;
        Debug.Log("������������ʼ�����");
    }

    void SetupCameras()
    {
        // ǿ�Ʋ��ҵ�ǰ�����е������
        FindCamerasInCurrentScene();

        // ����Ҳ��������������������ʾ
        if (camera1 == null)
        {
            Debug.LogError("�ڵ�ǰ�������Ҳ������1���������ȷ������������Ϊ'Player1Camera'�������");
        }

        if (camera2 == null)
        {
            Debug.LogError("�ڵ�ǰ�������Ҳ������2���������ȷ������������Ϊ'Player2Camera'�������");
        }

        // ������������
        SetupCameraComponents();
    }

    void FindCamerasInCurrentScene()
    {
        // ͨ�����Ʋ��ҵ�ǰ�����е������
        GameObject cam1Obj = GameObject.Find("Player1Camera");
        GameObject cam2Obj = GameObject.Find("Player2Camera");

        if (cam1Obj != null)
        {
            camera1 = cam1Obj.GetComponent<Camera>();
            if (camera1 == null)
            {
                Debug.LogWarning("Player1Camera����û��Camera������������...");
                camera1 = cam1Obj.AddComponent<Camera>();
            }
            Debug.Log($"�ҵ����1�����: {cam1Obj.name}");
        }

        if (cam2Obj != null)
        {
            camera2 = cam2Obj.GetComponent<Camera>();
            if (camera2 == null)
            {
                Debug.LogWarning("Player2Camera����û��Camera������������...");
                camera2 = cam2Obj.AddComponent<Camera>();
            }
            Debug.Log($"�ҵ����2�����: {cam2Obj.name}");
        }

        // ���ͨ�������Ҳ���������ͨ����ǩ����
        if (camera1 == null)
        {
            GameObject taggedCam1 = GameObject.FindGameObjectWithTag("Player1Camera");
            if (taggedCam1 != null)
            {
                camera1 = taggedCam1.GetComponent<Camera>();
                Debug.Log($"ͨ����ǩ�ҵ����1�����: {taggedCam1.name}");
            }
        }

        if (camera2 == null)
        {
            GameObject taggedCam2 = GameObject.FindGameObjectWithTag("Player2Camera");
            if (taggedCam2 != null)
            {
                camera2 = taggedCam2.GetComponent<Camera>();
                Debug.Log($"ͨ����ǩ�ҵ����2�����: {taggedCam2.name}");
            }
        }
    }

    void SetupCameraComponents()
    {
        if (camera1 != null)
        {
            player1Camera = camera1.GetComponent<PlayerCamera>();
            if (player1Camera == null)
            {
                player1Camera = camera1.gameObject.AddComponent<PlayerCamera>();
                Debug.Log("Ϊ���1��������PlayerCamera���");
            }
        }

        if (camera2 != null)
        {
            player2Camera = camera2.GetComponent<PlayerCamera>();
            if (player2Camera == null)
            {
                player2Camera = camera2.gameObject.AddComponent<PlayerCamera>();
                Debug.Log("Ϊ���2��������PlayerCamera���");
            }
        }
    }

    void InitializeCameras()
    {
        // �������1���������
        if (player1Camera != null)
        {
            player1Camera.height = cameraHeight;
            player1Camera.distance = cameraDistance;
            player1Camera.rotationSpeed = rotationSpeed;
            player1Camera.currentAngle = player1StartAngle;
            player1Camera.enableAutoRotation = false;

            // ����Ŀ�겢������׼
            if (player1 != null)
            {
                player1Camera.SetTarget(player1);
                player1Camera.SnapToTarget();
            }
            else
            {
                Debug.LogWarning("���1����Ϊ��");
            }
        }

        // �������2���������
        if (player2Camera != null)
        {
            player2Camera.height = cameraHeight;
            player2Camera.distance = cameraDistance;
            player2Camera.rotationSpeed = rotationSpeed;
            player2Camera.currentAngle = player2StartAngle;
            player2Camera.enableAutoRotation = false;

            // ����Ŀ�겢������׼
            if (player2 != null)
            {
                player2Camera.SetTarget(player2);
                player2Camera.SnapToTarget();
            }
            else
            {
                Debug.LogWarning("���2����Ϊ��");
            }
        }

        SetupCameraRendering();
    }

    void SetupCameraRendering()
    {
        if (camera1 != null)
        {
            camera1.allowHDR = false;
            camera1.allowMSAA = false;
            camera1.depthTextureMode = DepthTextureMode.Depth;
            camera1.depth = 0; // �������ȷ����Ⱦ˳��
        }

        if (camera2 != null)
        {
            camera2.allowHDR = false;
            camera2.allowMSAA = false;
            camera2.depthTextureMode = DepthTextureMode.Depth;
            camera2.depth = 1; // �������ȷ����Ⱦ˳��
        }
    }

    void SetupSplitScreen()
    {
        // �������������Ƿ���Ч
        if (camera1 == null || camera2 == null)
        {
            Debug.LogWarning("��������ö�ʧ�����²���...");
            SetupCameras();

            if (camera1 == null || camera2 == null)
            {
                Debug.LogError("�޷��ҵ���Ч����������ã�");
                return;
            }
        }

        // �����������������
        DisableOtherCameras();

        if (horizontalSplit)
        {
            camera1.rect = new Rect(0f, 0f, 0.5f, 1f);
            camera2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        }
        else
        {
            camera1.rect = new Rect(0f, 0.5f, 1f, 0.5f);
            camera2.rect = new Rect(0f, 0f, 1f, 0.5f);
        }

        camera1.enabled = true;
        camera2.enabled = true;

        Debug.Log($"�����������: ˮƽ����={horizontalSplit}");
    }

    void DisableOtherCameras()
    {
        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam != camera1 && cam != camera2 && cam.enabled)
            {
                cam.enabled = false;
                Debug.Log($"�������������: {cam.name}");
            }
        }
    }

    void ApplyShadowSettings()
    {
        if (!enableShadowFix) return;

        StartCoroutine(DelayedShadowFix());
    }

    IEnumerator DelayedShadowFix()
    {
        yield return null;

        QualitySettings.shadowDistance = shadowDistance;
        FixDirectionalLightShadows();

        if (camera1 != null) camera1.farClipPlane = Mathf.Max(camera1.farClipPlane, shadowDistance * 0.8f);
        if (camera2 != null) camera2.farClipPlane = Mathf.Max(camera2.farClipPlane, shadowDistance * 0.8f);

        DynamicGI.UpdateEnvironment();

        Debug.Log($"��Ӱ������Ӧ�� - ��Ӱ����: {shadowDistance}, ƫ��: {shadowBias}");
    }

    void FixDirectionalLightShadows()
    {
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional && light.shadows != LightShadows.None)
            {
                light.shadowBias = shadowBias;
                light.shadowNormalBias = shadowNormalBias;
                light.shadowStrength = 1.0f;

                Debug.Log($"�޸��������Ӱ: {light.name}, ƫ��: {light.shadowBias}");
            }
        }
    }

    void Update()
    {
        // ���Թ���
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCameraPositions();
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            ApplyShadowSettings();
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            ReinitializeSystem();
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isInitialized)
        {
            Debug.Log($"�����������: {scene.name}");
            Invoke("DelayedSceneFix", 0.2f);
        }
    }

    void DelayedSceneFix()
    {
        Debug.Log("ִ�г����޸�...");

        // �����������ã�ǿ��ʹ���³�������Դ
        camera1 = null;
        camera2 = null;
        player1Camera = null;
        player2Camera = null;
        player1 = null;
        player2 = null;

        // ���³�ʼ��ϵͳ
        InitializeSystem();

        Debug.Log("�����޸����");
    }

    void FindPlayers()
    {
        bool foundPlayer1 = false;
        bool foundPlayer2 = false;

        // ����ͨ�����Ʋ������
        if (player1 == null)
        {
            GameObject p1 = GameObject.Find("Player1");
            if (p1 != null)
            {
                player1 = p1.transform;
                foundPlayer1 = true;
                Debug.Log($"�ҵ����1: {p1.name}");
            }
        }

        if (player2 == null)
        {
            GameObject p2 = GameObject.Find("Player2");
            if (p2 != null)
            {
                player2 = p2.transform;
                foundPlayer2 = true;
                Debug.Log($"�ҵ����2: {p2.name}");
            }
        }

        // �������������Ŀ��
        if (player1Camera != null && player1 != null)
            player1Camera.SetTarget(player1);
        if (player2Camera != null && player2 != null)
            player2Camera.SetTarget(player2);

        Debug.Log($"��Ҳ��ҽ��: ���1={foundPlayer1}, ���2={foundPlayer2}");
    }

    // �������������������λ��
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

        Debug.Log("�����λ��������");
    }

    // �������������³�ʼ������ϵͳ
    [ContextMenu("���³�ʼ��ϵͳ")]
    public void ReinitializeSystem()
    {
        InitializeSystem();
    }

    // ������������������״̬
    [ContextMenu("��������״̬")]
    public void CheckCameraStatus()
    {
        Debug.Log("=== �����״̬��� ===");
        Debug.Log($"���1�����: {(camera1 != null ? camera1.name : "ȱʧ")}");
        Debug.Log($"���2�����: {(camera2 != null ? camera2.name : "ȱʧ")}");

        Camera[] allCameras = FindObjectsOfType<Camera>();
        Debug.Log($"�����������������: {allCameras.Length}");
        foreach (Camera cam in allCameras)
        {
            Debug.Log($"�����: {cam.name}, ����: {cam.enabled}, Rect: {cam.rect}");
        }
        Debug.Log("=====================");
    }

    // ����������ǿ���޸���Ӱ
    [ContextMenu("ǿ���޸���Ӱ")]
    public void ForceFixShadows()
    {
        ApplyShadowSettings();
    }

    void OnDestroy()
    {
        if (player1Camera != null)
            player1Camera.OnCameraRotated = null;
        if (player2Camera != null)
            player2Camera.OnCameraRotated = null;
    }
}
