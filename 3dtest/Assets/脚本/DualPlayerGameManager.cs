using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualPlayerGameManager : MonoBehaviour
{
    [Header("玩家设置")]
    public GameObject player1;
    public GameObject player2;

    [Header("交互按键设置")]
    public KeyCode player1InteractKey = KeyCode.F;
    public KeyCode player2InteractKey = KeyCode.H;

    [Header("物品池")]
    public InteractableItem[] interactableItems;

    void Start()
    {
        SetupPlayers();
        Debug.Log("双人交互游戏初始化完成");
        Debug.Log($"玩家1交互键: {player1InteractKey}");
        Debug.Log($"玩家2交互键: {player2InteractKey}");
    }

    void SetupPlayers()
    {
        // 设置玩家1
        if (player1 != null)
        {
            PlayerController p1Controller = player1.GetComponent<PlayerController>();
            if (p1Controller == null)
            {
                p1Controller = player1.AddComponent<PlayerController>();
            }
            p1Controller.interactKey = player1InteractKey;
            p1Controller.playerName = "玩家1";
            p1Controller.playerColor = Color.blue;
        }

        // 设置玩家2
        if (player2 != null)
        {
            PlayerController p2Controller = player2.GetComponent<PlayerController>();
            if (p2Controller == null)
            {
                p2Controller = player2.AddComponent<PlayerController>();
            }
            p2Controller.interactKey = player2InteractKey;
            p2Controller.playerName = "玩家2";
            p2Controller.playerColor = Color.red;
        }
    }

    //重置所有物品
    public void ResetAllItems()
    {
        foreach (InteractableItem item in interactableItems)
        {
            if (item != null)
            {
                item.ResetItem();
            }
        }
        Debug.Log("所有物品已重置");
    }

    // 检查是否所有物品都被收集（用于通关条件）
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
