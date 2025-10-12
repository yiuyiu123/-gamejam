using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DualPlayerSceneTransition : MonoBehaviour
{
    [Header("������������")]
    public string triggerName = "˫��ҳ���ת��";
    public float triggerRadius = 3f;
    public Color gizmoColor = Color.magenta;

    [Header("��������")]
    public string targetSceneName = "NextScene";
    public int targetSceneIndex = -1;

    [Header("ת������")]
    public KeyCode transitionKey = KeyCode.Space;
    public float transitionDelay = 2f;
    public bool requireAnyPlayer = true; // ������Ҵ�������

    [Header("ͬ������")]
    public float syncWaitTime = 5f; // �ȴ���һ��ҵ����ʱ��
    public bool showCountdown = true; // ��ʾ����ʱ

    [Header("UI��ʾ")]
    public Canvas interactionPromptCanvas;
    public UnityEngine.UI.Text promptText;
    public UnityEngine.UI.Text countdownText;
    public string singlePlayerPrompt = "���ո��������һ����";
    public string multiPlayerPrompt = "�ȴ���һ���ȷ��... {0}��";

    [Header("״̬")]
    public bool isPlayerInZone = false;
    public bool isTransitioning = false;
    public bool isCountingDown = false;

    private List<GameObject> playersInZone = new List<GameObject>();
    private List<GameObject> confirmedPlayers = new List<GameObject>();
    private AudioSource audioSource;
    private Renderer portalRenderer;
    private float countdownTimer;
    private Coroutine countdownCoroutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        portalRenderer = GetComponent<Renderer>();
        InitializePromptUI();
    }

    void Update()
    {
        if (isTransitioning) return;

        CheckPlayersInZone();

        if (isPlayerInZone && !isCountingDown)
        {
            // ����Ƿ�����Ұ���ת����
            CheckForTransitionInput();
        }
        else if (isCountingDown)
        {
            // ���µ���ʱ��ʾ
            UpdateCountdownDisplay();

            // ����Ƿ����������ȷ��
            CheckForAdditionalConfirmations();
        }
    }

    void InitializePromptUI()
    {
        if (interactionPromptCanvas != null)
        {
            interactionPromptCanvas.gameObject.SetActive(false);

            if (promptText != null)
            {
                promptText.text = singlePlayerPrompt;
            }

            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
            }
        }
    }

    void CheckPlayersInZone()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, triggerRadius);
        List<GameObject> currentPlayers = new List<GameObject>();

        foreach (var collider in hitColliders)
        {
            if (IsPlayer(collider.gameObject))
            {
                currentPlayers.Add(collider.gameObject);
            }
        }

        UpdatePlayersList(currentPlayers);

        bool wasPlayerInZone = isPlayerInZone;
        isPlayerInZone = currentPlayers.Count >= 1;

        if (isPlayerInZone != wasPlayerInZone)
        {
            UpdatePromptVisibility();
        }
    }

    void UpdatePlayersList(List<GameObject> currentPlayers)
    {
        // ����½�������
        foreach (var player in currentPlayers)
        {
            if (!playersInZone.Contains(player))
            {
                OnPlayerEnterZone(player);
            }
        }

        // ����뿪�����
        for (int i = playersInZone.Count - 1; i >= 0; i--)
        {
            if (!currentPlayers.Contains(playersInZone[i]))
            {
                OnPlayerExitZone(playersInZone[i]);
            }
        }

        playersInZone = new List<GameObject>(currentPlayers);
    }

    bool IsPlayer(GameObject obj)
    {
        return obj.CompareTag("Player") || obj.GetComponent<PlayerController>() != null;
    }

    void OnPlayerEnterZone(GameObject player)
    {
        Debug.Log($"��� {GetPlayerName(player)} ����ת������: {triggerName}");
    }

    void OnPlayerExitZone(GameObject player)
    {
        Debug.Log($"��� {GetPlayerName(player)} �뿪ת������: {triggerName}");

        // �������뿪�����ڵ���ʱ��ȡ������ʱ
        if (isCountingDown && confirmedPlayers.Contains(player))
        {
            CancelCountdown();
        }
    }

    string GetPlayerName(GameObject player)
    {
        // �����������������򷵻��������
        if (player.name.Contains("1") || player.CompareTag("Player1"))
            return "���1";
        else if (player.name.Contains("2") || player.CompareTag("Player2"))
            return "���2";
        else
            return player.name;
    }

    void CheckForTransitionInput()
    {
        // ��������������ڵ��������
        foreach (var player in playersInZone)
        {
            if (IsPlayerInputPressed(player) && !confirmedPlayers.Contains(player))
            {
                OnPlayerConfirm(player);
                break; // һ��ֻ����һ����ҵ�ȷ��
            }
        }
    }

    bool IsPlayerInputPressed(GameObject player)
    {
        // ������Ը�����������������
        // ��ʱʹ��ͳһ�Ŀո��
        return Input.GetKeyDown(transitionKey);
    }

    void OnPlayerConfirm(GameObject player)
    {
        Debug.Log($"��� {GetPlayerName(player)} ȷ�ϳ���ת��");

        confirmedPlayers.Add(player);

        if (requireAnyPlayer)
        {
            // �������ȷ�ϼ��ɿ�ʼת��
            StartCountdown();
        }
        else
        {
            // ��Ҫ����Ƿ�������Ҷ�ȷ��
            if (confirmedPlayers.Count >= playersInZone.Count)
            {
                StartTransition();
            }
            else
            {
                StartCountdown();
            }
        }
    }

    void StartCountdown()
    {
        if (isCountingDown) return;

        isCountingDown = true;
        countdownTimer = syncWaitTime;

        // ��ʾ����ʱUI
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        UpdatePromptForCountdown();

        // ��ʼ����ʱЭ��
        countdownCoroutine = StartCoroutine(CountdownCoroutine());

        Debug.Log($"��ʼ����ʱ: {syncWaitTime}��");
    }

    System.Collections.IEnumerator CountdownCoroutine()
    {
        while (countdownTimer > 0 && isCountingDown)
        {
            countdownTimer -= Time.deltaTime;
            yield return null;
        }

        if (isCountingDown)
        {
            // ����ʱ��������ʼת��
            StartTransition();
        }
    }

    void UpdateCountdownDisplay()
    {
        if (countdownText != null)
        {
            countdownText.text = $"������һ����: {Mathf.CeilToInt(countdownTimer)}��";
        }

        if (promptText != null)
        {
            string confirmedNames = "";
            foreach (var player in confirmedPlayers)
            {
                confirmedNames += GetPlayerName(player) + " ";
            }
            promptText.text = $"��ȷ��: {confirmedNames}";
        }
    }

    void UpdatePromptForCountdown()
    {
        if (promptText != null)
        {
            promptText.text = "����ת���Ѵ���...";
        }
    }

    void CheckForAdditionalConfirmations()
    {
        foreach (var player in playersInZone)
        {
            if (IsPlayerInputPressed(player) && !confirmedPlayers.Contains(player))
            {
                OnPlayerConfirm(player);
            }
        }
    }

    void CancelCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        isCountingDown = false;
        confirmedPlayers.Clear();

        // ���ص���ʱUI
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        UpdatePromptVisibility();

        Debug.Log("����ת����ȡ��");
    }

    void StartTransition()
    {
        isCountingDown = false;
        isTransitioning = true;

        // ���ص���ʱUI
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        Debug.Log($"��ʼ˫��ҳ���ת��: {triggerName} -> {targetSceneName}");

        // ����������ҵ�����
        DisableAllPlayersInput();

        // ����ת��Ч��
        PlayTransitionEffects();

        // ��ʼת������
        StartCoroutine(TransitionSequence());
    }

    System.Collections.IEnumerator TransitionSequence()
    {
        // ת���ӳ�
        yield return new WaitForSeconds(transitionDelay);

        // ����������ݣ������Ҫ��
        SavePlayerStates();

        // ����Ŀ�곡��
        LoadTargetScene();
    }

    void DisableAllPlayersInput()
    {
        // ������������ҵ�����
        foreach (var player in playersInZone)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // ���������ҿ�����ʵ�ֽ�������
                // playerController.SetInputEnabled(false);
            }
        }

        // ͬʱҲҪ�ҵ���������һ����ң���ʹ���������ڣ�
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in allPlayers)
        {
            if (!playersInZone.Contains(player))
            {
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    // playerController.SetInputEnabled(false);
                }
            }
        }
    }

    void SavePlayerStates()
    {
        // �������״̬���Ա����³����лָ�
        // ���磺λ�á�����ֵ��װ����
        PlayerStateSaver.SavePlayerStates();
    }

    void PlayTransitionEffects()
    {
        // ��Ļ����Ч��
        StartCoroutine(ScreenFadeEffect());
    }

    System.Collections.IEnumerator ScreenFadeEffect()
    {
        // ʵ����Ļ����Ч��
        // ����ʹ��UI Panel����������ҵ���Ļ
        yield return null;
    }

    void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else if (targetSceneIndex >= 0)
        {
            SceneManager.LoadScene(targetSceneIndex);
        }
        else
        {
            Debug.LogError($"����ת������: {triggerName} û������Ŀ�곡��!");
        }
    }

    void UpdatePromptVisibility()
    {
        if (interactionPromptCanvas != null)
        {
            bool shouldShow = isPlayerInZone && !isTransitioning && !isCountingDown;
            interactionPromptCanvas.gameObject.SetActive(shouldShow);

            if (shouldShow && promptText != null)
            {
                promptText.text = singlePlayerPrompt;
            }
        }
    }

    // �����������ֶ�����ת��
    public void ManualTriggerTransition()
    {
        if (!isTransitioning && !isCountingDown)
        {
            StartTransition();
        }
    }

    // ��Scene��ͼ����ʾ��������
    void OnDrawGizmos()
    {
        Gizmos.color = isTransitioning ? Color.red : (isCountingDown ? Color.yellow : gizmoColor);
        Gizmos.DrawWireSphere(transform.position, triggerRadius);

        GUIStyle style = new GUIStyle();
        style.normal.textColor = gizmoColor;
#if UNITY_EDITOR
        string status = isTransitioning ? "ת����" : (isCountingDown ? "����ʱ��" : "�ȴ���");
        string players = $"{playersInZone.Count}�������������";
        string confirmed = $"{confirmedPlayers.Count}�������ȷ��";

        string statusText = $"{triggerName}\n״̬: {status}\n{players}\n{confirmed}";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.3f, statusText, style);
#endif
    }
}