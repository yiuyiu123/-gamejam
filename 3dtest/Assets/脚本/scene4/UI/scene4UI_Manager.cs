using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Scene4UI_Manager : MonoBehaviour
{
    public static Scene4UI_Manager Instance;

    [Header("��Ϸ����״̬")]
    public GameProgress currentProgress = GameProgress.Initial;

    [Header("���ýű�")]
    public ItemTrigger DoorDelete;
    public PlayerMovement playerMovement;
    public ThreeItemCraftingManager threeItemCraftingManager;

    private bool hasDoorFirstOpen = false;
    private bool hasDoorSecondOpen = false;
    private bool hasOverReadDiary = false;
    private bool CanQuestion = false;


    [Header("�߼�����")]
    public Scene4DialogueManager dialogueManager; 
    public bool useSceneTransition = true;
    public string nextSceneName = "scene5";

    [Header("�����¼�")]
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

    [Header("����ѡ��")]
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
        Task1,   // ���ص���1����ϳ�����
        Plot1,   // ����1
        Task2,   // ���ص���2����ϳ�����
        Plot2,   // ����2
        Task3,   // ����ƪ�ռ��Ķ���
        Plot3,   // ����3
        FinalTask, // ���պϳ�����ɹ�
        Ending, //��ʼ����UI
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
        //�ռǱ������¼�����
        if (DoorDelete != null)
        {
            DoorDelete.OpenFirstDoor += OnDoorOpened1;
            DoorDelete.OpenSecondDoor += OnDoorOpened2;
        }
        // ����ƪ�ռ��Ķ���playerMovement����3����ײ��
        //if (playerMovement != null)
            //playerMovement.HasKeyAppear += OverReadDiary;
        // �������պϳ��¼�
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
        Log("Scene4����������");

        // ��������1
        if (currentProgress == GameProgress.Initial)
        {
            StartTask1();
        }
        while (true)
        {
            yield return null;
        }
    }

    #region �����¼�
    void OnDoorOpened1()
    {
        DoorDelete.OpenFirstDoor -= OnDoorOpened1;
        Debug.Log("�¼���������������");
        hasDoorFirstOpen = true;
        CheckTask1Completion();
    }

    void OnDoorOpened2()
    {
        DoorDelete.OpenSecondDoor -= OnDoorOpened2;
        Debug.Log("�¼������������ռǱ�");
        hasDoorSecondOpen = true;
        CheckTask2Completion();
    }

    void OverReadDiary()
    {
        Debug.Log("�¼�������A�Ķ����ռǱ�");
        hasOverReadDiary = true;
        CheckTask3Completion();
    }
    void OnQuestion()
    {
        CanQuestion = true;
        CheckFinalTaskCompletion();
    }
    #endregion


    #region �������
    void StartTask1()
    {
        currentProgress = GameProgress.Task1;
        Log("��ʼ����1���ϳ�����");
        OnTask1Start?.Invoke();
    }

    void CompleteTask1()
    {
        task1Completed = true;
        Log("����1��ɣ���������1");
        OnTask1Complete?.Invoke();
        StartPlot1();
    }

    void StartTask2()
    {
        currentProgress = GameProgress.Task2;
        Log("��ʼ����2���ϳ��ռǱ�");
        OnTask2Start?.Invoke();
    }

    void CompleteTask2()
    {
        task2Completed = true;
        Log("����2���,��������2");
        OnTask2Complete?.Invoke();
        StartPlot2();
    }
    void StartTask3()
    {
        currentProgress = GameProgress.Task3;
        Log("��ʼ����3");
        OnTask3Start?.Invoke();
    }
    void CompleteTask3()
    {
        task3Completed = true;
        Log("����3���,��������3");
        OnTask3Complete?.Invoke();
        StartPlot3();
    }
    void StartFinalTask()
    {
        currentProgress = GameProgress.FinalTask;
        Log("��ʼ���պϳ�����");
        OnFinalTaskStart?.Invoke();
    }
    void CompleteFinalTask()
    {
        FinalTaskCompleted = true;
        Log("�����������,��������");
        OnFinalTaskComplete?.Invoke();
        StartEnding();
    }
    #endregion


    #region ������ɼ��
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

    #region ��ʼ����
    public void StartPlot1()
    {
        currentProgress = GameProgress.Plot1;
        Log("��ʼ����1");
        dialogueManager.StartDialogueSequence("Plot1");
        plot1Completed = true;
    }

    public void StartPlot2()
    {
        currentProgress = GameProgress.Plot2;
        Log("��ʼ����2");
        dialogueManager.StartDialogueSequence("Plot2");
        plot2Completed = true;
    }

    public void StartPlot3()
    {
        currentProgress = GameProgress.Plot3;
        Log("��ʼ����3");
        dialogueManager.StartDialogueSequence("Plot3");
        plot3Completed = true;
    }
    public void StartEnding()
    {
        currentProgress = GameProgress.Ending;
        Log("��ʼ���ʾ���");
        dialogueManager.StartDialogueSequence("Ending");
        EndingCompleted = true;
        StartCoroutine(MonitorVideoEnd());
    }

    #endregion

    // ��ѯ����3�Ի��Ƿ񲥷����
    private IEnumerator MonitorVideoEnd()
    {
        // �ȴ�����3�Ի�����
        while (IsDialoguePlaying())
        {
            yield return null;
        }

        Log("����3�Ի�������ϣ��ӳ���ת����");
        StartCoroutine(DelayedSceneTransition(1f)); // �ӳ�2��������
    }

    // ʹ�� DialogueManager ��״̬�ж�
    private bool IsDialoguePlaying()
    {
        // ͨ���������˽���ֶ� isDialogueActive
        if (dialogueManager == null) return false;

        var type = dialogueManager.GetType();
        var field = type.GetField("isDialogueActive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return (bool)field.GetValue(dialogueManager);
        }

        return false;
    }

    #region ������ת
    private IEnumerator DelayedSceneTransition(float delay)
    {
        yield return new WaitForSeconds(delay);
        PrepareNextScene();
    }

    void PrepareNextScene()
    {
        //currentProgress = GameProgress.Complete;
        Log("Scene3������ɣ�׼��������һ����");
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

    #region ����
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
