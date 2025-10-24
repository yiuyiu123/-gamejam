using UnityEngine;

public class InteractionStateMonitor : MonoBehaviour
{
    [Header("�������")]
    public bool enableMonitoring = true;
    public float updateInterval = 2f; // �ӳ����¼��
    public bool showOnScreenDebug = false;

    private PlayerController playerController;
    private PlayerAnimationController animationController;
    private float lastUpdateTime;
    private string debugText = "";

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        animationController = GetComponent<PlayerAnimationController>();
    }

    void Update()
    {
        if (!enableMonitoring) return;

        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateDebugInfo();
            lastUpdateTime = Time.time;
        }
    }

    void UpdateDebugInfo()
    {
        if (playerController == null && animationController == null) return;

        debugText = $"{gameObject.name} ״̬���:\n";

        if (playerController != null)
        {
            debugText += $"- {playerController.GetInteractionState()}\n";
        }

        if (animationController != null)
        {
            debugText += $"- {animationController.GetStateRecoveryInfo()}\n";
            debugText += $"- ��������: {animationController.IsAnimationLocked()}\n";
        }

        Debug.Log(debugText);
    }

    void OnGUI()
    {
        if (!showOnScreenDebug || !enableMonitoring) return;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;
        style.wordWrap = true;

        Rect rect = new Rect(10, gameObject.CompareTag("Player1") ? 10 : 160, 500, 200);
        GUI.Box(rect, "");
        GUI.Label(rect, debugText, style);
    }

    [ContextMenu("��ӡ��ǰ״̬")]
    public void PrintCurrentState()
    {
        UpdateDebugInfo();
        Debug.Log(debugText);
    }

    [ContextMenu("ǿ�ƻָ�״̬")]
    public void ForceRecovery()
    {
        if (playerController != null)
        {
            playerController.ForceResetInteractionState();
        }
        if (animationController != null)
        {
            animationController.ResetAllAnimations();
        }
        Debug.Log($"{gameObject.name} ǿ�ƻָ����");
    }

    [ContextMenu("����״̬�ָ�")]
    public void DisableStateRecovery()
    {
        if (animationController != null)
        {
            animationController.enableStateRecovery = false;
            Debug.Log($"{gameObject.name} ״̬�ָ��ѽ���");
        }
    }

    [ContextMenu("����״̬�ָ�")]
    public void EnableStateRecovery()
    {
        if (animationController != null)
        {
            animationController.enableStateRecovery = true;
            Debug.Log($"{gameObject.name} ״̬�ָ�������");
        }
    }
}