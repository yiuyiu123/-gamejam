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

    private bool canAcceptInput = false;
    private bool isFadingIn = false;
    private bool isPlayingEnding = false;
    private bool isHoldingSpace = false;
    private float spaceHoldTime = 0f;
    private bool isChoiceMade = false;
    private bool hasStartedEndingCoroutine = false;

    private void Start()
    {
        // 初始化UI
        panel_Ending?.SetActive(false);
        I_Yes?.SetActive(false);
        I_No?.SetActive(false);
        I_mask1?.SetActive(false);
        I_mask2?.SetActive(false);

        // 订阅事件，只一次
        if (newManager != null)
            newManager.OnStartEnding += OnChoose;
    }

    private void OnDestroy()
    {
        if (newManager != null)
            newManager.OnStartEnding -= OnChoose;
    }

    private void OnChoose()
    {
        if (hasStartedEndingCoroutine) return; // 防止重复触发
        hasStartedEndingCoroutine = true;

        Debug.Log("监听到进入结局UI");
        canAcceptInput = true; // 允许 FH 输入
        StartCoroutine(ShowPanelAndTypewriter());
    }

    private IEnumerator ShowPanelAndTypewriter()
    {
        if (panel_Ending == null || panel_Ending.activeSelf || isFadingIn)
            yield break;

        // 淡入 Panel
        yield return StartCoroutine(FadeInPanel(panel_Ending));

        // 打字机显示提示
        if (typewriterText != null)
        {
            string message = "是否选择交换？";
            typewriterText.text = "";
            foreach (char c in message)
            {
                typewriterText.text += c;
                yield return new WaitForSeconds(0.05f);
            }
        }

        // 显示选择按钮
        I_Yes?.SetActive(true);
        I_No?.SetActive(true);
    }

    private IEnumerator FadeInPanel(GameObject panel)
    {
        if (panel == null || isFadingIn) yield break;
        isFadingIn = true;

        panel.SetActive(true);
        Image img = panel.GetComponent<Image>();
        if (img == null) { isFadingIn = false; yield break; }

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

        img.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
        isFadingIn = false;
    }

    private void Update()
    {
        if (!canAcceptInput || isPlayingEnding)
            return;

        // 空格未按满时，FH 可切换选择
        if (!isChoiceMade)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                I_mask1?.SetActive(true);
                I_mask2?.SetActive(false);
                Debug.Log("玩家按 F 选择 YES");
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                I_mask1?.SetActive(false);
                I_mask2?.SetActive(true);
                Debug.Log("玩家按 H 选择 NO");
            }
        }

        // 空格按住播放结局
        if ((I_mask1.activeSelf || I_mask2.activeSelf) && Input.GetKey(KeyCode.Space))
        {
            spaceHoldTime += Time.deltaTime;

            if (!isHoldingSpace && spaceHoldTime > 0.5f)
            {
                isHoldingSpace = true;
                Debug.Log("正在按住空格...");
            }

            if (spaceHoldTime >= skipHoldTime)
            {
                isPlayingEnding = true;
                Debug.Log("空格按满，播放结局视频");
                StartCoroutine(PlayEndingVideo());
            }
        }
        else
        {
            if (isHoldingSpace)
                Debug.Log("松开空格，重置计时");

            isHoldingSpace = false;
            spaceHoldTime = 0f;
        }
    }

    // 可保留 MakeChoice 用于其他逻辑或音效
    private void MakeChoice(bool isYes)
    {
        if (sureSound != null)
            audioSource.PlayOneShot(sureSound);

        I_mask1?.SetActive(isYes);
        I_mask2?.SetActive(!isYes);

        Debug.Log(isYes ? "玩家选择 YES" : "玩家选择 NO");
    }

    private IEnumerator PlayEndingVideo()
    {
        Debug.Log("开始播放结局视频");

        // 隐藏选择按钮
        I_Yes?.SetActive(false);
        I_No?.SetActive(false);

        // 判断播放哪个结局
        if (I_mask1.activeSelf && video_Ending1 != null)
        {
            I_mask1.SetActive(false);
            video_Ending1.gameObject.SetActive(true);
            video_Ending1.Play();
        }
        else if (I_mask2.activeSelf && video_Ending2 != null)
        {
            I_mask2.SetActive(false);
            video_Ending2.gameObject.SetActive(true);
            video_Ending2.Play();
        }

        // 等待视频播放完毕
        while ((video_Ending1 != null && video_Ending1.isPlaying) ||
               (video_Ending2 != null && video_Ending2.isPlaying))
        {
            yield return null;
        }

        // 延迟2秒再切场
        yield return new WaitForSeconds(9f);

        // 跳转scene5
        UnityEngine.SceneManagement.SceneManager.LoadScene("scene5");
        OnScene5Loaded?.Invoke();
    }
}
