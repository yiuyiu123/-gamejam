using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("交互设置")]
    public KeyCode interactKey = KeyCode.F;
    public float interactionRange = 2f;
    public float throwRange = 10f;

    [Header("调试选项")]
    public bool showInputDebug = false;
    public bool showInteractionDebug = true;

    private Rigidbody rb;
    private Vector3 movement;
    private InteractableItem heldItem;

    // 玩家属性
    public string playerName = "玩家";
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
            Debug.Log($"{playerName} 移动输入: {movement}");
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
                // 如果持有物品，尝试抛掷到合成区域
                if (TryThrowToSynthesisZone())
                {
                    // 成功抛掷
                    return;
                }
                else
                {
                    // 否则放下物品
                    DropItem();
                }
            }
            else
            {
                // 尝试捡起物品
                TryPickUpItem();
            }
        }
    }

    bool TryThrowToSynthesisZone()
    {
        // 查找前方的合成区域 - 同时检测 SynthesisZone 和 TutorialSynthesisZone
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, throwRange))
        {
            // 先尝试普通合成区域
            SynthesisZone zone = hit.collider.GetComponent<SynthesisZone>();
            if (zone != null && heldItem != null)
            {
                ThrowToZone(zone, heldItem);
                return true;
            }

            // 再尝试教学合成区域
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
        // 从玩家手中移除物品引用
        InteractableItem itemToThrow = heldItem;
        heldItem = null;

        // 重要：在抛掷前强制清除物品的持有状态
        itemToThrow.ForceRelease();

        // 调用区域的抛掷方法
        zone.ThrowItemToZone(itemToThrow);

        Debug.Log($"{playerName} 向合成区域抛掷 {itemToThrow.itemName}");
    }

    void ThrowToTutorialZone(TutorialSynthesisZone zone, InteractableItem item)
    {
        // 从玩家手中移除物品引用
        InteractableItem itemToThrow = heldItem;
        heldItem = null;

        // 重要：在抛掷前强制清除物品的持有状态
        itemToThrow.ForceRelease();

        // 调用区域的抛掷方法
        zone.ThrowItemToZone(itemToThrow);

        Debug.Log($"{playerName} 向教学区域抛掷 {itemToThrow.itemName}");
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
            Debug.Log($"{playerName} 附近没有可拾取的物品");
        }
    }

    void PickUpItem(InteractableItem item)
    {
        heldItem = item;
        item.Interact(gameObject);

        if (showInteractionDebug)
        {
            Debug.Log($"{playerName} 捡起了 {item.itemName}");
        }
    }

    void DropItem()
    {
        if (heldItem != null)
        {
            heldItem.Interact(gameObject);

            if (showInteractionDebug)
            {
                Debug.Log($"{playerName} 放下了 {heldItem.itemName}");
            }

            heldItem = null;
        }
    }

    // 检查是否持有物品
    public bool IsHoldingItem()
    {
        return heldItem != null;
    }

    // 获取当前持有的物品
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