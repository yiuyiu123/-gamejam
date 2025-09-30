using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    private InteractableItem heldItem;

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

    // ��PickUpItem����������heldItem
    void PickUpItem(InteractableItem item)
    {
        heldItem = item;
        item.Interact(gameObject);
    }

    // ��DropItem���������heldItem
    void DropItem()
    {
        if (heldItem != null)
        {
            heldItem.Interact(gameObject);
            heldItem = null;
        }
    }
}
