using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Scene2DualSynthesisManager : MonoBehaviour
{
    public static Scene2DualSynthesisManager Instance; // ����ģʽ��ȫ�ַ���

    [Header("�ϳ���������")]
    public SynthesisZone player1SynthesisZone;  // ���1�ĺϳ�����
    public SynthesisZone player2SynthesisZone;  // ���2�ĺϳ�����

    [Header("����1�ϳ�����")]
    public string laptopItemName = "�ʼǱ�����";   // ����1��Ҫ����Ʒ1
    public string emptyUSBItemName = "��U��";     // ����1��Ҫ����Ʒ2
    public string infoUSBItemName = "�����ϵ�U��"; // ����1�ϳɽ��
    public GameObject infoUSBPrefab;              // ����1�ϳɲ����Ԥ����

    [Header("����2�ϳ�����")]
    public string penItemName = "��";              // ����2��Ҫ����Ʒ1
    public string emptyPaperItemName = "���Ծ�";   // ����2��Ҫ����Ʒ2
    public string filledPaperItemName = "д����Ծ�"; // ����2�ϳɽ��
    public GameObject filledPaperPrefab;           // ����2�ϳɲ����Ԥ����

    [Header("�ϳ�λ������")]
    public Transform task1SpawnPoint;  // ����1�ϳɲ�������λ��
    public Transform task2SpawnPoint;  // ����2�ϳɲ�������λ��

    [Header("�ϳ��¼�")]
    public UnityEvent OnTask1SynthesisReady;   // ����1�ϳ���������ʱ����
    public UnityEvent OnTask1SynthesisComplete; // ����1�ϳ����ʱ����
    public UnityEvent OnTask2SynthesisReady;   // ����2�ϳ���������ʱ����
    public UnityEvent OnTask2SynthesisComplete; // ����2�ϳ����ʱ����

    [Header("�ϳ�ϵͳ����")]
    public bool enablePriorityControl = true; // �Ƿ��������ȼ�����
    public float synthesisCooldown = 2f;      // �ϳ���ȴʱ��

    [Header("����ѡ��")]
    public bool enableDebugLogs = true; // �Ƿ����õ�����־

    // ˽�б��� - ��Ϸ״̬����
    private GameProgress currentProgress = GameProgress.Task1; // ��ǰ��Ϸ����
    private bool task1Completed = false; // ����1���״̬
    private bool task2Completed = false; // ����2���״̬
    private List<InteractableItem> player1ZoneItems = new List<InteractableItem>(); // ���1������Ʒ�б�
    private List<InteractableItem> player2ZoneItems = new List<InteractableItem>(); // ���2������Ʒ�б�
    private float lastSynthesisTime = 0f;     // �ϴκϳ�ʱ��
    private bool isPerformingSynthesis = false; // �Ƿ�����ִ�кϳ�

    // ��Ϸ����ö��
    public enum GameProgress
    {
        Task1,    // ����1�׶�
        Task2,    // ����2�׶�
        Complete  // ��ɽ׶�
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
    }

    void Start()
    {
        InitializeSynthesisZones(); // ��ʼ���ϳ�����
        StartCoroutine(SynthesisCheckRoutine()); // ��ʼ�ϳɼ��Э��

        // ����CraftingManager�ĺϳ��¼�
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.OnCraftingSuccess.AddListener(OnCraftingSuccess);
        }
    }

    // �������ϳ�ϵͳ�ɹ�ʱ����
    void OnCraftingSuccess(CraftingRecipe recipe)
    {
        Log($"�յ��ϳɳɹ��¼�: {recipe.recipeName}");

        // �����䷽�����ж����ĸ�����
        if (recipe.recipeName.Contains("U��") || recipe.resultItemPrefab.name.Contains(infoUSBItemName))
        {
            if (!task1Completed)
            {
                Log($"��⵽����1�ϳɳɹ�: {recipe.recipeName}");
                CompleteTask1(); // �������1
            }
        }
        else if (recipe.recipeName.Contains("�Ծ�") || recipe.resultItemPrefab.name.Contains(filledPaperItemName))
        {
            if (!task2Completed)
            {
                Log($"��⵽����2�ϳɳɹ�: {recipe.recipeName}");
                CompleteTask2(); // �������2
            }
        }
    }

    void OnDestroy()
    {
        // ȡ���¼���������ֹ�ڴ�й©
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.OnCraftingSuccess.RemoveListener(OnCraftingSuccess);
        }
    }

    // ��ʼ���ϳ�����
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
        UpdateZoneItems();        // ���������ڵ���Ʒ
        CheckSynthesisConditions(); // ���ϳ�����
    }

    // ���������ϳ������ڵ���Ʒ�б�
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

    // ��ȡ��������Ч�Ŀɽ�����Ʒ
    List<InteractableItem> GetValidItemsInZone(SynthesisZone zone)
    {
        var validItems = new List<InteractableItem>();

        if (zone == null)
        {
            LogWarning("�ϳ�����Ϊnull");
            return validItems;
        }

        // ʹ�������������ڵ���Ʒ
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

    // �����Ʒ�Ƿ�����ںϳ�
    bool IsItemValidForSynthesis(InteractableItem item)
    {
        if (item == null) return false;
        if (item.isBeingHeld) return false;         // �����ŵ���Ʒ���ܺϳ�
        if (item.isInExchangeProcess) return false; // �ڽ��������е���Ʒ���ܺϳ�
        if (!item.canBePickedUp) return false;      // ����ʰȡ����Ʒ���ܺϳ�
        return true;
    }

    // ���ϳ������Ƿ�����
    void CheckSynthesisConditions()
    {
        if (Time.time - lastSynthesisTime < synthesisCooldown) return; // ��ȴ���
        if (isPerformingSynthesis) return; // ���ںϳ���
        if (task1Completed && task2Completed) return; // �������������

        // �������1�ϳ�����
        if (!task1Completed && currentProgress == GameProgress.Task1)
        {
            if (CheckTask1Synthesis())
            {
                Log("����1�ϳ��������㣬��ʼר�úϳ�");
                StartCoroutine(PerformTask1Synthesis()); // ��ʼ����1�ϳ�Э��
            }
        }

        // �������2�ϳ�����
        if (!task2Completed && currentProgress == GameProgress.Task2)
        {
            if (CheckTask2Synthesis())
            {
                Log("����2�ϳ��������㣬��ʼר�úϳ�");
                StartCoroutine(PerformTask2Synthesis()); // ��ʼ����2�ϳ�Э��
            }
        }
    }

    // �������1�ϳ����������1�бʼǱ����� + ���2�п�U��
    bool CheckTask1Synthesis()
    {
        bool player1HasLaptop = CheckItemInZone(player1ZoneItems, laptopItemName);
        bool player2HasEmptyUSB = CheckItemInZone(player2ZoneItems, emptyUSBItemName);

        if (player1HasLaptop && player2HasEmptyUSB)
        {
            Log("����1�ϳ��������㣺�ʼǱ����� + ��U��");
            OnTask1SynthesisReady?.Invoke(); // �����ϳ�׼���¼�
            return true;
        }
        return false;
    }

    // �������2�ϳ����������1�б� + ���2�п��Ծ�
    bool CheckTask2Synthesis()
    {
        bool player1HasPen = CheckItemInZone(player1ZoneItems, penItemName);
        bool player2HasEmptyPaper = CheckItemInZone(player2ZoneItems, emptyPaperItemName);

        if (player1HasPen && player2HasEmptyPaper)
        {
            Log("����2�ϳ��������㣺�� + ���Ծ�");
            OnTask2SynthesisReady?.Invoke(); // �����ϳ�׼���¼�
            return true;
        }
        return false;
    }

    // ����������Ƿ���ָ�����Ƶ���Ʒ
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

    // ִ������1�ϳɵ�Э��
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

        // ���Һϳ��������Ʒ
        InteractableItem laptop = FindItemInZone(player1ZoneItems, laptopItemName);
        InteractableItem emptyUSB = FindItemInZone(player2ZoneItems, emptyUSBItemName);

        if (laptop != null && emptyUSB != null && infoUSBPrefab != null)
        {
            LockItemsForSynthesis(laptop, emptyUSB); // ������Ʒ

            // ========== �޸Ŀ�ʼ������������Ϣ ==========
            // ���úϳ�����ʹ�����1��������Ϊ��������1�������ɣ�
            if (CraftingManager.Instance != null && player1SynthesisZone != null)
            {
                CraftingManager.Instance.SetLastUsedZone(player1SynthesisZone);
            }

            // ������Ʒ�б����ںϳ�
            List<InteractableItem> synthesisItems = new List<InteractableItem> { laptop, emptyUSB };

            // ���úϳɹ�����
            CraftingRecipe matchedRecipe = CraftingManager.Instance.CombineItems(synthesisItems, player1SynthesisZone);
            // ========== �޸Ľ��� ==========

            yield return StartCoroutine(PlaySynthesisEffects()); // ���źϳ�Ч��

            // ����ϳɳɹ�����������߼�
            if (matchedRecipe != null)
            {
                DestroySynthesisItems(laptop, emptyUSB); // ����ԭ����

                // ��������Ʒ
                yield return StartCoroutine(SpawnNewItem(infoUSBPrefab, task1SpawnPoint.position, infoUSBItemName));

                CompleteTask1(); // �������1���
            }
            else
            {
                LogError("����1�ϳ�ʧ��");
                if (laptop != null) UnlockItem(laptop);
                if (emptyUSB != null) UnlockItem(emptyUSB);
            }
        }
        else
        {
            LogError("����1�ϳ�����������");
            if (laptop != null) UnlockItem(laptop);
            if (emptyUSB != null) UnlockItem(emptyUSB);
        }

        isPerformingSynthesis = false;
    }

    // ִ������2�ϳɵ�Э��
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

        // ���Һϳ��������Ʒ
        InteractableItem pen = FindItemInZone(player1ZoneItems, penItemName);
        InteractableItem emptyPaper = FindItemInZone(player2ZoneItems, emptyPaperItemName);

        if (pen != null && emptyPaper != null && filledPaperPrefab != null)
        {
            LockItemsForSynthesis(pen, emptyPaper); // ������Ʒ

            // ========== �޸Ŀ�ʼ������������Ϣ ==========
            // ���úϳ�����ʹ�����2��������Ϊ��������2�������ɣ�
            if (CraftingManager.Instance != null && player2SynthesisZone != null)
            {
                CraftingManager.Instance.SetLastUsedZone(player2SynthesisZone);
            }

            // ������Ʒ�б����ںϳ�
            List<InteractableItem> synthesisItems = new List<InteractableItem> { pen, emptyPaper };

            // ���úϳɹ�����
            CraftingRecipe matchedRecipe = CraftingManager.Instance.CombineItems(synthesisItems, player2SynthesisZone);
            // ========== �޸Ľ��� ==========

            yield return StartCoroutine(PlaySynthesisEffects()); // ���źϳ�Ч��

            // ����ϳɳɹ�����������߼�
            if (matchedRecipe != null)
            {
                DestroySynthesisItems(pen, emptyPaper); // ����ԭ����

                // ��������Ʒ
                yield return StartCoroutine(SpawnNewItem(filledPaperPrefab, task2SpawnPoint.position, filledPaperItemName));

                CompleteTask2(); // �������2���
            }
            else
            {
                LogError("����2�ϳ�ʧ��");
                if (pen != null) UnlockItem(pen);
                if (emptyPaper != null) UnlockItem(emptyPaper);
            }
        }
        else
        {
            LogError("����2�ϳ�����������");
            if (pen != null) UnlockItem(pen);
            if (emptyPaper != null) UnlockItem(emptyPaper);
        }

        isPerformingSynthesis = false;
    }

    // �������ڲ���ָ�����Ƶ���Ʒ
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

    // �������ںϳɵ���Ʒ����ֹ���ƶ���
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
                    item.Rb.isKinematic = true; // ��Ϊ�˶�ѧ����������Ӱ��
                    item.Rb.velocity = Vector3.zero;
                }

                Log($"������Ʒ: {item.itemName} ����ר�úϳ�");
            }
        }
    }

    // ������Ʒ
    void UnlockItem(InteractableItem item)
    {
        if (item != null)
        {
            item.isInExchangeProcess = false;
            item.canBePickedUp = true;

            if (item.Rb != null)
            {
                item.Rb.isKinematic = false; // �ָ�����ģ��
            }
        }
    }

    // ���źϳ�Ч����Э��
    IEnumerator PlaySynthesisEffects()
    {
        Log("���źϳ�Ч��");
        yield return new WaitForSeconds(1.5f); // �ȴ�1.5��ģ��ϳɹ���
    }

    // ���ٺϳ��õ�ԭ����
    void DestroySynthesisItems(params InteractableItem[] items)
    {
        foreach (var item in items)
        {
            if (item != null)
            {
                RemoveItemFromZone(item); // �������б����Ƴ�
                Destroy(item.gameObject); // ������Ϸ����
                Log($"������Ʒ: {item.itemName}");
            }
        }
    }

    // ��������Ʒ�б����Ƴ�ָ����Ʒ
    void RemoveItemFromZone(InteractableItem item)
    {
        player1ZoneItems.Remove(item);
        player2ZoneItems.Remove(item);
    }

    // ��������Ʒ��Э��
    IEnumerator SpawnNewItem(GameObject prefab, Vector3 position, string itemName)
    {
        Log($"��������Ʒ: {itemName} ��λ��: {position}");

        GameObject newItemObj = Instantiate(prefab, position, Quaternion.identity);

        // ��ӵ���Ч��
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

    // �������1
    void CompleteTask1()
    {
        if (task1Completed)
        {
            LogWarning("����1�Ѿ���ɣ��ظ����� CompleteTask1");
            return;
        }

        task1Completed = true;
        currentProgress = GameProgress.Task2; // ��������2�׶�

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

    // �������2
    void CompleteTask2()
    {
        task2Completed = true;
        currentProgress = GameProgress.Complete; // ������ɽ׶�

        Log($"=== ����2��� ===");
        Log($"����UnityEvent������������: {OnTask2SynthesisComplete?.GetPersistentEventCount() ?? 0}");

        OnTask2SynthesisComplete?.Invoke();

        Log("����2��ɣ����кϳɽ���");
    }

    // ���ڼ��ϳɵ�Э��
    IEnumerator SynthesisCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            UpdateZoneItems(); // ÿ0.5�����һ��������Ʒ
        }
    }

    // ������������ʼ����1
    public void StartTask1()
    {
        currentProgress = GameProgress.Task1;
        task1Completed = false;
        Log("��ʼ����1�ϳɼ��");
    }

    // ������������ʼ����2
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

    // ��־���߷���
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