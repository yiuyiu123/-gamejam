using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class newmanage : MonoBehaviour
{
    public static newmanage Instance;

    [Header("玩家区域设置")]
    public item1 player1Zone;
    public item1 player2Zone;

    [Header("协作完成UI")]
    public Image panel_Ending; // 协作成功后显示的UI图片 
    public float uiDisplayDelay = 2f; // UI显示延迟 

    [Header("协作成功音效")]
    public AudioSource audioSource;
    public AudioClip collaborationClip;

    [Header("增加监听事件")]
    public bool canStartEnding = false;
    public event Action OnStartEnding;//开始禁用玩家控制器脚本并开启结局选择

    private bool player1Ready = false;
    private bool player2Ready = false;
    private bool isTransitioning = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 注册区域回调 
        if (player1Zone != null)
            player1Zone.OnItemStateChanged += OnPlayer1ZoneUpdated;
        if (player2Zone != null)
            player2Zone.OnItemStateChanged += OnPlayer2ZoneUpdated;
        // 初始化协作完成UI 
        if (panel_Ending != null) panel_Ending.gameObject.SetActive(false);
    }

    void OnPlayer1ZoneUpdated(bool hasRequiredItem)
    {
        player1Ready = hasRequiredItem;
        Debug.Log($"玩家1区域状态: {(hasRequiredItem ? "就绪" : "未就绪")}");
        CheckCollaborationStatus();
    }

    void OnPlayer2ZoneUpdated(bool hasRequiredItem)
    {
        player2Ready = hasRequiredItem;
        Debug.Log($"玩家2区域状态: {(hasRequiredItem ? "就绪" : "未就绪")}");
        CheckCollaborationStatus();
    }

    void CheckCollaborationStatus()
    {
        if (isTransitioning) return;

        bool bothReady = player1Ready && player2Ready;
        bool oneReady = player1Ready || player2Ready;
       
        // 如果双方都准备好了，开始完成流程 
        if (bothReady)
        {
            StartCoroutine(CompleteCollaboration());
        }
    }
    //进入结局UI
    IEnumerator CompleteCollaboration()
    {
        isTransitioning = true;
        Debug.Log("最终道具合成成功！准备进入结局");
        audioSource.PlayOneShot(collaborationClip);

        Debug.Log($"newmanage: 将在 {uiDisplayDelay} 秒后显示协作完成UI");

        // 等待一段时间 
        yield return new WaitForSeconds(uiDisplayDelay);

        OnStartEnding?.Invoke();

        // 显示协作完成UI 
        ShowCollaborationUI();
    }

    void ShowCollaborationUI()
    {
        if (panel_Ending != null)
        {
            panel_Ending.gameObject.SetActive(true);
            Debug.Log($"显示协作完成UI图片: {panel_Ending.name}");

            // 添加淡入效果 
            StartCoroutine(FadeInCollaborationUI());
        }
        else
        {
            Debug.LogWarning("未分配协作完成UI图片！");
        }
    }

    IEnumerator FadeInCollaborationUI()
    {
        if (panel_Ending == null) yield break;

        float duration = 1f;
        float elapsed = 0f;
        Color startColor = panel_Ending.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 1f);

        panel_Ending.color = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            panel_Ending.color = Color.Lerp(
                new Color(startColor.r, startColor.g, startColor.b, 0f),
                targetColor,
                elapsed / duration
            );
            yield return null;
        }

        panel_Ending.color = targetColor;
    }

    // 调试方法 
    [ContextMenu("强制完成协作")]
    public void ForceCompleteCollaboration()
    {
        if (!isTransitioning)
        {
            player1Ready = true;
            player2Ready = true;
            StartCoroutine(CompleteCollaboration());
        }
    }

    [ContextMenu("重置协作状态")]
    public void ResetCollaboration()
    {
        player1Ready = false;
        player2Ready = false;
        isTransitioning = false;

        // 重置协作完成UI 
        if (panel_Ending != null)
        {
            panel_Ending.gameObject.SetActive(false);
        }

        Debug.Log("协作状态已重置");
    }

    void OnDestroy()
    {
        // 取消注册回调 
        if (player1Zone != null)
            player1Zone.OnItemStateChanged -= OnPlayer1ZoneUpdated;

        if (player2Zone != null)
            player2Zone.OnItemStateChanged -= OnPlayer2ZoneUpdated;
    }
}