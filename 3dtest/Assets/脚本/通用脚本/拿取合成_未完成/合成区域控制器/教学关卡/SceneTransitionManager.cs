using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimpleSceneTransitionManager : MonoBehaviour
{
    public static SimpleSceneTransitionManager Instance;

    [Header("��������")]
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
        Debug.Log("�����򻯺�Ļ...");

        // ������Ļ����
        blackScreen = new GameObject("SimpleBlackScreen");
        blackScreen.transform.SetParent(transform);

        // ���Canvas
        Canvas canvas = blackScreen.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        // ���Image
        Image image = blackScreen.AddComponent<Image>();
        image.color = Color.black;

        // ����ȫ��
        RectTransform rect = blackScreen.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // ��ʼ����
        blackScreen.SetActive(false);

        Debug.Log("�򻯺�Ļ�������");
    }

    public IEnumerator TransitionToScene(string sceneName)
    {
        if (isTransitioning) yield break;

        isTransitioning = true;
        Debug.Log($"��ʼ���ɵ�����: {sceneName}");

        // ����
        yield return StartCoroutine(FadeIn());

        // ���س���
        SceneManager.LoadScene(sceneName);
        yield return null; // �ȴ�һ֡

        // ����
        yield return StartCoroutine(FadeOut());

        isTransitioning = false;
        Debug.Log("�����������");
    }

    private IEnumerator FadeIn()
    {
        Debug.Log("��ʼ����");

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

            // ȷ����ȫ��͸��
            Color finalColor = image.color;
            finalColor.a = 1f;
            image.color = finalColor;
        }

        Debug.Log("�������");
    }

    private IEnumerator FadeOut()
    {
        Debug.Log("��ʼ����");

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

            // ���غ�Ļ
            blackScreen.SetActive(false);
        }

        Debug.Log("�������");
    }

    [ContextMenu("���Թ���")]
    public void TestTransition()
    {
        StartCoroutine(TransitionToScene(SceneManager.GetActiveScene().name));
    }
}