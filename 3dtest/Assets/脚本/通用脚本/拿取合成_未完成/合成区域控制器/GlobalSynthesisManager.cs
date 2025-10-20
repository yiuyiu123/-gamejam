using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSynthesisManager : MonoBehaviour
{
    public static GlobalSynthesisManager Instance;

    [Header("ȫ�ֺϳ�����")]
    public float synthesisCheckInterval = 1.0f;

    [Header("�ϳ���Ʒ����λ������")]
    public SynthesisResultSpawnMode spawnMode = SynthesisResultSpawnMode.FirstZone;
    public SynthesisZone specificSpawnZone;
    public Transform customSpawnPoint;

    [Header("ȫ�ֺϳ�ʧ�ܵ�������")]
    public bool enableGlobalFailEjection = true;
    public float globalFailEjectionForce = 10f;
    public float globalFailEjectionHeight = 2.5f;

    public enum SynthesisResultSpawnMode
    {
        FirstZone,
        LastZone,
        SpecificZone,
        CustomPosition,
        RandomZone,
        UseGlobalSetting
    }

    private List<SynthesisZone> allZones = new List<SynthesisZone>();
    private bool isGlobalCombining = false;
    private float lastGlobalCheckTime = 0f;
    private float globalCooldown = 1.5f;

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
        StartCoroutine(GlobalSynthesisCheckRoutine());
    }

    public void RegisterZone(SynthesisZone zone)
    {
        if (!allZones.Contains(zone))
        {
            allZones.Add(zone);
        }
    }

    public void UnregisterZone(SynthesisZone zone)
    {
        if (allZones.Contains(zone))
        {
            allZones.Remove(zone);
        }
    }

    IEnumerator GlobalSynthesisCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(synthesisCheckInterval);

            if (!isGlobalCombining && Time.time - lastGlobalCheckTime >= globalCooldown)
            {
                CheckGlobalSynthesis();
            }
        }
    }

    public void CheckGlobalSynthesis()
    {
        if (isGlobalCombining) return;
        if (Time.time - lastGlobalCheckTime < globalCooldown) return;

        List<InteractableItem> allItems = GetAllItemsInAllZones();
        allItems.RemoveAll(item => item == null || item.isBeingHeld);

        if (allItems.Count >= 2)
        {
            StartCoroutine(PerformGlobalSynthesis(allItems));
        }
    }

    IEnumerator PerformGlobalSynthesis(List<InteractableItem> itemsToCombine)
    {
        isGlobalCombining = true;
        lastGlobalCheckTime = Time.time;

        if (itemsToCombine.Count > 2)
        {
            itemsToCombine = itemsToCombine.GetRange(0, 2);
        }

        // ������Ʒ
        foreach (var item in itemsToCombine)
        {
            item.isInExchangeProcess = true;
            item.canBePickedUp = false;
        }

        CraftingRecipe matchedRecipe = CraftingManager.Instance.CombineItems(itemsToCombine);

        if (matchedRecipe != null && matchedRecipe.resultItemPrefab != null)
        {
            // �ϳɳɹ�
            foreach (var item in itemsToCombine)
            {
                if (item != null)
                {
                    foreach (SynthesisZone zone in allZones)
                    {
                        zone.RemoveItemFromZone(item);
                    }
                    Destroy(item.gameObject);
                }
            }

            Vector3 spawnPosition = GetGlobalSpawnPosition();
            yield return StartCoroutine(SpawnResultItem(matchedRecipe.resultItemPrefab, spawnPosition));
        }
        else
        {
            // ȫ�ֺϳ�ʧ�� - ������Ʒ
            Debug.LogWarning($"ȫ�ֺϳ�ʧ�ܣ������� {itemsToCombine.Count} ����Ʒ");
            yield return StartCoroutine(EjectGlobalFailedItems(itemsToCombine));
        }

        isGlobalCombining = false;
    }

    // ������ȫ��ʧ����Ʒ����
    IEnumerator EjectGlobalFailedItems(List<InteractableItem> itemsToEject)
    {
        foreach (var item in itemsToEject)
        {
            if (item != null)
            {
                // �������������Ƴ�
                foreach (SynthesisZone zone in allZones)
                {
                    zone.RemoveItemFromZone(item);
                }

                // ������Ʒ
                item.isInExchangeProcess = false;
                item.canBePickedUp = true;

                // Ӧ�õ�����
                if (item.Rb != null)
                {
                    item.Rb.isKinematic = false;

                    // ȫ�ֵ���ʹ�ø�ǿ����
                    Vector3 ejectionDirection = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(0.5f, 1f),
                        Random.Range(-1f, 1f)
                    ).normalized;

                    item.Rb.AddForce(ejectionDirection * globalFailEjectionForce, ForceMode.Impulse);

                    // �����ת
                    Vector3 randomTorque = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(-1f, 1f),
                        Random.Range(-1f, 1f)
                    ) * globalFailEjectionForce * 0.5f;
                    item.Rb.AddTorque(randomTorque, ForceMode.Impulse);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
    }

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

        if (allZones.Count > 0)
        {
            SynthesisZone defaultZone = allZones[0];
            return defaultZone.itemSpawnPoint != null ? defaultZone.itemSpawnPoint.position : defaultZone.throwTarget.position + Vector3.up * 2f;
        }

        return Vector3.zero;
    }

    IEnumerator SpawnResultItem(GameObject resultPrefab, Vector3 spawnPosition)
    {
        GameObject newItemObj = Instantiate(resultPrefab, spawnPosition, Quaternion.identity);

        Rigidbody rb = newItemObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 popDirection = new Vector3(Random.Range(-0.5f, 0.5f), 1f, Random.Range(-0.5f, 0.5f)).normalized;
            rb.AddForce(popDirection * 5f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private List<InteractableItem> GetAllItemsInAllZones()
    {
        List<InteractableItem> allItems = new List<InteractableItem>();
        foreach (SynthesisZone zone in allZones)
        {
            List<InteractableItem> zoneItems = zone.GetItemsInZone();
            foreach (var item in zoneItems)
            {
                if (item != null && !allItems.Contains(item))
                {
                    allItems.Add(item);
                }
            }
        }
        return allItems;
    }

    [ContextMenu("����ȫ�ֵ���")]
    public void TestGlobalEjection()
    {
        List<InteractableItem> allItems = GetAllItemsInAllZones();
        if (allItems.Count > 0)
        {
            List<InteractableItem> testItems = allItems.GetRange(0, Mathf.Min(2, allItems.Count));
            StartCoroutine(EjectGlobalFailedItems(testItems));
            Debug.Log($"����ȫ�ֵ��� {testItems.Count} ����Ʒ");
        }
    }

    [ContextMenu("�����޸�������������Ʒ")]
    public void EmergencyUnlockAllItems()
    {
        foreach (SynthesisZone zone in allZones)
        {
            zone.EmergencyUnlockAllItems();
        }
        isGlobalCombining = false;
    }
}