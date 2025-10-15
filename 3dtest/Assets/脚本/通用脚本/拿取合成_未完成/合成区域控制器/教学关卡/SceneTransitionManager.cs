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

        // 淡出
        yield return StartCoroutine(FadeOut());

        isTransitioning = false;
        Debug.Log("场景过渡完成");
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