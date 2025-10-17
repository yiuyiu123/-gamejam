using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimpleSceneTransitionManager : MonoBehaviour
{
    public static SimpleSceneTransitionManager Instance;

    [Header("过渡设置")]
    public float fadeDuration = 1.5f;

    private GameObject blackScreen;
    private bool isTransitioning = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateSimpleBlackScreen();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void CreateSimpleBlackScreen()
    {
        Debug.Log("创建简化黑幕...");

        // 创建黑幕对象
        blackScreen = new GameObject("SimpleBlackScreen");
        blackScreen.transform.SetParent(transform);

        // 添加Canvas
        Canvas canvas = blackScreen.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        // 添加Image
        Image image = blackScreen.AddComponent<Image>();
        image.color = Color.black;

        // 设置全屏
        RectTransform rect = blackScreen.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // 初始隐藏
        blackScreen.SetActive(false);

        Debug.Log("简化黑幕创建完成");
    }

    public IEnumerator TransitionToScene(string sceneName)
    {
        if (isTransitioning) yield break;

        isTransitioning = true;
        Debug.Log($"开始过渡到场景: {sceneName}");

        // 淡入
        yield return StartCoroutine(FadeIn());

        // 加载场景
        SceneManager.LoadScene(sceneName);
        yield return null; // 等待一帧

        // 关键修复：在新场景中调用阴影修复
        yield return StartCoroutine(FixNewSceneShadows());

        // 淡出
        yield return StartCoroutine(FadeOut());

        isTransitioning = false;
        Debug.Log("场景过渡完成");
    }

    // 新增：修复新场景的阴影
    private IEnumerator FixNewSceneShadows()
    {
        Debug.Log("开始修复新场景阴影设置...");

        // 等待一帧确保所有对象已初始化
        yield return null;

        // 查找分屏管理器并强制修复阴影
        SplitScreenManager splitScreenManager = FindObjectOfType<SplitScreenManager>();
        if (splitScreenManager != null)
        {
            splitScreenManager.ForceFixShadows();
            Debug.Log("找到分屏管理器，已调用阴影修复");
        }
        else
        {
            Debug.LogWarning("未找到分屏管理器，尝试手动修复阴影");
            // 如果没有分屏管理器，手动修复基本阴影设置
            ManualShadowFix();
        }

        // 额外等待一帧确保修复完成
        yield return null;
    }

    // 手动阴影修复作为备用方案
    private void ManualShadowFix()
    {
        // 设置合理的阴影距离
        QualitySettings.shadowDistance = 100f;

        // 修复方向光阴影设置
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                light.shadowBias = 0.05f;
                light.shadowNormalBias = 1.0f;
                light.shadowStrength = 1.0f;
            }
        }

        // 更新环境光照
        DynamicGI.UpdateEnvironment();

        Debug.Log("手动阴影修复完成");
    }

    private IEnumerator FadeIn()
    {
        Debug.Log("开始淡入");

        if (blackScreen != null)
        {
            blackScreen.SetActive(true);
            Image image = blackScreen.GetComponent<Image>();
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                Color color = image.color;
                color.a = alpha;
                image.color = color;
                yield return null;
            }

            // 确保完全不透明
            Color finalColor = image.color;
            finalColor.a = 1f;
            image.color = finalColor;
        }

        Debug.Log("淡入完成");
    }

    private IEnumerator FadeOut()
    {
        Debug.Log("开始淡出");

        if (blackScreen != null)
        {
            Image image = blackScreen.GetComponent<Image>();
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
                Color color = image.color;
                color.a = alpha;
                image.color = color;
                yield return null;
            }

            // 隐藏黑幕
            blackScreen.SetActive(false);
        }

        Debug.Log("淡出完成");
    }

    [ContextMenu("测试过渡")]
    public void TestTransition()
    {
        StartCoroutine(TransitionToScene(SceneManager.GetActiveScene().name));
    }
}