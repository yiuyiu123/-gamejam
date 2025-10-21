/*
 * AdvancedCameraSwitch.cs  
 * 功能：三合一控制系统（摄像机切换/脚本禁用/物理冻结）
 * 最后更新：2025-10-21 10:30
 */
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ObjectControlSettings
{
    [Tooltip("要控制的Rigidbody物体")]
    public Rigidbody targetRigidbody;

    [Tooltip("初始物理状态")]
    public bool startKinematic = false;
}

public class AdvancedCameraSwitch : MonoBehaviour
{
    // ========== 摄像机设置 ==========
    [Header("【摄像机设置】")]
    [Tooltip("主摄像机")]
    public Camera cameraA;

    [Tooltip("副摄像机")]
    public Camera cameraB;

    // ========== 交互设置 ==========
    [Header("【交互设置】")]
    [Tooltip("切换快捷键")]
    public KeyCode switchKey = KeyCode.H;

    [Tooltip("允许无限次切换")]
    public bool canSwitchMultipleTimes = true;

    [Tooltip("切换冷却时间(秒)")]
    public float switchCooldown = 0.5f;

    // ========== 物体控制 ==========
    [Header("【物体控制】")]
    [Tooltip("要禁用的脚本组件")]
    public MonoBehaviour scriptToToggle;

    [Space(10)]
    public ObjectControlSettings physicsControl;

    // ========== UI设置 ==========
    [Header("【UI设置】")]
    [Tooltip("提示UI对象")]
    public GameObject hintUI;

    [Tooltip("动态提示文本")]
    public Text hintText;

    // ========== 状态变量 ==========
    private bool isPlayerInTrigger = false;
    private int switchCount = 0;
    private float lastSwitchTime = 0;
    private bool currentCameraState = false; // false=B相机 true=A相机 

    void Start()
    {
        // 初始化摄像机状态 
        SetCameraState(false, true);

        // 初始化物理状态 
        if (physicsControl.targetRigidbody != null)
        {
            physicsControl.targetRigidbody.isKinematic = physicsControl.startKinematic;
        }

        // 更新UI提示 
        UpdateHintText();

        Debug.Log($"系统初始化完成 | 当前时间：{System.DateTime.Now:HH:mm}");
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
        // 切换摄像机 
        currentCameraState = !currentCameraState;
        SetCameraState(currentCameraState, !currentCameraState);

        // 切换脚本状态
        if (scriptToToggle != null)
        {
            scriptToToggle.enabled = !scriptToToggle.enabled;
            Debug.Log($"脚本状态：{scriptToToggle.enabled}");
        }

        // 切换物理状态 
        if (physicsControl.targetRigidbody != null)
        {
            physicsControl.targetRigidbody.isKinematic = !physicsControl.targetRigidbody.isKinematic;
            Debug.Log($"物理状态：{physicsControl.targetRigidbody.isKinematic}");
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
            string cameraStatus = currentCameraState ? "主摄像机" : "副摄像机";
            string physicsStatus = (physicsControl.targetRigidbody != null) ?
                (physicsControl.targetRigidbody.isKinematic ? " | 物体静止" : " | 物体可交互") : "";

            hintText.text = $"当前：{cameraStatus}{physicsStatus}\n按 {switchKey} 切换";
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
        // 编辑器实时更新提示文本 
        if(hintText != null && !Application.isPlaying) 
        {
            hintText.text  = $"按 {switchKey} 切换状态";
        }
    }
#endif
}