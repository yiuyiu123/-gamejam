using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Scene2TaskManager : MonoBehaviour
{
    public static Scene2TaskManager Instance; // 单例模式

    [Header("游戏流程状态")]
    public GameProgress currentProgress = GameProgress.Initial; // 当前游戏进度

    [Header("玩家引用")]
    public GameObject player1;           // 玩家1对象
    public GameObject player2;           // 玩家2对象
    public string player1Tag = "Player1"; // 玩家1标签
    public string player2Tag = "Player2"; // 玩家2标签

    [Header("管理器引用")]
    public Scene2DualSynthesisManager synthesisManager; // 合成管理器引用
    public Scene2DialogueManager dialogueManager;       // 对话管理器引用

    [Header("任务事件")]
    public UnityEvent OnTask1Start;       // 任务1开始事件
    public UnityEvent OnTask1Complete;    // 任务1完成事件
    public UnityEvent OnTask2Start;       // 任务2开始事件
    public UnityEvent OnTask2Complete;    // 任务2完成事件
    public UnityEvent OnAllTasksComplete; // 所有任务完成事件

    [Header("调试选项")]
    public bool enableDebugLogs = true; // 是否启用调试日志

    // 游戏进度枚举
    public enum GameProgress
    {
        Initial,        // 初始状态
        Plot1,          // 剧情1：场景介绍
        Task1,          // 任务1：合成U盘
        Plot2,          // 剧情2：玩家A给老板U盘
        Plot4,          // 剧情4：老师检查试卷
        Task2,          // 任务2：合成试卷
        Plot3And5,      // 剧情3和5：老板和老师同时训斥玩家
        Complete        // 完成
    }

    // 任务状态
    private bool task1Completed = false; // 任务1完成状态
    private bool task2Completed = false; // 任务2完成状态
    private bool plot1Completed = false; // 剧情1完成状态
    private bool plot2Completed = false; // 剧情2完成状态
    private bool plot3Completed = false; // 剧情3完成状态
    private bool plot4Completed = false; // 剧情4完成状态
    private bool plot5Completed = false; // 剧情5完成状态

    void Awake()
    {
        // 单例模式初始化
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
        InitializeManagers(); // 初始化管理器
        StartGame();         // 开始游戏
    }

    // 初始化管理器引用
    void InitializeManagers()
    {
        if (synthesisManager == null)
            synthesisManager = FindObjectOfType<Scene2DualSynthesisManager>();
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<Scene2DialogueManager>();

        // 设置玩家标签
        if (player1 != null) player1.tag = player1Tag;
        if (player2 != null) player2.tag = player2Tag;

        if (synthesisManager == null)
            LogWarning("Scene2DualSynthesisManager 未找到！");
        if (dialogueManager == null)
            LogWarning("Scene2DialogueManager 未找到！");
    }

    // 开始游戏
    void StartGame()
    {
        Log("游戏开始 - Scene2 双人合作解密");
        currentProgress = GameProgress.Initial;
    }

    #region 剧情管理
    // 开始剧情1
    public void StartPlot1()
    {
        if (currentProgress != GameProgress.Initial) return;

        currentProgress = GameProgress.Plot1;
        Log("开始剧情1：办公室/学校场景介绍");
    }

    // 完成剧情1
    public void CompletePlot1()
    {
        plot1Completed = true;
        Log($"剧情1完成，准备开始任务1。当前进度: {currentProgress}");

        // 剧情1完成后开始任务1
        StartTask1();
    }

    // 完成任务1
    public void CompleteTask1()
    {
        task1Completed = true;
        Log("任务1完成：成功合成有资料的U盘");

        OnTask1Complete?.Invoke();
        // 任务1完成后开始剧情2
        StartPlot2();
    }

    // 开始剧情2
    public void StartPlot2()
    {
        currentProgress = GameProgress.Plot2;
        Log("开始剧情2：玩家A给老板U盘");

        // 触发剧情2对话
        if (dialogueManager != null)
        {
            dialogueManager.StartDialogueSequence("Plot2");
        }
    }

    // 完成剧情2
    public void CompletePlot2()
    {
        plot2Completed = true;
        Log("剧情2完成");

        // 剧情2完成后开始剧情4
        StartPlot4();
    }

    // 开始剧情4
    public void StartPlot4()
    {
        currentProgress = GameProgress.Plot4;
        Log("开始剧情4：老师检查试卷");

        // 触发剧情4对话
        if (dialogueManager != null)
        {
            dialogueManager.StartDialogueSequence("Plot4");
        }
    }

    // 完成剧情4
    public void CompletePlot4()
    {
        plot4Completed = true;
        Log("剧情4完成");

        // 剧情4完成后开始任务2
        StartTask2();
    }

    // 完成任务2
    public void CompleteTask2()
    {
        task2Completed = true;
        Log("任务2完成：成功合成试卷");

        OnTask2Complete?.Invoke();

        // 任务2完成后开始剧情3和5
        StartPlot3And5();
    }

    // 开始剧情3和5（合并剧情）
    public void StartPlot3And5()
    {
        currentProgress = GameProgress.Plot3And5;
        Log("开始剧情3和5：老板和老师同时训斥玩家");

        // 触发合并后的对话
        if (dialogueManager != null)
        {
            dialogueManager.StartDialogueSequence("Plot3And5");
        }
        else
        {
            Log("对话管理器为null，无法开始剧情3和5");
        }
    }

    // 完成剧情3和5
    public void CompletePlot3And5()
    {
        plot3Completed = true;
        plot5Completed = true;
        Log("剧情3和5完成");

        // 直接准备下一场景，因为对话已经由玩家按键结束
        PrepareNextScene();
    }
    #endregion

    #region 任务管理
    // 开始任务1
    void StartTask1()
    {
        currentProgress = GameProgress.Task1;
        Log($"开始任务1：合成有资料的U盘，当前进度: {currentProgress}");

        OnTask1Start?.Invoke();

        // 启用Scene2专用合成检测
        if (synthesisManager != null)
        {
            Log("通知合成管理器开始任务1");
            synthesisManager.StartTask1();
        }
        else
        {
            Log("合成管理器为null！");
        }
    }

    // 开始任务2
    void StartTask2()
    {
        currentProgress = GameProgress.Task2;
        Log("开始任务2：合成试卷");

        OnTask2Start?.Invoke();

        // 启用Scene2专用合成检测
        if (synthesisManager != null)
        {
            synthesisManager.StartTask2();
        }
    }
    #endregion

    #region 场景管理
    // 准备下一场景
    void PrepareNextScene()
    {
        currentProgress = GameProgress.Complete;
        Log("准备加载下一场景");

        OnAllTasksComplete?.Invoke();

        // 可以在这里添加过渡动画
        Invoke("LoadNextScene", 2f);
    }

    // 加载下一场景
    void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Log($"加载下一场景: {nextSceneIndex}");
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Log("这是最后一个场景，游戏结束");
            // 触发游戏结束
        }
    }

    // 重新开始游戏
    public void RestartGame()
    {
        Log("重新开始游戏");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion

    #region 公共方法
    // 合成事件回调 - 任务1合成完成
    public void OnTask1SynthesisComplete()
    {
        Log($"OnTask1SynthesisComplete 被调用，当前任务1完成状态: {task1Completed}");
        if (!task1Completed)
        {
            CompleteTask1();
        }
        else
        {
            LogWarning("任务1已经完成，重复调用 OnTask1SynthesisComplete");
        }
    }

    // 合成事件回调 - 任务2合成完成
    public void OnTask2SynthesisComplete()
    {
        if (!task2Completed)
        {
            CompleteTask2();
        }
    }

    // 对话事件回调
    public void OnDialogueSequenceComplete(string sequenceName)
    {
        switch (sequenceName)
        {
            case "Plot1":
                CompletePlot1();
                break;
            case "Plot2":
                CompletePlot2();
                break;
            case "Plot3And5":  // 改为处理合并的序列
                CompletePlot3And5();
                break;
            case "Plot4":
                CompletePlot4();
                break;
        }
    }


    // 获取当前状态
    public bool IsTask1Complete() => task1Completed;
    public bool IsTask2Complete() => task2Completed;
    public GameProgress GetCurrentProgress() => currentProgress;
    #endregion

    #region 调试工具
    [ContextMenu("强制开始任务1")]
    public void DebugStartTask1()
    {
        StartTask1();
    }

    [ContextMenu("强制完成任务1")]
    public void DebugCompleteTask1()
    {
        CompleteTask1();
    }

    [ContextMenu("强制开始任务2")]
    public void DebugStartTask2()
    {
        StartTask2();
    }

    [ContextMenu("强制完成任务2")]
    public void DebugCompleteTask2()
    {
        CompleteTask2();
    }

    [ContextMenu("跳转到下一场景")]
    public void DebugNextScene()
    {
        PrepareNextScene();
    }

    [ContextMenu("显示当前状态")]
    public void DebugShowStatus()
    {
        Log($"=== Scene2任务状态 ===");
        Log($"当前进度: {currentProgress}");
        Log($"任务1完成: {task1Completed}");
        Log($"任务2完成: {task2Completed}");
        Log($"剧情1完成: {plot1Completed}");
        Log($"剧情2完成: {plot2Completed}");
        Log($"剧情3完成: {plot3Completed}");
        Log($"剧情4完成: {plot4Completed}");
        Log($"剧情5完成: {plot5Completed}");
    }

    [ContextMenu("重置游戏状态")]
    public void DebugResetGame()
    {
        currentProgress = GameProgress.Initial;
        task1Completed = false;
        task2Completed = false;
        plot1Completed = false;
        plot2Completed = false;
        plot3Completed = false;
        plot4Completed = false;
        plot5Completed = false;

        Log("游戏状态已重置");
    }
    #endregion

    #region 工具方法
    // 日志工具方法
    void Log(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[Scene2TaskManager] {message}");
        }
    }

    void LogWarning(string message)
    {
        if (enableDebugLogs)
        {
            Debug.LogWarning($"[Scene2TaskManager] {message}");
        }
    }
    #endregion
}