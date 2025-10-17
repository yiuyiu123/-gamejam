using UnityEngine;
using TMPro;
using System.Collections;

public class TMPTypewriter : MonoBehaviour
{
    public float charInterval = 0.05f; // 每个字间隔

    private TMP_Text tmpText;
    private string fullText;

    private readonly char[] punctuation = new char[] { '，', '。', '！', '？', ',', '.', '!', '?' };

    private void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
    }

    public void StartTyping(string text)
    {
        StopAllCoroutines();
        fullText = text;
        tmpText.text = "";
        StartCoroutine(TypeRoutine());
    }

    private IEnumerator TypeRoutine()
    {
        // 开始循环播放滚动音效
        AudioManager.Instance.PlayLoop("文字滚动");

        for (int i = 0; i < fullText.Length; i++)
        {
            tmpText.text += fullText[i];

            // 检测标点符号，暂停音效
            if (System.Array.IndexOf(punctuation, fullText[i]) >= 0)
            {
                AudioManager.Instance.Pause("文字滚动", fadeOut: false); // 不淡出，立即暂停
                yield return new WaitForSeconds(charInterval * 2); // 标点停顿
                AudioManager.Instance.PlayLoop("文字滚动"); // 重新循环播放
            }

            yield return new WaitForSeconds(charInterval);
        }

        // 播放完成，停止音效
        AudioManager.Instance.Pause("文字滚动", fadeOut: false);
    }
}
