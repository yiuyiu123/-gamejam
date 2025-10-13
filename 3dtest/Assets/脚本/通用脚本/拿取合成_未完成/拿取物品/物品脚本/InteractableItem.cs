using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    //[Header("物品设置")]
    //public string itemName = "物品";
    //public bool canBePickedUp = true;
    //public Vector3 holdOffset = new Vector3(0, 1, 1);

    //[Header("传送设置")]
    //public bool canBeExchanged = true;
    //public bool isExchangeLocked = false;
    //public string lastExchangeZone = "";

    //[Header("状态")]
    //public bool isBeingHeld = false;
    //public bool isInExchangeProcess = false;
    //public GameObject currentHolder = null;

    //private Rigidbody rb;
    //private Collider itemCollider;
    //private Vector3 originalPosition;
    //private Quaternion originalRotation;
    //private Vector3 originalScale;
    //private ExchangeZone currentZone;

    //void Start()
    //{
    //    rb = GetComponent<Rigidbody>();
    //    itemCollider = GetComponent<Collider>();
    //    originalPosition = transform.position;
    //    originalRotation = transform.rotation;
    //    originalScale = transform.localScale;
    //}

    //void Update()
    //{
    //    if (isBeingHeld && currentHolder != null && !isInExchangeProcess)
    //    {
    //        FollowHolder();
    //    }

    //    CheckIfLeftZone();
    //}

    //void LateUpdate()
    //{
    //    // 在每帧的最后强制保持缩放，确保覆盖任何其他可能的缩放修改
    //    if (isBeingHeld)
    //    {
    //        transform.localScale = originalScale;
    //    }
    //}

    //public void Interact(GameObject player)
    //{
    //    if (!canBePickedUp || isInExchangeProcess) return;

    //    if (!isBeingHeld)
    //    {
    //        PickUp(player);
    //    }
    //    else
    //    {
    //        PutDown();
    //    }
    //}

    //void PickUp(GameObject player)
    //{
    //    isBeingHeld = true;
    //    currentHolder = player;

    //    // 在设置任何东西之前先保存和锁定缩放
    //    originalScale = transform.localScale;

    //    ResetExchangeLock();

    //    // 禁用物理效果
    //    if (rb != null)
    //    {
    //        rb.isKinematic = true;
    //        rb.useGravity = false;
    //    }

    //    // 禁用碰撞体
    //    if (itemCollider != null)
    //    {
    //        itemCollider.enabled = false;
    //    }

    //    // 关键修改：完全不设置父物体，避免任何缩放继承
    //    // transform.SetParent(player.transform, false); // 注释掉这行

    //    // 立即强制设置缩放
    //    transform.localScale = originalScale;

    //    Debug.Log($"{player.name} 拿起了 {itemName}");
    //}

    //public void PutDown()
    //{
    //    isBeingHeld = false;

    //    // 恢复物理效果
    //    if (rb != null)
    //    {
    //        rb.isKinematic = false;
    //        rb.useGravity = true;
    //    }

    //    // 恢复碰撞体
    //    if (itemCollider != null)
    //    {
    //        itemCollider.enabled = true;
    //    }

    //    // 不需要解除父物体，因为根本没有设置
    //    // transform.SetParent(null);

    //    // 最终确认缩放
    //    transform.localScale = originalScale;

    //    Debug.Log($"{currentHolder.name} 放下了 {itemName}");
    //    currentHolder = null;
    //}

    //void FollowHolder()
    //{
    //    if (currentHolder == null) return;

    //    Vector3 targetPosition = currentHolder.transform.position +
    //                            currentHolder.transform.forward * holdOffset.z +
    //                            currentHolder.transform.up * holdOffset.y +
    //                            currentHolder.transform.right * holdOffset.x;

    //    Quaternion targetRotation = currentHolder.transform.rotation;

    //    // 直接更新位置和旋转，不依赖父物体
    //    transform.position = Vector3.Lerp(transform.position, targetPosition, 10f * Time.deltaTime);
    //    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);

    //    // 在跟随过程中强制保持缩放
    //    transform.localScale = originalScale;
    //}

    //// 检查是否离开了交换区域
    //void CheckIfLeftZone()
    //{
    //    if (currentZone != null && !isBeingHeld)
    //    {
    //        float distance = Vector3.Distance(transform.position, currentZone.transform.position);
    //        if (distance > currentZone.detectionRadius * 1.2f)
    //        {
    //            OnLeftZone();
    //        }
    //    }
    //}

    //// 当物品离开区域时调用
    //void OnLeftZone()
    //{
    //    if (currentZone != null)
    //    {
    //        Debug.Log($"物品 {itemName} 离开了区域 {currentZone.zoneID}");
    //        currentZone.OnItemLeft(this.gameObject);
    //        currentZone = null;
    //        ResetExchangeLock();
    //    }
    //}

    //// 标记物品已被交换
    //public void MarkAsExchanged(string fromZoneID)
    //{
    //    isExchangeLocked = true;
    //    lastExchangeZone = fromZoneID;
    //    Debug.Log($"物品 {itemName} 被标记为已交换，来自区域 {fromZoneID}");
    //}

    //// 重置交换锁定
    //public void ResetExchangeLock()
    //{
    //    if (isExchangeLocked)
    //    {
    //        isExchangeLocked = false;
    //        lastExchangeZone = "";
    //        Debug.Log($"物品 {itemName} 的交换锁定已重置");
    //    }
    //}

    //// 检查是否可以交换到指定区域
    //public bool CanExchangeTo(string targetZoneID)
    //{
    //    if (!canBeExchanged || isExchangeLocked || isBeingHeld || isInExchangeProcess)
    //        return false;

    //    if (lastExchangeZone == targetZoneID)
    //        return false;

    //    return true;
    //}

    //// 设置当前所在的区域
    //public void SetCurrentZone(ExchangeZone zone)
    //{
    //    currentZone = zone;
    //}

    //// 重置物品到原始位置
    //public void ResetItem()
    //{
    //    PutDown();
    //    transform.position = originalPosition;
    //    transform.rotation = originalRotation;
    //    transform.localScale = originalScale;
    //    isInExchangeProcess = false;
    //    ResetExchangeLock();
    //}

    //// 重置物理状态
    //public void ResetPhysics()
    //{
    //    if (rb != null)
    //    {
    //        rb.velocity = Vector3.zero;
    //        rb.angularVelocity = Vector3.zero;
    //    }
    //}

    //void OnDrawGizmosSelected()
    //{
    //    if (isBeingHeld && currentHolder != null)
    //    {
    //        Gizmos.color = isExchangeLocked ? Color.red : (isInExchangeProcess ? Color.yellow : Color.green);
    //        Vector3 holdPosition = currentHolder.transform.position +
    //                              currentHolder.transform.forward * holdOffset.z +
    //                              currentHolder.transform.up * holdOffset.y +
    //                              currentHolder.transform.right * holdOffset.x;
    //        Gizmos.DrawWireSphere(holdPosition, 0.2f);
    //    }

    //    if (isExchangeLocked)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawWireCube(transform.position, Vector3.one * 1.2f);
    //    }
    //}
    [Header("物品设置")]
    public string itemName = "物品";
    public bool canBePickedUp = true;
    public Vector3 holdOffset = new Vector3(0, 1, 1);

    [Header("传送设置")]
    public bool canBeExchanged = true;
    public bool isExchangeLocked = false;
    public string lastExchangeZone = "";

    [Header("状态")]
    public bool isBeingHeld = false;
    public bool isInExchangeProcess = false;
    public GameObject currentHolder = null;

    private Rigidbody rb;
    private Collider itemCollider;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private ExchangeZone currentZone;


    // 添加公共属性来访问私有字段
    public Rigidbody Rb => rb;
    public Collider ItemCollider => itemCollider;
    public Vector3 OriginalPosition => originalPosition;
    public Quaternion OriginalRotation => originalRotation;
    public Vector3 OriginalScale => originalScale;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (isBeingHeld && currentHolder != null && !isInExchangeProcess)
        {
            FollowHolder();
        }

        CheckIfLeftZone();
    }

    void LateUpdate()
    {
        // 在每帧的最后强制保持缩放，确保覆盖任何其他可能的缩放修改
        if (isBeingHeld)
        {
            transform.localScale = originalScale;
        }
    }

    // 在 Interact 方法中添加检查
    public void Interact(GameObject player)
    {
        if (!canBePickedUp || isInExchangeProcess) return; // 添加 canBePickedUp 检查

        if (!isBeingHeld)
        {
            PickUp(player);
        }
        else
        {
            PutDown();
        }
    }

    void PickUp(GameObject player)
    {
        if (!canBePickedUp) return; // 额外检查

        // 设置持有状态 - 这是缺失的关键代码！
        isBeingHeld = true;
        currentHolder = player;

        // 在设置任何东西之前先保存和锁定缩放
        originalScale = transform.localScale;

        ResetExchangeLock();

        // 禁用物理效果
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 禁用碰撞体
        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }

        // 关键修改：完全不设置父物体，避免任何缩放继承
        // transform.SetParent(player.transform, false); // 注释掉这行

        // 立即强制设置缩放
        transform.localScale = originalScale;

        Debug.Log($"{player.name} 拿起了 {itemName}");
    }
    public void ResetItemState()
    {
        isBeingHeld = false;
        isInExchangeProcess = false;
        canBePickedUp = true;
        currentHolder = null;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }
    }
    public void PutDown()
    {
        if (!isBeingHeld) return; // 如果已经不是持有状态，直接返回

        isBeingHeld = false;

        // 恢复物理效果
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // 恢复碰撞体
        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }

        // 最终确认缩放
        transform.localScale = originalScale;

        Debug.Log($"{currentHolder?.name} 放下了 {itemName}");
        currentHolder = null;
    }

    void FollowHolder()
    {
        if (currentHolder == null)
        {
            Debug.LogWarning($"物品 {itemName} 的 currentHolder 为 null，无法跟随");
            return;
        }

        Vector3 targetPosition = currentHolder.transform.position +
                                currentHolder.transform.forward * holdOffset.z +
                                currentHolder.transform.up * holdOffset.y +
                                currentHolder.transform.right * holdOffset.x;

        Quaternion targetRotation = currentHolder.transform.rotation;

        // 直接更新位置和旋转，不依赖父物体
        transform.position = Vector3.Lerp(transform.position, targetPosition, 10f * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);

        // 在跟随过程中强制保持缩放
        transform.localScale = originalScale;

        // 调试信息
        if (showDebugInfo)
        {
            Debug.Log($"物品 {itemName} 跟随玩家: 位置={transform.position}, 目标位置={targetPosition}");
        }
    }

    [Header("调试选项")]
    public bool showDebugInfo = false;

    // 检查是否离开了交换区域
    void CheckIfLeftZone()
    {
        if (currentZone != null && !isBeingHeld)
        {
            float distance = Vector3.Distance(transform.position, currentZone.transform.position);
            if (distance > currentZone.detectionRadius * 1.2f)
            {
                OnLeftZone();
            }
        }
    }

    // 当物品离开区域时调用
    void OnLeftZone()
    {
        if (currentZone != null)
        {
            Debug.Log($"物品 {itemName} 离开了区域 {currentZone.zoneID}");
            currentZone.OnItemLeft(this.gameObject);
            currentZone = null;
            ResetExchangeLock();
        }
    }

    // 标记物品已被交换
    public void MarkAsExchanged(string fromZoneID)
    {
        isExchangeLocked = true;
        lastExchangeZone = fromZoneID;
        Debug.Log($"物品 {itemName} 被标记为已交换，来自区域 {fromZoneID}");
    }

    // 重置交换锁定
    public void ResetExchangeLock()
    {
        if (isExchangeLocked)
        {
            isExchangeLocked = false;
            lastExchangeZone = "";
            Debug.Log($"物品 {itemName} 的交换锁定已重置");
        }
    }

    // 检查是否可以交换到指定区域
    public bool CanExchangeTo(string targetZoneID)
    {
        if (!canBeExchanged || isExchangeLocked || isBeingHeld || isInExchangeProcess)
            return false;

        if (lastExchangeZone == targetZoneID)
            return false;

        return true;
    }

    // 设置当前所在的区域
    public void SetCurrentZone(ExchangeZone zone)
    {
        currentZone = zone;
    }

    // 重置物品到原始位置
    public void ResetItem()
    {
        PutDown();
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;
        isInExchangeProcess = false;
        ResetExchangeLock();
    }

    // 重置物理状态
    public void ResetPhysics()
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    // 添加强制释放方法
    public void ForceRelease()
    {
        if (isBeingHeld)
        {
            isBeingHeld = false;
            currentHolder = null;

            // 恢复物理效果
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            // 恢复碰撞体
            if (itemCollider != null)
            {
                itemCollider.enabled = true;
            }

            Debug.Log($"强制释放物品: {itemName}");
        }
    }
    void OnDrawGizmosSelected()
    {
        if (isBeingHeld && currentHolder != null)
        {
            Gizmos.color = isExchangeLocked ? Color.red : (isInExchangeProcess ? Color.yellow : Color.green);
            Vector3 holdPosition = currentHolder.transform.position +
                                  currentHolder.transform.forward * holdOffset.z +
                                  currentHolder.transform.up * holdOffset.y +
                                  currentHolder.transform.right * holdOffset.x;
            Gizmos.DrawWireSphere(holdPosition, 0.2f);
        }

        if (isExchangeLocked)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.2f);
        }
    }
}
