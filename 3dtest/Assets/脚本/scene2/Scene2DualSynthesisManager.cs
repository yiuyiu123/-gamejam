using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Scene2DualSynthesisManager : MonoBehaviour
{
    public static Scene2DualSynthesisManager Instance;

    [Header("�ϳ���������")]
    public SynthesisZone player1SynthesisZone;
    public SynthesisZone player2SynthesisZone;

    [Header("����1�ϳ�����")]
    public string laptopItemName = "�ʼǱ�����";
    public string emptyUSBItemName = "��U��";
    public string infoUSBItemName = "�����ϵ�U��";
    public GameObject infoUSBPrefab;

    [Header("����2�ϳ�����")]
    public string penItemName = "��";
    public string emptyPaperItemName = "���Ծ�";
    public string filledPaperItemName = "д����Ծ�";
    public GameObject filledPaperPrefab;

    [Header("�ϳ�λ������")]
    public Transform task1SpawnPoint;
    public Transform task2SpawnPoint;

    [Header("�ϳ��¼�")]
    public UnityEvent OnTask1SynthesisReady;
    public UnityEvent OnTask1SynthesisComplete;
    public UnityEvent OnTask2SynthesisReady;
    public UnityEvent OnTask2SynthesisComplete;

    [Header("�ϳ�ϵͳ����")]
    public bool enablePriorityControl = true;
    public float synthesisCooldown = 2f;

    [Header("����ѡ��")]
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

        // ����CraftingManager�ĺϳ��¼�
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.OnCraftingSuccess.AddListener(OnCraftingSuccess);
        }
    }

    void OnCraftingSuccess(CraftingRecipe recipe)
    {
        Log($"�յ��ϳɳɹ��¼�: {recipe.recipeName}");

        // �����䷽�����ж����ĸ�����
        if (recipe.recipeName.Contains("U��") || recipe.resultItemPrefab.name.Contains(infoUSBItemName))
        {
            if (!task1Completed)
            {
                Log($"��⵽����1�ϳɳɹ�: {recipe.recipeName}");
                CompleteTask1();
            }
        }
        else if (recipe.recipeName.Contains("�Ծ�") || recipe.resultItemPrefab.name.Contains(filledPaperItemName))
        {
            if (!task2Completed)
            {
                Log($"��⵽����2�ϳɳɹ�: {recipe.recipeName}");
                CompleteTask2();
            }
        }
    }
    void OnDestroy()
    {
        // ȡ���¼�����
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.OnCraftingSuccess.RemoveListener(OnCraftingSuccess);
        }
    }
    void InitializeSynthesisZones()
    {
        if (player1SynthesisZone != null)
        {
            Log("���1�ϳ������ʼ�����");
        }

        if (player2SynthesisZone != null)
        {
            Log("���2�ϳ������ʼ�����");
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
            LogWarning("�ϳ�����Ϊnull");
            return validItems;
        }

        // ʹ��������
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
                Log("����1�ϳ��������㣬��ʼר�úϳ�");
                StartCoroutine(PerformTask1Synthesis());
            }
        }

        if (!task2Completed && currentProgress == GameProgress.Task2)
        {
            if (CheckTask2Synthesis())
            {
                Log("����2�ϳ��������㣬��ʼר�úϳ�");
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
            Log("����1�ϳ��������㣺�ʼǱ����� + ��U��");
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
            Log("����2�ϳ��������㣺�� + ���Ծ�");
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
            LogWarning("���кϳ��ڽ����У������˴κϳ�");
            yield break;
        }

        isPerformingSynthesis = true;
        lastSynthesisTime = Time.time;

        Log("��ʼִ������1�ϳ�");

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
            LogError("����1�ϳ�����������");
            if (laptop != null) UnlockItem(laptop);
            if (emptyUSB != null) UnlockItem(emptyUSB);
        }

        isPerformingSynthesis = false;
    }

    IEnumerator PerformTask2Synthesis()
    {
        if (isPerformingSynthesis)
        {
            LogWarning("���кϳ��ڽ����У������˴κϳ�");
            yield break;
        }

        isPerformingSynthesis = true;
        lastSynthesisTime = Time.time;

        Log("��ʼִ������2�ϳ�");

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
            LogError("����2�ϳ�����������");
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

                Log($"������Ʒ: {item.itemName} ����ר�úϳ�");
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
        Log("���źϳ�Ч��");
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
                Log($"������Ʒ: {item.itemName}");
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
        Log($"��������Ʒ: {itemName} ��λ��: {position}");

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
            Log($"����Ʒ������: {newItem.itemName}");
        }

        yield return new WaitForSeconds(0.5f);
    }

    void CompleteTask1()
    {
        if (task1Completed)
        {
            LogWarning("����1�Ѿ���ɣ��ظ����� CompleteTask1");
            return;
        }

        task1Completed = true;
        currentProgress = GameProgress.Task2;

        Log($"=== ����1��� ===");
        Log($"����UnityEvent������������: {OnTask1SynthesisComplete?.GetPersistentEventCount() ?? 0}");

        // ����1��ʹ��UnityEvent
        try
        {
            OnTask1SynthesisComplete?.Invoke();
            Log("UnityEvent���óɹ�");
        }
        catch (System.Exception e)
        {
            LogError($"UnityEvent����ʧ��: {e.Message}");

            // ����2�����÷���
            Scene2TaskManager taskManager = FindObjectOfType<Scene2TaskManager>();
            if (taskManager != null)
            {
                taskManager.OnTask1SynthesisComplete();
                Log("���÷������óɹ�");
            }
            else
            {
                LogError("�޷��ҵ�Scene2TaskManager��");
            }
        }

        Log("����1��ɣ���ʼ����2");
    }

    void CompleteTask2()
    {
        task2Completed = true;
        currentProgress = GameProgress.Complete;

        Log($"=== ����2��� ===");
        Log($"����UnityEvent������������: {OnTask2SynthesisComplete?.GetPersistentEventCount() ?? 0}");

        OnTask2SynthesisComplete?.Invoke();

        Log("����2��ɣ����кϳɽ���");
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
        Log("��ʼ����1�ϳɼ��");
    }

    public void StartTask2()
    {
        currentProgress = GameProgress.Task2;
        task2Completed = false;
        Log("��ʼ����2�ϳɼ��");
    }

    // ���Թ���
    [ContextMenu("��ϸ�ϳ����")]
    public void DetailedSynthesisDiagnosis()
    {
        Log("=== ��ϸ�ϳ���� ===");
        Log($"���1����: {player1SynthesisZone?.zoneID ?? "null"}");
        Log($"���2����: {player2SynthesisZone?.zoneID ?? "null"}");

        UpdateZoneItems();
        Log($"���1������Ʒ��: {player1ZoneItems.Count}");
        Log($"���2������Ʒ��: {player2ZoneItems.Count}");

        Log($"����1��������: {CheckTask1Synthesis()}");
        Log($"����2��������: {CheckTask2Synthesis()}");
        Log($"��ǰ����: {currentProgress}");
        Log($"����1���: {task1Completed}");
        Log($"����2���: {task2Completed}");
    }

    [ContextMenu("ǿ�����úϳ�״̬")]
    public void ForceResetSynthesisState()
    {
        Log("ǿ�����úϳ�״̬");
        isPerformingSynthesis = false;
        lastSynthesisTime = 0f;
    }

    [ContextMenu("�ֶ���������1�ϳ�")]
    public void ManualTriggerTask1Synthesis()
    {
        if (!task1Completed && !isPerformingSynthesis)
        {
            StartCoroutine(PerformTask1Synthesis());
        }
    }

    [ContextMenu("�ֶ���������2�ϳ�")]
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