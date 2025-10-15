using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialCollaborationManager : MonoBehaviour
{
    public static TutorialCollaborationManager Instance;

    [Header("玩家区域设置")]
    public TutorialSynthesisZone player1Zone;
    public TutorialSynthesisZone player2Zone;

    [Header("场景跳转设置")]
    public string targetSceneName = "NextScene";
    public float sceneTransitionDelay = 2f;

    [Header("协作完成效果")]
    public ParticleSystem collaborationEffect;
    public AudioClip collaborationSound;
    public Light collaborationLight;

    [Header("UI提示")]
    public GameObject waitingPrompt; // 显示"等待另一位玩家"的UI
    public GameObject successPrompt; // 显示"协作成功"的UI

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

        // 初始化UI
        if (waitingPrompt != null) waitingPrompt.SetActive(false);
        if (successPrompt != null) successPrompt.SetActive(false);

        Debug.Log("协作教学关卡管理器已启动");
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

        // 更新UI提示
        if (waitingPrompt != null)
            waitingPrompt.SetActive(oneReady && !bothReady);

        if (successPrompt != null)
            successPrompt.SetActive(bothReady);

        // 如果双方都准备好了，开始完成流程
        if (bothReady)
        {
            StartCoroutine(CompleteCollaboration());
        }
    }

    IEnumerator CompleteCollaboration()
    {
        isTransitioning = true;
        Debug.Log("=== 双人协作完成！ ===");

        // 播放协作完成效果
        PlayCollaborationEffects();

        Debug.Log($"双人协作成功！将在 {sceneTransitionDelay} 秒后跳转到场景: {targetSceneName}");

        // 等待一段时间
        yield return new WaitForSeconds(sceneTransitionDelay);

        // 使用黑幕效果跳转场景
        yield return StartCoroutine(LoadNextSceneWithFade());
    }

    IEnumerator LoadNextSceneWithFade()
    {
        Debug.Log("开始加载场景带淡入淡出效果");

        // 使用简化版本
        if (SimpleSceneTransitionManager.Instance != null)
        {
            yield return StartCoroutine(SimpleSceneTransitionManager.Instance.TransitionToScene(targetSceneName));
        }
        else if (SimpleSceneTransitionManager.Instance != null)
        {
            yield return StartCoroutine(SimpleSceneTransitionManager.Instance.TransitionToScene(targetSceneName));
        }
        else
        {
            Debug.LogError("没有找到场景过渡管理器！");
            LoadNextScene();
        }
    }

    void PlayCollaborationEffects()
    {
        if (collaborationEffect != null)
        {
            collaborationEffect.Play();
        }

        if (collaborationSound != null)
        {
            AudioSource.PlayClipAtPoint(collaborationSound, Vector3.zero);
        }

        if (collaborationLight != null)
        {
            StartCoroutine(FlashCollaborationLight());
        }
    }

    IEnumerator FlashCollaborationLight()
    {
        collaborationLight.enabled = true;
        float originalIntensity = collaborationLight.intensity;

        // 更华丽的闪烁效果表示协作成功
        for (int i = 0; i < 5; i++)
        {
            collaborationLight.intensity = originalIntensity * 3f;
            collaborationLight.color = i % 2 == 0 ? Color.blue : Color.green;
            yield return new WaitForSeconds(0.15f);
            collaborationLight.intensity = originalIntensity;
            yield return new WaitForSeconds(0.15f);
        }

        collaborationLight.intensity = originalIntensity;
        collaborationLight.color = Color.white;
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"跳转到场景: {targetSceneName}");
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("目标场景名称未设置！");
        }
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

        if (waitingPrompt != null) waitingPrompt.SetActive(false);
        if (successPrompt != null) successPrompt.SetActive(false);

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