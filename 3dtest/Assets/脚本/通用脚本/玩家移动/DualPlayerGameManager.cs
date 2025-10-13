using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualPlayerGameManager : MonoBehaviour
{
    [Header("�������")]
    public GameObject player1;
    public GameObject player2;

    [Header("������������")]
    public KeyCode player1InteractKey = KeyCode.F;
    public KeyCode player2InteractKey = KeyCode.H;

    [Header("��Ʒ��")]
    public InteractableItem[] interactableItems;

    void Start()
    {
        SetupPlayers();
        Debug.Log("˫�˽�����Ϸ��ʼ�����");
        Debug.Log($"���1������: {player1InteractKey}");
        Debug.Log($"���2������: {player2InteractKey}");
    }

    void SetupPlayers()
    {
        // �������1
        if (player1 != null)
        {
            PlayerController p1Controller = player1.GetComponent<PlayerController>();
            if (p1Controller == null)
            {
                p1Controller = player1.AddComponent<PlayerController>();
            }
            p1Controller.interactKey = player1InteractKey;
            p1Controller.playerName = "���1";
            p1Controller.playerColor = Color.blue;
        }

        // �������2
        if (player2 != null)
        {
            PlayerController p2Controller = player2.GetComponent<PlayerController>();
            if (p2Controller == null)
            {
                p2Controller = player2.AddComponent<PlayerController>();
            }
            p2Controller.interactKey = player2InteractKey;
            p2Controller.playerName = "���2";
            p2Controller.playerColor = Color.red;
        }
    }

    //����������Ʒ
    public void ResetAllItems()
    {
        foreach (InteractableItem item in interactableItems)
        {
            if (item != null)
            {
                item.ResetItem();
            }
        }
        Debug.Log("������Ʒ������");
    }

    // ����Ƿ�������Ʒ�����ռ�������ͨ��������
    public bool AreAllItemsCollected()
    {
        foreach (InteractableItem item in interactableItems)
        {
            if (item != null && !item.isBeingHeld)
            {
                return false;
            }
        }
        return true;
    }
}
