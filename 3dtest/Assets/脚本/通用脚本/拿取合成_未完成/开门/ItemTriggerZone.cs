using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTriggerZone : MonoBehaviour
{
    [Header("������������")]
    public string zoneName = "��������";
    public float triggerRadius = 3f;
    public Color gizmoColor = Color.cyan;

    [Header("������Ʒ")]
    public string requiredItemName = "Կ��"; // ��Ҫƥ�����Ʒ����
    public bool requireExactMatch = true;   // �Ƿ���Ҫ��ȷƥ������

    [Header("����Ч��")]
    public bool destroyItemOnTrigger = true; // �������Ƿ�������Ʒ
    public float triggerDelay = 0.5f;       // �����ӳ�

    [Header("�¼���Ӧ")]
    public UnityEngine.Events.UnityEvent onTriggerSuccess; // �����ɹ��¼�
    public UnityEngine.Events.UnityEvent onTriggerFail;    // ����ʧ���¼�

    [Header("״̬")]
    public bool isTriggered = false;
    public bool isPlayerInZone = false;

    private GameObject currentPlayerInZone;
    private List<InteractableItem> detectedItems = new List<InteractableItem>();

    void Update()
    {
        if (isTriggered) return; // ����Ѿ������������ټ��

        CheckForPlayers();

        if (isPlayerInZone && currentPlayerInZone != null)
        {
            CheckPlayerForRequiredItem(currentPlayerInZone);
        }
    }

    void CheckForPlayers()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, triggerRadius);
        bool playerFound = false;
        GameObject foundPlayer = null;

        foreach (var collider in hitColliders)
        {
            // ͨ����ǩ�����ң�����ͨ����ҿ��������
            if (collider.CompareTag("Player") || collider.GetComponent<PlayerController>() != null)
            {
                playerFound = true;
                foundPlayer = collider.gameObject;
                break;
            }
        }

        if (playerFound != isPlayerInZone)
        {
            isPlayerInZone = playerFound;
            currentPlayerInZone = foundPlayer;

            if (isPlayerInZone)
            {
                OnPlayerEnterZone(foundPlayer);
            }
            else
            {
                OnPlayerExitZone();
            }
        }
    }

    void CheckPlayerForRequiredItem(GameObject player)
    {
        // ��ȡ��ҳ��е���Ʒ
        InteractableItem heldItem = GetPlayerHeldItem(player);

        if (heldItem != null && IsItemMatch(heldItem))
        {
            OnRequiredItemDetected(player, heldItem);
        }
    }

    InteractableItem GetPlayerHeldItem(GameObject player)
    {
        // ����1: ͨ����ҿ�������ȡ
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null && playerController.IsHoldingItem())
        {
            return playerController.GetHeldItem();
        }

        // ����2: ͨ����Ʒ�ĳ�������Ϣ��ȡ
        InteractableItem[] allItems = FindObjectsOfType<InteractableItem>();
        foreach (InteractableItem item in allItems)
        {
            if (item.isBeingHeld && item.currentHolder == player)
            {
                return item;
            }
        }

        return null;
    }

    bool IsItemMatch(InteractableItem item)
    {
        if (requireExactMatch)
        {
            return item.itemName == requiredItemName;
        }
        else
        {
            return item.itemName.Contains(requiredItemName);
        }
    }

    void OnPlayerEnterZone(GameObject player)
    {
        Debug.Log($"��ҽ��� {zoneName}");

        // �����������Ƿ����������Ʒ
        CheckPlayerForRequiredItem(player);
    }

    void OnPlayerExitZone()
    {
        Debug.Log($"����뿪 {zoneName}");
        currentPlayerInZone = null;
        detectedItems.Clear();
    }

    void OnRequiredItemDetected(GameObject player, InteractableItem item)
    {
        if (isTriggered) return;

        Debug.Log($"��⵽������Ʒ: {item.itemName} �� {zoneName}");
        StartCoroutine(TriggerSequence(player, item));
    }

    System.Collections.IEnumerator TriggerSequence(GameObject player, InteractableItem item)
    {
        isTriggered = true;

        Debug.Log($"������ʼ: {zoneName}");

        // �����ӳ�
        yield return new WaitForSeconds(triggerDelay);

        // �����ɹ��¼�
        onTriggerSuccess?.Invoke();

        // ������Ʒ
        if (destroyItemOnTrigger)
        {
            DestroyItem(item);
        }
        else
        {
            // �����������Ʒ�����Խ������
            item.PutDown();
        }

        Debug.Log($"�������: {zoneName}");
    }

    void DestroyItem(InteractableItem item)
    {
        if (item != null)
        {
            Debug.Log($"������Ʒ: {item.itemName}");
            Destroy(item.gameObject);
        }
    }

    // �����������ֶ����������ڲ��Ի����������
    public void ManualTrigger()
    {
        if (!isTriggered)
        {
            onTriggerSuccess?.Invoke();
            isTriggered = true;
        }
    }

    // �������������ô���״̬
    public void ResetTrigger()
    {
        isTriggered = false;
        Debug.Log($"���ô�������: {zoneName}");
    }

    // ��Scene��ͼ����ʾ��������
    void OnDrawGizmos()
    {
        Gizmos.color = isTriggered ? Color.green : gizmoColor;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);

        // ��ʾ��������
        GUIStyle style = new GUIStyle();
        style.normal.textColor = gizmoColor;
#if UNITY_EDITOR
        string statusText = $"{zoneName}\n����: {requiredItemName}\n״̬: {(isTriggered ? "�Ѵ���" : "�ȴ���")}";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.3f, statusText, style);
#endif
    }
}
