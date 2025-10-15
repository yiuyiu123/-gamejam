using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialSynthesisZone : MonoBehaviour
{
    [Header("��ѧ�ؿ�����")]
    public string zoneID = "TutorialSynthesisZone";
    public Transform throwTarget;
    public float detectionRadius = 3f;

    [Header("��������")]
    public float throwHeight = 3f;
    public float throwDuration = 0.8f;

    [Header("������ת����")]
    public string targetSceneName = "NextScene";
    public float sceneTransitionDelay = 2f;

    [Header("���Ч��")]
    public ParticleSystem completionEffect;
    public AudioClip completionSound;
    public Light completionLight;

    [Header("��ѧ�ؿ��ض���Ʒ")]
    public List<string> requiredItemNames = new List<string>();

    [Header("����ѡ��")]
    public bool showDebugGUI = false;

    // �¼�����������Ʒ״̬�ı�ʱ����
    public System.Action<bool> OnItemStateChanged;

    private List<InteractableItem> itemsInZone = new List<InteractableItem>();
    private AudioSource audioSource;
    private bool isTutorialCompleted = false;
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

        Debug.Log($"��ѧ�ؿ��ϳ����� {zoneID} �ѳ�ʼ��");
        Debug.Log($"Ŀ�곡��: {targetSceneName}");
        Debug.Log($"��Ҫ��Ʒ: {string.Join(", ", requiredItemNames)}");
    }

    void EnsureColliderSize()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(5, 3, 5);
            Debug.Log($"Ϊ��ѧ���� {zoneID} ����˴�����ײ��");
        }
        else if (!collider.isTrigger)
        {
            collider.isTrigger = true;
            Debug.Log($"�ѽ���ѧ���� {zoneID} ����ײ������Ϊ������");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTutorialCompleted) return;

        InteractableItem item = other.GetComponent<InteractableItem>();
        if (item != null)
        {
            if (ShouldIgnoreItem(item))
            {
                Debug.Log($"������Ʒ {item.itemName} - ������: {item.isBeingHeld}, ������: {item.isInExchangeProcess}");
                return;
            }

            if (!itemsInZone.Contains(item))
            {
                itemsInZone.Add(item);
                itemEnterTimes[item] = Time.time;

                Debug.Log($"��Ʒ {item.itemName} �����ѧ���� {zoneID}����ǰ������Ʒ��: {itemsInZone.Count}");
                DebugItemsInZone();

                // ��������Ƿ���������
                CheckAndUpdateZoneState();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (isTutorialCompleted) return;

        InteractableItem item = other.GetComponent<InteractableItem>();
        if (item != null && itemsInZone.Contains(item))
        {
            itemsInZone.Remove(item);
            itemEnterTimes.Remove(item);

            Debug.Log($"��Ʒ {item.itemName} �뿪��ѧ���� {zoneID}��ʣ����Ʒ��: {itemsInZone.Count}");
            DebugItemsInZone();

            // ���״̬������
            CheckAndUpdateZoneState();
        }
    }

    bool ShouldIgnoreItem(InteractableItem item)
    {
        if (item == null) return true;
        if (item.isBeingHeld) return true;
        if (item.isInExchangeProcess) return true;

        return false;
    }

    void DebugItemsInZone()
    {
        string debugInfo = $"��ѧ���� {zoneID} ��Ʒ�б� ({itemsInZone.Count} ��): ";
        foreach (var item in itemsInZone)
        {
            if (item != null)
            {
                debugInfo += $"{item.itemName} ";
            }
        }
        Debug.Log(debugInfo);
    }

    void CheckAndUpdateZoneState()
    {
        bool hasRequiredItem = HasRequiredItem();

        // �����¼�
        OnItemStateChanged?.Invoke(hasRequiredItem);
    }

    bool HasRequiredItem()
    {
        List<InteractableItem> validItems = new List<InteractableItem>();
        foreach (var item in itemsInZone)
        {
            if (item != null && !item.isBeingHeld && !item.isInExchangeProcess)
            {
                validItems.Add(item);
            }
        }

        if (requiredItemNames.Count == 0)
        {
            return validItems.Count >= 1; // ������һ����Ʒ
        }

        List<string> currentItemNames = new List<string>();
        foreach (var item in validItems)
        {
            if (item != null)
            {
                currentItemNames.Add(item.itemName);
            }
        }

        // ����Ƿ�������б�����Ʒ
        foreach (string requiredName in requiredItemNames)
        {
            if (!currentItemNames.Contains(requiredName))
            {
                return false;
            }
        }

        return true;
    }

    // ������Ʒ������ - ��ȫ���� SynthesisZone ��ʵ��
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

        // ʹ���� SynthesisZone ��ȫ��ͬ����������
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

        // ���״̬������
        CheckAndUpdateZoneState();
    }

    // ���Է���
    [ContextMenu("��ʾ����״̬")]
    public void ShowZoneStatus()
    {
        Debug.Log($"=== ��ѧ���� {zoneID} ״̬ ===");
        Debug.Log($"��Ʒ����: {itemsInZone.Count}");
        Debug.Log($"��ѧ�ؿ����: {isTutorialCompleted}");
        Debug.Log($"��Ҫ��Ʒ: {string.Join(", ", requiredItemNames)}");
        Debug.Log($"�Ƿ���������: {HasRequiredItem()}");

        foreach (var item in itemsInZone)
        {
            if (item != null)
            {
                Debug.Log($"- {item.itemName} (����: {item.isBeingHeld}, ����: {item.isInExchangeProcess})");
            }
        }
    }

    [ContextMenu("������������")]
    public void TestThrowAnimation()
    {
        // ���Ҹ�������Ʒ���в���
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider collider in colliders)
        {
            InteractableItem item = collider.GetComponent<InteractableItem>();
            if (item != null && !item.isBeingHeld)
            {
                StartCoroutine(ThrowItemCoroutine(item));
                break;
            }
        }
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
}