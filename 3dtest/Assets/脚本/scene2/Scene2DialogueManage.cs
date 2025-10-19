using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Scene2DialogueManager : MonoBehaviour
{
    public static Scene2DialogueManager Instance; // ����ģʽ

    [Header("�����Ի�UI")]
    public GameObject player1DialoguePanel;  // �����ҶԻ����
    public GameObject player2DialoguePanel;  // �Ҳ���ҶԻ����

    [Header("���1�Ի�UI���")]
    public TextMeshProUGUI player1DialogueText; // ���1�Ի��ı�
    public Image player1SpeakerIcon;           // ���1˵����ͼ��
    public TextMeshProUGUI player1SpeakerName; // ���1˵��������

    [Header("���2�Ի�UI���")]
    public TextMeshProUGUI player2DialogueText; // ���2�Ի��ı�
    public Image player2SpeakerIcon;           // ���2˵����ͼ��
    public TextMeshProUGUI player2SpeakerName; // ���2˵��������

    [Header("�Ի�����")]
    public float typingSpeed = 0.05f;     // �����ٶ�
    public KeyCode player1NextKey = KeyCode.F; // ���1��һҳ����
    public KeyCode player2NextKey = KeyCode.H; // ���2��һҳ����

    [Header("������Ч����")]
    public AudioClip typingSound;         // ������Ч
    public float typingSoundVolume = 0.5f; // ��Ч����
    [Range(1, 10)]
    public int charactersPerSound = 2;    // ÿ���ٸ��ַ�����һ����Ч
    public bool playSoundOnSpace = false; // �Ƿ��ڿո�ʱ������Ч
    public bool playSoundOnPunctuation = true; // �Ƿ��ڱ��ʱ������Ч

    [Header("�Ի���������")]
    public bool showEndingPrompt = true;  // �Ƿ���ʾ������ʾ
    public string endingPromptText = "�� F �� H ������"; // ������ʾ�ı�

    [Header("�Ի�����")]
    public List<DialogueSequence> dialogueSequences; // �Ի������б�

    [Header("�ƶ���������")]
    public bool lockMovementDuringDialogue = true; // �Ի��ڼ������ƶ�

    [Header("����ѡ��")]
    public bool enableDialogueDebug = true; // �Ƿ����õ���

    // ��ǰ�Ի�״̬
    private DialogueSequence currentSequence;    // ��ǰ�Ի�����
    private int currentDialogueIndex = 0;        // ��ǰ�Ի�������
    private bool isDialogueActive = false;       // �Ի��Ƿ񼤻�
    private bool waitingForPlayer1 = false;      // �ȴ����1����
    private bool waitingForPlayer2 = false;      // �ȴ����2����
    private bool isLastLine = false;             // �Ƿ������һ��
    private Coroutine typingCoroutine;           // ����Э������
    private AudioSource audioSource;             // ��ƵԴ

    // ��ҿ���������
    private PlayerController player1Controller; // ���1������
    private PlayerController player2Controller; // ���2������
    private DualPlayerController dualPlayerController; // ˫���ƶ�������

    // �Ի�������
    [System.Serializable]
    public class DialogueSequence
    {
        public string sequenceName;           // ��������
        public List<DialogueLine> dialogueLines; // �Ի����б�
        public UnityEvent onSequenceStart;    // ���п�ʼ�¼�
        public UnityEvent onSequenceEnd;      // ���н����¼�
    }

    // �Ի�����
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 5)]
        public string player1Text;        // ���1�������ı�
        [TextArea(3, 5)]
        public string player2Text;        // ���2�������ı�
        public string speakerName;        // ˵��������
        public Sprite speakerIcon;        // ˵����ͼ��
        public AudioClip voiceClip;       // ����Ƭ��
        public bool autoAdvance = false;  // �Ƿ��Զ�ǰ��
        public float autoAdvanceDelay = 3f; // �Զ�ǰ���ӳ�
        public AudioClip customTypingSound; // �Զ��������Ч

        // ���ƶԻ���ʾ����һ���Լ���Ҫ�ĸ����ȷ��
        public DialogueSide dialogueSide = DialogueSide.Both;
        public PlayerInputRequirement inputRequirement = PlayerInputRequirement.Both;

        // ����������Ƿ�Ϊ���н����У���Ҫ˫��ȷ�ϣ�
        public bool isSequenceEndLine = false;
    }

    // ö�٣��Ի���ʾ��
    public enum DialogueSide
    {
        Both,       // ���߶���ʾ
        LeftOnly,   // ֻ��ʾ�����
        RightOnly   // ֻ��ʾ���ұ�
    }

    // ö�٣��������Ҫ��
    public enum PlayerInputRequirement
    {
        Both,       // ��Ҫ������Ҷ�ȷ��
        Player1Only, // ֻ��Ҫ���1ȷ��
        Player2Only  // ֻ��Ҫ���2ȷ��
    }

    void Awake()
    {
        // ����ģʽ��ʼ��
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // ���AudioSource������ڲ�����Ч
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = typingSoundVolume;
    }

    void Start()
    {
        // ������ҿ�����
        player1Controller = FindPlayerController("Player1");
        player2Controller = FindPlayerController("Player2");

        // ����˫���ƶ�������
        dualPlayerController = FindObjectOfType<DualPlayerController>();
        if (dualPlayerController == null)
        {
            LogWarning("δ�ҵ� DualPlayerController���ƶ��������ܽ�������");
        }
        else
        {
            Log("�ҵ� DualPlayerController���ƶ���������������");
        }

        // ���ضԻ����
        if (player1DialoguePanel != null) player1DialoguePanel.SetActive(false);
        if (player2DialoguePanel != null) player2DialoguePanel.SetActive(false);

        // �ȴ����뿪ʼ�Ի�
        StartCoroutine(WaitForStartInput());
    }

    // �ȴ���ʼ�����Э��
    IEnumerator WaitForStartInput()
    {
        Log("�ȴ����ⰴ����ʼ�Ի�...");

        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        StartDialogueSequence("Plot1"); // ��ʼ��һ���Ի�����
    }

    // ���ݱ�ǩ������ҿ�����
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

        // �����Ұ���
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

    // ��ʼָ�����ƵĶԻ�����
    public void StartDialogueSequence(string sequenceName)
    {
        Log($"����ʼ�Ի�����: {sequenceName}");

        DialogueSequence sequence = dialogueSequences.Find(s => s.sequenceName == sequenceName);
        if (sequence != null)
        {
            Log($"�ҵ�����: {sequenceName}���Ի�����: {sequence.dialogueLines.Count}");
            StartDialogueSequence(sequence);
        }
        else
        {
            Debug.LogError($"δ�ҵ��Ի�����: {sequenceName}");
            Log($"��������: {string.Join(", ", dialogueSequences.Select(s => s.sequenceName))}");
        }
    }

    // ��ʼ�Ի�����
    void StartDialogueSequence(DialogueSequence sequence)
    {
        if (isDialogueActive) return;

        currentSequence = sequence;
        currentDialogueIndex = 0;
        isDialogueActive = true;

        // ��������ƶ��ͽ���
        if (lockMovementDuringDialogue)
        {
            LockPlayerControls(true);
        }

        // ��ʾ�Ի����
        if (player1DialoguePanel != null) player1DialoguePanel.SetActive(true);
        if (player2DialoguePanel != null) player2DialoguePanel.SetActive(true);

        sequence.onSequenceStart?.Invoke(); // �������п�ʼ�¼�

        ShowDialogueLine(currentSequence.dialogueLines[currentDialogueIndex]); // ��ʾ��һ�жԻ�
    }

    void ShowDialogueLine(DialogueLine line)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        isLastLine = (currentDialogueIndex == currentSequence.dialogueLines.Count - 1);

        // ���ݶԻ���������ʾ��Ӧ��UI
        switch (line.dialogueSide)
        {
            case DialogueSide.LeftOnly:
                // ֻ��ʾ��ߣ������ұ�
                if (player1DialoguePanel != null) player1DialoguePanel.SetActive(true);
                if (player2DialoguePanel != null) player2DialoguePanel.SetActive(false);
                break;
            case DialogueSide.RightOnly:
                // ֻ��ʾ�ұߣ��������
                if (player1DialoguePanel != null) player1DialoguePanel.SetActive(false);
                if (player2DialoguePanel != null) player2DialoguePanel.SetActive(true);
                break;
            case DialogueSide.Both:
            default:
                // ���߶���ʾ
                if (player1DialoguePanel != null) player1DialoguePanel.SetActive(true);
                if (player2DialoguePanel != null) player2DialoguePanel.SetActive(true);
                break;
        }

        // ����˵������Ϣ
        if (player1SpeakerName != null) player1SpeakerName.text = line.speakerName;
        if (player2SpeakerName != null) player2SpeakerName.text = line.speakerName;
        if (player1SpeakerIcon != null) player1SpeakerIcon.sprite = line.speakerIcon;
        if (player2SpeakerIcon != null) player2SpeakerIcon.sprite = line.speakerIcon;

        // �����Զ��������Ч������У�
        AudioClip currentTypingSound = line.customTypingSound != null ? line.customTypingSound : typingSound;

        // ��ʼ����Ч��
        typingCoroutine = StartCoroutine(TypeDialogueLine(line, currentTypingSound));

        if (line.autoAdvance)
        {
            // �Զ�ǰ��
            StartCoroutine(AutoAdvanceDialogue(line.autoAdvanceDelay));
        }
        else
        {
            // ��������н����У�ǿ��Ҫ��˫��ȷ��
            if (line.isSequenceEndLine)
            {
                waitingForPlayer1 = true;
                waitingForPlayer2 = true;
            }
            else
            {
                // ��������Ҫ�����õȴ��ĸ���Ұ���
                switch (line.inputRequirement)
                {
                    case PlayerInputRequirement.Player1Only:
                        // ֻ�ȴ����1����
                        waitingForPlayer1 = true;
                        waitingForPlayer2 = false;
                        break;
                    case PlayerInputRequirement.Player2Only:
                        // ֻ�ȴ����2����
                        waitingForPlayer1 = false;
                        waitingForPlayer2 = true;
                        break;
                    case PlayerInputRequirement.Both:
                    default:
                        // �ȴ�������Ұ���
                        waitingForPlayer1 = true;
                        waitingForPlayer2 = true;
                        break;
                }
            }

            if (showEndingPrompt)
            {
                StartCoroutine(AddEndingPrompt(line)); // ��ӽ�����ʾ
            }
        }
    }

    // ����Ч����Э��
    IEnumerator TypeDialogueLine(DialogueLine line, AudioClip soundClip)
    {
        int characterCount = 0;

        // ���ݶԻ���������ʾ��Ӧ���ı�
        switch (line.dialogueSide)
        {
            case DialogueSide.LeftOnly:
                // ֻ�������ʾ����Ч��
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
                // �ұ߱��ֿհ�
                if (player2DialogueText != null) player2DialogueText.text = "";
                break;

            case DialogueSide.RightOnly:
                // ֻ���ұ���ʾ����Ч��
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
                // ��߱��ֿհ�
                if (player1DialogueText != null) player1DialogueText.text = "";
                break;

            case DialogueSide.Both:
            default:
                // ���߶���ʾ����Ч��
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

    // ���Ŵ�����Ч
    void PlayTypingSound(char character, AudioClip soundClip, int characterCount)
    {
        if (soundClip == null) return;

        // ����Ƿ���Ҫ��������ַ�����Ч
        if (character == ' ' && !playSoundOnSpace)
            return;

        if (IsPunctuation(character) && !playSoundOnPunctuation)
            return;

        // �������õ�Ƶ�ʲ�����Ч
        if (characterCount % charactersPerSound == 0)
        {
            audioSource.PlayOneShot(soundClip);
        }
    }

    // ����ַ��Ƿ��Ǳ�����
    bool IsPunctuation(char c)
    {
        // ����������
        char[] punctuations = { '.', ',', '!', '?', ';', ':', '"', '\'', '(', ')', '[', ']', '{', '}' };
        return System.Array.IndexOf(punctuations, c) >= 0;
    }

    // ��ӽ�����ʾ��Э�̣���������Ҫ����ʾ��ͬ����ʾ��
    IEnumerator AddEndingPrompt(DialogueLine line)
    {
        yield return new WaitUntil(() => typingCoroutine == null);

        // ��������н����У�ǿ����ʾ˫��ȷ����ʾ
        if (line.isSequenceEndLine)
        {
            if (player1DialogueText != null && !string.IsNullOrEmpty(player1DialogueText.text))
            {
                player1DialogueText.text += $"\n\n<color=#FFFF00>�� F �� H ������</color>";
            }
            if (player2DialogueText != null && !string.IsNullOrEmpty(player2DialogueText.text))
            {
                player2DialogueText.text += $"\n\n<color=#FFFF00>�� F �� H ������</color>";
            }
        }
        else
        {
            // ��������Ҫ����ʾ��ͬ����ʾ
            switch (line.inputRequirement)
            {
                case PlayerInputRequirement.Player1Only:
                    // ֻ�������ʾ��ʾ
                    if (player1DialogueText != null && !string.IsNullOrEmpty(player1DialogueText.text))
                    {
                        player1DialogueText.text += $"\n\n<color=#FFFF00>�� F ������</color>";
                    }
                    break;

                case PlayerInputRequirement.Player2Only:
                    // ֻ���ұ���ʾ��ʾ
                    if (player2DialogueText != null && !string.IsNullOrEmpty(player2DialogueText.text))
                    {
                        player2DialogueText.text += $"\n\n<color=#FFFF00>�� H ������</color>";
                    }
                    break;

                case PlayerInputRequirement.Both:
                default:
                    // ���߶���ʾ��ʾ
                    if (player1DialogueText != null && !string.IsNullOrEmpty(player1DialogueText.text))
                    {
                        player1DialogueText.text += $"\n\n<color=#FFFF00>�� F �� H ������</color>";
                    }
                    if (player2DialogueText != null && !string.IsNullOrEmpty(player2DialogueText.text))
                    {
                        player2DialogueText.text += $"\n\n<color=#FFFF00>�� F �� H ������</color>";
                    }
                    break;
            }
        }

        Log("��ʾ������ʾ���ȴ���Ұ���");
    }

    // �Զ�ǰ���Ի���Э��
    IEnumerator AutoAdvanceDialogue(float delay)
    {
        yield return new WaitForSeconds(delay);
        NextDialogueLine();
    }

    // ����Ƿ����ǰ������һ�жԻ�
    void CheckAdvanceDialogue()
    {
        if (!waitingForPlayer1 && !waitingForPlayer2)
        {
            NextDialogueLine();
        }
    }

    // ǰ������һ�жԻ�
    void NextDialogueLine()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex < currentSequence.dialogueLines.Count)
        {
            ShowDialogueLine(currentSequence.dialogueLines[currentDialogueIndex]);
        }
        else
        {
            EndDialogueSequence(); // �����Ի�����
        }
    }

    // �����Ի�����
    void EndDialogueSequence()
    {
        isDialogueActive = false;
        isLastLine = false;

        // ���ضԻ����
        if (player1DialoguePanel != null) player1DialoguePanel.SetActive(false);
        if (player2DialoguePanel != null) player2DialoguePanel.SetActive(false);

        // ��������ƶ��ͽ���
        if (lockMovementDuringDialogue)
        {
            LockPlayerControls(false);
        }

        currentSequence.onSequenceEnd?.Invoke(); // �������н����¼�

        Log($"�Ի����н���: {currentSequence.sequenceName}");
    }

    // ����/������ҿ��ƣ��ƶ��ͽ�����
    void LockPlayerControls(bool locked)
    {
        // ���� DualPlayerController ���ƶ�
        if (dualPlayerController != null)
        {
            dualPlayerController.SetMovementLock(locked);
            Log($"DualPlayerController �ƶ� {(locked ? "����" : "����")}");
        }

        // ���� PlayerController �Ľ���
        if (player1Controller != null)
        {
            player1Controller.SetTemporaryLock(locked);
            Log($"���1���� {(locked ? "����" : "����")}");
        }

        if (player2Controller != null)
        {
            player2Controller.SetTemporaryLock(locked);
            Log($"���2���� {(locked ? "����" : "����")}");
        }
    }

    // ������ǰ�Ի�
    public void SkipCurrentDialogue()
    {
        if (isDialogueActive)
        {
            EndDialogueSequence();
        }
    }

    // ���Թ���
    [ContextMenu("��ʼ����1�Ի�")]
    public void DebugStartPlot1()
    {
        StartDialogueSequence("Plot1");
    }

    [ContextMenu("��ʼ����2�Ի�")]
    public void DebugStartPlot2()
    {
        StartDialogueSequence("Plot2");
    }

    [ContextMenu("��ʾ��ǰ�Ի�״̬")]
    public void DebugShowDialogueState()
    {
        Log($"=== �Ի�״̬ ===");
        Log($"��ǰ����: {currentSequence?.sequenceName ?? "None"}");
        Log($"��ǰ����: {currentDialogueIndex}");
        Log($"�ȴ����1: {waitingForPlayer1}");
        Log($"�ȴ����2: {waitingForPlayer2}");
        Log($"���һ��: {isLastLine}");
        Log($"�ƶ�����: {lockMovementDuringDialogue}");
        if (dualPlayerController != null)
        {
            Log($"DualPlayerController ״̬: {(dualPlayerController.IsMovementLocked() ? "����" : "����")}");
        }
    }

    [ContextMenu("�����ƶ�����")]
    public void TestMovementLock()
    {
        if (dualPlayerController != null)
        {
            bool currentState = dualPlayerController.IsMovementLocked();
            dualPlayerController.SetMovementLock(!currentState);
            Log($"�л��ƶ�����״̬: {(!currentState ? "����" : "����")}");
        }
        else
        {
            LogWarning("DualPlayerController δ�ҵ�");
        }
    }

    [ContextMenu("���Դ�����Ч")]
    public void TestTypingSound()
    {
        if (typingSound != null)
        {
            audioSource.PlayOneShot(typingSound);
            Log("���Ŵ�����Ч����");
        }
        else
        {
            Log("û�����ô�����Ч");
        }
    }

    // ��־���߷���
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