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

    // �Ի�������
    [System.Serializable]
    public class DialogueSequence
    {
        public string sequenceName;           // ��������
        public List<DialogueLine> dialogueLines; // �Ի����б�
        public bool requireBothPlayers = true; // �Ƿ���Ҫ˫���ȷ��
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

        LockPlayerMovement(true); // ��������ƶ�

        // ��ʾ�Ի����
        if (player1DialoguePanel != null) player1DialoguePanel.SetActive(true);
        if (player2DialoguePanel != null) player2DialoguePanel.SetActive(true);

        sequence.onSequenceStart?.Invoke(); // �������п�ʼ�¼�

        ShowDialogueLine(currentSequence.dialogueLines[currentDialogueIndex]); // ��ʾ��һ�жԻ�
    }

    // ��ʾ�Ի���
    void ShowDialogueLine(DialogueLine line)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        isLastLine = (currentDialogueIndex == currentSequence.dialogueLines.Count - 1);

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
            // �ȴ���Ұ���
            waitingForPlayer1 = currentSequence.requireBothPlayers;
            waitingForPlayer2 = currentSequence.requireBothPlayers;

            if (isLastLine && showEndingPrompt)
            {
                StartCoroutine(AddEndingPrompt()); // ��ӽ�����ʾ
            }
        }
    }

    // ����Ч����Э��
    IEnumerator TypeDialogueLine(DialogueLine line, AudioClip soundClip)
    {
        int characterCount = 0;

        // ���1�ı�����Ч��
        if (player1DialogueText != null)
        {
            player1DialogueText.text = "";
            foreach (char c in line.player1Text)
            {
                player1DialogueText.text += c;
                characterCount++;

                // ���Ŵ�����Ч
                PlayTypingSound(c, soundClip, characterCount);

                yield return new WaitForSeconds(typingSpeed);
            }
        }

        // ���2�ı�����Ч��
        if (player2DialogueText != null)
        {
            player2DialogueText.text = "";
            foreach (char c in line.player2Text)
            {
                player2DialogueText.text += c;
                characterCount++;

                // ���Ŵ�����Ч
                PlayTypingSound(c, soundClip, characterCount);

                yield return new WaitForSeconds(typingSpeed);
            }
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

    // ��ӽ�����ʾ��Э��
    IEnumerator AddEndingPrompt()
    {
        yield return new WaitUntil(() => typingCoroutine == null);

        // �ڶԻ��ı��������ʾ
        if (player1DialogueText != null && !string.IsNullOrEmpty(player1DialogueText.text))
        {
            player1DialogueText.text += $"\n\n<color=#FFFF00>{endingPromptText}</color>";
        }
        if (player2DialogueText != null && !string.IsNullOrEmpty(player2DialogueText.text))
        {
            player2DialogueText.text += $"\n\n<color=#FFFF00>{endingPromptText}</color>";
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

        LockPlayerMovement(false); // ��������ƶ�

        currentSequence.onSequenceEnd?.Invoke(); // �������н����¼�

        Log($"�Ի����н���: {currentSequence.sequenceName}");
    }

    // ����/��������ƶ�
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
}