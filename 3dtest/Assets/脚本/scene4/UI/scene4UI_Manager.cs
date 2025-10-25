using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Scene4UI_Manager : MonoBehaviour
{
    public static Scene4UI_Manager Instance;

    [Header("游戏流程状态")]
    public GameProgress currentProgress = GameProgress.Initial;

    [Header("引用脚本")]
    public ItemTrigger DoorDelete;
    public PlayerMovement playerMovement;
    public ThreeItemCraftingManager threeItemCraftingManager;

    private bool hasDoorFirstOpen = false;
    private bool hasDoorSecondOpen = false;
    private bool hasOverReadDiary = false;
    private bool CanQuestion = false;


    [Header("逻辑引用")]
    public Scene4DialogueManager dialogueManager; 
    public bool useSceneTransition = true;
    public string nextSceneName = "scene5";

    [Header("任务事件")]
    public UnityEvent OnTask1Start;
    public UnityEvent OnTask1Complete;
    public UnityEvent OnTask2Start;
    public UnityEvent OnTask2Complete;
    public UnityEvent OnTask3Start;
    public UnityEvent OnTask3Complete;
    public UnityEvent OnFinalTaskStart;
    public UnityEvent OnFinalTaskComplete;
    public UnityEvent OnEndingStart;
    public UnityEvent OnEndingComplete;

    [Header("调试选项")]
    public bool enableDebugLogs = true;

    private bool plot1Completed = false;
    private bool plot2Completed = false;
    private bool plot3Completed = false;
    private bool task1Completed = false;
    private bool task2Completed = false;
    private bool task3Completed = false;
    private bool FinalTaskCompleted = false;
    private bool EndingCompleted = false;

    private SimpleSceneTransitionManager sceneTransition;

    public enum GameProgress
    {
        Initial,
        Task1,   // 神秘道具1放入合成区域
        Plot1,   // 剧情1
        Task2,   // 神秘道具2放入合成区域
        Plot2,   // 剧情2
        Task3,   // 第三篇日记阅读完
        Plot3,   // 剧情3
        FinalTask, // 最终合成任务成功
        Ending, //开始提问UI
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
        //日记本开门事件监听
        if (DoorDelete != null)
        {
            DoorDelete.OpenFirstDoor += OnDoorOpened1;
            DoorDelete.OpenSecondDoor += OnDoorOpened2;
        }
        // 第三篇日记阅读完playerMovement碰到3的碰撞体
        //if (playerMovement != null)
            //playerMovement.HasKeyAppear += OverReadDiary;
        // 订阅最终合成事件
        //if (threeItemCraftingManager != null)
            //threeItemCraftingManager.OnQuestion += OnQuestion;
        else
        {
            dialogueManager = FindObjectOfType<Scene4DialogueManager>();
            threeItemCraftingManager=FindObjectOfType<ThreeItemCraftingManager>();
            DoorDelete = FindObjectOfType<ItemTrigger>();
            playerMovement = FindObjectOfType<PlayerMovement>();
        }
        sceneTransition = FindObjectOfType<SimpleSceneTransitionManager>();

        StartCoroutine(MonitorSceneProgress());
    }

    IEnumerator MonitorSceneProgress()
    {
        yield return new WaitForSeconds(1f);
        Log("Scene4剧情监控启动");

        // 启动任务1
        if (currentProgress == GameProgress.Initial)
        {
            StartTask1();
        }
        while (true)
        {
            yield return null;
        }
    }

    #region 监听事件
    void OnDoorOpened1()
    {
        DoorDelete.OpenFirstDoor -= OnDoorOpened1;
        Debug.Log("事件触发：捡起漫画");
        hasDoorFirstOpen = true;
        CheckTask1Completion();
    }

    void OnDoorOpened2()
    {
        DoorDelete.OpenSecondDoor -= OnDoorOpened2;
        Debug.Log("事件触发：捡起日记本");
        hasDoorSecondOpen = true;
        CheckTask2Completion();
    }

    void OverReadDiary()
    {
        Debug.Log("事件触发：A阅读完日记本");
        hasOverReadDiary = true;
        CheckTask3Completion();
    }
    void OnQuestion()
    {
        CanQuestion = true;
        CheckFinalTaskCompletion();
    }
    #endregion


    #region 任务控制
    void StartTask1()
    {
        currentProgress = GameProgress.Task1;
        Log("开始任务1：合成漫画");
        OnTask1Start?.Invoke();
    }

    void CompleteTask1()
    {
        task1Completed = true;
        Log("任务1完成，触发剧情1");
        OnTask1Complete?.Invoke();
        StartPlot1();
    }

    void StartTask2()
    {
        currentProgress = GameProgress.Task2;
        Log("开始任务2：合成日记本");
        OnTask2Start?.Invoke();
    }

    void CompleteTask2()
    {
        task2Completed = true;
        Log("任务2完成,触发剧情2");
        OnTask2Complete?.Invoke();
        StartPlot2();
    }
    void StartTask3()
    {
        currentProgress = GameProgress.Task3;
        Log("开始任务3");
        OnTask3Start?.Invoke();
    }
    void CompleteTask3()
    {
        task3Completed = true;
        Log("任务3完成,触发剧情3");
        OnTask3Complete?.Invoke();
        StartPlot3();
    }
    void StartFinalTask()
    {
        currentProgress = GameProgress.FinalTask;
        Log("开始最终合成任务");
        OnFinalTaskStart?.Invoke();
    }
    void CompleteFinalTask()
    {
        FinalTaskCompleted = true;
        Log("最终任务完成,触发提问");
        OnFinalTaskComplete?.Invoke();
        StartEnding();
    }
    #endregion


    #region 任务完成检查
    void CheckTask1Completion()
    {
        if (!task1Completed && hasDoorFirstOpen)
        {
            task1Completed = true;
            CompleteTask1();
        }
    }
    void CheckTask2Completion()
    {
        if (!task2Completed && hasDoorSecondOpen)
        {
            task2Completed = true;
            CompleteTask2();
        }
    }
    void CheckTask3Completion()
    {
        if (!task3Completed && hasOverReadDiary)
        {
            task3Completed = true;
            CompleteTask3();
        }
    }
    void CheckFinalTaskCompletion()
    {
        if (!FinalTaskCompleted && CanQuestion)
        {
            FinalTaskCompleted = true;
            CompleteFinalTask();
        }
    }
    #endregion

    #region 开始剧情
    public void StartPlot1()
    {
        currentProgress = GameProgress.Plot1;
        Log("开始剧情1");
        dialogueManager.StartDialogueSequence("Plot1");
        plot1Completed = true;
    }

    public void StartPlot2()
    {
        currentProgress = GameProgress.Plot2;
        Log("开始剧情2");
        dialogueManager.StartDialogueSequence("Plot2");
        plot2Completed = true;
    }

    public void StartPlot3()
    {
        currentProgress = GameProgress.Plot3;
        Log("开始剧情3");
        dialogueManager.StartDialogueSequence("Plot3");
        plot3Completed = true;
    }
    public void StartEnding()
    {
        currentProgress = GameProgress.Ending;
        Log("开始提问剧情");
        dialogueManager.StartDialogueSequence("Ending");
        EndingCompleted = true;
        StartCoroutine(MonitorVideoEnd());
    }

    #endregion

    // 轮询剧情3对话是否播放完毕
    private IEnumerator MonitorVideoEnd()
    {
        // 等待剧情3对话结束
        while (IsDialoguePlaying())
        {
            yield return null;
        }

        Log("剧情3对话播放完毕，延迟跳转场景");
        StartCoroutine(DelayedSceneTransition(1f)); // 延迟2秒跳场景
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

    #region 场景跳转
    private IEnumerator DelayedSceneTransition(float delay)
    {
        yield return new WaitForSeconds(delay);
        PrepareNextScene();
    }

    void PrepareNextScene()
    {
        //currentProgress = GameProgress.Complete;
        Log("Scene3剧情完成，准备加载下一场景");
        OnEndingComplete?.Invoke();

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
        if (enableDebugLogs) Debug.Log($"[Scene4UI_Manager] {msg}");
    }

    void LogWarning(string msg)
    {
        if (enableDebugLogs) Debug.LogWarning($"[Scene4UI_Manager] {msg}");
    }
    #endregion
}
