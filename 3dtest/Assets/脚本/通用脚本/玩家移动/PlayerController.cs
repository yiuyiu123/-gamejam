using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("交互设置")]
    public KeyCode interactKey = KeyCode.F; // 交互按键
    public float interactionRange = 2f;     // 交互范围

    [Header("调试选项")]
    public bool showInputDebug = false;
    public bool showInteractionDebug = true;

    private Rigidbody rb;
    private Vector3 movement;
    private InteractableItem heldItem; // 当前持有的物品

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

        Debug.Log($"{playerName} 控制器初始化完成 - 交互键: {interactKey}");
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
                // 如果已经持有物品，则放下
                DropItem();
            }
            else
            {
                // 否则尝试捡起物品
                TryPickUpItem();
            }
        }
    }

    void TryPickUpItem()
    {
        // 查找范围内的可交互物品
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

    // 强制放下物品（例如当玩家死亡时）
    public void ForceDropItem()
    {
        if (heldItem != null)
        {
            DropItem();
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

    // 在Scene视图中显示交互范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    void OnGUI()
    {
        if (showInputDebug)
        {
            GUI.Label(new Rect(10, 30, 300, 20), $"{playerName} 移动输入: {movement}");
            GUI.Label(new Rect(10, 50, 300, 20), $"{playerName} 速度: {rb.velocity}");
            GUI.Label(new Rect(10, 70, 300, 20), $"{playerName} 持有物品: {IsHoldingItem()}");

            if (IsHoldingItem())
            {
                GUI.Label(new Rect(10, 90, 300, 20), $"持有: {heldItem.itemName}");
            }
        }
    }
}
