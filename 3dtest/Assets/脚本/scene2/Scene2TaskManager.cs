using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Scene2TaskManager : MonoBehaviour
{
    public static Scene2TaskManager Instance;

    [Header("��Ϸ����״̬")]
    public GameProgress currentProgress = GameProgress.Initial;

    [Header("�������")]
    public GameObject player1;
    public GameObject player2;
    public string player1Tag = "Player1";
    public string player2Tag = "Player2";

    [Header("����������")]
    public Scene2DualSynthesisManager synthesisManager;
    public Scene2DialogueManager dialogueManager;

    [Header("�����¼�")]
    public UnityEvent OnTask1Start;
    public UnityEvent OnTask1Complete;
    public UnityEvent OnTask2Start;
    public UnityEvent OnTask2Complete;
    public UnityEvent OnAllTasksComplete;

    [Header("����ѡ��")]
    public bool enableDebugLogs = true;

    // ��Ϸ����ö��
    public enum GameProgress
    {
        Initial,        // ��ʼ״̬
        Plot1,          // ����1
        Task1,          // ����1���ϳ�U��
        Plot2,          // ����2
        Plot4,          // ����4
        Task2,          // ����2���ϳ��Ծ�
        Plot3And5,      // ����3��5
        Complete        // ���
    }

    // ����״̬
    private bool task1Completed = false;
    private bool task2Completed = false;
    private bool plot1Completed = false;
    private bool plot2Completed = false;
    private bool plot3Completed = false;
    private bool plot4Completed = false;
    private bool plot5Completed = false;

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
        InitializeManagers();
        StartGame();
    }

    void InitializeManagers()
    {
        if (synthesisManager == null)
            synthesisManager = FindObjectOfType<Scene2DualSynthesisManager>();
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<Scene2DialogueManager>();

        // ������ұ�ǩ
        if (player1 != null) player1.tag = player1Tag;
        if (player2 != null) player2.tag = player2Tag;

        if (synthesisManager == null)
            LogWarning("Scene2DualSynthesisManager δ�ҵ���");
        if (dialogueManager == null)
            LogWarning("Scene2DialogueManager δ�ҵ���");
    }

    void StartGame()
    {
        Log("��Ϸ��ʼ - Scene2 ˫�˺�������");
        currentProgress = GameProgress.Initial;
    }

    #region �������
    public void StartPlot1()
    {
        if (currentProgress != GameProgress.Initial) return;

        currentProgress = GameProgress.Plot1;
        Log("��ʼ����1���칫��/ѧУ��������");
    }

    public void CompletePlot1()
    {
        plot1Completed = true;
        Log($"����1��ɣ�׼����ʼ����1����ǰ����: {currentProgress}");

        // ����1��ɺ�ʼ����1
        StartTask1();
    }

    public void CompleteTask1()
    {
        task1Completed = true;
        Log("����1��ɣ��ɹ��ϳ������ϵ�U��");

        OnTask1Complete?.Invoke();
        // ����1��ɺ�ʼ����2
        StartPlot2();
    }
    
    public void StartPlot2()
    {
        currentProgress = GameProgress.Plot2;
        Log("��ʼ����2�����A���ϰ�U��");

        // ��������2�Ի�
        if (dialogueManager != null)
        {
            dialogueManager.StartDialogueSequence("Plot2");
        }
    }

    public void CompletePlot2()
    {
        plot2Completed = true;
        Log("����2���");

        // ����2��ɺ�ʼ����4
        StartPlot4();
    }

    public void StartPlot4()
    {
        currentProgress = GameProgress.Plot4;
        Log("��ʼ����4����ʦ����Ծ�");

        // ��������4�Ի�
        if (dialogueManager != null)
        {
            dialogueManager.StartDialogueSequence("Plot4");
        }
    }

    public void CompletePlot4()
    {
        plot4Completed = true;
        Log("����4���");

        // ����4��ɺ�ʼ����2
        StartTask2();
    }

    public void CompleteTask2()
    {
        task2Completed = true;
        Log("����2��ɣ��ɹ��ϳ��Ծ�");

        OnTask2Complete?.Invoke();

        // ����2��ɺ�ʼ����3��5
        StartPlot3And5();
    }

    public void StartPlot3And5()
    {
        currentProgress = GameProgress.Plot3And5;
        Log("��ʼ����3��5���ϰ����ʦͬʱѵ�����");

        // �����ϲ���ĶԻ�
        if (dialogueManager != null)
        {
            dialogueManager.StartDialogueSequence("Plot3And5");
        }
        else
        {
            Log("�Ի�������Ϊnull���޷���ʼ����3��5");
        }
    }

    public void CompletePlot3()
    {
        plot3Completed = true;
        Log("����3���");
    }

    public void CompletePlot5()
    {
        plot5Completed = true;
        Log("����5���");
    }
    #endregion

    #region �������
    void StartTask1()
    {
        currentProgress = GameProgress.Task1;
        Log($"��ʼ����1���ϳ������ϵ�U�̣���ǰ����: {currentProgress}");

        OnTask1Start?.Invoke();

        // ����Scene2ר�úϳɼ��
        if (synthesisManager != null)
        {
            Log("֪ͨ�ϳɹ�������ʼ����1");
            synthesisManager.StartTask1();
        }
        else
        {
            Log("�ϳɹ�����Ϊnull��");
        }
    }
 
    void StartTask2()
    {
        currentProgress = GameProgress.Task2;
        Log("��ʼ����2���ϳ��Ծ�");

        OnTask2Start?.Invoke();

        // ����Scene2ר�úϳɼ��
        if (synthesisManager != null)
        {
            synthesisManager.StartTask2();
        }
    }
    #endregion

    #region ��������
    void PrepareNextScene()
    {
        currentProgress = GameProgress.Complete;
        Log("׼��������һ����");

        OnAllTasksComplete?.Invoke();

        // ������������ӹ��ɶ���
        Invoke("LoadNextScene", 2f);
    }

    void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Log($"������һ����: {nextSceneIndex}");
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Log("�������һ����������Ϸ����");
            // ������Ϸ����
        }
    }

    public void RestartGame()
    {
        Log("���¿�ʼ��Ϸ");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion

    #region ��������
    // �ϳ��¼��ص�
    public void OnTask1SynthesisComplete()
    {
        Log($"OnTask1SynthesisComplete �����ã���ǰ����1���״̬: {task1Completed}");
        if (!task1Completed)
        {
            CompleteTask1();
        }
        else
        {
            LogWarning("����1�Ѿ���ɣ��ظ����� OnTask1SynthesisComplete");
        }
    }

    public void OnTask2SynthesisComplete()
    {
        if (!task2Completed)
        {
            CompleteTask2();
        }
    }

    // �Ի��¼��ص�
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
            case "Plot3And5":  // ��Ϊ����ϲ�������
                CompletePlot3And5();
                break;
            case "Plot4":
                CompletePlot4();
                break;
        }
    }
    public void CompletePlot3And5()
    {
        plot3Completed = true;
        plot5Completed = true;
        Log("����3��5���");

        // ׼����һ����
        PrepareNextScene();
    }

    // ��ȡ��ǰ״̬
    public bool IsTask1Complete() => task1Completed;
    public bool IsTask2Complete() => task2Completed;
    public GameProgress GetCurrentProgress() => currentProgress;
    #endregion

    #region ���Թ���
    [ContextMenu("ǿ�ƿ�ʼ����1")]
    public void DebugStartTask1()
    {
        StartTask1();
    }

    [ContextMenu("ǿ���������1")]
    public void DebugCompleteTask1()
    {
        CompleteTask1();
    }

    [ContextMenu("ǿ�ƿ�ʼ����2")]
    public void DebugStartTask2()
    {
        StartTask2();
    }

    [ContextMenu("ǿ���������2")]
    public void DebugCompleteTask2()
    {
        CompleteTask2();
    }

    [ContextMenu("��ת����һ����")]
    public void DebugNextScene()
    {
        PrepareNextScene();
    }

    [ContextMenu("��ʾ��ǰ״̬")]
    public void DebugShowStatus()
    {
        Log($"=== Scene2����״̬ ===");
        Log($"��ǰ����: {currentProgress}");
        Log($"����1���: {task1Completed}");
        Log($"����2���: {task2Completed}");
        Log($"����1���: {plot1Completed}");
        Log($"����2���: {plot2Completed}");
        Log($"����3���: {plot3Completed}");
        Log($"����4���: {plot4Completed}");
        Log($"����5���: {plot5Completed}");
    }

    [ContextMenu("������Ϸ״̬")]
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

        Log("��Ϸ״̬������");
    }
    #endregion

    #region ���߷���
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