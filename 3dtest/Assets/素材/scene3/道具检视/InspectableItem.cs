using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class InspectableItem : MonoBehaviour
{
    [Header("道具设置")]
    public string itemName;
    public string inspectSceneName = "InspectScene";

    [Header("检测区域")]
    public float interactDistance = 2f;
    public KeyCode interactKey = KeyCode.F;

    private bool isPlayerInRange = false;
    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player2");
    }

    void Update()
    {
        // 检测玩家是否在交互范围内
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            isPlayerInRange = distance <= interactDistance;

            // 检测按键输入
            if (isPlayerInRange && Input.GetKeyDown(interactKey))
            {
                EnterInspectMode();
            }
        }

        // 显示/隐藏交互提示
        UpdateInteractionUI();
    }

    void UpdateInteractionUI()
    {
        // 这里可以添加UI提示，比如显示"按F查看"
        if (isPlayerInRange)
        {
            // 显示交互提示UI
            ShowInteractionPrompt();
        }
        else
        {
            // 隐藏交互提示UI
            HideInteractionPrompt();
        }
    }

    void EnterInspectMode()
    {
        // 获取场景管理器
        SceneTransitionManager sceneManager = FindObjectOfType<SceneTransitionManager>();
        if (sceneManager == null)
        {
            // 如果没有找到场景管理器，创建一个
            GameObject managerObj = new GameObject("SceneTransitionManager");
            sceneManager = managerObj.AddComponent<SceneTransitionManager>();
        }

        // 保存当前道具信息（用于在查看场景中显示）
        sceneManager.SetCurrentInspectItem(this);

        // 进入查看模式
        sceneManager.LoadInspectScene(inspectSceneName);
    }

    void ShowInteractionPrompt()
    {
        // 实现显示交互提示UI的逻辑
        // 例如：显示Canvas文本或UI图标
    }

    void HideInteractionPrompt()
    {
        // 实现隐藏交互提示UI的逻辑
    }

    // 可视化交互范围（在Scene视图中显示）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}