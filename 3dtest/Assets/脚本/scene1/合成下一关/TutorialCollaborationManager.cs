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
    //public AudioClip collaborationSound;
    public Light collaborationLight;

    [Header("协作成功音效")]
    public string collaborationSoundGroupID = "协作成功"; // 音效组ID

    [Header("UI提示")]
    public GameObject waitingPrompt; // 显示"等待另一位玩家"的UI
    public GameObject successPrompt; // 显示"协作成功"的UI

    [Header("张奕忻scene1：UI_Play开始游戏")]
    public GameObject UI_Play;
    public bool isNotScene1 = true;
    public float skipHoldTime = 5.0f;
    private bool skipPressed = false;

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
        //张奕忻
        if (UI_Play != null) UI_Play.SetActive(false);

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

    // 播放协作成功音效（双声道）
    void PlayCollaborationSound()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(collaborationSoundGroupID))
        {
            // 计算两个区域的中点位置来决定声道
            Vector3 midpoint = CalculateMidpoint();
            bool isPlayer1 = DetermineSoundChannel(midpoint);

            AudioManager.Instance.PlayOneShot(
                collaborationSoundGroupID,
                -1,           // 随机选择音效
                false,        // 不淡入
                0f,
                false,        // 不淡出
                0f,
                isPlayer1,    // 声道分配
                false         // 2D音效
            );

            Debug.Log($"播放协作成功音效 - 声道: {(isPlayer1 ? "左(玩家1)" : "右(玩家2)")}");
        }
        else
        {
            //// 备用：使用原来的音效
            //if (collaborationSound != null)
            //{
            //    AudioSource.PlayClipAtPoint(collaborationSound, Vector3.zero);
            //}
        }
    }

    // 计算两个区域的中点
    Vector3 CalculateMidpoint()
    {
        if (player1Zone != null && player2Zone != null)
        {
            return (player1Zone.transform.position + player2Zone.transform.position) / 2f;
        }
        else if (player1Zone != null)
        {
            return player1Zone.transform.position;
        }
        else if (player2Zone != null)
        {
            return player2Zone.transform.position;
        }

        return Vector3.zero;
    }

    // 根据中点位置决定声道
    bool DetermineSoundChannel(Vector3 midpoint)
    {
        // 查找玩家对象
        GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");

        if (player1 != null && player2 != null)
        {
            float distanceToPlayer1 = Vector3.Distance(midpoint, player1.transform.position);
            float distanceToPlayer2 = Vector3.Distance(midpoint, player2.transform.position);

            // 距离玩家1更近则使用左声道，否则使用右声道
            return distanceToPlayer1 <= distanceToPlayer2;
        }

        // 默认使用左声道
        return true;
    }

    //张奕忻改动
    IEnumerator CompleteCollaboration()
    {
        isTransitioning = true;
        Debug.Log("=== 双人协作完成！ ===");

        // 播放协作完成效果
        PlayCollaborationEffects();

        // 播放协作成功音效
        PlayCollaborationSound();

        Debug.Log($"双人协作成功！将在 {sceneTransitionDelay} 秒后跳转到场景: {targetSceneName}");

        //如果是scene1，等待数秒弹出Play，如果玩家长按五秒或点击Play，则切换第二关
        if (!isNotScene1)
        {
            yield return new WaitForSeconds(2f);
            if (UI_Play != null)
            {
                UI_Play.SetActive(true);
                while (true)
                {
                    if (Input.GetKey(KeyCode.Space))
                    {
                        skipPressed = true;
                        Debug.Log("按下空格键");
                    }
                    else
                    {
                        skipPressed = false;
                    }

                    if (skipPressed)
                    {
                        skipHoldTime -= Time.deltaTime;

                        if (skipHoldTime <= 0)
                        {
                            Debug.Log("空格长达五秒");
                            StartCoroutine(LoadNextSceneWithFade());
                            yield break; // 跳出当前协程
                        }
                    }
                    else
                    {
                        skipHoldTime = 5.0f; // 重置倒计时
                    }

                    yield return null; // 每帧等待一次
                }
            }
        }
        //如果不是scene1，按唤洋的程序合成后等待两秒自动跳转
        else
        {
            // 等待一段时间
            yield return new WaitForSeconds(sceneTransitionDelay);

            // 使用黑幕效果跳转场景
            yield return StartCoroutine(LoadNextSceneWithFade());
        }
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

        if (isNotScene1 && collaborationLight != null)
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