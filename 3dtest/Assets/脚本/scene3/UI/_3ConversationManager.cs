using System.Collections;
using UnityEngine;
using TMPro;

public class _3ConversationManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelA;
    public GameObject panelB;
    public TMPTypewriter typewriterA;
    public TMPTypewriter typewriterB;

    [Header("Conversation Settings")]
    public float typingSpeed = 0.05f;

    private int currentLineIndex = 0;
    private Line[] conversationLines;

    private bool isConversationActive = false;

    private void Start()
    {
        // 定义剧情文本
        conversationLines = new Line[]
        {
            // 剧情1
            new Line("a","回到家里了....."),
            new Line("b","什么声音！"),
            new Line("b","我怎么能听到你说话了！？"),
            new Line("a",".....！"),
            new Line("a","家里怎么全黑了！难道是灯坏了？"),
            new Line("a","好在家中布局没变过，快帮我找到电闸！"),
            new Line("b","那就跟着我说的路线走吧"),

            // 剧情2
            new Line("a","呼...灯终于开了，这场梦还没有结束吗"),
            new Line("b","或许，这不是梦"),
            new Line("b","无论如何，要找到试卷拿给老师才行"),
            new Line("a","哦对.....我的u盘！我记得在卧室里面"),
            new Line("b","可恶，门怎么上锁了！得想办法打开才行"),
            new Line("n","-----请找到卧室钥匙"),

            // 剧情3
            new Line("a","记得小时候总是梦想着长大能有一个自己的花园，观蔷薇爬满木架，筛下细碎光斑。捧半盏清茶，看云影在草叶上缓缓游走，悄无声息"),
            new Line("b","所以现在的你有属于自己的花园了吗？"),
            new Line("a","没有....."),
            new Line("a","现在被繁忙的工作缠身，哪还有时间收拾花？连阳台的多肉都没时间浇水，不久前干死了"),
            new Line("n","养花")
        };
    }

    private void Update()
    {
        if (!isConversationActive)
            return;

        if (currentLineIndex >= conversationLines.Length)
            return;

        Line line = conversationLines[currentLineIndex];

        // 按键控制
        bool nextPressed = false;
        if (line.speaker == "a" && Input.GetKeyDown(KeyCode.F))
            nextPressed = true;
        else if (line.speaker == "b" && Input.GetKeyDown(KeyCode.H))
            nextPressed = true;
        else if (line.speaker == "n" && Input.GetKey(KeyCode.F) && Input.GetKey(KeyCode.H))
            nextPressed = true;

        if (nextPressed)
        {
            ShowNextLine();
        }
    }

    public void StartConversation()
    {
        isConversationActive = true;
        currentLineIndex = 0;
        ShowNextLine();
    }

    private void ShowNextLine()
    {
        if (currentLineIndex >= conversationLines.Length)
        {
            EndConversation();
            return;
        }

        Line line = conversationLines[currentLineIndex];

        // 切换显示Panel
        panelA.SetActive(line.speaker == "a" || line.speaker == "n");
        panelB.SetActive(line.speaker == "b" || line.speaker == "n");

        if (line.speaker == "a")
        {
            typewriterA.SetText(line.content);
            typewriterA.StartTypewriter(true);
        }
        else if (line.speaker == "b")
        {
            typewriterB.SetText(line.content);
            typewriterB.StartTypewriter(false);
        }
        else if (line.speaker == "n")
        {
            // 旁白在AB面板同时显示
            typewriterA.SetText(line.content);
            typewriterB.SetText(line.content);
            typewriterA.StartTypewriter(true);
            typewriterB.StartTypewriter(false);
        }

        currentLineIndex++;
    }

    private void EndConversation()
    {
        isConversationActive = false;
        panelA.SetActive(false);
        panelB.SetActive(false);

        // 恢复玩家移动（根据你的scene2DialogueManager逻辑）
        Scene2TaskManager.Instance.player1.GetComponent<PlayerController>().enabled = true;
        Scene2TaskManager.Instance.player2.GetComponent<PlayerController>().enabled = true;
    }

    // 用于必须打完字再切换的方法
    public void ForceNextLineAfterTyping()
    {
        if (currentLineIndex >= conversationLines.Length)
            return;

        Line line = conversationLines[currentLineIndex - 1];

        if ((line.speaker == "a" && typewriterA.IsTyping) ||
            (line.speaker == "b" && typewriterB.IsTyping) ||
            (line.speaker == "n" && (typewriterA.IsTyping || typewriterB.IsTyping)))
        {
            // 打完字后再继续
            if (line.speaker == "a") typewriterA.SetText(line.content);
            if (line.speaker == "b") typewriterB.SetText(line.content);
            if (line.speaker == "n")
            {
                typewriterA.SetText(line.content);
                typewriterB.SetText(line.content);
            }
        }
    }

    [System.Serializable]
    private class Line
    {
        public string speaker; // "a", "b", "n"
        public string content;

        public Line(string s, string c)
        {
            speaker = s;
            content = c;
        }
    }
}
