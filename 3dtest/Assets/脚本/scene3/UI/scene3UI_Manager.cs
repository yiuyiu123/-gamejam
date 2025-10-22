using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Scene3UI_Manager : MonoBehaviour
{
    public static Scene3UI_Manager Instance;

    [Header("游戏流程状态")]
    public GameProgress currentProgress = GameProgress.Initial;

    [Header("引用脚本")]
    public PlayerController playerController;              // 关卡逻辑（灯、钥匙、门等）
    public DetectLightSwitch detectLightSwitch;
    public FlowerPotZone flowerPotZone;

    [Header("逻辑引用")]
    public Scene2DialogueManager dialogueManager; // 通用对话系统
    public bool useSceneTransition = true;
    public string nextSceneName = "scene4";

    [Header("任务事件")]
    public UnityEvent OnTask1Start;
    public UnityEvent OnTask1Complete;
    public UnityEvent OnTask2Start;
    public UnityEvent OnTask2Complete;
    public UnityEvent OnAllTasksComplete;

    [Header("调试选项")]
    public bool enableDebugLogs = true;

    private bool plot1Completed = false;
    private bool plot2Completed = false;
    private bool plot3Completed = false;
    private bool task1Completed = false;
    private bool task2Completed = false;

    private SimpleSceneTransitionManager sceneTransition;

    public enum GameProgress
    {
        Initial,
        Plot1,   // 电闸任务前剧情
        Task1,   // 找电闸
        Plot2,   // 钥匙任务前剧情
        Task2,   // 找钥匙
        Plot3,   // 开门剧情
        Complete // 全部完成
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<Scene2DialogueManager>();

        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
        if (detectLightSwitch == null)
            detectLightSwitch = FindObjectOfType<DetectLightSwitch>();

        sceneTransition = FindObjectOfType<SimpleSceneTransitionManager>();

        StartCoroutine(MonitorSceneProgress());
    }

    IEnumerator MonitorSceneProgress()
    {
        yield return new WaitForSeconds(1f);
        Log("Scene3剧情监控启动");

        while (true)
        {
            // 初始剧情
            if (currentProgress == GameProgress.Initial)
            {
                StartPlot1();
            }

            // 任务1完成检测
            if (currentProgress == GameProgress.Task1 && playerController.isHoldFlashLight && detectLightSwitch.IsSwitchOpen&&!plot2Completed)
            {
                CompleteTask1();
            }

            //  任务2完成检测
            if (currentProgress == GameProgress.Task2 && flowerPotZone.isKeyAppear && !plot3Completed)
            {
                CompleteTask2();
            }

            yield return null;
        }
    }

    #region 剧情控制
    public void StartPlot1()
    {
        currentProgress = GameProgress.Plot1;
        Log("开始剧情1：电闸任务前对话");

        if (dialogueManager != null)
            dialogueManager.StartDialogueSequence("Plot1");
        else
            LogWarning("对话管理器未找到");

        plot1Completed = true;
        StartTask1();
    }

    public void StartPlot2()
    {
        currentProgress = GameProgress.Plot2;
        Log("开始剧情2：钥匙任务前对话");

        if (dialogueManager != null)
            dialogueManager.StartDialogueSequence("Plot2");
        else
            LogWarning("对话管理器未找到");

        plot2Completed = true;
        StartTask2();
    }

    public void StartPlot3()
    {
        currentProgress = GameProgress.Plot3;
        Log("开始剧情3：开门剧情");

        if (dialogueManager != null)
            dialogueManager.StartDialogueSequence("Plot3");
        else
            LogWarning("对话管理器未找到");

        plot3Completed = true;
        PrepareNextScene();
    }
    #endregion

    #region 任务控制
    void StartTask1()
    {
        currentProgress = GameProgress.Task1;
        Log("开始任务1：寻找电闸手电筒");
        OnTask1Start?.Invoke();
    }

    void CompleteTask1()
    {
        task1Completed = true;
        Log("任务1完成：电闸和手电筒已打开");
        OnTask1Complete?.Invoke();
        StartPlot2();
    }

    void StartTask2()
    {
        currentProgress = GameProgress.Task2;
        Log("开始任务2：合成花洒寻找钥匙");
        OnTask2Start?.Invoke();
    }

    void CompleteTask2()
    {
        task2Completed = true;
        Log("任务2完成：获得钥匙");
        OnTask2Complete?.Invoke();
        StartPlot3();
    }
    #endregion

    #region 场景跳转
    void PrepareNextScene()
    {
        currentProgress = GameProgress.Complete;
        Log("Scene3剧情完成，准备加载下一场景");
        OnAllTasksComplete?.Invoke();

        if (useSceneTransition && sceneTransition != null)
            StartCoroutine(LoadNextSceneWithTransition());
        else
            Invoke("LoadNextSceneDirectly", 2f);
    }

    IEnumerator LoadNextSceneWithTransition()
    {
        yield return null;
        yield return StartCoroutine(sceneTransition.TransitionToScene(nextSceneName));
    }

    void LoadNextSceneDirectly()
    {
        SceneManager.LoadScene(nextSceneName);
    }
    #endregion

    #region 工具
    void Log(string msg)
    {
        if (enableDebugLogs) Debug.Log($"[Scene3UI_Manager] {msg}");
    }

    void LogWarning(string msg)
    {
        if (enableDebugLogs) Debug.LogWarning($"[Scene3UI_Manager] {msg}");
    }
    #endregion
}
