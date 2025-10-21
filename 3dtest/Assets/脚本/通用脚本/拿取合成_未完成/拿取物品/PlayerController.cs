using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("交互设置")]
    public KeyCode interactKey = KeyCode.F;
    public float interactionRange = 2f;
    public float throwRange = 10f;

    [Header("特殊物品设置")]
    public string wateringCanItemName = "水壶"; // 水壶的物品名称

    [Header("调试选项")]
    public bool showInputDebug = false;
    public bool showInteractionDebug = true;

    [Header("张奕忻")]    
    public string flashLight = "手电筒";
    public bool isHoldFlashLight = false;

    private Rigidbody rb;
    private Vector3 movement;
    private InteractableItem heldItem;
    private bool isTemporarilyLocked = false; // 临时锁定状态（用于浇花等特殊动作）

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
        if (isTemporarilyLocked) return; // 如果被临时锁定，不处理输入

        //GetWASDInput();
        HandleInteraction();

        if (showInputDebug && movement.magnitude > 0.1f)
        {
            Debug.Log($"{playerName} 移动输入: {movement}");
        }
    }

    //void GetWASDInput()
    //{
    //    float horizontal = 0f;
    //    float vertical = 0f;

    //    //if (Input.GetKey(KeyCode.D)) horizontal += 1f;
    //    //if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
    //    //if (Input.GetKey(KeyCode.W)) vertical += 1f;
    //    //if (Input.GetKey(KeyCode.S)) vertical -= 1f;

    //    movement = new Vector3(horizontal, 0f, vertical).normalized;
    //}

    void HandleInteraction()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (showInteractionDebug)
                Debug.Log($"{playerName} 按下交互键，持有物品: {heldItem?.itemName}");

            if (heldItem != null)
            {
                // 如果持有水壶，优先尝试浇花
                if (heldItem.itemName == wateringCanItemName && TryWateringFlowers())
                {
                    Debug.Log($"{playerName} 成功开始浇花");
                    return;
                }

                //张奕忻：如果持有手电筒，增加布尔值
                if(heldItem.itemName == flashLight)
                {
                    isHoldFlashLight = true;
                    Debug.Log($"{playerName} 拿到手电筒，准备开始scene3剧情2");
                }

                // 如果持有物品，尝试抛掷到合成区域
                if (TryThrowToSynthesisZone())
                {
                    Debug.Log($"{playerName} 成功抛掷物品到合成区域");
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

    // 尝试浇花 - 修复逻辑
    bool TryWateringFlowers()
    {
        // 检查是否持有水壶
        if (heldItem == null || heldItem.itemName != wateringCanItemName)
        {
            if (showInteractionDebug)
                Debug.Log($"{playerName} 没有持有水壶或水壶名称不匹配");
            return false;
        }

        // 查找附近的花盆区域
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRange);
        if (showInteractionDebug)
            Debug.Log($"{playerName} 检测到 {hitColliders.Length} 个碰撞体");

        foreach (var hitCollider in hitColliders)
        {
            FlowerPotZone flowerPot = hitCollider.GetComponent<FlowerPotZone>();
            if (flowerPot != null)
            {
                if (showInteractionDebug)
                    Debug.Log($"{playerName} 找到花盆区域: {flowerPot.zoneID}");

                // 检查玩家是否在花盆区域内
                if (flowerPot.IsPlayerInZone(gameObject))
                {
                    if (showInteractionDebug)
                        Debug.Log($"{playerName} 在花盆区域内，开始浇花");

                    // 开始浇花
                    bool wateringStarted = flowerPot.StartWatering(gameObject, heldItem.gameObject);
                    if (wateringStarted)
                    {
                        // 浇花成功后，立即释放水壶引用，因为花盆会处理水壶的销毁
                        heldItem = null;
                        return true;
                    }
                    else
                    {
                        Debug.Log($"{playerName} 花盆拒绝开始浇花");
                    }
                }
                else
                {
                    if (showInteractionDebug)
                        Debug.Log($"{playerName} 在花盆附近但不在区域内");
                }
            }
        }

        if (showInteractionDebug)
            Debug.Log($"{playerName} 没有找到可用的花盆区域");
        return false;
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

    // 新增：设置临时锁定状态
    public void SetTemporaryLock(bool locked)
    {
        isTemporarilyLocked = locked;
        Debug.Log($"{playerName} 临时锁定状态: {locked}");
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

    // 强制释放持有的物品（用于浇花等特殊操作后）
    public void ForceReleaseItem()
    {
        if (heldItem != null)
        {
            heldItem.ForceRelease();
            heldItem = null;
            Debug.Log($"{playerName} 强制释放了物品");
        }
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