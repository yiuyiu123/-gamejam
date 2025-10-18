using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Scene2DualSynthesisManager : MonoBehaviour
{
    public static Scene2DualSynthesisManager Instance;

    [Header("合成区域设置")]
    public SynthesisZone player1SynthesisZone;
    public SynthesisZone player2SynthesisZone;

    [Header("任务1合成设置")]
    public string laptopItemName = "笔记本电脑";
    public string emptyUSBItemName = "空U盘";
    public string infoUSBItemName = "有资料的U盘";
    public GameObject infoUSBPrefab;

    [Header("任务2合成设置")]
    public string penItemName = "笔";
    public string emptyPaperItemName = "空试卷";
    public string filledPaperItemName = "写完的试卷";
    public GameObject filledPaperPrefab;

    [Header("合成位置设置")]
    public Transform task1SpawnPoint;
    public Transform task2SpawnPoint;

    [Header("合成事件")]
    public UnityEvent OnTask1SynthesisReady;
    public UnityEvent OnTask1SynthesisComplete;
    public UnityEvent OnTask2SynthesisReady;
    public UnityEvent OnTask2SynthesisComplete;

    [Header("合成系统控制")]
    public bool enablePriorityControl = true;
    public float synthesisCooldown = 2f;

    [Header("调试选项")]
    public bool enableDebugLogs = true;

    private GameProgress currentProgress = GameProgress.Task1;
    private bool task1Completed = false;
    private bool task2Completed = false;
    private List<InteractableItem> player1ZoneItems = new List<InteractableItem>();
    private List<InteractableItem> player2ZoneItems = new List<InteractableItem>();
    private float lastSynthesisTime = 0f;
    private bool isPerformingSynthesis = false;

    public enum GameProgress
    {
        Task1,
        Task2,
        Complete
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
        InitializeSynthesisZones();
        StartCoroutine(SynthesisCheckRoutine());

        // 监听CraftingManager的合成事件
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.OnCraftingSuccess.AddListener(OnCraftingSuccess);
        }
    }

    void OnCraftingSuccess(CraftingRecipe recipe)
    {
        Log($"收到合成成功事件: {recipe.recipeName}");

        // 根据配方名称判断是哪个任务
        if (recipe.recipeName.Contains("U盘") || recipe.resultItemPrefab.name.Contains(infoUSBItemName))
        {
            if (!task1Completed)
            {
                Log($"检测到任务1合成成功: {recipe.recipeName}");
                CompleteTask1();
            }
        }
        else if (recipe.recipeName.Contains("试卷") || recipe.resultItemPrefab.name.Contains(filledPaperItemName))
        {
            if (!task2Completed)
            {
                Log($"检测到任务2合成成功: {recipe.recipeName}");
                CompleteTask2();
            }
        }
    }
    void OnDestroy()
    {
        // 取消事件监听
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.OnCraftingSuccess.RemoveListener(OnCraftingSuccess);
        }
    }
    void InitializeSynthesisZones()
    {
        if (player1SynthesisZone != null)
        {
            Log("玩家1合成区域初始化完成");
        }

        if (player2SynthesisZone != null)
        {
            Log("玩家2合成区域初始化完成");
        }
    }

    void Update()
    {
        UpdateZoneItems();
        CheckSynthesisConditions();
    }

    void UpdateZoneItems()
    {
        if (player1SynthesisZone != null)
        {
            player1ZoneItems = GetValidItemsInZone(player1SynthesisZone);
        }

        if (player2SynthesisZone != null)
        {
            player2ZoneItems = GetValidItemsInZone(player2SynthesisZone);
        }
    }

    List<InteractableItem> GetValidItemsInZone(SynthesisZone zone)
    {
        var validItems = new List<InteractableItem>();

        if (zone == null)
        {
            LogWarning("合成区域为null");
            return validItems;
        }

        // 使用物理检测
        float detectionRadius = 3f;
        Collider[] colliders = Physics.OverlapSphere(zone.transform.position, detectionRadius);
        foreach (Collider collider in colliders)
        {
            InteractableItem item = collider.GetComponent<InteractableItem>();
            if (item != null && IsItemValidForSynthesis(item))
            {
                validItems.Add(item);
            }
        }

        return validItems;
    }

    bool IsItemValidForSynthesis(InteractableItem item)
    {
        if (item == null) return false;
        if (item.isBeingHeld) return false;
        if (item.isInExchangeProcess) return false;
        if (!item.canBePickedUp) return false;
        return true;
    }

    void CheckSynthesisConditions()
    {
        if (Time.time - lastSynthesisTime < synthesisCooldown) return;
        if (isPerformingSynthesis) return;
        if (task1Completed && task2Completed) return;

        if (!task1Completed && currentProgress == GameProgress.Task1)
        {
            if (CheckTask1Synthesis())
            {
                Log("任务1合成条件满足，开始专用合成");
                StartCoroutine(PerformTask1Synthesis());
            }
        }

        if (!task2Completed && currentProgress == GameProgress.Task2)
        {
            if (CheckTask2Synthesis())
            {
                Log("任务2合成条件满足，开始专用合成");
                StartCoroutine(PerformTask2Synthesis());
            }
        }
    }

    bool CheckTask1Synthesis()
    {
        bool player1HasLaptop = CheckItemInZone(player1ZoneItems, laptopItemName);
        bool player2HasEmptyUSB = CheckItemInZone(player2ZoneItems, emptyUSBItemName);

        if (player1HasLaptop && player2HasEmptyUSB)
        {
            Log("任务1合成条件满足：笔记本电脑 + 空U盘");
            OnTask1SynthesisReady?.Invoke();
            return true;
        }
        return false;
    }

    bool CheckTask2Synthesis()
    {
        bool player1HasPen = CheckItemInZone(player1ZoneItems, penItemName);
        bool player2HasEmptyPaper = CheckItemInZone(player2ZoneItems, emptyPaperItemName);

        if (player1HasPen && player2HasEmptyPaper)
        {
            Log("任务2合成条件满足：笔 + 空试卷");
            OnTask2SynthesisReady?.Invoke();
            return true;
        }
        return false;
    }

    bool CheckItemInZone(List<InteractableItem> zoneItems, string itemName)
    {
        foreach (var item in zoneItems)
        {
            if (item != null && item.itemName == itemName)
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator PerformTask1Synthesis()
    {
        if (isPerformingSynthesis)
        {
            LogWarning("已有合成在进行中，跳过此次合成");
            yield break;
        }

        isPerformingSynthesis = true;
        lastSynthesisTime = Time.time;

        Log("开始执行任务1合成");

        InteractableItem laptop = FindItemInZone(player1ZoneItems, laptopItemName);
        InteractableItem emptyUSB = FindItemInZone(player2ZoneItems, emptyUSBItemName);

        if (laptop != null && emptyUSB != null && infoUSBPrefab != null)
        {
            LockItemsForSynthesis(laptop, emptyUSB);

            yield return StartCoroutine(PlaySynthesisEffects());

            DestroySynthesisItems(laptop, emptyUSB);

            yield return StartCoroutine(SpawnNewItem(infoUSBPrefab, task1SpawnPoint.position, infoUSBItemName));

            CompleteTask1();
        }
        else
        {
            LogError("任务1合成条件不完整");
            if (laptop != null) UnlockItem(laptop);
            if (emptyUSB != null) UnlockItem(emptyUSB);
        }

        isPerformingSynthesis = false;
    }

    IEnumerator PerformTask2Synthesis()
    {
        if (isPerformingSynthesis)
        {
            LogWarning("已有合成在进行中，跳过此次合成");
            yield break;
        }

        isPerformingSynthesis = true;
        lastSynthesisTime = Time.time;

        Log("开始执行任务2合成");

        InteractableItem pen = FindItemInZone(player1ZoneItems, penItemName);
        InteractableItem emptyPaper = FindItemInZone(player2ZoneItems, emptyPaperItemName);

        if (pen != null && emptyPaper != null && filledPaperPrefab != null)
        {
            LockItemsForSynthesis(pen, emptyPaper);

            yield return StartCoroutine(PlaySynthesisEffects());

            DestroySynthesisItems(pen, emptyPaper);

            yield return StartCoroutine(SpawnNewItem(filledPaperPrefab, task2SpawnPoint.position, filledPaperItemName));

            CompleteTask2();
        }
        else
        {
            LogError("任务2合成条件不完整");
            if (pen != null) UnlockItem(pen);
            if (emptyPaper != null) UnlockItem(emptyPaper);
        }

        isPerformingSynthesis = false;
    }

    InteractableItem FindItemInZone(List<InteractableItem> zoneItems, string itemName)
    {
        foreach (var item in zoneItems)
        {
            if (item != null && item.itemName == itemName)
            {
                return item;
            }
        }
        return null;
    }

    void LockItemsForSynthesis(params InteractableItem[] items)
    {
        foreach (var item in items)
        {
            if (item != null)
            {
                item.isInExchangeProcess = true;
                item.canBePickedUp = false;

                if (item.Rb != null)
                {
                    item.Rb.isKinematic = true;
                    item.Rb.velocity = Vector3.zero;
                }

                Log($"锁定物品: {item.itemName} 用于专用合成");
            }
        }
    }

    void UnlockItem(InteractableItem item)
    {
        if (item != null)
        {
            item.isInExchangeProcess = false;
            item.canBePickedUp = true;

            if (item.Rb != null)
            {
                item.Rb.isKinematic = false;
            }
        }
    }

    IEnumerator PlaySynthesisEffects()
    {
        Log("播放合成效果");
        yield return new WaitForSeconds(1.5f);
    }

    void DestroySynthesisItems(params InteractableItem[] items)
    {
        foreach (var item in items)
        {
            if (item != null)
            {
                RemoveItemFromZone(item);
                Destroy(item.gameObject);
                Log($"销毁物品: {item.itemName}");
            }
        }
    }

    void RemoveItemFromZone(InteractableItem item)
    {
        player1ZoneItems.Remove(item);
        player2ZoneItems.Remove(item);
    }

    IEnumerator SpawnNewItem(GameObject prefab, Vector3 position, string itemName)
    {
        Log($"生成新物品: {itemName} 在位置: {position}");

        GameObject newItemObj = Instantiate(prefab, position, Quaternion.identity);

        Rigidbody rb = newItemObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 popDirection = new Vector3(Random.Range(-0.3f, 0.3f), 1f, Random.Range(-0.3f, 0.3f)).normalized;
            rb.AddForce(popDirection * 3f, ForceMode.Impulse);
        }

        InteractableItem newItem = newItemObj.GetComponent<InteractableItem>();
        if (newItem != null)
        {
            Log($"新物品已生成: {newItem.itemName}");
        }

        yield return new WaitForSeconds(0.5f);
    }

    void CompleteTask1()
    {
        if (task1Completed)
        {
            LogWarning("任务1已经完成，重复调用 CompleteTask1");
            return;
        }

        task1Completed = true;
        currentProgress = GameProgress.Task2;

        Log($"=== 任务1完成 ===");
        Log($"调用UnityEvent，监听器数量: {OnTask1SynthesisComplete?.GetPersistentEventCount() ?? 0}");

        // 方法1：使用UnityEvent
        try
        {
            OnTask1SynthesisComplete?.Invoke();
            Log("UnityEvent调用成功");
        }
        catch (System.Exception e)
        {
            LogError($"UnityEvent调用失败: {e.Message}");

            // 方法2：备用方法
            Scene2TaskManager taskManager = FindObjectOfType<Scene2TaskManager>();
            if (taskManager != null)
            {
                taskManager.OnTask1SynthesisComplete();
                Log("备用方法调用成功");
            }
            else
            {
                LogError("无法找到Scene2TaskManager！");
            }
        }

        Log("任务1完成，开始任务2");
    }

    void CompleteTask2()
    {
        task2Completed = true;
        currentProgress = GameProgress.Complete;

        Log($"=== 任务2完成 ===");
        Log($"调用UnityEvent，监听器数量: {OnTask2SynthesisComplete?.GetPersistentEventCount() ?? 0}");

        OnTask2SynthesisComplete?.Invoke();

        Log("任务2完成，所有合成结束");
    }

    IEnumerator SynthesisCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            UpdateZoneItems();
        }
    }

    public void StartTask1()
    {
        currentProgress = GameProgress.Task1;
        task1Completed = false;
        Log("开始任务1合成检测");
    }

    public void StartTask2()
    {
        currentProgress = GameProgress.Task2;
        task2Completed = false;
        Log("开始任务2合成检测");
    }

    // 调试工具
    [ContextMenu("详细合成诊断")]
    public void DetailedSynthesisDiagnosis()
    {
        Log("=== 详细合成诊断 ===");
        Log($"玩家1区域: {player1SynthesisZone?.zoneID ?? "null"}");
        Log($"玩家2区域: {player2SynthesisZone?.zoneID ?? "null"}");

        UpdateZoneItems();
        Log($"玩家1区域物品数: {player1ZoneItems.Count}");
        Log($"玩家2区域物品数: {player2ZoneItems.Count}");

        Log($"任务1条件满足: {CheckTask1Synthesis()}");
        Log($"任务2条件满足: {CheckTask2Synthesis()}");
        Log($"当前进度: {currentProgress}");
        Log($"任务1完成: {task1Completed}");
        Log($"任务2完成: {task2Completed}");
    }

    [ContextMenu("强制重置合成状态")]
    public void ForceResetSynthesisState()
    {
        Log("强制重置合成状态");
        isPerformingSynthesis = false;
        lastSynthesisTime = 0f;
    }

    [ContextMenu("手动触发任务1合成")]
    public void ManualTriggerTask1Synthesis()
    {
        if (!task1Completed && !isPerformingSynthesis)
        {
            StartCoroutine(PerformTask1Synthesis());
        }
    }

    [ContextMenu("手动触发任务2合成")]
    public void ManualTriggerTask2Synthesis()
    {
        if (!task2Completed && !isPerformingSynthesis)
        {
            StartCoroutine(PerformTask2Synthesis());
        }
    }

    void Log(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[Scene2Synthesis] {message}");
        }
    }

    void LogWarning(string message)
    {
        if (enableDebugLogs)
        {
            Debug.LogWarning($"[Scene2Synthesis] {message}");
        }
    }

    void LogError(string message)
    {
        if (enableDebugLogs)
        {
            Debug.LogError($"[Scene2Synthesis] {message}");
        }
    }
}