using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartVideoPlay : MonoBehaviour
{
    public RawImage rawImage;           
    public VideoPlayer videoPlayer;    
    public GameObject skipButton;       
    public float skipHoldTime = 1.0f;
    public float fadeTime = 2.0f;

    private bool skipPressed = false;

    private void Start()
    {
        // 判断是否第一次进入游戏
        bool hasSeenVideo = PlayerPrefs.GetInt("HasSeenIntroVideo", 0) == 1;
        // 第一次进入游戏隐藏按钮，之后显示
        //skipButton.SetActive(hasSeenVideo);
        videoPlayer.Play();
        // 监听视频播放完成
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            skipPressed = true;
        }
        else
        {
            skipPressed = false;
        }

        if (skipPressed)
        {
            skipHoldTime -= Time.deltaTime;
            if (skipHoldTime <= 0)
            {
                SkipVideo();
            }
        }
        else
        {
            skipHoldTime = 1.0f; // 重置倒计时
        }
    }

    public void OnClickSkipButton()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
        OnVideoFinished(videoPlayer);
    }

    private void SkipVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            print("成功跳过");
        }
        OnVideoFinished(videoPlayer);
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // 标记已经看过，存储在缓存里
        PlayerPrefs.SetInt("HasSeenIntroVideo", 1);
        PlayerPrefs.Save();

        // 多帧协程启动函数，开启渐变加载场景1
        StartCoroutine(LoadScene1());
    }

    private IEnumerator LoadScene1()
    {
        // 渐变淡出
        CanvasGroup cg = rawImage.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = rawImage.gameObject.AddComponent<CanvasGroup>();
        }
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            cg.alpha = 1 - (t / fadeTime);
            yield return null;
        }
        // 确保全黑
        cg.alpha = 0;

        // 加载场景1
        SceneManager.LoadScene("scene1");
    }
}
