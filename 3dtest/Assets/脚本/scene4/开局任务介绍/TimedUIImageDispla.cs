using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimedUIImageDisplay : MonoBehaviour
{
    [Header("UI图片设置")]
    public Image uiImage;

    [Header("显示时间设置")]
    public float displayDuration = 3f;

    [Header("淡入淡出设置")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 1f;

    private CanvasGroup canvasGroup;

    void Start()
    {
        InitializeDisplay();
        StartCoroutine(DisplaySequence());
    }

    void InitializeDisplay()
    {
        if (uiImage == null)
        {
            Debug.LogError("未分配UI Image组件！");
            return;
        }

        if (canvasGroup == null)
        {
            canvasGroup = uiImage.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = uiImage.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0f;
            uiImage.gameObject.SetActive(true);
        }
    }

    IEnumerator DisplaySequence()
    {
        Debug.Log($"开始显示UI图片，将持续 {displayDuration} 秒");

        // 淡入效果 
        yield return StartCoroutine(FadeImage(0f, 1f, fadeInDuration));

        yield return new WaitForSeconds(displayDuration);

        yield return StartCoroutine(FadeImage(1f, 0f, fadeOutDuration));

        uiImage.gameObject.SetActive(false);
        Debug.Log("UI图片显示完成");
    }

    IEnumerator FadeImage(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    public void ShowImage(float customDuration = -1)
    {
        if (customDuration > 0) displayDuration = customDuration;

        StopAllCoroutines();
        InitializeDisplay();
        StartCoroutine(DisplaySequence());
    }

    public void HideImageImmediately()
    {
        StopAllCoroutines();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            uiImage.gameObject.SetActive(false);
        }
    }
}