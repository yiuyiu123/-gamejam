using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("��������")]
    public KeyCode interactKey = KeyCode.F; // ��������
    public float interactionRange = 2f;     // ������Χ

    [Header("����ѡ��")]
    public bool showInputDebug = false;
    public bool showInteractionDebug = true;

    private Rigidbody rb;
    private Vector3 movement;
    private InteractableItem heldItem; // ��ǰ���е���Ʒ

    // �������
    public string playerName = "���";
    public Color playerColor = Color.white;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.freezeRotation = true;
        }

        Debug.Log($"{playerName} ��������ʼ����� - ������: {interactKey}");
    }

    void Update()
    {
        GetWASDInput();
        HandleInteraction();

        if (showInputDebug && movement.magnitude > 0.1f)
        {
            Debug.Log($"{playerName} �ƶ�����: {movement}");
        }
    }

    void GetWASDInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.D)) horizontal += 1f;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;

        movement = new Vector3(horizontal, 0f, vertical).normalized;
    }
    void HandleInteraction()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (heldItem != null)
            {
                // ����Ѿ�������Ʒ�������
                DropItem();
            }
            else
            {
                // �����Լ�����Ʒ
                TryPickUpItem();
            }
        }
    }

    void TryPickUpItem()
    {
        // ���ҷ�Χ�ڵĿɽ�����Ʒ
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRange);
        InteractableItem closestItem = null;
        float closestDistance = Mathf.Infinity;

        foreach (var hitCollider in hitColliders)
        {
            InteractableItem item = hitCollider.GetComponent<InteractableItem>();
            if (item != null && item.canBePickedUp && !item.isBeingHeld)
            {
                float distance = Vector3.Distance(transform.position, item.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = item;
                }
            }
        }

        if (closestItem != null)
        {
            PickUpItem(closestItem);
        }
        else if (showInteractionDebug)
        {
            Debug.Log($"{playerName} ����û�п�ʰȡ����Ʒ");
        }
    }

    void PickUpItem(InteractableItem item)
    {
        heldItem = item;
        item.Interact(gameObject);

        if (showInteractionDebug)
        {
            Debug.Log($"{playerName} ������ {item.itemName}");
        }
    }

    void DropItem()
    {
        if (heldItem != null)
        {
            heldItem.Interact(gameObject);

            if (showInteractionDebug)
            {
                Debug.Log($"{playerName} ������ {heldItem.itemName}");
            }

            heldItem = null;
        }
    }

    // ǿ�Ʒ�����Ʒ�����統�������ʱ��
    public void ForceDropItem()
    {
        if (heldItem != null)
        {
            DropItem();
        }
    }

    // ����Ƿ������Ʒ
    public bool IsHoldingItem()
    {
        return heldItem != null;
    }

    // ��ȡ��ǰ���е���Ʒ
    public InteractableItem GetHeldItem()
    {
        return heldItem;
    }

    // ��Scene��ͼ����ʾ������Χ
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    void OnGUI()
    {
        if (showInputDebug)
        {
            GUI.Label(new Rect(10, 30, 300, 20), $"{playerName} �ƶ�����: {movement}");
            GUI.Label(new Rect(10, 50, 300, 20), $"{playerName} �ٶ�: {rb.velocity}");
            GUI.Label(new Rect(10, 70, 300, 20), $"{playerName} ������Ʒ: {IsHoldingItem()}");

            if (IsHoldingItem())
            {
                GUI.Label(new Rect(10, 90, 300, 20), $"����: {heldItem.itemName}");
            }
        }
    }
}
