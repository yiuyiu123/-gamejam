using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("��������")]
    public KeyCode interactKey = KeyCode.F;
    public float interactionRange = 2f;
    public float throwRange = 10f;

    [Header("����ѡ��")]
    public bool showInputDebug = false;
    public bool showInteractionDebug = true;

    private Rigidbody rb;
    private Vector3 movement;
    private InteractableItem heldItem;

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
                // ���������Ʒ�������������ϳ�����
                if (TryThrowToSynthesisZone())
                {
                    // �ɹ�����
                    return;
                }
                else
                {
                    // ���������Ʒ
                    DropItem();
                }
            }
            else
            {
                // ���Լ�����Ʒ
                TryPickUpItem();
            }
        }
    }

    bool TryThrowToSynthesisZone()
    {
        // ����ǰ���ĺϳ����� - ͬʱ��� SynthesisZone �� TutorialSynthesisZone
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, throwRange))
        {
            // �ȳ�����ͨ�ϳ�����
            SynthesisZone zone = hit.collider.GetComponent<SynthesisZone>();
            if (zone != null && heldItem != null)
            {
                ThrowToZone(zone, heldItem);
                return true;
            }

            // �ٳ��Խ�ѧ�ϳ�����
            TutorialSynthesisZone tutorialZone = hit.collider.GetComponent<TutorialSynthesisZone>();
            if (tutorialZone != null && heldItem != null)
            {
                ThrowToTutorialZone(tutorialZone, heldItem);
                return true;
            }
        }

        return false;
    }

    void ThrowToZone(SynthesisZone zone, InteractableItem item)
    {
        // ����������Ƴ���Ʒ����
        InteractableItem itemToThrow = heldItem;
        heldItem = null;

        // ��Ҫ��������ǰǿ�������Ʒ�ĳ���״̬
        itemToThrow.ForceRelease();

        // �����������������
        zone.ThrowItemToZone(itemToThrow);

        Debug.Log($"{playerName} ��ϳ��������� {itemToThrow.itemName}");
    }

    void ThrowToTutorialZone(TutorialSynthesisZone zone, InteractableItem item)
    {
        // ����������Ƴ���Ʒ����
        InteractableItem itemToThrow = heldItem;
        heldItem = null;

        // ��Ҫ��������ǰǿ�������Ʒ�ĳ���״̬
        itemToThrow.ForceRelease();

        // �����������������
        zone.ThrowItemToZone(itemToThrow);

        Debug.Log($"{playerName} ���ѧ�������� {itemToThrow.itemName}");
    }

    void TryPickUpItem()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRange);
        InteractableItem closestItem = null;
        float closestDistance = Mathf.Infinity;

        foreach (var hitCollider in hitColliders)
        {
            InteractableItem item = hitCollider.GetComponent<InteractableItem>();
            if (item != null && item.canBePickedUp && !item.isBeingHeld && !item.isInExchangeProcess)
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        Gizmos.color = Color.blue;
        Vector3 throwEnd = transform.position + transform.forward * throwRange;
        Gizmos.DrawLine(transform.position, throwEnd);
        Gizmos.DrawWireSphere(throwEnd, 0.3f);
    }
}