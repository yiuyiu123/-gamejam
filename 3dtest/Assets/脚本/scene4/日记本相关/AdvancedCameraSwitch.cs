/*
 * AdvancedCameraSwitch.cs  
 * ���ܣ�����һ����ϵͳ��������л�/�ű�����/�����ᣩ
 * �����£�2025-10-21 10:30
 */
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ObjectControlSettings
{
    [Tooltip("Ҫ���Ƶ�Rigidbody����")]
    public Rigidbody targetRigidbody;

    [Tooltip("��ʼ����״̬")]
    public bool startKinematic = false;
}

public class AdvancedCameraSwitch : MonoBehaviour
{
    // ========== ��������� ==========
    [Header("����������á�")]
    [Tooltip("�������")]
    public Camera cameraA;

    [Tooltip("�������")]
    public Camera cameraB;

    // ========== �������� ==========
    [Header("���������á�")]
    [Tooltip("�л���ݼ�")]
    public KeyCode switchKey = KeyCode.H;

    [Tooltip("�������޴��л�")]
    public bool canSwitchMultipleTimes = true;

    [Tooltip("�л���ȴʱ��(��)")]
    public float switchCooldown = 0.5f;

    // ========== ������� ==========
    [Header("��������ơ�")]
    [Tooltip("Ҫ���õĽű����")]
    public MonoBehaviour scriptToToggle;

    [Space(10)]
    public ObjectControlSettings physicsControl;

    // ========== UI���� ==========
    [Header("��UI���á�")]
    [Tooltip("��ʾUI����")]
    public GameObject hintUI;

    [Tooltip("��̬��ʾ�ı�")]
    public Text hintText;

    // ========== ״̬���� ==========
    private bool isPlayerInTrigger = false;
    private int switchCount = 0;
    private float lastSwitchTime = 0;
    private bool currentCameraState = false; // false=B��� true=A��� 

    void Start()
    {
        // ��ʼ�������״̬ 
        SetCameraState(false, true);

        // ��ʼ������״̬ 
        if (physicsControl.targetRigidbody != null)
        {
            physicsControl.targetRigidbody.isKinematic = physicsControl.startKinematic;
        }

        // ����UI��ʾ 
        UpdateHintText();

        Debug.Log($"ϵͳ��ʼ����� | ��ǰʱ�䣺{System.DateTime.Now:HH:mm}");
    }

    void Update()
    {
        if (isPlayerInTrigger && Input.GetKeyDown(switchKey) && Time.time > lastSwitchTime + switchCooldown)
        {
            if (canSwitchMultipleTimes || switchCount == 0)
            {
                ExecuteSwitch();
                lastSwitchTime = Time.time;
                switchCount++;
            }
        }
    }

    void ExecuteSwitch()
    {
        // �л������ 
        currentCameraState = !currentCameraState;
        SetCameraState(currentCameraState, !currentCameraState);

        // �л��ű�״̬
        if (scriptToToggle != null)
        {
            scriptToToggle.enabled = !scriptToToggle.enabled;
            Debug.Log($"�ű�״̬��{scriptToToggle.enabled}");
        }

        // �л�����״̬ 
        if (physicsControl.targetRigidbody != null)
        {
            physicsControl.targetRigidbody.isKinematic = !physicsControl.targetRigidbody.isKinematic;
            Debug.Log($"����״̬��{physicsControl.targetRigidbody.isKinematic}");
        }

        UpdateHintText();
    }

    void SetCameraState(bool stateA, bool stateB)
    {
        if (cameraA != null) cameraA.gameObject.SetActive(stateA);
        if (cameraB != null) cameraB.gameObject.SetActive(stateB);
    }

    void UpdateHintText()
    {
        if (hintText != null)
        {
            string cameraStatus = currentCameraState ? "�������" : "�������";
            string physicsStatus = (physicsControl.targetRigidbody != null) ?
                (physicsControl.targetRigidbody.isKinematic ? " | ���徲ֹ" : " | ����ɽ���") : "";

            hintText.text = $"��ǰ��{cameraStatus}{physicsStatus}\n�� {switchKey} �л�";
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player2"))
        {
            isPlayerInTrigger = true;
            if (hintUI != null) hintUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player2"))
        {
            isPlayerInTrigger = false;
            if (hintUI != null) hintUI.SetActive(false);
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // �༭��ʵʱ������ʾ�ı� 
        if(hintText != null && !Application.isPlaying) 
        {
            hintText.text  = $"�� {switchKey} �л�״̬";
        }
    }
#endif
}