using UnityEngine;

public class InteractionStateMonitor : MonoBehaviour
{
    [Header("¼à¿ØÉèÖÃ")]
    public bool enableMonitoring = true;
    public float updateInterval = 2f; // ÑÓ³¤¸üÐÂ¼ä¸ô
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

        debugText = $"{gameObject.name} ×´Ì¬¼à¿Ø:\n";

        if (playerController != null)
        {
            debugText += $"- {playerController.GetInteractionState()}\n";
        }

        if (animationController != null)
        {
            debugText += $"- {animationController.GetStateRecoveryInfo()}\n";
            debugText += $"- ¶¯»­Ëø¶¨: {animationController.IsAnimationLocked()}\n";
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

    [ContextMenu("´òÓ¡µ±Ç°×´Ì¬")]
    public void PrintCurrentState()
    {
        UpdateDebugInfo();
        Debug.Log(debugText);
    }

    [ContextMenu("Ç¿ÖÆ»Ö¸´×´Ì¬")]
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
        Debug.Log($"{gameObject.name} Ç¿ÖÆ»Ö¸´Íê³É");
    }

    [ContextMenu("½ûÓÃ×´Ì¬»Ö¸´")]
    public void DisableStateRecovery()
    {
        if (animationController != null)
        {
            animationController.enableStateRecovery = false;
            Debug.Log($"{gameObject.name} ×´Ì¬»Ö¸´ÒÑ½ûÓÃ");
        }
    }

    [ContextMenu("ÆôÓÃ×´Ì¬»Ö¸´")]
    public void EnableStateRecovery()
    {
        if (animationController != null)
        {
            animationController.enableStateRecovery = true;
            Debug.Log($"{gameObject.name} ×´Ì¬»Ö¸´ÒÑÆôÓÃ");
        }
    }
}