using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    private InteractableItem heldItem;

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

    // 在PickUpItem方法中设置heldItem
    void PickUpItem(InteractableItem item)
    {
        heldItem = item;
        item.Interact(gameObject);
    }

    // 在DropItem方法中清空heldItem
    void DropItem()
    {
        if (heldItem != null)
        {
            heldItem.Interact(gameObject);
            heldItem = null;
        }
    }
}
