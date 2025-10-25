using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class scene1UI_Manager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject UI_Conversation;
    public GameObject UI_Settings;
    public GameObject panel_TaskPrompt;
    public bool noInputAnything=false;//只能点击F H

    [Header("引用打字机")]
    public TextMeshProUGUI text_Task;
    private AudioSource audioSource;
    public AudioClip taskSound; 
    public float typingSpeed = 0.05f;
    public bool IsTyping { get; private set; }
    public string OriginalText { get; private set; }
    private TextMeshProUGUI textMeshPro;
    private Coroutine typingCoroutine;

    private bool conversationStarted = false; // 是否已经进入对话阶段
    private Conversation_Manager conversation;

    void Start()
    {
        UI_Settings.SetActive(false);
        UI_Conversation.SetActive(false);
        panel_TaskPrompt.SetActive(false);

        conversation = FindObjectOfType<Conversation_Manager>();
        //FindObjectOfType<SceneBGM>().PlayBGM();
    }

    private void Update()
    {
        if (!conversation.player1Finished)
        {
            if (!conversationStarted && Input.anyKeyDown)
            {
                UI_Conversation.SetActive(true);
                conversationStarted = true;
                noInputAnything = true;

                if (conversation != null)
                    conversation.StartConversation();
            }
            if (conversation != null && conversationStarted && noInputAnything)
            {
                if (Input.anyKeyDown && Input.GetKeyDown(KeyCode.F) == false && Input.GetKeyDown(KeyCode.H) == false)
                {
                    Debug.Log("输入被禁用");
                }
            }
        }
    }

    private IEnumerator TypeRoutine(TextMeshProUGUI text)
    {
        // 不再尝试 GetComponent
        textMeshPro = text;

        // 获取原始文字（你可以改成动态文本）
        OriginalText = "合成拼图";

        IsTyping = true;

        // 播放文字滚动音效
        if (audioSource != null && taskSound != null)
            audioSource.PlayOneShot(taskSound);

        // 确保清空旧文字
        textMeshPro.text = "";

        foreach (char c in OriginalText)
        {
            textMeshPro.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        IsTyping = false;
    }

    public void OverConversation()
    {
        UI_Conversation.SetActive(false);
        panel_TaskPrompt.SetActive(true);
        noInputAnything =false;
        conversationStarted = false;//恢复输入
        StartCoroutine(TypeRoutine(text_Task));
        //OnSettings();
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

    /*private void OnSettings()
    {
        TowUI(UI_Settings, UI_Conversation);
    }*/

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