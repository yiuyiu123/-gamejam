using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;


public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("场景设置")]
    public string mainSceneName = "MainScene";

    private InspectableItem currentInspectItem;
    private string currentMainScene;
    private bool isInInspectMode = false;

    void Awake()
    {
        // 单例模式
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

        // 保存当前主场景名称
        currentMainScene = SceneManager.GetActiveScene().name;

        // 异步加载查看场景（叠加模式）
        StartCoroutine(LoadSceneAdditive(inspectSceneName));
    }

    private IEnumerator LoadSceneAdditive(string sceneName)
    {
        // 显示加载界面（可选）
        ShowLoadingScreen();

        // 异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            // 可以在这里更新加载进度条
            yield return null;
        }

        // 设置新场景为活动场景
        Scene newScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(newScene);

        isInInspectMode = true;

        // 隐藏加载界面
        HideLoadingScreen();

        Debug.Log($"进入查看模式，主场景: {currentMainScene}");
    }

    public void ReturnToMainScene()
    {
        if (!isInInspectMode) return;

        // 获取当前查看场景
        Scene inspectScene = SceneManager.GetActiveScene();

        // 异步卸载查看场景
        StartCoroutine(UnloadInspectScene(inspectScene));
    }

    private IEnumerator UnloadInspectScene(Scene inspectScene)
    {
        // 显示加载界面（可选）
        ShowLoadingScreen();

        // 卸载查看场景
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(inspectScene);

        while (!asyncUnload.isDone)
        {
            yield return null;
        }

        // 重新激活主场景
        Scene mainScene = SceneManager.GetSceneByName(currentMainScene);
        SceneManager.SetActiveScene(mainScene);

        isInInspectMode = false;

        // 隐藏加载界面
        HideLoadingScreen();

        Debug.Log("返回主场景");
    }

    void ShowLoadingScreen()
    {
        // 实现加载界面的显示逻辑
        // 例如：激活一个包含加载动画的Canvas
    }

    void HideLoadingScreen()
    {
        // 实现加载界面的隐藏逻辑
    }
}