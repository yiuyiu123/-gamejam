using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class scene1UI_Manager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject UI_Conversation;
    public GameObject UI_Settings;
    public bool noInputAnything=false;

    void Start()
    {
        //禁用除了FH以外的键盘输入
        if (noInputAnything&&Input.anyKeyDown)
        {
            if (!Input.GetKeyDown(KeyCode.F) && !Input.GetKeyDown(KeyCode.H))
            {
                return;
            }
        }
        UI_Settings.SetActive(false);
        UI_Conversation.SetActive(false);
        //FindObjectOfType<SceneBGM>().PlayBGM();
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            UI_Conversation.SetActive(true);
        }
    }

    public void OverConversation()
    {
        OnSettings();
        // 播放 "点击" 组第0个元素点击音效
        //AudioManager.Instance.PlayClick("点击", volume: 1f, index: 0);
    }

    #region 场景转换界面文字
    /*public void ClickGameButton()
    {
        SceneLoader.LoadSceneWithLoading(
            "FEI",
            "长期吸烟会使健康的粉色肺变黑，增加肺癌风险。",
            "大笑能扩张肺部，促进深呼吸，帮助清理呼吸道。",
            7.5f
            );


        // 播放 "点击" 组第0个元素点击音效
        AudioManager.Instance.PlayClick("点击", volume: 1f, index: 0);

    }*/
    #endregion

    private void OnSettings()
    {
        TowUI(UI_Settings, UI_Conversation);
    }

    private void TowUI(GameObject show, GameObject off)
    {
        show.SetActive(true);
        off.SetActive(false);
        noInputAnything = true;
        // 播放 "点击" 组第0个元素点击音效
        //AudioManager.Instance.PlayClick("点击", volume: 1f, index: 0);
    }

    private void ToggleUI(GameObject showObj)
    {
        showObj.SetActive(!showObj.activeSelf);
        // 播放 "点击" 组第0个元素点击音效
        //AudioManager.Instance.PlayClick("点击", volume: 1f, index: 0);
    }
}