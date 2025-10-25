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
    public PlayerController player1Controller;
    public PlayerController player2Controller; 
    public DetectLightSwitch detectLightSwitch;
    public FlowerPotZone flowerPotZone;

    private bool hasFlashlight = false;
    private bool hasOpenedSwitch = false;
    private bool keyHasAppeared = false;


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
        // 订阅 Player1、Player2 的拾取事件
        if (player1Controller != null)
            player1Controller.OnFlashlightPickedUp += OnFlashlightPickedUp;
        if (player2Controller != null)
            player2Controller.OnFlashlightPickedUp += OnFlashlightPickedUp;

        // 灯闸
        if (detectLightSwitch != null)
            detectLightSwitch.LightSwitchOn += OnLightSwitchOn;

        // 钥匙生成
        if (flowerPotZone != null)
            flowerPotZone.HasKeyAppear += OnKeyAppear;
        else
        {
            dialogueManager = FindObjectOfType<Scene2DialogueManager>();
            player1Controller = FindObjectOfType<PlayerController>();
            player2Controller = FindObjectOfType<PlayerController>();
            detectLightSwitch = FindObjectOfType<DetectLightSwitch>();
        }
        sceneTransition = FindObjectOfType<SimpleSceneTransitionManager>();

        StartCoroutine(MonitorSceneProgress());
    }

    IEnumerator MonitorSceneProgress()
    {
        yield return new WaitForSeconds(1f);
        Log("Scene3剧情监控启动");

        // 启动初始剧情
        if (currentProgress == GameProgress.Initial)
        {
            StartPlot1();
        }

        while (true)
        {
            yield return null;
        }
    }

    #region 剧情控制
    void OnFlashlightPickedUp()
    {
        Debug.Log("事件触发：手电筒捡起");
        hasFlashlight = true;
        CheckTask1Completion();
    }

    void OnLightSwitchOn()
    {
        Debug.Log("事件触发：灯闸打开");
        hasOpenedSwitch = true;
        CheckTask1Completion();
    }

    void OnKeyAppear()
    {
        keyHasAppeared = true;
        CheckTask2Completion();
    }

    // 新增方法：任务1完成检查
    void CheckTask1Completion()
    {
        if (!task1Completed && hasOpenedSwitch)//&& hasFlashlight 
        {
            task1Completed = true;

            // 调用 Scene2DialogueManager 的任务完成函数
            if (dialogueManager != null)
            {
                dialogueManager.OnTaskCompleted(); // <-- 这里直接调用
            }

            CompleteTask1();
        }
    }

    void CheckTask2Completion()
    {
        if (!task2Completed && keyHasAppeared)
        {
            task2Completed = true;

            if (dialogueManager != null)
            {
                dialogueManager.OnTaskCompleted(); // <-- 调用任务完成
            }

            CompleteTask2();
        }
    }

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
        {
            Debug.Log("调用 dialogueManager.StartDialogueSequence(\"Plot2\")");
            dialogueManager.StartDialogueSequence("Plot2");
        }
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
        {
            dialogueManager.StartDialogueSequence("Plot3");
            // 启动协程监控剧情3对话是否结束
            StartCoroutine(MonitorPlot3DialogueEnd());
        }
        else
        {
            LogWarning("对话管理器未找到");
        }

        plot3Completed = true;
    }

    // 轮询剧情3对话是否播放完毕
    private IEnumerator MonitorPlot3DialogueEnd()
    {
        // 等待剧情3对话结束
        while (IsDialoguePlaying())
        {
            yield return null;
        }
        Log("剧情3对话播放完毕");
        //StartCoroutine(DelayedSceneTransition(1f)); // 延迟2秒跳场景
    }

    // 使用 DialogueManager 的状态判断
    private bool IsDialoguePlaying()
    {
        // 通过反射访问私有字段 isDialogueActive
        if (dialogueManager == null) return false;

        var type = dialogueManager.GetType();
        var field = type.GetField("isDialogueActive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return (bool)field.GetValue(dialogueManager);
        }

        return false;
    }

    public IEnumerator DelayedSceneTransition(float delay)
    {
        yield return new WaitForSeconds(delay);
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
        Debug.Log("任务1完成，触发剧情2");
        OnTask1Complete?.Invoke();
        // 触发剧情2
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
            Invoke("LoadNextSceneDirectly", 1f);
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
