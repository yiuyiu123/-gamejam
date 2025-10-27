using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

public class Ending : MonoBehaviour
{
    [Header("引用脚本")]
    public newmanage newManager;
    public static event Action OnScene5Loaded;

    [Header("UI组件")]
    public GameObject I_Yes;
    public GameObject I_No;
    public GameObject I_mask1;
    public GameObject I_mask2;
    public GameObject panel_Ending;    // 协作成功UI
    public TMP_Text typewriterText;    // 打字提示文本

    [Header("结局视频")]
    public VideoPlayer video_Ending1; // Yes
    public VideoPlayer video_Ending2; // No

    [Header("音效")]
    public AudioSource audioSource;
    public AudioClip sureSound;

    [Header("操作参数")]
    public float skipHoldTime = 5f;
    private bool isHoldingSpace = false;
    private float spaceHoldTime = 0f;
    private bool isPlayingEnding = false;
    private bool isChoiceMade = false;

    private void Start()
    {
        // 初始化UI
        if (panel_Ending != null) panel_Ending.SetActive(false);
        if (I_Yes != null) I_Yes.SetActive(false);
        if (I_No != null) I_No.SetActive(false);
        if (I_mask1 != null) I_mask1.SetActive(false);
        if (I_mask2 != null) I_mask2.SetActive(false);

        // 订阅事件（一次性）
        if (newManager != null)
            newManager.OnStartEnding += OnChoose;
    }

    private void OnDestroy()
    {
        if (newManager != null)
            newManager.OnStartEnding -= OnChoose;
    }

    void OnChoose()
    {
        Debug.Log("监听到进入结局UI");
        StartCoroutine(ShowPanelAndTypewriter());
    }

    // 淡入Panel + 打字提示 + 显示选择按钮
    private IEnumerator ShowPanelAndTypewriter()
    {
        if (panel_Ending != null)
        {
            //panel_Ending.SetActive(true);
            yield return StartCoroutine(FadeInPanel(panel_Ending));
        }

        if (typewriterText != null)
        {
            string message = "是否选择交换？";
            typewriterText.text = "";
            foreach (char c in message)
            {
                typewriterText.text += c;
                yield return new WaitForSeconds(0.05f); // 打字速度
            }
        }

        if (I_Yes != null) I_Yes.SetActive(true);
        if (I_No != null) I_No.SetActive(true);
    }

    // 简单淡入协程
    private IEnumerator FadeInPanel(GameObject panel)
    {
        Image img = panel.GetComponent<Image>();
        if (img == null) yield break;

        Color startColor = img.color;
        startColor.a = 0f;
        img.color = startColor;

        float duration = 1f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            img.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
    }

    private void Update()
    {
        if (!isChoiceMade)
        {
            // 玩家1选择 Yes (F键)
            if (CompareTag("Player1") && Input.GetKeyDown(KeyCode.F))
            {
                MakeChoice(true);
            }

            // 玩家2选择 No (H键)
            if (CompareTag("Player2") && Input.GetKeyDown(KeyCode.H))
            {
                MakeChoice(false);
            }
        }

        // 按住空格播放结局逻辑（可选）
        if ((I_Yes.activeSelf || I_No.activeSelf) && Input.GetKey(KeyCode.Space))
        {
            spaceHoldTime += Time.deltaTime;

            if (spaceHoldTime >= skipHoldTime && !isPlayingEnding)
            {
                isPlayingEnding = true;
                StartCoroutine(PlayEndingVideo());
            }
        }
        else
        {
            spaceHoldTime = 0f;
        }
    }

    private void MakeChoice(bool isYes)
    {
        isChoiceMade = true;

        if (sureSound != null)
            audioSource.PlayOneShot(sureSound);

        if (I_mask1 != null) I_mask1.SetActive(isYes);
        if (I_mask2 != null) I_mask2.SetActive(!isYes);

        Debug.Log(isYes ? "玩家选择 YES" : "玩家选择 NO");

        // 保存选择状态，用于播放不同视频
        StartCoroutine(PlayEndingVideo());
    }

    private IEnumerator PlayEndingVideo()
    {
        Debug.Log("开始播放结局视频");

        // 隐藏选择按钮
        if (I_Yes != null) I_Yes.SetActive(false);
        if (I_No != null) I_No.SetActive(false);

        yield return null;

        // 判断播放哪一个结局
        if (CompareTag("Player1") && video_Ending1 != null)
        {
            video_Ending1.gameObject.SetActive(true);
            video_Ending1.Play();
        }
        else if (CompareTag("Player2") && video_Ending2 != null)
        {
            video_Ending2.gameObject.SetActive(true);
            video_Ending2.Play();
        }

        // 等待视频播放完毕
        while ((video_Ending1 != null && video_Ending1.isPlaying) || (video_Ending2 != null && video_Ending2.isPlaying))
        {
            yield return null;
        }

        // 延迟2秒再切场
        yield return new WaitForSeconds(2f);

        // 跳转scene5
        UnityEngine.SceneManagement.SceneManager.LoadScene("scene5");
        OnScene5Loaded?.Invoke();
    }
}
