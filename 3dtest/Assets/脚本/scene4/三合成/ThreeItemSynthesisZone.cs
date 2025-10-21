using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeItemSynthesisZone : MonoBehaviour
{
    [Header("���ϳ���������")]
    public string zoneID = "ThreeItemSynthesisZone";

    [Header("����Ŀ��λ�� - ������Ʒ�ֱ����ͬλ��")]
    public Transform throwTarget1;
    public Transform throwTarget2;
    public Transform throwTarget3;

    public float detectionRadius = 3f;

    [Header("��������")]
    public float throwHeight = 3f;
    public float throwDuration = 0.8f;

    [Header("�ϳ���ȴʱ��")]
    public float synthesisCooldown = 2f;

    [Header("�ϳ�ʧ�ܵ�������")]
    public bool enableFailEjection = true;
    public float failEjectionForce = 10f;
    public float failEjectionHeight = 2.5f;
    public float failEjectionRandomness = 0.8f;

    private List<InteractableItem> itemsInZone = new List<InteractableItem>();
    private bool isCombining = false;
    private bool hasCheckedThisFrame = false;
    private float lastSynthesisTime = 0f;

    // ����ÿ����Ʒ��Ŀ��λ��
    private Dictionary<InteractableItem, Transform> itemTargetMap = new Dictionary<InteractableItem, Transform>();

    // ������Ŀ��λ��ʹ�ö��У�ȷ����ƽ����
    private Queue<Transform> availableTargets = new Queue<Transform>();

    void Start()
    {
        // ���û������Ŀ��λ�ã�ʹ��Ĭ��λ��
        if (throwTarget1 == null) throwTarget1 = CreateDefaultTarget("Target1", new Vector3(-1, 0, 0));
        if (throwTarget2 == null) throwTarget2 = CreateDefaultTarget("Target2", new Vector3(0, 0, 0));
        if (throwTarget3 == null) throwTarget3 = CreateDefaultTarget("Target3", new Vector3(1, 0, 0));

        // ��ʼ������Ŀ�����
        InitializeTargetQueue();

        EnsureColliderSize();

        Debug.Log($"���ϳ������ʼ����ɣ�����Ŀ��λ��: {throwTarget1.position}, {throwTarget2.position}, {throwTarget3.position}");
    }

    // ��ʼ��Ŀ�����
    void InitializeTargetQueue()
    {
        availableTargets.Clear();
        availableTargets.Enqueue(throwTarget1);
        availableTargets.Enqueue(throwTarget2);
        availableTargets.Enqueue(throwTarget3);
    }

    // ����Ĭ��Ŀ��λ��
    Transform CreateDefaultTarget(string name, Vector3 offset)
    {
        GameObject targetObj = new GameObject($"{zoneID}_{name}");
        targetObj.transform.SetParent(transform);
        targetObj.transform.localPosition = offset;
        return targetObj.transform;
    }

    void EnsureColliderSize()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(5, 3, 5);
        }
        else if (!collider.isTrigger)
        {
            collider.isTrigger = true;
        }
    }

    void Update()
    {
        if (!hasCheckedThisFrame)
        {
            if (Time.time - lastSynthesisTime >= synthesisCooldown)
            {
                CheckForThreeItemSynthesis();
            }
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
        if (item != null && ShouldProcessItem(item))
        {
            if (!itemsInZone.Contains(item))
            {
                itemsInZone.Add(item);

                if (Time.time - lastSynthesisTime >= synthesisCooldown)
                {
                    CheckForThreeItemSynthesis();
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (isCombining) return;

        InteractableItem item = other.GetComponent<InteractableItem>();
        if (item != null && itemsInZone.Contains(item))
        {
            itemsInZone.Remove(item);
            // ��Ŀ��ӳ�����Ƴ�������Ŀ��λ�����¼�����ö���
            if (itemTargetMap.ContainsKey(item))
            {
                Transform freedTarget = itemTargetMap[item];
                itemTargetMap.Remove(item);

                // ֻ�е����Ŀ�겻�ڵ�ǰ���ö�����ʱ�����¼���
                if (!availableTargets.Contains(freedTarget))
                {
                    availableTargets.Enqueue(freedTarget);
                    Debug.Log($"Ŀ��λ�� {freedTarget.name} ���ͷŲ����¼������");
                }
            }
        }
    }

    bool ShouldProcessItem(InteractableItem item)
    {
        if (item == null) return false;
        if (item.isBeingHeld) return false;
        if (item.isInExchangeProcess) return false;
        return true;
    }

    void CheckForThreeItemSynthesis()
    {
        if (isCombining) return;
        if (Time.time - lastSynthesisTime < synthesisCooldown) return;

        // ������Ч��Ʒ
        itemsInZone.RemoveAll(item => item == null || item.isBeingHeld);

        // ֻ��ǡ��3����Ʒʱ�ų������ϳ�
        if (itemsInZone.Count == 3)
        {
            StartCoroutine(PerformThreeItemSynthesis());
        }
    }

    IEnumerator PerformThreeItemSynthesis()
    {
        isCombining = true;
        lastSynthesisTime = Time.time;

        List<InteractableItem> itemsToCombine = new List<InteractableItem>();
        foreach (var item in itemsInZone)
        {
            if (item != null && !item.isBeingHeld && !item.isInExchangeProcess)
            {
                itemsToCombine.Add(item);
            }
        }

        // ȷ������3����Ʒ
        if (itemsToCombine.Count != 3)
        {
            isCombining = false;
            yield break;
        }

        // ����������Ʒ
        foreach (var item in itemsToCombine)
        {
            item.isInExchangeProcess = true;
            if (item.Rb != null)
            {
                item.Rb.isKinematic = true;
                item.Rb.velocity = Vector3.zero;
            }
        }

        // ���źϳ�Ч��
        if (ThreeItemCraftingManager.Instance.synthesisEffect != null)
            Instantiate(ThreeItemCraftingManager.Instance.synthesisEffect, transform.position, Quaternion.identity);

        if (ThreeItemCraftingManager.Instance.synthesisSound != null)
            AudioSource.PlayClipAtPoint(ThreeItemCraftingManager.Instance.synthesisSound, transform.position);

        yield return new WaitForSeconds(ThreeItemCraftingManager.Instance.synthesisDelay);

        // �������ϳ�
        ThreeItemRecipe matchedRecipe = ThreeItemCraftingManager.Instance.CombineThreeItems(itemsToCombine);

        if (matchedRecipe != null && matchedRecipe.resultItemPrefab != null)
        {
            // ���ϳɳɹ�
            foreach (var item in itemsToCombine)
            {
                if (item != null)
                {
                    itemsInZone.Remove(item);
                    // ��Ŀ��ӳ�����Ƴ�������Ŀ��λ�����¼�����ö���
                    if (itemTargetMap.ContainsKey(item))
                    {
                        Transform freedTarget = itemTargetMap[item];
                        itemTargetMap.Remove(item);

                        // ֻ�е����Ŀ�겻�ڵ�ǰ���ö�����ʱ�����¼���
                        if (!availableTargets.Contains(freedTarget))
                        {
                            availableTargets.Enqueue(freedTarget);
                            Debug.Log($"�ϳɳɹ���Ŀ��λ�� {freedTarget.name} ���ͷ�");
                        }
                    }
                    Destroy(item.gameObject);
                }
            }

            // ���ɽ����Ʒ
            Vector3 spawnPosition = ThreeItemCraftingManager.Instance.GetSpawnPosition(matchedRecipe);
            yield return StartCoroutine(SpawnResultItem(matchedRecipe.resultItemPrefab, spawnPosition));

            Debug.Log($"���ϳɳɹ��������� {matchedRecipe.recipeName}");
        }
        else
        {
            // ���ϳ�ʧ�� - ����������Ʒ
            Debug.LogWarning($"���ϳ�ʧ�ܣ������� {itemsToCombine.Count} ����Ʒ");
            yield return StartCoroutine(EjectFailedItems(itemsToCombine));
        }

        isCombining = false;
    }

    IEnumerator EjectFailedItems(List<InteractableItem> itemsToEject)
    {
        Debug.Log($"��ʼ���� {itemsToEject.Count} ��ʧ����Ʒ");

        foreach (var item in itemsToEject)
        {
            if (item != null)
            {
                // �������б����Ƴ�
                itemsInZone.Remove(item);
                // ��Ŀ��ӳ�����Ƴ�������Ŀ��λ�����¼�����ö���
                if (itemTargetMap.ContainsKey(item))
                {
                    Transform freedTarget = itemTargetMap[item];
                    itemTargetMap.Remove(item);

                    // ֻ�е����Ŀ�겻�ڵ�ǰ���ö�����ʱ�����¼���
                    if (!availableTargets.Contains(freedTarget))
                    {
                        availableTargets.Enqueue(freedTarget);
                        Debug.Log($"�ϳ�ʧ�ܣ�Ŀ��λ�� {freedTarget.name} ���ͷ�");
                    }
                }

                // ������Ʒ״̬
                item.isInExchangeProcess = false;

                // ��������
                if (item.Rb != null)
                {
                    item.Rb.isKinematic = false;
                    item.Rb.useGravity = true;

                    // ���㵯������
                    Vector3 ejectionDirection = CalculateEjectionDirection(item.transform.position);

                    // Ӧ�õ�����
                    item.Rb.AddForce(ejectionDirection * failEjectionForce, ForceMode.Impulse);

                    // ��������ת
                    Vector3 randomTorque = new Vector3(
                        Random.Range(-failEjectionRandomness, failEjectionRandomness),
                        Random.Range(-failEjectionRandomness, failEjectionRandomness),
                        Random.Range(-failEjectionRandomness, failEjectionRandomness)
                    ) * failEjectionForce;
                    item.Rb.AddTorque(randomTorque, ForceMode.Impulse);
                }

                Debug.Log($"������Ʒ: {item.itemName}");
            }
        }

        yield return new WaitForSeconds(0.5f);
    }

    Vector3 CalculateEjectionDirection(Vector3 itemPosition)
    {
        // �������򣺴�������������
        Vector3 baseDirection = (itemPosition - transform.position).normalized;

        // ȷ�������ϵķ���
        baseDirection.y = Mathf.Max(baseDirection.y, 0.3f);

        // ��������
        Vector3 randomVariation = new Vector3(
            Random.Range(-failEjectionRandomness, failEjectionRandomness),
            Random.Range(0, failEjectionRandomness * 0.5f),
            Random.Range(-failEjectionRandomness, failEjectionRandomness)
        );

        Vector3 finalDirection = (baseDirection + randomVariation).normalized;
        finalDirection.y += failEjectionHeight * 0.1f;

        return finalDirection;
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

    public void ThrowItemToZone(InteractableItem item)
    {
        StartCoroutine(ThrowItemCoroutine(item));
    }

    IEnumerator ThrowItemCoroutine(InteractableItem item)
    {
        item.isInExchangeProcess = true;
        if (item.Rb != null)
        {
            item.Rb.isKinematic = true;
            item.Rb.velocity = Vector3.zero;
        }

        // Ϊ��Ʒ����Ŀ��λ�� - ʹ�ö���ȷ����ƽ����
        Transform target = GetNextAvailableTarget();
        if (target == null)
        {
            Debug.LogWarning("û�п��õ�Ŀ��λ�ã�ʹ��Ĭ��λ��");
            target = throwTarget1;
        }

        // ��¼��Ʒ��Ŀ��λ��
        itemTargetMap[item] = target;
        Debug.Log($"��Ʒ {item.itemName} ��������λ��: {target.name}");

        Vector3 startPosition = item.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < throwDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / throwDuration;
            Vector3 currentPos = Vector3.Lerp(startPosition, target.position, progress);
            currentPos.y += Mathf.Sin(progress * Mathf.PI) * throwHeight;
            item.transform.position = currentPos;
            yield return null;
        }

        item.transform.position = target.position;

        if (!itemsInZone.Contains(item))
        {
            itemsInZone.Add(item);
        }

        item.isInExchangeProcess = false;
        if (item.Rb != null) item.Rb.isKinematic = false;
    }

    // �޸ģ�ʹ�ö��л�ȡ��һ������Ŀ��
    Transform GetNextAvailableTarget()
    {
        if (availableTargets.Count == 0)
        {
            Debug.LogWarning("����Ŀ��λ�ö���ռ�ã��޷�������Ŀ��");
            return null;
        }

        Transform nextTarget = availableTargets.Dequeue();
        Debug.Log($"����Ŀ��λ��: {nextTarget.name}��ʣ�����Ŀ��: {availableTargets.Count}");
        return nextTarget;
    }

    [ContextMenu("��ʾ����״̬")]
    public void ShowZoneStatus()
    {
        Debug.Log($"=== ���ϳ����� {zoneID} ״̬ ===");
        Debug.Log($"��Ʒ����: {itemsInZone.Count}");
        Debug.Log($"�ϳ���: {isCombining}");
        Debug.Log($"����Ŀ��λ������: {availableTargets.Count}");

        foreach (var item in itemsInZone)
        {
            if (item != null)
            {
                string targetInfo = itemTargetMap.ContainsKey(item) ?
                    $"Ŀ��: {itemTargetMap[item].name}" : "Ŀ��: δ����";
                Debug.Log($"- {item.itemName} ({targetInfo})");
            }
        }

        // ��ʾ�����е�Ŀ��λ��
        Debug.Log("����Ŀ�����:");
        foreach (var target in availableTargets)
        {
            Debug.Log($"  - {target.name}");
        }
    }

    [ContextMenu("��������������Ʒ")]
    public void EmergencyUnlockAllItems()
    {
        foreach (var item in itemsInZone)
        {
            if (item != null)
            {
                item.isInExchangeProcess = false;
                if (item.Rb != null) item.Rb.isKinematic = false;
            }
        }
        itemsInZone.Clear();
        itemTargetMap.Clear();

        // ���³�ʼ��Ŀ�����
        InitializeTargetQueue();

        isCombining = false;
        Debug.Log("������Ʒ�ѽ�����Ŀ�����������");
    }

    void OnDrawGizmosSelected()
    {
        // ��������Ŀ��λ�õ�Gizmos
        Gizmos.color = Color.red;
        if (throwTarget1 != null) Gizmos.DrawWireSphere(throwTarget1.position, 0.3f);

        Gizmos.color = Color.green;
        if (throwTarget2 != null) Gizmos.DrawWireSphere(throwTarget2.position, 0.3f);

        Gizmos.color = Color.blue;
        if (throwTarget3 != null) Gizmos.DrawWireSphere(throwTarget3.position, 0.3f);

        // ����������
        Gizmos.color = Color.white;
        if (throwTarget1 != null && throwTarget2 != null)
            Gizmos.DrawLine(throwTarget1.position, throwTarget2.position);
        if (throwTarget2 != null && throwTarget3 != null)
            Gizmos.DrawLine(throwTarget2.position, throwTarget3.position);
        if (throwTarget3 != null && throwTarget1 != null)
            Gizmos.DrawLine(throwTarget3.position, throwTarget1.position);
    }
}