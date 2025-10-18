using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Scene2DialogueManager : MonoBehaviour
{
    public static Scene2DialogueManager Instance;

    [Header("分屏对话UI")]
    public GameObject player1DialoguePanel;  // 左侧屏幕对话面板
    public GameObject player2DialoguePanel;  // 右侧屏幕对话面板

    [Header("玩家1对话UI组件")]
    public TextMeshProUGUI player1DialogueText;
    public Image player1SpeakerIcon;
    public TextMeshProUGUI player1SpeakerName;

    [Header("玩家2对话UI组件")]
    public TextMeshProUGUI player2DialogueText;
    public Image player2SpeakerIcon;
    public TextMeshProUGUI player2SpeakerName;

    [Header("对话设置")]
    public float typingSpeed = 0.05f;
    public KeyCode player1NextKey = KeyCode.F;
    public KeyCode player2NextKey = KeyCode.H;

    [Header("对话数据")]
    public List<DialogueSequence> dialogueSequences;

    // 当前对话状态
    private DialogueSequence currentSequence;
    private int currentDialogueIndex = 0;
    private bool isDialogueActive = false;
    private bool waitingForPlayer1 = false;
    private bool waitingForPlayer2 = false;
    private Coroutine typingCoroutine;

    // 玩家控制器引用（用于锁定移动）
    private PlayerController player1Controller;
    private PlayerController player2Controller;

    // 对话序列类
    [System.Serializable]
    public class DialogueSequence
    {
        public string sequenceName;
        public List<DialogueLine> dialogueLines;
        public bool requireBothPlayers = true; // 是否需要两个玩家都按键
        public UnityEvent onSequenceStart;
        public UnityEvent onSequenceEnd;
    }

    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 5)]
        public string player1Text; // 玩家1屏幕显示的文本
        [TextArea(3, 5)]
        public string player2Text; // 玩家2屏幕显示的文本
        public string speakerName; // 说话者名字
        public Sprite speakerIcon; // 说话者图标
        public AudioClip voiceClip; // 语音
        public bool autoAdvance = false; // 是否自动前进
        public float autoAdvanceDelay = 3f; // 自动前进延迟
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 获取玩家控制器
        player1Controller = FindPlayerController("Player1");
        player2Controller = FindPlayerController("Player2");

        // 隐藏对话面板
        if (player1DialoguePanel != null) player1DialoguePanel.SetActive(false);
        if (player2DialoguePanel != null) player2DialoguePanel.SetActive(false);

        // 开始游戏时等待任意按键触发初始对话
        StartCoroutine(WaitForStartInput());
    }

    IEnumerator WaitForStartInput()
    {
        Debug.Log("等待任意按键开始对话...");

        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        // 开始剧情1对话
        StartDialogueSequence("Plot1");
    }

    PlayerController FindPlayerController(string playerTag)
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            return player.GetComponent<PlayerController>();
        }
        return null;
    }

    void Update()
    {
        if (!isDialogueActive) return;

        // 检查玩家按键
        if (waitingForPlayer1 && Input.GetKeyDown(player1NextKey))
        {
            waitingForPlayer1 = false;
            CheckAdvanceDialogue();
        }

        if (waitingForPlayer2 && Input.GetKeyDown(player2NextKey))
        {
            waitingForPlayer2 = false;
            CheckAdvanceDialogue();
        }
    }

    // 在关键方法中添加日志
    public void StartDialogueSequence(string sequenceName)
    {
        Log($"请求开始对话序列: {sequenceName}");

        DialogueSequence sequence = dialogueSequences.Find(s => s.sequenceName == sequenceName);
        if (sequence != null)
        {
            Log($"找到序列: {sequenceName}，对话行数: {sequence.dialogueLines.Count}");
            StartDialogueSequence(sequence);
        }
        else
        {
            Debug.LogError($"未找到对话序列: {sequenceName}");
            Log($"可用序列: {string.Join(", ", dialogueSequences.Select(s => s.sequenceName))}");
        }
    }

    void StartDialogueSequence(DialogueSequence sequence)
    {
        if (isDialogueActive) return;

        currentSequence = sequence;
        currentDialogueIndex = 0;
        isDialogueActive = true;

        // 锁定玩家移动
        LockPlayerMovement(true);

        // 显示对话面板
        if (player1DialoguePanel != null) player1DialoguePanel.SetActive(true);
        if (player2DialoguePanel != null) player2DialoguePanel.SetActive(true);

        // 触发序列开始事件
        sequence.onSequenceStart?.Invoke();

        // 显示第一句对话
        ShowDialogueLine(currentSequence.dialogueLines[currentDialogueIndex]);
    }

    void ShowDialogueLine(DialogueLine line)
    {
        // 停止之前的打字效果
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        // 设置说话者信息
        if (player1SpeakerName != null) player1SpeakerName.text = line.speakerName;
        if (player2SpeakerName != null) player2SpeakerName.text = line.speakerName;
        if (player1SpeakerIcon != null) player1SpeakerIcon.sprite = line.speakerIcon;
        if (player2SpeakerIcon != null) player2SpeakerIcon.sprite = line.speakerIcon;

        // 开始打字效果
        typingCoroutine = StartCoroutine(TypeDialogueLine(line));

        //// 播放语音
        //if (line.voiceClip != null)
        //{
        //    AudioManager.Instance.PlayVoice(line.voiceClip);
        //}

        // 自动前进设置
        if (line.autoAdvance)
        {
            StartCoroutine(AutoAdvanceDialogue(line.autoAdvanceDelay));
        }
        else
        {
            // 等待玩家按键
            waitingForPlayer1 = currentSequence.requireBothPlayers;
            waitingForPlayer2 = currentSequence.requireBothPlayers;
        }
    }

    IEnumerator TypeDialogueLine(DialogueLine line)
    {
        // 玩家1文本打字效果
        if (player1DialogueText != null)
        {
            player1DialogueText.text = "";
            foreach (char c in line.player1Text)
            {
                player1DialogueText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        // 玩家2文本打字效果
        if (player2DialogueText != null)
        {
            player2DialogueText.text = "";
            foreach (char c in line.player2Text)
            {
                player2DialogueText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }
        }
    }

    IEnumerator AutoAdvanceDialogue(float delay)
    {
        yield return new WaitForSeconds(delay);
        NextDialogueLine();
    }

    void CheckAdvanceDialogue()
    {
        if (!waitingForPlayer1 && !waitingForPlayer2)
        {
            NextDialogueLine();
        }
    }

    void NextDialogueLine()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex < currentSequence.dialogueLines.Count)
        {
            ShowDialogueLine(currentSequence.dialogueLines[currentDialogueIndex]);
        }
        else
        {
            EndDialogueSequence();
        }
    }

    void EndDialogueSequence()
    {
        isDialogueActive = false;

        // 隐藏对话面板
        if (player1DialoguePanel != null) player1DialoguePanel.SetActive(false);
        if (player2DialoguePanel != null) player2DialoguePanel.SetActive(false);

        // 解锁玩家移动
        LockPlayerMovement(false);

        // 触发序列结束事件
        currentSequence.onSequenceEnd?.Invoke();

        Debug.Log($"对话序列结束: {currentSequence.sequenceName}");
    }

    void LockPlayerMovement(bool locked)
    {
        if (player1Controller != null)
        {
            player1Controller.SetTemporaryLock(locked);
        }
        if (player2Controller != null)
        {
            player2Controller.SetTemporaryLock(locked);
        }
    }

    // 快速跳过当前对话
    public void SkipCurrentDialogue()
    {
        if (isDialogueActive)
        {
            EndDialogueSequence();
        }
    }

    // 强制开始特定对话（用于调试）
    [ContextMenu("开始剧情1对话")]
    public void DebugStartPlot1()
    {
        StartDialogueSequence("Plot1");
    }

    [ContextMenu("开始剧情2对话")]
    public void DebugStartPlot2()
    {
        StartDialogueSequence("Plot2");
    }
    [Header("调试选项")]
    public bool enableDialogueDebug = true;

    void Log(string message)
    {
        if (enableDialogueDebug)
        {
            Debug.Log($"[DialogueManager] {message}");
        }
    }


}