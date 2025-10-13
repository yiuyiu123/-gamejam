using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynthesisZone : MonoBehaviour
{
    [Header("�ϳ���������")]
    public string zoneID = "SynthesisZone";
    public Transform throwTarget;
    public float detectionRadius = 3f;
    public Transform itemSpawnPoint;

    [Header("�ϳ�Ч��")]
    public ParticleSystem synthesisEffect;
    public AudioClip synthesisSound;
    public float synthesisDelay = 1f;

    [Header("��������")]
    public float throwHeight = 3f;
    public float throwDuration = 0.8f;

    [Header("����ѡ��")]
    public bool showDebugGUI = false;

    private List<InteractableItem> itemsInZone = new List<InteractableItem>();
    private AudioSource audioSource;
    private bool isCombining = false;
    private bool hasCheckedThisFrame = false;

    // �����Ʒ��������ĸ���
    private Dictionary<InteractableItem, float> itemEnterTimes = new Dictionary<InteractableItem, float>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (throwTarget == null)
            throwTarget = transform;

        EnsureColliderSize();

        // ע�ᵽȫ�ֹ�����
        if (GlobalSynthesisManager.Instance != null)
        {
            GlobalSynthesisManager.Instance.RegisterZone(this);
        }
    }


    void EnsureColliderSize()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(5, 3, 5);
            Debug.Log($"Ϊ�ϳ����� {zoneID} ����˴�����ײ��");
        }
        else if (!collider.isTrigger)
        {
            collider.isTrigger = true;
            Debug.Log($"�ѽ��ϳ����� {zoneID} ����ײ������Ϊ������");
        }
    }

    void Update()
    {
        if (!hasCheckedThisFrame)
        {
            CheckForSynthesis();
            hasCheckedThisFrame = true;
        }
    }

    void LateUpdate()
    {
        hasCheckedThisFrame = false;
    }

    void OnTriggerEnter(Collider other)
    {
        InteractableItem item = other.GetComponent<InteractableItem>();
        if (item != null)
        {
            // ���ϸ�ĺ����������
            if (ShouldIgnoreItem(item))
            {
                Debug.Log($"������Ʒ {item.itemName} - ������: {item.isBeingHeld}, ������: {item.isInExchangeProcess}");
                return;
            }

            if (!itemsInZone.Contains(item))
            {
                itemsInZone.Add(item);
                // ��¼����ʱ��
                itemEnterTimes[item] = Time.time;

                Debug.Log($"��Ʒ {item.itemName} ����ϳ����� {zoneID}����ǰ������Ʒ��: {itemsInZone.Count}");
                DebugItemsInZone();

                // �������ϳ�
                CheckForSynthesis();
            }
        }
    }
    void OnDestroy()
    {
        // ȡ��ע��
        if (GlobalSynthesisManager.Instance != null)
        {
            GlobalSynthesisManager.Instance.UnregisterZone(this);
        }
    }
    // ��ӹ���������ȫ�ֹ���������
    public List<InteractableItem> GetItemsInZone()
    {
        // ������Ч��Ʒ������Ҫ���˵� canBePickedUp = false ����Ʒ
        // ��Ϊ�ϳ�ʧ�ܺ���Ʒ��Ҫ�����¼��
        itemsInZone.RemoveAll(item =>
            item == null ||
            item.isBeingHeld//||
            //item.isInExchangeProcess || 
            //!item.canBePickedUp         
        );

        // ���������������ڵ���Ʒ����ȫ�ֹ������Լ�����״̬���
        return new List<InteractableItem>(itemsInZone);
    }

    // ���������Ƴ�ָ����Ʒ
    public void RemoveItemFromZone(InteractableItem item)
    {
        if (itemsInZone.Contains(item))
        {
            itemsInZone.Remove(item);
            itemEnterTimes.Remove(item);
            Debug.Log($"������ {zoneID} �Ƴ���Ʒ: {item.itemName}");
        }
    }
    void OnTriggerStay(Collider other)
    {
        InteractableItem item = other.GetComponent<InteractableItem>();
        if (item != null && !itemsInZone.Contains(item))
        {
            if (ShouldIgnoreItem(item))
            {
                return;
            }

            itemsInZone.Add(item);
            itemEnterTimes[item] = Time.time;

            Debug.Log($"��Ʒ {item.itemName} �ںϳ������ڱ���⵽����ǰ������Ʒ��: {itemsInZone.Count}");
            DebugItemsInZone();

            CheckForSynthesis();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (isCombining) return;

        InteractableItem item = other.GetComponent<InteractableItem>();
        if (item != null && itemsInZone.Contains(item))
        {
            itemsInZone.Remove(item);
            itemEnterTimes.Remove(item);

            Debug.Log($"��Ʒ {item.itemName} �뿪�ϳ����� {zoneID}��ʣ����Ʒ��: {itemsInZone.Count}");
            DebugItemsInZone();
        }
    }

    // �Ľ�����Ʒ���Լ��
    bool ShouldIgnoreItem(InteractableItem item)
    {
        if (item == null) return true;
        if (item.isBeingHeld) return true;
        if (item.isInExchangeProcess) return true;

        // �����Ʒ�Ƿ�ձ������������ظ���⣩
        if (itemEnterTimes.ContainsKey(item))
        {
            float timeSinceEnter = Time.time - itemEnterTimes[item];
            if (timeSinceEnter < 0.1f) // 100ms�ڸս������Ʒ
            {
                return false; // �����½������Ʒ
            }
        }

        return false;
    }

    void DebugItemsInZone()
    {
        string debugInfo = $"���� {zoneID} ��Ʒ�б� ({itemsInZone.Count} ��): ";
        foreach (var item in itemsInZone)
        {
            if (item != null)
            {
                debugInfo += $"{item.itemName} ";
            }
        }
        Debug.Log(debugInfo);
    }

    void CheckForSynthesis()
    {
        // ... ���еı��غϳɼ����� ...

        // ͬʱ����ȫ�ֺϳɼ��
        if (GlobalSynthesisManager.Instance != null)
        {
            GlobalSynthesisManager.Instance.CheckGlobalSynthesis();
        }
    }

    IEnumerator PerformSynthesis()
    {
        isCombining = true;
        Debug.Log("=== ��ʼ�ϳɹ��� ===");

        // ʹ�õ�ǰ��Ч����Ʒ�б�
        List<InteractableItem> itemsToCombine = new List<InteractableItem>();
        foreach (var item in itemsInZone)
        {
            if (item != null && !item.isBeingHeld && !item.isInExchangeProcess)
            {
                itemsToCombine.Add(item);
            }
        }

        Debug.Log($"׼���ϳɵ���Ʒ����: {itemsToCombine.Count}");

        if (itemsToCombine.Count < 2)
        {
            Debug.LogWarning("�ϳ�ʧ�ܣ���Ч��Ʒ����2��");
            isCombining = false;
            yield break;
        }

        // ֻȡǰ2����Ʒ
        if (itemsToCombine.Count > 2)
        {
            itemsToCombine = itemsToCombine.GetRange(0, 2);
        }

        string combineItems = "";
        foreach (var item in itemsToCombine)
        {
            combineItems += $"{item.itemName} ";
            item.isInExchangeProcess = true;
            if (item.Rb != null)
            {
                item.Rb.isKinematic = true;
                item.Rb.velocity = Vector3.zero;
            }
            Debug.Log($"�����ϳ���Ʒ: {item.itemName}");
        }
        Debug.Log($"��Ҫ�ϳɵ���Ʒ: {combineItems}");

        if (synthesisEffect != null) synthesisEffect.Play();
        if (synthesisSound != null) audioSource.PlayOneShot(synthesisSound);

        yield return new WaitForSeconds(synthesisDelay);

        GameObject resultPrefab = CraftingManager.Instance.CombineItems(itemsToCombine);

        if (resultPrefab != null)
        {
            Debug.Log("=== �ϳɳɹ��� ===");

            // ����������б����Ƴ���Ʒ
            foreach (var item in itemsToCombine)
            {
                if (item != null)
                {
                    if (itemsInZone.Contains(item)) itemsInZone.Remove(item);
                    itemEnterTimes.Remove(item);
                    Destroy(item.gameObject);
                }
            }

            yield return StartCoroutine(SpawnResultItem(resultPrefab));
            Debug.Log("=== �ϳɹ������ ===");
        }
        else
        {
            Debug.LogWarning("=== �ϳ�ʧ�ܣ�û��ƥ����䷽ ===");
            foreach (var item in itemsToCombine)
            {
                if (item != null)
                {
                    item.isInExchangeProcess = false;
                    if (item.Rb != null) item.Rb.isKinematic = false;
                }
            }
        }

        isCombining = false;
        yield return new WaitForSeconds(0.5f);
        CheckForSynthesis();
    }

    public IEnumerator SpawnResultItem(GameObject resultPrefab)
    {
        Vector3 spawnPosition = itemSpawnPoint != null ? itemSpawnPoint.position : throwTarget.position + Vector3.up * 2f;
        GameObject newItemObj = Instantiate(resultPrefab, spawnPosition, Quaternion.identity);

        Rigidbody rb = newItemObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 popDirection = new Vector3(Random.Range(-0.5f, 0.5f), 1f, Random.Range(-0.5f, 0.5f)).normalized;
            rb.AddForce(popDirection * 5f, ForceMode.Impulse);
        }

        InteractableItem newItem = newItemObj.GetComponent<InteractableItem>();
        if (newItem != null)
        {
            Debug.Log($"�µ���������: {newItem.itemName}");
        }

        yield return new WaitForSeconds(0.5f);
    }

    public void ThrowItemToZone(InteractableItem item)
    {
        StartCoroutine(ThrowItemCoroutine(item));
    }

    IEnumerator ThrowItemCoroutine(InteractableItem item)
    {
        Debug.Log($"��ʼ������Ʒ: {item.itemName} ������ {zoneID}");

        item.isInExchangeProcess = true;
        if (item.Rb != null)
        {
            item.Rb.isKinematic = true;
            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
        }

        Vector3 startPosition = item.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < throwDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / throwDuration;
            Vector3 currentPos = Vector3.Lerp(startPosition, throwTarget.position, progress);
            currentPos.y += Mathf.Sin(progress * Mathf.PI) * throwHeight;
            item.transform.position = currentPos;
            yield return null;
        }

        item.transform.position = throwTarget.position;
        yield return null;

        // ȷ����Ʒ����ӵ�����
        if (!itemsInZone.Contains(item))
        {
            itemsInZone.Add(item);
            itemEnterTimes[item] = Time.time;
            Debug.Log($"������ɺ������Ʒ: {item.itemName} ������ {zoneID}");
        }

        item.isInExchangeProcess = false;
        if (item.Rb != null) item.Rb.isKinematic = false;

        Debug.Log($"��Ʒ {item.itemName} ������ɵ����� {zoneID}");
        DebugItemsInZone();
        CheckForSynthesis();
    }

    // ���Է���
    [ContextMenu("ǿ�ƴ����ϳɼ��")]
    public void ForceSynthesisCheck()
    {
        Debug.Log("=== ǿ�ƴ����ϳɼ�� ===");
        ManualCheckItemsInZone();
        CheckForSynthesis();
    }

    [ContextMenu("��ʾ����״̬")]
    public void ShowZoneStatus()
    {
        Debug.Log($"=== �ϳ����� {zoneID} ״̬ ===");
        Debug.Log($"��Ʒ����: {itemsInZone.Count}");
        Debug.Log($"�ϳ���: {isCombining}");

        foreach (var item in itemsInZone)
        {
            if (item != null)
            {
                Debug.Log($"- {item.itemName} (����: {item.isBeingHeld}, ����: {item.isInExchangeProcess})");
            }
        }
    }

    public void ManualCheckItemsInZone()
    {
        Debug.Log("=== ��ʼ�ֶ����������Ʒ ===");
        itemsInZone.RemoveAll(item => item == null);

        Collider zoneCollider = GetComponent<Collider>();
        if (zoneCollider != null)
        {
            Collider[] collidersInZone = Physics.OverlapBox(zoneCollider.bounds.center, zoneCollider.bounds.extents);
            Debug.Log($"�����⵽ {collidersInZone.Length} ����ײ����������");

            foreach (Collider collider in collidersInZone)
            {
                InteractableItem item = collider.GetComponent<InteractableItem>();
                if (item != null && !itemsInZone.Contains(item) && !ShouldIgnoreItem(item))
                {
                    itemsInZone.Add(item);
                    itemEnterTimes[item] = Time.time;
                    Debug.Log($"�ֶ������Ʒ: {item.itemName}");
                }
            }
        }

        Debug.Log($"�ֶ������ɣ�������Ʒ��: {itemsInZone.Count}");
        DebugItemsInZone();
        CheckForSynthesis();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        if (throwTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(throwTarget.position, 0.5f);
        }
    }
    // ��ӽ�������������Ʒ�ķ���
    [ContextMenu("��������������Ʒ")]
    public void EmergencyUnlockAllItems()
    {
        Debug.Log("=== ��������������Ʒ ===");

        // ���������ڵ���Ʒ
        foreach (var item in itemsInZone)
        {
            if (item != null)
            {
                item.isInExchangeProcess = false;
                item.canBePickedUp = true;
                if (item.Rb != null) item.Rb.isKinematic = false;
                Debug.Log($"������Ʒ: {item.itemName}");
            }
        }

        // ���������б�
        itemsInZone.Clear();
        itemEnterTimes.Clear();
    }
}