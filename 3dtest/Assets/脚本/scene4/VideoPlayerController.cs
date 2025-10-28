using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

public class VideoPlayerController : MonoBehaviour
{
    [Header("引用脚本")]
    public static bool isChooseEnding1 = true;
    public static bool lastChoiceWasYes = true;
    public TextMeshProUGUI T_Question;
    public GameObject XH_music;

    [Header("按钮设置")]
    public Button leftButton;      // 左边按钮 
    public Button rightButton;     // 右边按钮 

    [Header("视频播放器设置")]
    public VideoPlayer videoPlayer1;  // 视频1播放器 
    public VideoPlayer videoPlayer2;  // 视频2播放器 

    [Header("视频渲染组件")]
    public RawImage videoDisplay1;   // 视频1显示组件 
    public RawImage videoDisplay2;   // 视频2显示组件 

    [Header("场景设置")]
    public string nextSceneName;     // 下一个场景的名称

    private bool isFirstChoiceMade = false; // 标记是否已经做出第一次选择 
    private VideoPlayer currentVideoPlayer; // 当前正在播放的视频播放器 

    void Start()
    {
        if (T_Question == null)
        {
            Debug.Log("提问文字为空");
        }
        // 初始化隐藏所有视频 
        HideAllVideos();

        // 绑定按钮点击事件 
        leftButton.onClick.AddListener(PlayVideo1);
        rightButton.onClick.AddListener(PlayVideo2);

        // 监听视频播放结束事件
        videoPlayer1.loopPointReached += OnVideoEnd;
        videoPlayer2.loopPointReached += OnVideoEnd;
    }

    // 播放视频1 
    void PlayVideo1()
    {
        // 停止所有视频 
        StopAllVideos();
        T_Question.gameObject.SetActive(false);
        XH_music.SetActive(false);

        isChooseEnding1 = true;
        lastChoiceWasYes = true;

    // 显示视频1 
    videoDisplay1.gameObject.SetActive(true);

        // 播放视频1 
        videoPlayer1.Play();
        currentVideoPlayer = videoPlayer1;

        Debug.Log("开始播放视频1");

        // 隐藏按钮（如果是第一次选择）
        HideButtonsAfterFirstChoice();
    }

    // 播放视频2 
    void PlayVideo2()
    {
        // 停止所有视频 
        StopAllVideos();
        T_Question.gameObject.SetActive(false);
        XH_music.SetActive(false);

        isChooseEnding1 = false;
        lastChoiceWasYes = false;

        // 显示视频2
        videoDisplay2.gameObject.SetActive(true);

        // 播放视频2 
        videoPlayer2.Play();
        currentVideoPlayer = videoPlayer2;

        Debug.Log("开始播放视频2");

        // 隐藏按钮（如果是第一次选择）
        HideButtonsAfterFirstChoice();
    }

    // 视频播放结束事件 
    void OnVideoEnd(VideoPlayer source)
    {
        Debug.Log("视频播放完毕，准备加载下一个场景: " + nextSceneName);

        // 检查场景名称是否有效
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            // 加载下一个场景
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("下一个场景名称未设置！");
        }
    }

    // 第一次选择后隐藏按钮 
    void HideButtonsAfterFirstChoice()
    {
        if (!isFirstChoiceMade)
        {
            isFirstChoiceMade = true;
            HideAllButtons();
        }
    }

    // 隐藏所有按钮 
    void HideAllButtons()
    {
        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);
    }

    // 显示所有按钮（如果需要重新显示）
    public void ShowAllButtons()
    {
        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);
        isFirstChoiceMade = false;
    }

    // 停止所有视频并隐藏 
    void StopAllVideos()
    {
        // 安全停止视频播放
        if (videoPlayer1 != null && !videoPlayer1.Equals(null))
        {
            if (videoPlayer1.isPlaying)
                videoPlayer1.Stop();
        }

        if (videoPlayer2 != null && !videoPlayer2.Equals(null))
        {
            if (videoPlayer2.isPlaying)
                videoPlayer2.Stop();
        }

        // 隐藏视频显示
        if (videoDisplay1 != null && !videoDisplay1.Equals(null))
            videoDisplay1.gameObject.SetActive(false);

        if (videoDisplay2 != null && !videoDisplay2.Equals(null))
            videoDisplay2.gameObject.SetActive(false);
    }

    // 隐藏所有视频显示 
    void HideAllVideos()
    {
        videoDisplay1.gameObject.SetActive(false);
        videoDisplay2.gameObject.SetActive(false);
    }

    // 可选：当脚本禁用时停止所有视频 
    void OnDisable()
    {
        // 移除事件监听（安全检查）
        if (videoPlayer1 != null && !videoPlayer1.Equals(null))
            videoPlayer1.loopPointReached -= OnVideoEnd;

        if (videoPlayer2 != null && !videoPlayer2.Equals(null))
            videoPlayer2.loopPointReached -= OnVideoEnd;

        // 尝试安全停止视频
        try
        {
            StopAllVideos();
        }
        catch (MissingReferenceException)
        {
            Debug.LogWarning("OnDisable() 调用 StopAllVideos 时 VideoPlayer 已被销毁，跳过停止操作。");
        }

    }
}