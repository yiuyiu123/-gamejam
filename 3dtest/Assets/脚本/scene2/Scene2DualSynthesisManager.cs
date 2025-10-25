using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Scene2DualSynthesisManager : MonoBehaviour
{
    public static Scene2DualSynthesisManager Instance; // 单例模式，全局访问

    [Header("合成区域设置")]
    public SynthesisZone player1SynthesisZone;  // 玩家1的合成区域
    public SynthesisZone player2SynthesisZone;  // 玩家2的合成区域

    [Header("任务1合成设置")]
    public string laptopItemName = "笔记本电脑";   // 任务1需要的物品1
    public string emptyUSBItemName = "空U盘";     // 任务1需要的物品2
    public string infoUSBItemName = "有资料的U盘"; // 任务1合成结果
    public GameObject infoUSBPrefab;              // 任务1合成产物的预制体

    [Header("任务2合成设置")]
    public string penItemName = "笔";              // 任务2需要的物品1
    public string emptyPaperItemName = "空试卷";   // 任务2需要的物品2
    public string filledPaperItemName = "写完的试卷"; // 任务2合成结果
    public GameObject filledPaperPrefab;           // 任务2合成产物的预制体

    [Header("合成位置设置")]
    public Transform task1SpawnPoint;  // 任务1合成产物生成位置
    public Transform task2SpawnPoint;  // 任务2合成产物生成位置

    [Header("合成事件")]
    public UnityEvent OnTask1SynthesisReady;   // 任务1合成条件满足时触发
    public UnityEvent OnTask1SynthesisComplete; // 任务1合成完成时触发
    public UnityEvent OnTask2SynthesisReady;   // 任务2合成条件满足时触发
    public UnityEvent OnTask2SynthesisComplete; // 任务2合成完成时触发

    [Header("合成系统控制")]
    public bool enablePriorityControl = true; // 是否启用优先级控制
    public float synthesisCooldown = 2f;      // 合成冷却时间

    [Header("调试选项")]
    public bool enableDebugLogs = true; // 是否启用调试日志

    // 私有变量 - 游戏状态管理
    private GameProgress currentProgress = GameProgress.Task1; // 当前游戏进度
    private bool task1Completed = false; // 任务1完成状态
    private bool task2Completed = false; // 任务2完成状态
    private List<InteractableItem> player1ZoneItems = new List<InteractableItem>(); // 玩家1区域物品列表
    private List<InteractableItem> player2ZoneItems = new List<InteractableItem>(); // 玩家2区域物品列表
    private float lastSynthesisTime = 0f;     // 上次合成时间
    private bool isPerformingSynthesis = false; // 是否正在执行合成

    // 游戏进度枚举
    public enum GameProgress
    {
        Task1,    // 任务1阶段
        Task2,    // 任务2阶段
        Complete  // 完成阶段
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
    }

    void Start()
    {
        InitializeSynthesisZones(); // 初始化合成区域
        StartCoroutine(SynthesisCheckRoutine()); // 开始合成检查协程

        // 监听CraftingManager的合成事件
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.OnCraftingSuccess.AddListener(OnCraftingSuccess);
        }
    }

    // 当其他合成系统成功时调用
    void OnCraftingSuccess(CraftingRecipe recipe)
    {
        Log($"收到合成成功事件: {recipe.recipeName}");

        // 根据配方名称判断是哪个任务
        if (recipe.recipeName.Contains("U盘") || recipe.resultItemPrefab.name.Contains(infoUSBItemName))
        {
            if (!task1Completed)
            {
                Log($"检测到任务1合成成功: {recipe.recipeName}");
                CompleteTask1(); // 完成任务1
            }
        }
        else if (recipe.recipeName.Contains("试卷") || recipe.resultItemPrefab.name.Contains(filledPaperItemName))
        {
            if (!task2Completed)
            {
                Log($"检测到任务2合成成功: {recipe.recipeName}");
                CompleteTask2(); // 完成任务2
            }
        }
    }

    void OnDestroy()
    {
        // 取消事件监听，防止内存泄漏
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.OnCraftingSuccess.RemoveListener(OnCraftingSuccess);
        }
    }

    // 初始化合成区域
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
        UpdateZoneItems();        // 更新区域内的物品
        CheckSynthesisConditions(); // 检查合成条件
    }

    // 更新两个合成区域内的物品列表
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

    // 获取区域内有效的可交互物品
    List<InteractableItem> GetValidItemsInZone(SynthesisZone zone)
    {
        var validItems = new List<InteractableItem>();

        if (zone == null)
        {
            LogWarning("合成区域为null");
            return validItems;
        }

        // 使用物理检测区域内的物品
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

    // 检查物品是否可用于合成
    bool IsItemValidForSynthesis(InteractableItem item)
    {
        if (item == null) return false;
        if (item.isBeingHeld) return false;         // 被拿着的物品不能合成
        if (item.isInExchangeProcess) return false; // 在交换过程中的物品不能合成
        if (!item.canBePickedUp) return false;      // 不能拾取的物品不能合成
        return true;
    }

    // 检查合成条件是否满足
    void CheckSynthesisConditions()
    {
        if (Time.time - lastSynthesisTime < synthesisCooldown) return; // 冷却检查
        if (isPerformingSynthesis) return; // 正在合成中
        if (task1Completed && task2Completed) return; // 所有任务已完成

        // 检查任务1合成条件
        if (!task1Completed && currentProgress == GameProgress.Task1)
        {
            if (CheckTask1Synthesis())
            {
                Log("任务1合成条件满足，开始专用合成");
                StartCoroutine(PerformTask1Synthesis()); // 开始任务1合成协程
            }
        }

        // 检查任务2合成条件
        if (!task2Completed && currentProgress == GameProgress.Task2)
        {
            if (CheckTask2Synthesis())
            {
                Log("任务2合成条件满足，开始专用合成");
                StartCoroutine(PerformTask2Synthesis()); // 开始任务2合成协程
            }
        }
    }

    // 检查任务1合成条件：玩家1有笔记本电脑 + 玩家2有空U盘
    bool CheckTask1Synthesis()
    {
        bool player1HasLaptop = CheckItemInZone(player1ZoneItems, laptopItemName);
        bool player2HasEmptyUSB = CheckItemInZone(player2ZoneItems, emptyUSBItemName);

        if (player1HasLaptop && player2HasEmptyUSB)
        {
            Log("任务1合成条件满足：笔记本电脑 + 空U盘");
            OnTask1SynthesisReady?.Invoke(); // 触发合成准备事件
            return true;
        }
        return false;
    }

    // 检查任务2合成条件：玩家1有笔 + 玩家2有空试卷
    bool CheckTask2Synthesis()
    {
        bool player1HasPen = CheckItemInZone(player1ZoneItems, penItemName);
        bool player2HasEmptyPaper = CheckItemInZone(player2ZoneItems, emptyPaperItemName);

        if (player1HasPen && player2HasEmptyPaper)
        {
            Log("任务2合成条件满足：笔 + 空试卷");
            OnTask2SynthesisReady?.Invoke(); // 触发合成准备事件
            return true;
        }
        return false;
    }

    // 检查区域内是否有指定名称的物品
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

    // 执行任务1合成的协程
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

        // 查找合成所需的物品
        InteractableItem laptop = FindItemInZone(player1ZoneItems, laptopItemName);
        InteractableItem emptyUSB = FindItemInZone(player2ZoneItems, emptyUSBItemName);

        if (laptop != null && emptyUSB != null && infoUSBPrefab != null)
        {
            LockItemsForSynthesis(laptop, emptyUSB); // 锁定物品

            // ========== 修改开始：传递区域信息 ==========
            // 设置合成区域（使用玩家1的区域，因为结果在玩家1区域生成）
            if (CraftingManager.Instance != null && player1SynthesisZone != null)
            {
                CraftingManager.Instance.SetLastUsedZone(player1SynthesisZone);
            }

            // 创建物品列表用于合成
            List<InteractableItem> synthesisItems = new List<InteractableItem> { laptop, emptyUSB };

            // 调用合成管理器
            CraftingRecipe matchedRecipe = CraftingManager.Instance.CombineItems(synthesisItems, player1SynthesisZone);
            // ========== 修改结束 ==========

            yield return StartCoroutine(PlaySynthesisEffects()); // 播放合成效果

            // 如果合成成功，处理后续逻辑
            if (matchedRecipe != null)
            {
                DestroySynthesisItems(laptop, emptyUSB); // 销毁原材料

                // 生成新物品
                yield return StartCoroutine(SpawnNewItem(infoUSBPrefab, task1SpawnPoint.position, infoUSBItemName));

                CompleteTask1(); // 标记任务1完成
            }
            else
            {
                LogError("任务1合成失败");
                if (laptop != null) UnlockItem(laptop);
                if (emptyUSB != null) UnlockItem(emptyUSB);
            }
        }
        else
        {
            LogError("任务1合成条件不完整");
            if (laptop != null) UnlockItem(laptop);
            if (emptyUSB != null) UnlockItem(emptyUSB);
        }

        isPerformingSynthesis = false;
    }

    // 执行任务2合成的协程
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

        // 查找合成所需的物品
        InteractableItem pen = FindItemInZone(player1ZoneItems, penItemName);
        InteractableItem emptyPaper = FindItemInZone(player2ZoneItems, emptyPaperItemName);

        if (pen != null && emptyPaper != null && filledPaperPrefab != null)
        {
            LockItemsForSynthesis(pen, emptyPaper); // 锁定物品

            // ========== 修改开始：传递区域信息 ==========
            // 设置合成区域（使用玩家2的区域，因为结果在玩家2区域生成）
            if (CraftingManager.Instance != null && player2SynthesisZone != null)
            {
                CraftingManager.Instance.SetLastUsedZone(player2SynthesisZone);
            }

            // 创建物品列表用于合成
            List<InteractableItem> synthesisItems = new List<InteractableItem> { pen, emptyPaper };

            // 调用合成管理器
            CraftingRecipe matchedRecipe = CraftingManager.Instance.CombineItems(synthesisItems, player2SynthesisZone);
            // ========== 修改结束 ==========

            yield return StartCoroutine(PlaySynthesisEffects()); // 播放合成效果

            // 如果合成成功，处理后续逻辑
            if (matchedRecipe != null)
            {
                DestroySynthesisItems(pen, emptyPaper); // 销毁原材料

                // 生成新物品
                yield return StartCoroutine(SpawnNewItem(filledPaperPrefab, task2SpawnPoint.position, filledPaperItemName));

                CompleteTask2(); // 标记任务2完成
            }
            else
            {
                LogError("任务2合成失败");
                if (pen != null) UnlockItem(pen);
                if (emptyPaper != null) UnlockItem(emptyPaper);
            }
        }
        else
        {
            LogError("任务2合成条件不完整");
            if (pen != null) UnlockItem(pen);
            if (emptyPaper != null) UnlockItem(emptyPaper);
        }

        isPerformingSynthesis = false;
    }

    // 在区域内查找指定名称的物品
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

    // 锁定用于合成的物品（防止被移动）
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
                    item.Rb.isKinematic = true; // 设为运动学，不受物理影响
                    item.Rb.velocity = Vector3.zero;
                }

                Log($"锁定物品: {item.itemName} 用于专用合成");
            }
        }
    }

    // 解锁物品
    void UnlockItem(InteractableItem item)
    {
        if (item != null)
        {
            item.isInExchangeProcess = false;
            item.canBePickedUp = true;

            if (item.Rb != null)
            {
                item.Rb.isKinematic = false; // 恢复物理模拟
            }
        }
    }

    // 播放合成效果的协程
    IEnumerator PlaySynthesisEffects()
    {
        Log("播放合成效果");
        yield return new WaitForSeconds(1.5f); // 等待1.5秒模拟合成过程
    }

    // 销毁合成用的原材料
    void DestroySynthesisItems(params InteractableItem[] items)
    {
        foreach (var item in items)
        {
            if (item != null)
            {
                RemoveItemFromZone(item); // 从区域列表中移除
                Destroy(item.gameObject); // 销毁游戏对象
                Log($"销毁物品: {item.itemName}");
            }
        }
    }

    // 从区域物品列表中移除指定物品
    void RemoveItemFromZone(InteractableItem item)
    {
        player1ZoneItems.Remove(item);
        player2ZoneItems.Remove(item);
    }

    // 生成新物品的协程
    IEnumerator SpawnNewItem(GameObject prefab, Vector3 position, string itemName)
    {
        Log($"生成新物品: {itemName} 在位置: {position}");

        GameObject newItemObj = Instantiate(prefab, position, Quaternion.identity);

        // 添加弹出效果
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

    // 完成任务1
    void CompleteTask1()
    {
        if (task1Completed)
        {
            LogWarning("任务1已经完成，重复调用 CompleteTask1");
            return;
        }

        task1Completed = true;
        currentProgress = GameProgress.Task2; // 进入任务2阶段

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

    // 完成任务2
    void CompleteTask2()
    {
        task2Completed = true;
        currentProgress = GameProgress.Complete; // 进入完成阶段

        Log($"=== 任务2完成 ===");
        Log($"调用UnityEvent，监听器数量: {OnTask2SynthesisComplete?.GetPersistentEventCount() ?? 0}");

        OnTask2SynthesisComplete?.Invoke();

        Log("任务2完成，所有合成结束");
    }

    // 定期检查合成的协程
    IEnumerator SynthesisCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            UpdateZoneItems(); // 每0.5秒更新一次区域物品
        }
    }

    // 公共方法：开始任务1
    public void StartTask1()
    {
        currentProgress = GameProgress.Task1;
        task1Completed = false;
        Log("开始任务1合成检测");
    }

    // 公共方法：开始任务2
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

    // 日志工具方法
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