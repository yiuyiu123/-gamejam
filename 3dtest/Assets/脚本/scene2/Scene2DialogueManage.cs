using System;
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

    [Header("张奕忻：任务提示相关UI")]
    public GameObject Panel_TaskPrompt;
    public TextMeshProUGUI Text_TaskPrompt;//当前任务内容
    public AudioClip taskSuccessSound; // 成功音效

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

    [Header("移动控制设置")]
    public bool lockMovementDuringDialogue = true; // 对话期间锁定移动

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
    private DualPlayerController dualPlayerController; // 双人移动控制器

    // 对话序列类
    [System.Serializable]
    public class DialogueSequence
    {
        public string sequenceName;           // 序列名称
        public List<DialogueLine> dialogueLines; // 对话行列表
        public UnityEvent onSequenceStart;    // 序列开始事件
        public UnityEvent onSequenceEnd;      // 序列结束事件
        //张奕忻任务提示
        public bool hasTaskPrompt = false;     //判断有没有任务提示
        public string taskContent;
        public AudioClip taskTypingSound;         // 任务提示打字音效
        public Coroutine taskTypingCoroutine; // 保存任务提示协程引用

        public Func<bool> taskCompletionCheck; // 委托，用来检查任务是否完成
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

        // 控制对话显示在哪一侧以及需要哪个玩家确认
        public DialogueSide dialogueSide = DialogueSide.Both;
        public PlayerInputRequirement inputRequirement = PlayerInputRequirement.Both;

        // 新增：标记是否为序列结束行（需要双人确认）
        public bool isSequenceEndLine = false;
    }

    // 枚举：对话显示侧
    public enum DialogueSide
    {
        Both,       // 两边都显示
        LeftOnly,   // 只显示在左边
        RightOnly   // 只显示在右边
    }

    // 枚举：玩家输入要求
    public enum PlayerInputRequirement
    {
        Both,       // 需要两个玩家都确认
        Player1Only, // 只需要玩家1确认
        Player2Only  // 只需要玩家2确认
    }

    void Awake()
    {
        //张奕忻：序列重命名
        //DialogueLine line;

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

        // 查找双人移动控制器
        dualPlayerController = FindObjectOfType<DualPlayerController>();
        if (dualPlayerController == null)
        {
            LogWarning("未找到 DualPlayerController！移动锁定功能将不可用");
        }
        else
        {
            Log("找到 DualPlayerController，移动锁定功能已启用");
        }

        // 隐藏对话面板
        if (player1DialoguePanel != null) player1DialoguePanel.SetActive(false);
        if (player2DialoguePanel != null) player2DialoguePanel.SetActive(false);
        //张奕忻：隐藏任务提示相关UI
        Panel_TaskPrompt.SetActive(false);

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
        //张奕忻：检测当前任务是否完成
        if (currentSequence != null && currentSequence.hasTaskPrompt)
        {
            if (currentSequence.taskCompletionCheck != null && currentSequence.taskCompletionCheck())
            {
                OnTaskCompleted();
            }
        }
    }

    //张奕忻：任务检查完成
    public void OnTaskCompleted()
    {
        if (currentSequence == null || !currentSequence.hasTaskPrompt)
            return;
        // 1️⃣ 停掉正在运行的任务提示协程
        if (currentSequence.taskTypingCoroutine != null)
        {
            StopCoroutine(currentSequence.taskTypingCoroutine);
            currentSequence.taskTypingCoroutine = null;
        }
        // 2️⃣ 播放任务完成成功音效
        if (taskSuccessSound != null)
            audioSource.PlayOneShot(taskSuccessSound);
        // 3️⃣ 直接显示完整任务提示内容（可选）
        if (Text_TaskPrompt != null)
            Text_TaskPrompt.text = "...";//currentSequence.taskContent;

        // 4️⃣ 关闭任务提示面板和文字
        /*if (Panel_TaskPrompt != null)
            Panel_TaskPrompt.SetActive(false);
        if (Text_TaskPrompt != null)
            Text_TaskPrompt.gameObject.SetActive(false);*/
        
        // 确保面板和文本重新启用（防止下一个任务时UI灰掉）
        if (Panel_TaskPrompt != null && !Panel_TaskPrompt.activeSelf)
            Panel_TaskPrompt.SetActive(true);
        if (Text_TaskPrompt != null && !Text_TaskPrompt.gameObject.activeSelf)
            Text_TaskPrompt.gameObject.SetActive(true);

        // 5️⃣ 标记任务提示已完成，防止重复触发
        currentSequence.hasTaskPrompt = false;

        // 6️⃣ 可选：触发任务完成事件（如果你需要通知其他系统）
        // currentSequence.onTaskCompleted?.Invoke(); // 需要在 DialogueSequence 添加 UnityEvent
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

        // 锁定玩家移动和交互
        if (lockMovementDuringDialogue)
        {
            LockPlayerControls(true);
        }

        // 显示对话面板
        if (player1DialoguePanel != null) player1DialoguePanel.SetActive(true);
        if (player2DialoguePanel != null) player2DialoguePanel.SetActive(true);

        sequence.onSequenceStart?.Invoke(); // 触发序列开始事件

        ShowDialogueLine(currentSequence.dialogueLines[currentDialogueIndex]); // 显示第一行对话
    }

    void ShowDialogueLine(DialogueLine line)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        isLastLine = (currentDialogueIndex == currentSequence.dialogueLines.Count - 1);

        // 根据对话侧设置显示对应的UI
        switch (line.dialogueSide)
        {
            case DialogueSide.LeftOnly:
                // 只显示左边，隐藏右边
                if (player1DialoguePanel != null) player1DialoguePanel.SetActive(true);
                if (player2DialoguePanel != null) player2DialoguePanel.SetActive(false);
                break;
            case DialogueSide.RightOnly:
                // 只显示右边，隐藏左边
                if (player1DialoguePanel != null) player1DialoguePanel.SetActive(false);
                if (player2DialoguePanel != null) player2DialoguePanel.SetActive(true);
                break;
            case DialogueSide.Both:
            default:
                // 两边都显示
                if (player1DialoguePanel != null) player1DialoguePanel.SetActive(true);
                if (player2DialoguePanel != null) player2DialoguePanel.SetActive(true);
                break;
        }

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
            // 如果是序列结束行，强制要求双人确认
            if (line.isSequenceEndLine)
            {
                waitingForPlayer1 = true;
                waitingForPlayer2 = true;
            }
            else
            {
                // 根据输入要求设置等待哪个玩家按键
                switch (line.inputRequirement)
                {
                    case PlayerInputRequirement.Player1Only:
                        // 只等待玩家1按键
                        waitingForPlayer1 = true;
                        waitingForPlayer2 = false;
                        break;
                    case PlayerInputRequirement.Player2Only:
                        // 只等待玩家2按键
                        waitingForPlayer1 = false;
                        waitingForPlayer2 = true;
                        break;
                    case PlayerInputRequirement.Both:
                    default:
                        // 等待两个玩家按键
                        waitingForPlayer1 = true;
                        waitingForPlayer2 = true;
                        break;
                }
            }

            if (showEndingPrompt)
            {
                StartCoroutine(AddEndingPrompt(line)); // 添加结束提示
            }
        }
    }

    // 打字效果的协程
    IEnumerator TypeDialogueLine(DialogueLine line, AudioClip soundClip)
    {
        int characterCount = 0;

        // 根据对话侧设置显示对应的文本
        switch (line.dialogueSide)
        {
            case DialogueSide.LeftOnly:
                // 只在左边显示打字效果
                if (player1DialogueText != null)
                {
                    player1DialogueText.text = "";
                    foreach (char c in line.player1Text)
                    {
                        player1DialogueText.text += c;
                        characterCount++;
                        PlayTypingSound(c, soundClip, characterCount);
                        yield return new WaitForSeconds(typingSpeed);
                    }
                }
                // 右边保持空白
                if (player2DialogueText != null) player2DialogueText.text = "";
                break;

            case DialogueSide.RightOnly:
                // 只在右边显示打字效果
                if (player2DialogueText != null)
                {
                    player2DialogueText.text = "";
                    foreach (char c in line.player2Text)
                    {
                        player2DialogueText.text += c;
                        characterCount++;
                        PlayTypingSound(c, soundClip, characterCount);
                        yield return new WaitForSeconds(typingSpeed);
                    }
                }
                // 左边保持空白
                if (player1DialogueText != null) player1DialogueText.text = "";
                break;

            case DialogueSide.Both:
            default:
                // 两边都显示打字效果
                if (player1DialogueText != null)
                {
                    player1DialogueText.text = "";
                    foreach (char c in line.player1Text)
                    {
                        player1DialogueText.text += c;
                        characterCount++;
                        PlayTypingSound(c, soundClip, characterCount);
                        yield return new WaitForSeconds(typingSpeed);
                    }
                }

                if (player2DialogueText != null)
                {
                    player2DialogueText.text = "";
                    foreach (char c in line.player2Text)
                    {
                        player2DialogueText.text += c;
                        characterCount++;
                        PlayTypingSound(c, soundClip, characterCount);
                        yield return new WaitForSeconds(typingSpeed);
                    }
                }
                break;
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

    // 添加结束提示的协程（根据输入要求显示不同的提示）
    IEnumerator AddEndingPrompt(DialogueLine line)
    {
        yield return new WaitUntil(() => typingCoroutine == null);

        // 如果是序列结束行，强制显示双人确认提示
        if (line.isSequenceEndLine)
        {
            if (player1DialogueText != null && !string.IsNullOrEmpty(player1DialogueText.text))
            {
                player1DialogueText.text += $"\n\n<color=#FFFF00>按 FFFFFFFFF 和 H 键继续</color>";
            }
            if (player2DialogueText != null && !string.IsNullOrEmpty(player2DialogueText.text))
            {
                player2DialogueText.text += $"\n\n<color=#FFFF00>按 F 和 H 键继续</color>";
            }
        }
        else
        {
            // 根据输入要求显示不同的提示
            switch (line.inputRequirement)
            {
                case PlayerInputRequirement.Player1Only:
                    // 只在左边显示提示
                    if (player1DialogueText != null && !string.IsNullOrEmpty(player1DialogueText.text))
                    {
                        player1DialogueText.text += $"\n\n<color=#FFFF00>按 F 键继续</color>";
                    }
                    break;

                case PlayerInputRequirement.Player2Only:
                    // 只在右边显示提示
                    if (player2DialogueText != null && !string.IsNullOrEmpty(player2DialogueText.text))
                    {
                        player2DialogueText.text += $"\n\n<color=#FFFF00>按 H 键继续</color>";
                    }
                    break;

                case PlayerInputRequirement.Both:
                default:
                    // 两边都显示提示
                    if (player1DialogueText != null && !string.IsNullOrEmpty(player1DialogueText.text))
                    {
                        player1DialogueText.text += $"\n\n<color=#FFFF00>按 F 和 H 键继续</color>";
                    }
                    if (player2DialogueText != null && !string.IsNullOrEmpty(player2DialogueText.text))
                    {
                        player2DialogueText.text += $"\n\n<color=#FFFF00>按 F 和 H 键继续</color>";
                    }
                    break;
            }
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

    // 已执行：结束对话序列
    void EndDialogueSequence()//DialogueLine line
    {
        isDialogueActive = false;
        isLastLine = false;

        // 隐藏对话面板
        if (player1DialoguePanel != null) player1DialoguePanel.SetActive(false);
        if (player2DialoguePanel != null) player2DialoguePanel.SetActive(false);

        // 解锁玩家移动和交互
        if (lockMovementDuringDialogue)
        {
            LockPlayerControls(false);
        }
        //张奕忻：如果有序列结束的任务提示
        //DialogueLine lastLine = null;//获取最后一行
        //if (currentSequence != null && currentSequence.dialogueLines.Count > 0)
        //    lastLine = currentSequence.dialogueLines[currentSequence.dialogueLines.Count - 1];

        if (currentSequence != null && currentSequence.hasTaskPrompt)
        {
            Panel_TaskPrompt.SetActive(true);

            if (currentSequence.taskTypingCoroutine != null) StopCoroutine(currentSequence.taskTypingCoroutine);

            // 使用任务提示音效启动打字协程
            AudioClip clip = currentSequence.taskTypingSound != null ? currentSequence.taskTypingSound : typingSound;
            currentSequence.taskTypingCoroutine = StartCoroutine(TypeTaskPrompt(currentSequence.taskContent, clip));
        }
        //张奕忻结束
        currentSequence.onSequenceEnd?.Invoke(); // 触发序列结束事件

        Log($"对话序列结束: {currentSequence.sequenceName}");
    }

    //张奕忻：任务提示打字机协程
    private IEnumerator TypeTaskPrompt(string text, AudioClip soundClip)
    {
        Text_TaskPrompt.text = "";
        int characterCount = 0;

        foreach (char c in text)
        {
            Text_TaskPrompt.text += c;
            characterCount++;

            // 播放打字音效
            PlayTypingSound(c, soundClip, characterCount);

            yield return new WaitForSeconds(typingSpeed);
        }
    }

    // 锁定/解锁玩家控制（移动和交互）
    void LockPlayerControls(bool locked)
    {
        // 锁定 DualPlayerController 的移动
        if (dualPlayerController != null)
        {
            dualPlayerController.SetMovementLock(locked);
            Log($"DualPlayerController 移动 {(locked ? "锁定" : "解锁")}");
        }

        // 锁定 PlayerController 的交互
        if (player1Controller != null)
        {
            player1Controller.SetTemporaryLock(locked);
            Log($"玩家1交互 {(locked ? "锁定" : "解锁")}");
        }

        if (player2Controller != null)
        {
            player2Controller.SetTemporaryLock(locked);
            Log($"玩家2交互 {(locked ? "锁定" : "解锁")}");
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

    [ContextMenu("显示当前对话状态")]
    public void DebugShowDialogueState()
    {
        Log($"=== 对话状态 ===");
        Log($"当前序列: {currentSequence?.sequenceName ?? "None"}");
        Log($"当前索引: {currentDialogueIndex}");
        Log($"等待玩家1: {waitingForPlayer1}");
        Log($"等待玩家2: {waitingForPlayer2}");
        Log($"最后一行: {isLastLine}");
        Log($"移动锁定: {lockMovementDuringDialogue}");
        if (dualPlayerController != null)
        {
            Log($"DualPlayerController 状态: {(dualPlayerController.IsMovementLocked() ? "锁定" : "正常")}");
        }
    }

    [ContextMenu("测试移动锁定")]
    public void TestMovementLock()
    {
        if (dualPlayerController != null)
        {
            bool currentState = dualPlayerController.IsMovementLocked();
            dualPlayerController.SetMovementLock(!currentState);
            Log($"切换移动锁定状态: {(!currentState ? "锁定" : "解锁")}");
        }
        else
        {
            LogWarning("DualPlayerController 未找到");
        }
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

    void LogWarning(string message)
    {
        if (enableDialogueDebug)
        {
            Debug.LogWarning($"[DialogueManager] {message}");
        }
    }
}