// AdvancedCameraSwitch.cs  
using UnityEngine;
using UnityEngine.UI;

public class AdvancedCameraSwitch : MonoBehaviour
{
    [Header("���������")]
    public Camera cameraA;
    public Camera cameraB;

    [Header("��������")]
    public KeyCode switchKey = KeyCode.H;
    public bool canSwitchMultipleTimes = true;

    [Header("UI��ʾ")]
    public GameObject hintUI;
    public string hintText = "�� H �л������";

    [Header("״̬")]
    private bool isPlayerInTrigger = false;
    private int switchCount = 0;

    void Start()
    {
        // ��ʼ״̬�������A���ã������B���� 
        SetCameraState(false, true);

        Debug.Log("�߼�������л�ϵͳ��ʼ��");
    }

    void Update()
    {
        // ����Ƿ��ڴ����������Ұ���H�� 
        if (isPlayerInTrigger && Input.GetKeyDown(switchKey))
        {
            if (canSwitchMultipleTimes || switchCount == 0)
            {
                SwitchCameras();
                switchCount++;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player2"))
        {
            isPlayerInTrigger = true;
            ShowInteractionHint(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player2"))
        {
            isPlayerInTrigger = false;
            ShowInteractionHint(false);
        }
    }

    void SwitchCameras()
    {
        // �л������״̬ 
        bool newCameraAState = !cameraA.gameObject.activeInHierarchy;
        SetCameraState(newCameraAState, !newCameraAState);
    }

    void SetCameraState(bool cameraAState, bool cameraBState)
    {
        if (cameraA != null)
        {
            cameraA.gameObject.SetActive(cameraAState);
        }

        if (cameraB != null)
        {
            cameraB.gameObject.SetActive(cameraBState);
        }

        Debug.Log($"�����״̬: A={cameraAState}, B={cameraBState}");
    }

    void ShowInteractionHint(bool show)
    {
        if (hintUI != null)
        {
            hintUI.SetActive(show);
        }
    }

    void OnDrawGizmos()
    {
        // ��ʾ�������� 
        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}