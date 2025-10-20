// AdvancedCameraSwitch.cs  
using UnityEngine;
using UnityEngine.UI;

public class AdvancedCameraSwitch : MonoBehaviour
{
    [Header("摄像机设置")]
    public Camera cameraA;
    public Camera cameraB;

    [Header("交互设置")]
    public KeyCode switchKey = KeyCode.H;
    public bool canSwitchMultipleTimes = true;

    [Header("UI提示")]
    public GameObject hintUI;
    public string hintText = "按 H 切换摄像机";

    [Header("状态")]
    private bool isPlayerInTrigger = false;
    private int switchCount = 0;

    void Start()
    {
        // 初始状态：摄像机A禁用，摄像机B启用 
        SetCameraState(false, true);

        Debug.Log("高级摄像机切换系统初始化");
    }

    void Update()
    {
        // 检查是否在触发区域内且按下H键 
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
        // 切换摄像机状态 
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

        Debug.Log($"摄像机状态: A={cameraAState}, B={cameraBState}");
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
        // 显示触发区域 
        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}