using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPTypewriter : MonoBehaviour
{
    public float typingSpeed = 0.05f;

    public bool IsTyping { get; private set; }
    public string OriginalText { get; private set; }

    private TextMeshProUGUI textMeshPro;
    private Coroutine typingCoroutine;

    // 标点符号，用于音效停顿
    private char[] punctuation = new char[] { '，', '。', '！', '？', ',', '.', '!', '?' };

    /// <summary>
    /// 开始逐字播放
    /// </summary>
    /// <param name="isPlayer1">是否玩家1（左声道）</param>
    public void StartTypewriter(bool isPlayer1 = true)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeRoutine(isPlayer1));
    }

    public void SetTextInstant()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (textMeshPro == null)
            textMeshPro = GetComponent<TextMeshProUGUI>();

        textMeshPro.text = OriginalText;
        IsTyping = false;

        // 停止音效
        AudioManager.Instance.Pause("文字滚动", isFadeOut: false);
        AudioManager.Instance.Pause("文字滚动", isFadeOut: false);
    }

    private IEnumerator TypeRoutine(bool isPlayer1)
    {
        if (textMeshPro == null)
            textMeshPro = GetComponent<TextMeshProUGUI>();

        // 等待 GameObject 完全激活
        while (!gameObject.activeInHierarchy)
            yield return null;

        // 等待 UI 渲染
        yield return null;

        // 获取原始文字
        OriginalText = textMeshPro.text;
        textMeshPro.text = "";

        IsTyping = true;

        // 播放文字滚动音效（循环播放）
        AudioManager.Instance.PlayLoop("文字滚动", 0, isFadeIn: false, isPlayer1: isPlayer1);

        foreach (char c in OriginalText)
        {
            textMeshPro.text += c;

            // 遇到标点符号，暂停音效并停顿
            if (System.Array.IndexOf(punctuation, c) >= 0)
            {
                AudioManager.Instance.Pause("文字滚动",isFadeOut: false);
                yield return new WaitForSeconds(typingSpeed * 2); // 标点停顿
                AudioManager.Instance.PlayLoop("文字滚动", 0, isFadeIn: false,isPlayer1: isPlayer1);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        // 播放完成，停止音效
        AudioManager.Instance.Pause("文字滚动", isFadeOut: false);

        IsTyping = false;
    }

    //scene3新加
    public void SetText(string text)
    {
        OriginalText = text;
        if (textMeshPro == null)
            textMeshPro = GetComponent<TextMeshProUGUI>();
        textMeshPro.text = text; // 可选：直接显示初始文本
    }

}
