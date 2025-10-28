using UnityEditor;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance { get; private set; }

    [Header("播放其他结局")]
    public GameObject _1_back; 
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
        if (ending == null) FindObjectOfType<Ending>();
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
        if (ending.isChooseEnding1)
        {
            PlayVideo(Ending1);
        }
        if (!ending.isChooseEnding1){
            PlayVideo(Ending2);
        }
    }

    private void PlayVideo(VideoPlayer player)
    {
        player.gameObject.SetActive(true);
        StartCoroutine(WaitForVideoEnd(player));

    }
    private IEnumerator WaitForVideoEnd(VideoPlayer vp)
    {
        while (vp.isPlaying)
            yield return null;

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
        if (_1_back != null) _1_back.SetActive(false);
        if (_1_team != null) _1_team.SetActive(false);
        if (Ending1 != null) Ending1.gameObject.SetActive(false);
        if (Ending2 != null) Ending2.gameObject.SetActive(false);
    }
}
