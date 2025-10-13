using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSynthesisManager : MonoBehaviour
{
    public static GlobalSynthesisManager Instance;

    [Header("ȫ�ֺϳ�����")]
    public float synthesisCheckInterval = 0.5f;

    [Header("�ϳ���Ʒ����λ������")]
    public SynthesisResultSpawnMode spawnMode = SynthesisResultSpawnMode.FirstZone;
    public SynthesisZone specificSpawnZone; // ��ѡ���ض�����ʱʹ��
    public Transform customSpawnPoint; // ��ѡ���Զ���λ��ʱʹ��

    public enum SynthesisResultSpawnMode
    {
        FirstZone,      // �ڵ�һ����������
        LastZone,       // �����һ����������  
        SpecificZone,   // ��ָ����������
        CustomPosition, // ���Զ���λ������
        RandomZone,     // �������������
        UseGlobalSetting // ʹ��ȫ������
    }

    private List<SynthesisZone> allZones = new List<SynthesisZone>();
    private List<InteractableItem> allItemsInAllZones = new List<InteractableItem>();
    private bool isGlobalCombining = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // ���ڼ��ȫ�ֺϳ�
        StartCoroutine(GlobalSynthesisCheckRoutine());
    }

    // ע��ϳ�����
    public void RegisterZone(SynthesisZone zone)
    {
        if (!allZones.Contains(zone))
        {
            allZones.Add(zone);
            Debug.Log($"ע��ϳ�����: {zone.zoneID}");
        }
    }

    // ȡ��ע��ϳ�����
    public void UnregisterZone(SynthesisZone zone)
    {
        if (allZones.Contains(zone))
        {
            allZones.Remove(zone);
            Debug.Log($"ȡ��ע��ϳ�����: {zone.zoneID}");
        }
    }

    // ��ȡ���������е�������Ʒ
    public List<InteractableItem> GetAllItemsInAllZones()
    {
        allItemsInAllZones.Clear();

        foreach (SynthesisZone zone in allZones)
        {
            List<InteractableItem> zoneItems = zone.GetItemsInZone();
            foreach (InteractableItem item in zoneItems)
            {
                if (item != null && !allItemsInAllZones.Contains(item))
                {
                    allItemsInAllZones.Add(item);
                }
            }
        }

        Debug.Log($"ȫ����Ʒ���: {allItemsInAllZones.Count} ����Ʒ�� {allZones.Count} ��������");
        return new List<InteractableItem>(allItemsInAllZones);
    }

    // �����䷽��ȡ�ϳ���Ʒ�ĳ���λ��
    private Vector3 GetRecipeSpawnPosition(CraftingRecipe recipe)
    {
        // ����䷽����Ϊʹ��ȫ�����ã���ʹ��ȫ������
        if (recipe.spawnMode == SynthesisResultSpawnMode.UseGlobalSetting)
        {
            return GetGlobalSpawnPosition();
        }

        // ����ʹ���䷽���ض�����
        switch (recipe.spawnMode)
        {
            case SynthesisResultSpawnMode.FirstZone:
                if (allZones.Count > 0)
                {
                    SynthesisZone zone = allZones[0];
                    return zone.itemSpawnPoint != null ? zone.itemSpawnPoint.position : zone.throwTarget.position + Vector3.up * 2f;
                }
                break;

            case SynthesisResultSpawnMode.LastZone:
                if (allZones.Count > 0)
                {
                    SynthesisZone zone = allZones[allZones.Count - 1];
                    return zone.itemSpawnPoint != null ? zone.itemSpawnPoint.position : zone.throwTarget.position + Vector3.up * 2f;
                }
                break;

            case SynthesisResultSpawnMode.SpecificZone:
                if (recipe.specificSpawnZone != null)
                {
                    return recipe.specificSpawnZone.itemSpawnPoint != null ?
                        recipe.specificSpawnZone.itemSpawnPoint.position :
                        recipe.specificSpawnZone.throwTarget.position + Vector3.up * 2f;
                }
                break;

            case SynthesisResultSpawnMode.CustomPosition:
                if (recipe.customSpawnPoint != null)
                {
                    return recipe.customSpawnPoint.position;
                }
                break;

            case SynthesisResultSpawnMode.RandomZone:
                if (allZones.Count > 0)
                {
                    SynthesisZone randomZone = allZones[Random.Range(0, allZones.Count)];
                    return randomZone.itemSpawnPoint != null ? randomZone.itemSpawnPoint.position : randomZone.throwTarget.position + Vector3.up * 2f;
                }
                break;
        }

        // Ĭ�ϻ��˵�ȫ������
        return GetGlobalSpawnPosition();
    }

    // ��ȡȫ�ֺϳ���Ʒ�ĳ���λ�� - ��Ϊ public
    public Vector3 GetGlobalSpawnPosition()
    {
        switch (spawnMode)
        {
            case SynthesisResultSpawnMode.FirstZone:
                if (allZones.Count > 0)
                {
                    SynthesisZone zone = allZones[0];
                    return zone.itemSpawnPoint != null ? zone.itemSpawnPoint.position : zone.throwTarget.position + Vector3.up * 2f;
                }
                break;

            case SynthesisResultSpawnMode.LastZone:
                if (allZones.Count > 0)
                {
                    SynthesisZone zone = allZones[allZones.Count - 1];
                    return zone.itemSpawnPoint != null ? zone.itemSpawnPoint.position : zone.throwTarget.position + Vector3.up * 2f;
                }
                break;

            case SynthesisResultSpawnMode.SpecificZone:
                if (specificSpawnZone != null)
                {
                    return specificSpawnZone.itemSpawnPoint != null ? specificSpawnZone.itemSpawnPoint.position : specificSpawnZone.throwTarget.position + Vector3.up * 2f;
                }
                break;

            case SynthesisResultSpawnMode.CustomPosition:
                if (customSpawnPoint != null)
                {
                    return customSpawnPoint.position;
                }
                break;

            case SynthesisResultSpawnMode.RandomZone:
                if (allZones.Count > 0)
                {
                    SynthesisZone randomZone = allZones[Random.Range(0, allZones.Count)];
                    return randomZone.itemSpawnPoint != null ? randomZone.itemSpawnPoint.position : randomZone.throwTarget.position + Vector3.up * 2f;
                }
                break;
        }

        // Ĭ�ϻ��˵���һ������
        if (allZones.Count > 0)
        {
            SynthesisZone defaultZone = allZones[0];
            return defaultZone.itemSpawnPoint != null ? defaultZone.itemSpawnPoint.position : defaultZone.throwTarget.position + Vector3.up * 2f;
        }

        return Vector3.zero;
    }

    // �����䷽��ȡ����������Ʒ�ĺϳ�����
    private SynthesisZone GetRecipeSpawnZone(CraftingRecipe recipe)
    {
        // ����䷽����Ϊʹ��ȫ�����ã���ʹ��ȫ������
        if (recipe.spawnMode == SynthesisResultSpawnMode.UseGlobalSetting)
        {
            return GetGlobalSpawnZone();
        }

        // ����ʹ���䷽���ض�����
        switch (recipe.spawnMode)
        {
            case SynthesisResultSpawnMode.FirstZone:
                return allZones.Count > 0 ? allZones[0] : null;

            case SynthesisResultSpawnMode.LastZone:
                return allZones.Count > 0 ? allZones[allZones.Count - 1] : null;

            case SynthesisResultSpawnMode.SpecificZone:
                return recipe.specificSpawnZone;

            case SynthesisResultSpawnMode.RandomZone:
                return allZones.Count > 0 ? allZones[Random.Range(0, allZones.Count)] : null;

            default:
                return GetGlobalSpawnZone();
        }
    }

    // ��ȡȫ������������Ʒ�ĺϳ�����
    private SynthesisZone GetGlobalSpawnZone()
    {
        switch (spawnMode)
        {
            case SynthesisResultSpawnMode.FirstZone:
                return allZones.Count > 0 ? allZones[0] : null;

            case SynthesisResultSpawnMode.LastZone:
                return allZones.Count > 0 ? allZones[allZones.Count - 1] : null;

            case SynthesisResultSpawnMode.SpecificZone:
                return specificSpawnZone;

            case SynthesisResultSpawnMode.RandomZone:
                return allZones.Count > 0 ? allZones[Random.Range(0, allZones.Count)] : null;

            default:
                return allZones.Count > 0 ? allZones[0] : null;
        }
    }

    // ȫ�ֺϳɼ��Э��
    IEnumerator GlobalSynthesisCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(synthesisCheckInterval);

            if (!isGlobalCombining)
            {
                CheckGlobalSynthesis();
            }
        }
    }

    // ���ȫ�ֺϳ�
    public void CheckGlobalSynthesis()
    {
        List<InteractableItem> allItems = GetAllItemsInAllZones();

        // ������Ч��Ʒ - ֻ���˱����к�null����Ʒ
        allItems.RemoveAll(item => item == null || item.isBeingHeld);

        Debug.Log($"ȫ�ֺϳɼ��: {allItems.Count} ����Ч��Ʒ");

        if (allItems.Count >= 2 && !isGlobalCombining)
        {
            Debug.Log($"=== ����ȫ�ֺϳ���������ʼȫ�ֺϳ� ===");
            StartCoroutine(PerformGlobalSynthesis(allItems));
        }
    }

    // ִ��ȫ�ֺϳ�
    IEnumerator PerformGlobalSynthesis(List<InteractableItem> itemsToCombine)
    {
        isGlobalCombining = true;
        Debug.Log("=== ��ʼȫ�ֺϳɹ��� ===");

        // ֻȡǰ2����Ʒ���кϳ�
        if (itemsToCombine.Count > 2)
        {
            itemsToCombine = itemsToCombine.GetRange(0, 2);
        }

        // ��ʾҪ�ϳɵľ�����Ʒ
        string combineItems = "";
        foreach (var item in itemsToCombine)
        {
            combineItems += $"{item.itemName} ";
            item.isInExchangeProcess = true;
            item.canBePickedUp = false;
            Debug.Log($"���úϳ�״̬: {item.itemName} (������: {item.isInExchangeProcess}, ��ʰȡ: {item.canBePickedUp})");
        }
        Debug.Log($"��Ҫȫ�ֺϳɵ���Ʒ: {combineItems}");

        // ʹ�� CraftingManager ��ȡ�ϳɽ�� - ���ڷ��� CraftingRecipe
        Debug.Log($"�� CraftingManager ����ϳ�...");
        CraftingRecipe matchedRecipe = CraftingManager.Instance.CombineItems(itemsToCombine);

        if (matchedRecipe != null && matchedRecipe.resultItemPrefab != null)
        {
            Debug.Log("=== ȫ�ֺϳɳɹ��� ===");

            // �������������Ƴ�Ҫ���ٵ���Ʒ
            foreach (var item in itemsToCombine)
            {
                if (item != null)
                {
                    // �������������Ƴ������Ʒ
                    foreach (SynthesisZone zone in allZones)
                    {
                        zone.RemoveItemFromZone(item);
                    }
                    Debug.Log($"������Ʒ: {item.itemName}");
                    Destroy(item.gameObject);
                }
            }

            // ��ȡ�䷽�ض��ĳ���λ�ú�����
            Vector3 spawnPosition = GetRecipeSpawnPosition(matchedRecipe);
            SynthesisZone spawnZone = GetRecipeSpawnZone(matchedRecipe);

            Debug.Log($"�䷽ {matchedRecipe.recipeName} ���� {matchedRecipe.spawnMode} ģʽ���ɣ�λ��: {spawnPosition}");

            // ��������Ʒ
            Debug.Log($"��λ�� {spawnPosition} ��������Ʒ...");
            yield return StartCoroutine(SpawnResultItem(matchedRecipe.resultItemPrefab, spawnPosition, spawnZone));

            Debug.Log("=== ȫ�ֺϳɹ������ ===");
        }
        else
        {
            Debug.LogWarning("=== ȫ�ֺϳ�ʧ�ܣ�û��ƥ����䷽ ===");

            // ������Ʒ
            foreach (var item in itemsToCombine)
            {
                if (item != null)
                {
                    item.isInExchangeProcess = false;
                    item.canBePickedUp = true;
                    if (item.Rb != null)
                    {
                        item.Rb.isKinematic = false;
                        item.Rb.useGravity = true;
                    }
                    Collider itemCollider = item.ItemCollider;
                    if (itemCollider != null)
                    {
                        itemCollider.enabled = true;
                    }
                    Debug.Log($"��ȫ������Ʒ: {item.itemName}");
                }
            }
        }

        isGlobalCombining = false;
        yield return new WaitForSeconds(0.5f);
        CheckGlobalSynthesis();
    }

    // �޸����ɷ���������λ�ò������������
    IEnumerator SpawnResultItem(GameObject resultPrefab, Vector3 spawnPosition, SynthesisZone spawnZone)
    {
        GameObject newItemObj = Instantiate(resultPrefab, spawnPosition, Quaternion.identity);

        // ��ӵ���Ч��
        Rigidbody rb = newItemObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 popDirection = new Vector3(Random.Range(-0.5f, 0.5f), 1f, Random.Range(-0.5f, 0.5f)).normalized;
            rb.AddForce(popDirection * 5f, ForceMode.Impulse);
        }

        InteractableItem newItem = newItemObj.GetComponent<InteractableItem>();
        if (newItem != null)
        {
            Debug.Log($"�µ���������: {newItem.itemName} ��λ��: {spawnPosition}");

            // ��ѡ��������Ʒ��ӵ�����������
            if (spawnZone != null && !spawnZone.GetItemsInZone().Contains(newItem))
            {
                // ʹ�÷��������������ӵ����򣬻��߲����
                Debug.Log($"����Ʒ {newItem.itemName} ������������ {spawnZone.zoneID} ����");
            }
        }
        else
        {
            Debug.LogWarning("�����ɵ���Ʒû�� InteractableItem ���");
        }

        yield return new WaitForSeconds(0.5f);
    }

    // ǿ�ƺϳɼ�ⷽ��
    [ContextMenu("ǿ��ȫ�ֺϳɼ��")]
    public void ForceGlobalSynthesisCheck()
    {
        Debug.Log("=== ǿ��ȫ�ֺϳɼ�� ===");
        CheckGlobalSynthesis();
    }

    [ContextMenu("��ʾȫ��״̬")]
    public void ShowGlobalStatus()
    {
        List<InteractableItem> allItems = GetAllItemsInAllZones();
        Debug.Log($"=== ȫ�ֺϳ�״̬ ===");
        Debug.Log($"��������: {allZones.Count}");
        Debug.Log($"����Ʒ����: {allItems.Count}");
        Debug.Log($"ȫ�ֺϳ���: {isGlobalCombining}");
        Debug.Log($"����ģʽ: {spawnMode}");
        Debug.Log($"ָ����������: {specificSpawnZone?.zoneID ?? "δ����"}");
        Debug.Log($"�Զ��������: {customSpawnPoint?.name ?? "δ����"}");

        foreach (var item in allItems)
        {
            if (item != null)
            {
                Debug.Log($"- {item.itemName} (����: {item.isBeingHeld}, ����: {item.isInExchangeProcess})");
            }
        }

        // ��ʾÿ���������ϸ״̬
        foreach (SynthesisZone zone in allZones)
        {
            Debug.Log($"���� {zone.zoneID}: {zone.GetItemsInZone().Count} ����Ʒ");
        }
    }

    [ContextMenu("�����޸�������������Ʒ")]
    public void EmergencyUnlockAllItems()
    {
        Debug.Log("=== �����޸�������������Ʒ ===");

        // ���������������Ʒ
        foreach (SynthesisZone zone in allZones)
        {
            zone.EmergencyUnlockAllItems();
        }

        // ����ȫ�ֹ������е���Ʒ
        foreach (var item in allItemsInAllZones)
        {
            if (item != null)
            {
                item.isInExchangeProcess = false;
                item.canBePickedUp = true;
                if (item.Rb != null) item.Rb.isKinematic = false;
            }
        }
        allItemsInAllZones.Clear();

        isGlobalCombining = false;
        Debug.Log("������Ʒ�ѽ���");
    }

    // ���ó���ģʽ�ķ���������������ʱ���ã�
    public void SetSpawnMode(SynthesisResultSpawnMode mode)
    {
        spawnMode = mode;
        Debug.Log($"���úϳ���Ʒ����ģʽΪ: {mode}");
    }

    public void SetSpecificSpawnZone(SynthesisZone zone)
    {
        specificSpawnZone = zone;
        Debug.Log($"����ָ����������Ϊ: {zone?.zoneID ?? "null"}");
    }

    public void SetCustomSpawnPoint(Transform point)
    {
        customSpawnPoint = point;
        Debug.Log($"�����Զ��������Ϊ: {point?.name ?? "null"}");
    }
}