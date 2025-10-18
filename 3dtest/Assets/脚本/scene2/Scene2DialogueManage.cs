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

    [Header("�����Ի�UI")]
    public GameObject player1DialoguePanel;  // �����Ļ�Ի����
    public GameObject player2DialoguePanel;  // �Ҳ���Ļ�Ի����

    [Header("���1�Ի�UI���")]
    public TextMeshProUGUI player1DialogueText;
    public Image player1SpeakerIcon;
    public TextMeshProUGUI player1SpeakerName;

    [Header("���2�Ի�UI���")]
    public TextMeshProUGUI player2DialogueText;
    public Image player2SpeakerIcon;
    public TextMeshProUGUI player2SpeakerName;

    [Header("�Ի�����")]
    public float typingSpeed = 0.05f;
    public KeyCode player1NextKey = KeyCode.F;
    public KeyCode player2NextKey = KeyCode.H;

    [Header("�Ի�����")]
    public List<DialogueSequence> dialogueSequences;

    // ��ǰ�Ի�״̬
    private DialogueSequence currentSequence;
    private int currentDialogueIndex = 0;
    private bool isDialogueActive = false;
    private bool waitingForPlayer1 = false;
    private bool waitingForPlayer2 = false;
    private Coroutine typingCoroutine;

    // ��ҿ��������ã����������ƶ���
    private PlayerController player1Controller;
    private PlayerController player2Controller;

    // �Ի�������
    [System.Serializable]
    public class DialogueSequence
    {
        public string sequenceName;
        public List<DialogueLine> dialogueLines;
        public bool requireBothPlayers = true; // �Ƿ���Ҫ������Ҷ�����
        public UnityEvent onSequenceStart;
        public UnityEvent onSequenceEnd;
    }

    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 5)]
        public string player1Text; // ���1��Ļ��ʾ���ı�
        [TextArea(3, 5)]
        public string player2Text; // ���2��Ļ��ʾ���ı�
        public string speakerName; // ˵��������
        public Sprite speakerIcon; // ˵����ͼ��
        public AudioClip voiceClip; // ����
        public bool autoAdvance = false; // �Ƿ��Զ�ǰ��
        public float autoAdvanceDelay = 3f; // �Զ�ǰ���ӳ�
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
        // ��ȡ��ҿ�����
        player1Controller = FindPlayerController("Player1");
        player2Controller = FindPlayerController("Player2");

        // ���ضԻ����
        if (player1DialoguePanel != null) player1DialoguePanel.SetActive(false);
        if (player2DialoguePanel != null) player2DialoguePanel.SetActive(false);

        // ��ʼ��Ϸʱ�ȴ����ⰴ��������ʼ�Ի�
        StartCoroutine(WaitForStartInput());
    }

    IEnumerator WaitForStartInput()
    {
        Debug.Log("�ȴ����ⰴ����ʼ�Ի�...");

        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        // ��ʼ����1�Ի�
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

    // �ڹؼ������������־
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

    void StartDialogueSequence(DialogueSequence sequence)
    {
        if (isDialogueActive) return;

        currentSequence = sequence;
        currentDialogueIndex = 0;
        isDialogueActive = true;

        // ��������ƶ�
        LockPlayerMovement(true);

        // ��ʾ�Ի����
        if (player1DialoguePanel != null) player1DialoguePanel.SetActive(true);
        if (player2DialoguePanel != null) player2DialoguePanel.SetActive(true);

        // �������п�ʼ�¼�
        sequence.onSequenceStart?.Invoke();

        // ��ʾ��һ��Ի�
        ShowDialogueLine(currentSequence.dialogueLines[currentDialogueIndex]);
    }

    void ShowDialogueLine(DialogueLine line)
    {
        // ֹ֮ͣǰ�Ĵ���Ч��
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        // ����˵������Ϣ
        if (player1SpeakerName != null) player1SpeakerName.text = line.speakerName;
        if (player2SpeakerName != null) player2SpeakerName.text = line.speakerName;
        if (player1SpeakerIcon != null) player1SpeakerIcon.sprite = line.speakerIcon;
        if (player2SpeakerIcon != null) player2SpeakerIcon.sprite = line.speakerIcon;

        // ��ʼ����Ч��
        typingCoroutine = StartCoroutine(TypeDialogueLine(line));

        //// ��������
        //if (line.voiceClip != null)
        //{
        //    AudioManager.Instance.PlayVoice(line.voiceClip);
        //}

        // �Զ�ǰ������
        if (line.autoAdvance)
        {
            StartCoroutine(AutoAdvanceDialogue(line.autoAdvanceDelay));
        }
        else
        {
            // �ȴ���Ұ���
            waitingForPlayer1 = currentSequence.requireBothPlayers;
            waitingForPlayer2 = currentSequence.requireBothPlayers;
        }
    }

    IEnumerator TypeDialogueLine(DialogueLine line)
    {
        // ���1�ı�����Ч��
        if (player1DialogueText != null)
        {
            player1DialogueText.text = "";
            foreach (char c in line.player1Text)
            {
                player1DialogueText.text += c;
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

        // ���ضԻ����
        if (player1DialoguePanel != null) player1DialoguePanel.SetActive(false);
        if (player2DialoguePanel != null) player2DialoguePanel.SetActive(false);

        // ��������ƶ�
        LockPlayerMovement(false);

        // �������н����¼�
        currentSequence.onSequenceEnd?.Invoke();

        Debug.Log($"�Ի����н���: {currentSequence.sequenceName}");
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

    // ����������ǰ�Ի�
    public void SkipCurrentDialogue()
    {
        if (isDialogueActive)
        {
            EndDialogueSequence();
        }
    }

    // ǿ�ƿ�ʼ�ض��Ի������ڵ��ԣ�
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
    [Header("����ѡ��")]
    public bool enableDialogueDebug = true;

    void Log(string message)
    {
        if (enableDialogueDebug)
        {
            Debug.Log($"[DialogueManager] {message}");
        }
    }


}