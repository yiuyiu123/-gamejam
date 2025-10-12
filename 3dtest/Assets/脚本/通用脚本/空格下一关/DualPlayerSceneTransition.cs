using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DualPlayerSceneTransition : MonoBehaviour
{
    [Header("触发区域设置")]
    public string triggerName = "双玩家场景转换";
    public float triggerRadius = 3f;
    public Color gizmoColor = Color.magenta;

    [Header("场景设置")]
    public string targetSceneName = "NextScene";
    public int targetSceneIndex = -1;

    [Header("转换设置")]
    public KeyCode transitionKey = KeyCode.Space;
    public float transitionDelay = 2f;
    public bool requireAnyPlayer = true; // 任意玩家触发即可

    [Header("同步设置")]
    public float syncWaitTime = 5f; // 等待另一玩家的最大时间
    public bool showCountdown = true; // 显示倒计时

    [Header("UI提示")]
    public Canvas interactionPromptCanvas;
    public UnityEngine.UI.Text promptText;
    public UnityEngine.UI.Text countdownText;
    public string singlePlayerPrompt = "按空格键进入下一场景";
    public string multiPlayerPrompt = "等待另一玩家确认... {0}秒";

    [Header("状态")]
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
            // 检查是否有玩家按下转换键
            CheckForTransitionInput();
        }
        else if (isCountingDown)
        {
            // 更新倒计时显示
            UpdateCountdownDisplay();

            // 检查是否有其他玩家确认
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
        // 检查新进入的玩家
        foreach (var player in currentPlayers)
        {
            if (!playersInZone.Contains(player))
            {
                OnPlayerEnterZone(player);
            }
        }

        // 检查离开的玩家
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
        Debug.Log($"玩家 {GetPlayerName(player)} 进入转换区域: {triggerName}");
    }

    void OnPlayerExitZone(GameObject player)
    {
        Debug.Log($"玩家 {GetPlayerName(player)} 离开转换区域: {triggerName}");

        // 如果玩家离开且正在倒计时，取消倒计时
        if (isCountingDown && confirmedPlayers.Contains(player))
        {
            CancelCountdown();
        }
    }

    string GetPlayerName(GameObject player)
    {
        // 根据你的玩家命名规则返回玩家名称
        if (player.name.Contains("1") || player.CompareTag("Player1"))
            return "玩家1";
        else if (player.name.Contains("2") || player.CompareTag("Player2"))
            return "玩家2";
        else
            return player.name;
    }

    void CheckForTransitionInput()
    {
        // 检查所有在区域内的玩家输入
        foreach (var player in playersInZone)
        {
            if (IsPlayerInputPressed(player) && !confirmedPlayers.Contains(player))
            {
                OnPlayerConfirm(player);
                break; // 一次只处理一个玩家的确认
            }
        }
    }

    bool IsPlayerInputPressed(GameObject player)
    {
        // 这里可以根据玩家身份区分输入
        // 暂时使用统一的空格键
        return Input.GetKeyDown(transitionKey);
    }

    void OnPlayerConfirm(GameObject player)
    {
        Debug.Log($"玩家 {GetPlayerName(player)} 确认场景转换");

        confirmedPlayers.Add(player);

        if (requireAnyPlayer)
        {
            // 任意玩家确认即可开始转换
            StartCountdown();
        }
        else
        {
            // 需要检查是否所有玩家都确认
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

        // 显示倒计时UI
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        UpdatePromptForCountdown();

        // 开始倒计时协程
        countdownCoroutine = StartCoroutine(CountdownCoroutine());

        Debug.Log($"开始倒计时: {syncWaitTime}秒");
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
            // 倒计时结束，开始转换
            StartTransition();
        }
    }

    void UpdateCountdownDisplay()
    {
        if (countdownText != null)
        {
            countdownText.text = $"进入下一场景: {Mathf.CeilToInt(countdownTimer)}秒";
        }

        if (promptText != null)
        {
            string confirmedNames = "";
            foreach (var player in confirmedPlayers)
            {
                confirmedNames += GetPlayerName(player) + " ";
            }
            promptText.text = $"已确认: {confirmedNames}";
        }
    }

    void UpdatePromptForCountdown()
    {
        if (promptText != null)
        {
            promptText.text = "场景转换已触发...";
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

        // 隐藏倒计时UI
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        UpdatePromptVisibility();

        Debug.Log("场景转换已取消");
    }

    void StartTransition()
    {
        isCountingDown = false;
        isTransitioning = true;

        // 隐藏倒计时UI
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        Debug.Log($"开始双玩家场景转换: {triggerName} -> {targetSceneName}");

        // 禁用所有玩家的输入
        DisableAllPlayersInput();

        // 播放转换效果
        PlayTransitionEffects();

        // 开始转换序列
        StartCoroutine(TransitionSequence());
    }

    System.Collections.IEnumerator TransitionSequence()
    {
        // 转换延迟
        yield return new WaitForSeconds(transitionDelay);

        // 保存玩家数据（如果需要）
        SavePlayerStates();

        // 加载目标场景
        LoadTargetScene();
    }

    void DisableAllPlayersInput()
    {
        // 禁用区域内玩家的输入
        foreach (var player in playersInZone)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // 根据你的玩家控制器实现禁用输入
                // playerController.SetInputEnabled(false);
            }
        }

        // 同时也要找到并禁用另一个玩家（即使不在区域内）
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
        // 保存玩家状态，以便在新场景中恢复
        // 例如：位置、生命值、装备等
        PlayerStateSaver.SavePlayerStates();
    }

    void PlayTransitionEffects()
    {
        // 屏幕淡出效果
        StartCoroutine(ScreenFadeEffect());
    }

    System.Collections.IEnumerator ScreenFadeEffect()
    {
        // 实现屏幕淡出效果
        // 可以使用UI Panel覆盖两个玩家的屏幕
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
            Debug.LogError($"场景转换错误: {triggerName} 没有设置目标场景!");
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

    // 公共方法：手动触发转换
    public void ManualTriggerTransition()
    {
        if (!isTransitioning && !isCountingDown)
        {
            StartTransition();
        }
    }

    // 在Scene视图中显示触发区域
    void OnDrawGizmos()
    {
        Gizmos.color = isTransitioning ? Color.red : (isCountingDown ? Color.yellow : gizmoColor);
        Gizmos.DrawWireSphere(transform.position, triggerRadius);

        GUIStyle style = new GUIStyle();
        style.normal.textColor = gizmoColor;
#if UNITY_EDITOR
        string status = isTransitioning ? "转换中" : (isCountingDown ? "倒计时中" : "等待中");
        string players = $"{playersInZone.Count}名玩家在区域内";
        string confirmed = $"{confirmedPlayers.Count}名玩家已确认";

        string statusText = $"{triggerName}\n状态: {status}\n{players}\n{confirmed}";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.3f, statusText, style);
#endif
    }
}