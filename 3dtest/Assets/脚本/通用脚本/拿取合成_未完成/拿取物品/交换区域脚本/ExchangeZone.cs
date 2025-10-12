using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExchangeZone : MonoBehaviour
{
    [Header("������������")]
    public string zoneID = "Zone1";
    public ExchangeZone targetZone;
    public float detectionRadius = 2f;
    public Color gizmoColor = Color.green;

    [Header("��������")]
    public bool enableAutoExchange = true;

    [Header("״̬")]
    public bool hasItem = false;
    public GameObject currentItem = null;

    private void Update()
    {
        if (enableAutoExchange)
        {
            CheckForItems();
        }
    }

    void CheckForItems()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        bool itemFound = false;
        GameObject foundItem = null;

        foreach (var collider in hitColliders)
        {
            InteractableItem item = collider.GetComponent<InteractableItem>();
            if (item != null && item.CanExchangeTo(targetZone.zoneID))
            {
                itemFound = true;
                foundItem = item.gameObject;
                break;
            }
        }

        // ���״̬�����仯
        if (itemFound != hasItem)
        {
            hasItem = itemFound;
            currentItem = foundItem;

            if (hasItem && foundItem != null)
            {
                // ������Ʒ�ĵ�ǰ����
                InteractableItem itemComponent = foundItem.GetComponent<InteractableItem>();
                if (itemComponent != null)
                {
                    itemComponent.SetCurrentZone(this);
                }

                OnItemPlaced(foundItem);
            }
            else
            {
                OnItemRemoved();
            }
        }
    }

    void OnItemPlaced(GameObject item)
    {
        if (!enableAutoExchange) return;

        Debug.Log($"��Ʒ {item.name} �����õ��������� {zoneID}");

        // ���������߼�
        if (targetZone != null)
        {
            StartCoroutine(ExchangeItem(item));
        }
        else
        {
            Debug.LogWarning($"�������� {zoneID} û������Ŀ������");
        }
    }

    void OnItemRemoved()
    {
        Debug.Log($"��Ʒ�ӽ������� {zoneID} �Ƴ�");
        currentItem = null;
    }

    // ����Ʒ�뿪����ʱ���ã�����Ʒ�ű����ã�
    public void OnItemLeft(GameObject item)
    {
        if (currentItem == item)
        {
            hasItem = false;
            currentItem = null;
        }
    }

    IEnumerator ExchangeItem(GameObject item)
    {
        InteractableItem itemComponent = item.GetComponent<InteractableItem>();
        if (itemComponent == null) yield break;

        // �����Ʒ���ڽ���������
        itemComponent.isInExchangeProcess = true;

        Debug.Log($"��ʼ������Ʒ: {item.name} �� {zoneID} �� {targetZone.zoneID}");

        // �����Ʒ�ѱ���������ֹ�ظ�������
        itemComponent.MarkAsExchanged(zoneID);

        // �ȴ�����ʱ�䣬����ҿ�����Ʒ������
        yield return new WaitForSeconds(0.5f);

        // ������Ʒ��Ŀ������
        Vector3 targetPosition = targetZone.transform.position;
        item.transform.position = targetPosition;

        // ������Ʒ������״̬
        itemComponent.ResetPhysics();

        Debug.Log($"��Ʒ {item.name} �Ѵ��͵����� {targetZone.zoneID}");

        // ֪ͨĿ����������Ʒ����
        targetZone.OnItemArrived(item);

        // ���õ�ǰ����״̬
        hasItem = false;
        currentItem = null;

        // �ȴ���ȡ����Ʒ����״̬���
        yield return new WaitForSeconds(0.5f);
        itemComponent.isInExchangeProcess = false;
    }

    // ����Ʒ�����������͹���ʱ����
    public void OnItemArrived(GameObject item)
    {
        // ���õ�ǰ��Ʒ
        currentItem = item;
        hasItem = true;

        // ������Ʒ�ĵ�ǰ����
        InteractableItem itemComponent = item.GetComponent<InteractableItem>();
        if (itemComponent != null)
        {
            itemComponent.SetCurrentZone(this);
        }

        Debug.Log($"��Ʒ {item.name} �������� {zoneID}");
    }

    // ��Scene��ͼ����ʾ����Χ
    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // ��ʾ��Ŀ�����������
        if (targetZone != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetZone.transform.position);
        }

        // ��ʾ�����ʶ
        GUIStyle style = new GUIStyle();
        style.normal.textColor = gizmoColor;
#if UNITY_EDITOR
        string statusText = $"{zoneID}\n{(hasItem ? "����Ʒ" : "��")}";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, statusText, style);
#endif
    }
}
