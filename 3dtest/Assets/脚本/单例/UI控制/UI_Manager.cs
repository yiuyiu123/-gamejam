using UnityEditor;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance { get; private set; }

    [Header("播放其他结局")]
    public GameObject _1_team;  
    public VideoPlayer Ending1;
    public VideoPlayer Ending2;
    public VideoPlayerController videoPlayerController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        if (videoPlayerController == null) videoPlayerController = FindObjectOfType<VideoPlayerController>();

        HideAll();

    }
    public void OnClickBack()
    {
        gameObject.SetActive(false);
        HideAll();
    }

    
    public void ShowTeamMenu()
    {
        _1_team.SetActive(true);
    }

    public void PlayTheOtherVideo()
    {
        // 直接使用静态字段
        bool choiceYes = VideoPlayerController.isChooseEnding1;

        // 如果上一个选择是 “Yes”，那么这里播放 “另一个结局”
        if (choiceYes)
        {
            Debug.Log("上次选的是 YES，播放 Ending2（另一个结局）");
            if (Ending2 != null) PlayVideo(Ending2);
            else Debug.LogWarning("Ending2 未分配");
        }
        else
        {
            Debug.Log("上次选的是 NO，播放 Ending1（另一个结局）");
            if (Ending1 != null) PlayVideo(Ending1);
            else Debug.LogWarning("Ending1 未分配");
        }
    }


    private void PlayVideo(VideoPlayer player)
    {
        if (player == null) return;
        player.gameObject.SetActive(true);
        player.Play();
        StartCoroutine(WaitForVideoEnd(player));
    }

    public void OnClickGoToScene2()
    {
        Debug.Log("点击按钮，跳转 Scene2");
        SceneManager.LoadScene("Scene2");
    }
    private IEnumerator WaitForVideoEnd(VideoPlayer vp)
    {
        // 等待开始播放（防止直接检查 isPlaying 还没开始）
        yield return null;
        while (vp.isPlaying)
            yield return null;
        yield return new WaitForSeconds(17f);

        vp.Stop();
        vp.gameObject.SetActive(false);
    }

    public void OnClickQuit()
    {
        #if UNITY_EDITOR
                // 编辑器下停止播放
                EditorApplication.isPlaying = false;
        #else
                // 打包后退出游戏
                Application.Quit();
        #endif
                Debug.Log("退出游戏");
    }

    public void HideAll()
    {
        if (_1_team != null) _1_team.SetActive(false);
        if (Ending1 != null) Ending1.gameObject.SetActive(false);
        if (Ending2 != null) Ending2.gameObject.SetActive(false);
    }
}
