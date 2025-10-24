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
    public PlayerController player1Controller;
    public PlayerController player2Controller;
    public DetectLightSwitch detectLightSwitch;
    public FlowerPotZone flowerPotZone;

    private bool hasFlashlight = false;
    private bool hasOpenedSwitch = false;
    private bool keyHasAppeared = false;


    [Header("�߼�����")]
    public Scene2DialogueManager dialogueManager; // ͨ�öԻ�ϵͳ
    public bool useSceneTransition = true;
    public string nextSceneName = "scene4";

    [Header("�����¼�")]
    public UnityEvent OnTask1Start;
    public UnityEvent OnTask1Complete;
    public UnityEvent OnTask2Start;
    public UnityEvent OnTask2Complete;
    public UnityEvent OnAllTasksComplete;

    [Header("����ѡ��")]
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
        Plot1,   // ��բ����ǰ����
        Task1,   // �ҵ�բ
        Plot2,   // Կ������ǰ����
        Task2,   // ��Կ��
        Plot3,   // ���ž���
        Complete // ȫ�����
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
        // ���� Player1��Player2 ��ʰȡ�¼�
        if (player1Controller != null)
            player1Controller.OnFlashlightPickedUp += OnFlashlightPickedUp;
        if (player2Controller != null)
            player2Controller.OnFlashlightPickedUp += OnFlashlightPickedUp;

        // ��բ
        if (detectLightSwitch != null)
            detectLightSwitch.LightSwitchOn += OnLightSwitchOn;

        // Կ������
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
        Log("Scene3����������");

        // ������ʼ����
        if (currentProgress == GameProgress.Initial)
        {
            StartPlot1();
        }

        while (true)
        {
            yield return null;
        }
    }

    #region �������
    void OnFlashlightPickedUp()
    {
        Debug.Log("�¼��������ֵ�Ͳ����");
        hasFlashlight = true;
        CheckTask1Completion();
    }

    void OnLightSwitchOn()
    {
        Debug.Log("�¼���������բ��");
        hasOpenedSwitch = true;
        CheckTask1Completion();
    }

    void OnKeyAppear()
    {
        keyHasAppeared = true;
        CheckTask2Completion();
    }

    // ��������������1��ɼ��
    void CheckTask1Completion()
    {
        if (!task1Completed && hasFlashlight && hasOpenedSwitch)
        {
            task1Completed = true;
            CompleteTask1();
        }
    }

    void CheckTask2Completion()
    {
        if (!task2Completed && keyHasAppeared)
        {
            task2Completed = true;
            CompleteTask2();
        }
    }
    public void StartPlot1()
    {
        currentProgress = GameProgress.Plot1;
        Log("��ʼ����1����բ����ǰ�Ի�");

        if (dialogueManager != null)
            dialogueManager.StartDialogueSequence("Plot1");
        else
            LogWarning("�Ի�������δ�ҵ�");

        plot1Completed = true;
        StartTask1();
    }

    public void StartPlot2()
    {
        currentProgress = GameProgress.Plot2;
        Log("��ʼ����2��Կ������ǰ�Ի�");

        if (dialogueManager != null)
        {
            Debug.Log("���� dialogueManager.StartDialogueSequence(\"Plot2\")");
            dialogueManager.StartDialogueSequence("Plot2");
        }
        else
            LogWarning("�Ի�������δ�ҵ�");

        plot2Completed = true;
        StartTask2();
    }

    public void StartPlot3()
    {
        currentProgress = GameProgress.Plot3;
        Log("��ʼ����3�����ž���");

        if (dialogueManager != null)
        {
            dialogueManager.StartDialogueSequence("Plot3");
            // ����Э�̼�ؾ���3�Ի��Ƿ����
            StartCoroutine(MonitorPlot3DialogueEnd());
        }
        else
        {
            LogWarning("�Ի�������δ�ҵ�");
        }

        plot3Completed = true;
    }

    // ��ѯ����3�Ի��Ƿ񲥷����
    private IEnumerator MonitorPlot3DialogueEnd()
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

    private IEnumerator DelayedSceneTransition(float delay)
    {
        yield return new WaitForSeconds(delay);
        PrepareNextScene();
    }
    #endregion

    #region �������
    void StartTask1()
    {
        currentProgress = GameProgress.Task1;
        Log("��ʼ����1��Ѱ�ҵ�բ�ֵ�Ͳ");
        OnTask1Start?.Invoke();
    }

    void CompleteTask1()
    {
        task1Completed = true;
        Debug.Log("����1��ɣ���������2");
        OnTask1Complete?.Invoke();
        // ��������2
        StartPlot2();
    }

    void StartTask2()
    {
        currentProgress = GameProgress.Task2;
        Log("��ʼ����2���ϳɻ���Ѱ��Կ��");
        OnTask2Start?.Invoke();
    }

    void CompleteTask2()
    {
        task2Completed = true;
        Log("����2��ɣ����Կ��");
        OnTask2Complete?.Invoke();
        StartPlot3();
    }
    #endregion

    #region ������ת
    void PrepareNextScene()
    {
        currentProgress = GameProgress.Complete;
        Log("Scene3������ɣ�׼��������һ����");
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

    #region ����
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
