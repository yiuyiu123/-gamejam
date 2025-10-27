using UnityEditor;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance { get; private set; }

    [Header("scene1")]
    public GameObject _1_back;   // 当前面板
    public GameObject _1_set;    // Options 面板
    public GameObject _1_team;   // Team / Credits 面板

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void OnClickBack()
    {
        gameObject.SetActive(false);
        HideAll();
    }

    public void ShowSetMenu()
    {
        _1_set.SetActive(true);
    }

    public void ShowTeamMenu()
    {
        _1_team.SetActive(true);
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
        //if (_0_skip != null) _0_skip.SetActive(false);
        //if (_1_back != null) _1_back.SetActive(false);
        if (_1_set != null) _1_set.SetActive(false);
        if (_1_team != null) _1_team.SetActive(false);
    }
}
