using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Scene2DialogueManager : MonoBehaviour
{
    public static Scene2DialogueManager Instance; // 单例模式

    [Header("分屏对话UI")]
    public GameObject player1DialoguePanel;  // 左侧玩家对话面板
    public GameObject player2DialoguePanel;  // 右侧玩家对话面板

    [Header("玩家1对话UI组件")]
    public TextMeshProUGUI player1DialogueText; // 玩家1对话文本
    public Image player1SpeakerIcon;           // 玩家1说话者图标
    public TextMeshProUGUI player1SpeakerName; // 玩家1说话者名称

    [Header("玩家2对话UI组件")]
    public TextMeshProUGUI player2DialogueText; // 玩家2对话文本
    public Image player2SpeakerIcon;           // 玩家2说话者图标
    public TextMeshProUGUI player2SpeakerName; // 玩家2说话者名称

    [Header("对话设置")]
    public float typingSpeed = 0.05f;     // 打字速度
    public KeyCode player1NextKey = KeyCode.F; // 玩家1下一页按键
    public KeyCode player2NextKey = KeyCode.H; // 玩家2下一页按键

    [Header("打字音效设置")]
    public AudioClip typingSound;         // 打字音效
    public float typingSoundVolume = 0.5f; // 音效音量
    [Range(1, 10)]
    public int charactersPerSound = 2;    // 每多少个字符播放一次音效
    public bool playSoundOnSpace = false; // 是否在空格时播放音效
    public bool playSoundOnPunctuation = true; // 是否在标点时播放音效

    [Header("对话结束设置")]
    public bool showEndingPrompt = true;  // 是否显示结束提示
    public string endingPromptText = "按 F 和 H 键继续"; // 结束提示文本

    [Header("对话数据")]
    public List<DialogueSequence> dialogueSequences; // 对话序列列表

    [Header("调试选项")]
    public bool enableDialogueDebug = true; // 是否启用调试

    // 当前对话状态
    private DialogueSequence currentSequence;    // 当前对话序列
    private int currentDialogueIndex = 0;        // 当前对话行索引
    private bool isDialogueActive = false;       // 对话是否激活
    private bool waitingForPlayer1 = false;      // 等待玩家1按键
    private bool waitingForPlayer2 = false;      // 等待玩家2按键
    private bool isLastLine = false;             // 是否是最后一行
    private Coroutine typingCoroutine;           // 打字协程引用
    private AudioSource audioSource;             // 音频源

    // 玩家控制器引用
    private PlayerController player1Controller; // 玩家1控制器
    private PlayerController player2Controller; // 玩家2控制器

    // 对话序列类
    [System.Serializable]
    public class DialogueSequence
    {
        public string sequenceName;           // 序列名称
        public List<DialogueLine> dialogueLines; // 对话行列表
        public bool requireBothPlayers = true; // 是否需要双玩家确认
        public UnityEvent onSequenceStart;    // 序列开始事件
        public UnityEvent onSequenceEnd;      // 序列结束事件
    }

    // 对话行类
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 5)]
        public string player1Text;        // 玩家1看到的文本
        [TextArea(3, 5)]
        public string player2Text;        // 玩家2看到的文本
        public string speakerName;        // 说话者名称
        public Sprite speakerIcon;        // 说话者图标
        public AudioClip voiceClip;       // 语音片段
        public bool autoAdvance = false;  // 是否自动前进
        public float autoAdvanceDelay = 3f; // 自动前进延迟
        public AudioClip customTypingSound; // 自定义打字音效
    }

    void Awake()
    {
        // 单例模式初始化
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 添加AudioSource组件用于播放音效
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = typingSoundVolume;
    }

    void Start()
    {
        // 查找玩家控制器
        player1Controller = FindPlayerController("Player1");
        player2Controller = FindPlayerController("Player2");

        // 隐藏对话面板
        if (player1DialoguePanel != null) player1DialoguePanel.SetActive(false);
        if (player2DialoguePanel != null) player2DialoguePanel.SetActive(false);

        // 等待输入开始对话
        StartCoroutine(WaitForStartInput());
    }

    // 等待开始输入的协程
    IEnumerator WaitForStartInput()
    {
        Log("等待任意按键开始对话...");

        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        StartDialogueSequence("Plot1"); // 开始第一个对话序列
    }

    // 根据标签查找玩家控制器
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

        // 检测玩家按键
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

    // 开始指定名称的对话序列
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

    // 开始对话序列
    void StartDialogueSequence(DialogueSequence sequence)
    {
        if (isDialogueActive) return;

        currentSequence = sequence;
        currentDialogueIndex = 0;
        isDialogueActive = true;

        LockPlayerMovement(true); // 锁定玩家移动

        // 显示对话面板
        if (player1DialoguePanel != null) player1DialoguePanel.SetActive(true);
        if (player2DialoguePanel != null) player2DialoguePanel.SetActive(true);

        sequence.onSequenceStart?.Invoke(); // 触发序列开始事件

        ShowDialogueLine(currentSequence.dialogueLines[currentDialogueIndex]); // 显示第一行对话
    }

    // 显示对话行
    void ShowDialogueLine(DialogueLine line)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        isLastLine = (currentDialogueIndex == currentSequence.dialogueLines.Count - 1);

        // 设置说话者信息
        if (player1SpeakerName != null) player1SpeakerName.text = line.speakerName;
        if (player2SpeakerName != null) player2SpeakerName.text = line.speakerName;
        if (player1SpeakerIcon != null) player1SpeakerIcon.sprite = line.speakerIcon;
        if (player2SpeakerIcon != null) player2SpeakerIcon.sprite = line.speakerIcon;

        // 设置自定义打字音效（如果有）
        AudioClip currentTypingSound = line.customTypingSound != null ? line.customTypingSound : typingSound;

        // 开始打字效果
        typingCoroutine = StartCoroutine(TypeDialogueLine(line, currentTypingSound));

        if (line.autoAdvance)
        {
            // 自动前进
            StartCoroutine(AutoAdvanceDialogue(line.autoAdvanceDelay));
        }
        else
        {
            // 等待玩家按键
            waitingForPlayer1 = currentSequence.requireBothPlayers;
            waitingForPlayer2 = currentSequence.requireBothPlayers;

            if (isLastLine && showEndingPrompt)
            {
                StartCoroutine(AddEndingPrompt()); // 添加结束提示
            }
        }
    }

    // 打字效果的协程
    IEnumerator TypeDialogueLine(DialogueLine line, AudioClip soundClip)
    {
        int characterCount = 0;

        // 玩家1文本打字效果
        if (player1DialogueText != null)
        {
            player1DialogueText.text = "";
            foreach (char c in line.player1Text)
            {
                player1DialogueText.text += c;
                characterCount++;

                // 播放打字音效
                PlayTypingSound(c, soundClip, characterCount);

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
                characterCount++;

                // 播放打字音效
                PlayTypingSound(c, soundClip, characterCount);

                yield return new WaitForSeconds(typingSpeed);
            }
        }
    }

    // 播放打字音效
    void PlayTypingSound(char character, AudioClip soundClip, int characterCount)
    {
        if (soundClip == null) return;

        // 检查是否需要跳过这个字符的音效
        if (character == ' ' && !playSoundOnSpace)
            return;

        if (IsPunctuation(character) && !playSoundOnPunctuation)
            return;

        // 根据设置的频率播放音效
        if (characterCount % charactersPerSound == 0)
        {
            audioSource.PlayOneShot(soundClip);
        }
    }

    // 检查字符是否是标点符号
    bool IsPunctuation(char c)
    {
        // 常见标点符号
        char[] punctuations = { '.', ',', '!', '?', ';', ':', '"', '\'', '(', ')', '[', ']', '{', '}' };
        return System.Array.IndexOf(punctuations, c) >= 0;
    }

    // 添加结束提示的协程
    IEnumerator AddEndingPrompt()
    {
        yield return new WaitUntil(() => typingCoroutine == null);

        // 在对话文本后添加提示
        if (player1DialogueText != null && !string.IsNullOrEmpty(player1DialogueText.text))
        {
            player1DialogueText.text += $"\n\n<color=#FFFF00>{endingPromptText}</color>";
        }
        if (player2DialogueText != null && !string.IsNullOrEmpty(player2DialogueText.text))
        {
            player2DialogueText.text += $"\n\n<color=#FFFF00>{endingPromptText}</color>";
        }

        Log("显示结束提示，等待玩家按键");
    }

    // 自动前进对话的协程
    IEnumerator AutoAdvanceDialogue(float delay)
    {
        yield return new WaitForSeconds(delay);
        NextDialogueLine();
    }

    // 检查是否可以前进到下一行对话
    void CheckAdvanceDialogue()
    {
        if (!waitingForPlayer1 && !waitingForPlayer2)
        {
            NextDialogueLine();
        }
    }

    // 前进到下一行对话
    void NextDialogueLine()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex < currentSequence.dialogueLines.Count)
        {
            ShowDialogueLine(currentSequence.dialogueLines[currentDialogueIndex]);
        }
        else
        {
            EndDialogueSequence(); // 结束对话序列
        }
    }

    // 结束对话序列
    void EndDialogueSequence()
    {
        isDialogueActive = false;
        isLastLine = false;

        // 隐藏对话面板
        if (player1DialoguePanel != null) player1DialoguePanel.SetActive(false);
        if (player2DialoguePanel != null) player2DialoguePanel.SetActive(false);

        LockPlayerMovement(false); // 解锁玩家移动

        currentSequence.onSequenceEnd?.Invoke(); // 触发序列结束事件

        Log($"对话序列结束: {currentSequence.sequenceName}");
    }

    // 锁定/解锁玩家移动
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

    // 跳过当前对话
    public void SkipCurrentDialogue()
    {
        if (isDialogueActive)
        {
            EndDialogueSequence();
        }
    }

    // 调试工具
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

    [ContextMenu("测试打字音效")]
    public void TestTypingSound()
    {
        if (typingSound != null)
        {
            audioSource.PlayOneShot(typingSound);
            Log("播放打字音效测试");
        }
        else
        {
            Log("没有设置打字音效");
        }
    }

    // 日志工具方法
    void Log(string message)
    {
        if (enableDialogueDebug)
        {
            Debug.Log($"[DialogueManager] {message}");
        }
    }
}