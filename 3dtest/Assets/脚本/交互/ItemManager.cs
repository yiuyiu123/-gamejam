using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [Header("��Ʒ����")]
    public List<InteractableItem> allItems = new List<InteractableItem>();

    [Header("����ѡ��")]
    public bool showItemStatus = true;

    void Start()
    {
        // �Զ����ҳ����е�������Ʒ
        if (allItems.Count == 0)
        {
            InteractableItem[] foundItems = FindObjectsOfType<InteractableItem>();
            allItems.AddRange(foundItems);
        }

        Debug.Log($"��Ʒ��������ʼ����ɣ��ҵ� {allItems.Count} ����Ʒ");
    }

    void Update()
    {
        if (showItemStatus)
        {
            DisplayItemStatus();
        }
    }

    void DisplayItemStatus()
    {
        foreach (InteractableItem item in allItems)
        {
            if (item != null)
            {
                string status = $"��Ʒ: {item.itemName} | ";
                status += item.isBeingHeld ? "������" : "δ������";
                status += item.isExchangeLocked ? " | ��������" : " | �ɽ���";

                if (item.isExchangeLocked && !string.IsNullOrEmpty(item.lastExchangeZone))
                {
                    status += $" (���� {item.lastExchangeZone})";
                }

                // ���������ʾ��UI�ϻ����̨
                // Debug.Log(status);
            }
        }
    }

    // ����������Ʒ�Ľ�������
    public void ResetAllItemLocks()
    {
        foreach (InteractableItem item in allItems)
        {
            if (item != null)
            {
                item.ResetExchangeLock();
            }
        }

        Debug.Log("������Ʒ�Ľ�������������");
    }

    // �����ض�״̬����Ʒ
    public List<InteractableItem> FindItemsByStatus(bool isLocked, bool isHeld)
    {
        List<InteractableItem> result = new List<InteractableItem>();

        foreach (InteractableItem item in allItems)
        {
            if (item != null && item.isExchangeLocked == isLocked && item.isBeingHeld == isHeld)
            {
                result.Add(item);
            }
        }

        return result;
    }
}
