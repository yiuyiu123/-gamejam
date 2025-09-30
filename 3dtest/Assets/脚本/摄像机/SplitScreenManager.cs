using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitScreenManager : MonoBehaviour
{
    //[Header("�������")]
    //public Transform player1;
    //public Transform player2;

    //[Header("���������")]
    //public Camera camera1;
    //public Camera camera2;

    //[Header("����ģʽ")]
    //public bool horizontalSplit = true; // ˮƽ����

    //[Header("���������")]
    //public float cameraHeight1 = 10f;
    //public float cameraDistance1 = 8f;
    //public float cameraHeight2 = 10f;
    //public float cameraDistance2 = 8f;
    //public float currentAngle1 = 8f;
    //public float currentAngle2 = 8f;
    //public float rotationSpeed = 2f;



    //private PlayerCamera player1Camera;
    //private PlayerCamera player2Camera;

    //void Start()
    //{
    //    SetupCameras();
    //    SetupSplitScreen();
    //}

    //void SetupCameras()
    //{
    //    // �������ȡ��������
    //    if (camera1 == null || camera2 == null)
    //    {
    //        CreateCameras();
    //    }

    //    //// ��������������
    //    //player1Camera = camera1.gameObject.AddComponent<PlayerCamera>();
    //    //player2Camera = camera2.gameObject.AddComponent<PlayerCamera>();

    //    // �������������
    //    SetCameraParameters();

    //    // ����Ŀ��
    //    player1Camera.SetTarget(player1);
    //    player2Camera.SetTarget(player2);
    //}

    ////�������
    //void CreateCameras()
    //{
    //    // �������1�����
    //    GameObject cam1Obj = new GameObject("Player1Camera");
    //    camera1 = cam1Obj.AddComponent<Camera>();

    //    // �������2�����
    //    GameObject cam2Obj = new GameObject("Player2Camera");
    //    camera2 = cam2Obj.AddComponent<Camera>();
    //}

    //void SetCameraParameters()
    //{
    //    // ���1���������
    //    player1Camera.height = cameraHeight1;
    //    player1Camera.distance = cameraDistance1;
    //    player1Camera.rotationSpeed = rotationSpeed;
    //    player1Camera.currentAngle = currentAngle1;
    //    player1Camera.rotateLeftKey = KeyCode.E;  // e����ת
    //    player1Camera.rotateRightKey = KeyCode.Q; // q����ת

    //    // ���2���������
    //    player2Camera.height = cameraHeight2;
    //    player2Camera.distance = cameraDistance2;
    //    player2Camera.rotationSpeed = rotationSpeed;
    //    player2Camera.currentAngle = currentAngle2;
    //    player2Camera.rotateLeftKey = KeyCode.O;  // o����ת
    //    player2Camera.rotateRightKey = KeyCode.U; // u����ת
    //}

    ////����
    //void SetupSplitScreen()
    //{
    //    if (horizontalSplit)
    //    {
    //        //ˮƽ�������������1���������2
    //        camera1.rect = new Rect(0f, 0f, 0.5f, 1f);
    //        camera2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
    //    }
    //    else
    //    {
    //        //��ֱ�������������1���������2
    //        camera1.rect = new Rect(0f, 0.5f, 1f, 0.5f);
    //        camera2.rect = new Rect(0f, 0f, 1f, 0.5f);
    //    }
    //}
    [Header("�������")]
    public Transform player1;
    public Transform player2;

    [Header("���������")]
    public Camera camera1;
    public Camera camera2;

    [Header("����ģʽ")]
    public bool horizontalSplit = true; // ˮƽ����

    [Header("���������")]
    public float cameraHeight = 10f;
    public float cameraDistance = 8f;
    public float rotationSpeed = 90f; // ���ӵ�90

    [Header("��ʼ�Ƕ�����")]
    public float player1StartAngle = 45f;
    public float player2StartAngle = 45f;

    private PlayerCamera player1Camera;
    private PlayerCamera player2Camera;

    void Start()
    {
        SetupCameras();
        SetupSplitScreen();
        InitializeCameras();
    }

    void SetupCameras()
    {
        // ȷ�����������
        if (camera1 == null || camera2 == null)
        {
            CreateCameras();
        }

        // ��ȡ����������������
        player1Camera = camera1.GetComponent<PlayerCamera>();
        if (player1Camera == null)
        {
            player1Camera = camera1.gameObject.AddComponent<PlayerCamera>();
        }

        player2Camera = camera2.GetComponent<PlayerCamera>();
        if (player2Camera == null)
        {
            player2Camera = camera2.gameObject.AddComponent<PlayerCamera>();
        }
    }

    void CreateCameras()
    {
        // �������1�����
        if (camera1 == null)
        {
            GameObject cam1Obj = new GameObject("Player1Camera");
            camera1 = cam1Obj.AddComponent<Camera>();
            camera1.tag = "Untagged"; // �����Ϊ�������
        }

        // �������2�����
        if (camera2 == null)
        {
            GameObject cam2Obj = new GameObject("Player2Camera");
            camera2 = cam2Obj.AddComponent<Camera>();
            camera2.tag = "Untagged"; // �����Ϊ�������
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
            player1Camera.rotateLeftKey = KeyCode.Q;
            player1Camera.rotateRightKey = KeyCode.E;
            player1Camera.currentAngle = player1StartAngle;

            // ����Ŀ�겢������׼
            if (player1 != null)
            {
                player1Camera.SetTarget(player1);
                player1Camera.SnapToTarget(); // ������׼
            }
        }

        // �������2���������
        if (player2Camera != null)
        {
            player2Camera.height = cameraHeight;
            player2Camera.distance = cameraDistance;
            player2Camera.rotationSpeed = rotationSpeed;
            player2Camera.rotateLeftKey = KeyCode.U;
            player2Camera.rotateRightKey = KeyCode.O;
            player2Camera.currentAngle = player2StartAngle;

            // ����Ŀ�겢������׼
            if (player2 != null)
            {
                player2Camera.SetTarget(player2);
                player2Camera.SnapToTarget(); // ������׼
            }
        }

        Debug.Log($"�����������ʼ����� - ���1�Ƕ�: {player1StartAngle}��, ���2�Ƕ�: {player2StartAngle}��");
    }

    void SetupSplitScreen()
    {
        if (horizontalSplit)
        {
            // ˮƽ�������������1���������2
            camera1.rect = new Rect(0f, 0f, 0.5f, 1f);
            camera2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        }
        else
        {
            // ��ֱ�������������1���������2
            camera1.rect = new Rect(0f, 0.5f, 1f, 0.5f);
            camera2.rect = new Rect(0f, 0f, 1f, 0.5f);
        }

        // ȷ�����������������
        camera1.enabled = true;
        camera2.enabled = true;
    }

    void Update()
    {
        // ���Թ��ܣ���R�����������λ��
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCameraPositions();
        }
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

    // �����������ֶ�����������Ƕ�
    public void SetCameraAngles(float angle1, float angle2)
    {
        if (player1Camera != null)
        {
            player1Camera.SetCameraAngle(angle1);
        }

        if (player2Camera != null)
        {
            player2Camera.SetCameraAngle(angle2);
        }
    }

    // ������������������״̬
    public void CheckCameraStatus()
    {
        Debug.Log("=== �����״̬��� ===");
        Debug.Log($"���1�����: {(player1Camera != null ? "����" : "ȱʧ")}, Ŀ��: {(player1Camera?.target != null ? player1Camera.target.name : "��")}");
        Debug.Log($"���2�����: {(player2Camera != null ? "����" : "ȱʧ")}, Ŀ��: {(player2Camera?.target != null ? player2Camera.target.name : "��")}");
        Debug.Log($"���1�Ƕ�: {player1Camera?.GetCurrentAngle()}, ���2�Ƕ�: {player2Camera?.GetCurrentAngle()}");
        Debug.Log("=====================");
    }
}
