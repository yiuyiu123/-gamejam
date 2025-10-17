using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;


public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("��������")]
    public string mainSceneName = "MainScene";

    private InspectableItem currentInspectItem;
    private string currentMainScene;
    private bool isInInspectMode = false;

    void Awake()
    {
        // ����ģʽ
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCurrentInspectItem(InspectableItem item)
    {
        currentInspectItem = item;
    }

    public void LoadInspectScene(string inspectSceneName)
    {
        if (isInInspectMode) return;

        // ���浱ǰ����������
        currentMainScene = SceneManager.GetActiveScene().name;

        // �첽���ز鿴����������ģʽ��
        StartCoroutine(LoadSceneAdditive(inspectSceneName));
    }

    private IEnumerator LoadSceneAdditive(string sceneName)
    {
        // ��ʾ���ؽ��棨��ѡ��
        ShowLoadingScreen();

        // �첽���س���
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            // ������������¼��ؽ�����
            yield return null;
        }

        // �����³���Ϊ�����
        Scene newScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(newScene);

        isInInspectMode = true;

        // ���ؼ��ؽ���
        HideLoadingScreen();

        Debug.Log($"����鿴ģʽ��������: {currentMainScene}");
    }

    public void ReturnToMainScene()
    {
        if (!isInInspectMode) return;

        // ��ȡ��ǰ�鿴����
        Scene inspectScene = SceneManager.GetActiveScene();

        // �첽ж�ز鿴����
        StartCoroutine(UnloadInspectScene(inspectScene));
    }

    private IEnumerator UnloadInspectScene(Scene inspectScene)
    {
        // ��ʾ���ؽ��棨��ѡ��
        ShowLoadingScreen();

        // ж�ز鿴����
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(inspectScene);

        while (!asyncUnload.isDone)
        {
            yield return null;
        }

        // ���¼���������
        Scene mainScene = SceneManager.GetSceneByName(currentMainScene);
        SceneManager.SetActiveScene(mainScene);

        isInInspectMode = false;

        // ���ؼ��ؽ���
        HideLoadingScreen();

        Debug.Log("����������");
    }

    void ShowLoadingScreen()
    {
        // ʵ�ּ��ؽ������ʾ�߼�
        // ���磺����һ���������ض�����Canvas
    }

    void HideLoadingScreen()
    {
        // ʵ�ּ��ؽ���������߼�
    }
}