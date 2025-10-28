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
    public Ending ending;

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
        if (ending == null) ending = FindObjectOfType<Ending>();

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
        bool choiceYes;
        if (ending != null)
        {
            choiceYes = Ending.isChooseEnding1;
        }
        else
        {
            // fallback 使用静态字段（如果你在 Ending 中设置了 lastChoiceWasYes）
            choiceYes = Ending.lastChoiceWasYes;
            // 如果你没有静态字段，这里可以设默认值： bool choiceYes = true;
        }

        // 播放对应视频（并防止 player 为 null）
        if (choiceYes)
        {
            if (Ending1 != null) PlayVideo(Ending1);
            else Debug.LogWarning("Ending1 未分配");
        }
        else
        {
            if (Ending2 != null) PlayVideo(Ending2);
            else Debug.LogWarning("Ending2 未分配");
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
