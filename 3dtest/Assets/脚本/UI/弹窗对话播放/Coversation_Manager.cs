using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Conversation_Manager : MonoBehaviour
{
    [Header("玩家1对话")]
    public GameObject[] player1Conversations;
    [Header("玩家2对话")]
    public GameObject[] player2Conversations;

    [Header("引用的UI管理器，用于结束时隐藏UI")]
    public scene1UI_Manager scene1UI_Manager;

    [Header("输入按键设置")]
    public KeyCode player1Key = KeyCode.F;
    public KeyCode player2Key = KeyCode.H;

    private int player1Index = 0;
    private int player2Index = 0;

    public bool player1Finished = false;
    public bool player2Finished = false;

    public void StartConversation()
    {
        // 初始化：隐藏所有对话图片
        InitConversation(player1Conversations);
        InitConversation(player2Conversations);

        // 显示各自的第一张
        ShowConversation(player1Conversations, 0);
        ShowConversation(player2Conversations, 0);
    }

    void Update()
    {
        // 玩家1按F切换
        if (!player1Finished && Input.GetKeyDown(player1Key))
        {
            HandleNext(player: 1);
        }

        // 玩家2按H切换
        if (!player2Finished && Input.GetKeyDown(player2Key))
        {
            HandleNext(player: 2);
        }
    }

    // 统一的对话初始化
    private void InitConversation(GameObject[] convArray)
    {
        foreach (var img in convArray)
        {
            if (img != null)
                img.SetActive(false);
        }
    }

    // 显示指定数组的指定下标
    private void ShowConversation(GameObject[] convArray, int index)
    {
        var ui = FindObjectOfType<scene1UI_Manager>();
        
        if (convArray == null || convArray.Length == 0) 
            return;
        if (index >= 0 && index < convArray.Length)
            convArray[index].SetActive(true);
    }

    // 隐藏指定数组的所有
    private void HideAll(GameObject[] convArray)
    {
        foreach (var img in convArray)
        {
            if (img != null)
                img.SetActive(false);
        }
    }

    // 玩家点击下一步逻辑
    public void HandleNext(int player)
    {
        GameObject[] convArray = player == 1 ? player1Conversations : player2Conversations;
        int index = player == 1 ? player1Index : player2Index;

        // 隐藏前一张（非最后一张）
        if (index < convArray.Length)
            HideConversation(convArray, index);

        // 递增索引
        if (player == 1) player1Index++;
        else player2Index++;

        if ((player == 1 && player1Index < player1Conversations.Length) ||
            (player == 2 && player2Index < player2Conversations.Length))
        {
            // 显示当前对话
            int newIndex = player == 1 ? player1Index : player2Index;
            GameObject go = convArray[newIndex];
            go.SetActive(true);

            // 获取子物体 TMP 并播放文字
            TMPTypewriter tw = go.GetComponentInChildren<TMPTypewriter>();
            TMP_Text tmp = go.GetComponentInChildren<TMP_Text>();
            if (tw != null && tmp != null)
            {
                tw.StartTyping(tmp.text); // 自动播放当前文字
            }
        }
        else
        {
            // 播放完毕
            if (player == 1) player1Finished = true;
            else player2Finished = true;
            CheckBothFinished(player);
        }
    }


    // 隐藏单个对话图片（不影响最后一张）
    private void HideConversation(GameObject[] convArray, int index)
    {
        if (convArray == null || index < 0 || index >= convArray.Length) return;

        // 如果是最后一张就不隐藏
        if (index < convArray.Length - 1)
            convArray[index].SetActive(false);
    }


    // 检查双方是否结束
    private void CheckBothFinished(int justFinishedPlayer)
    {
        if (player1Finished && player2Finished)
        {
            Debug.Log("双方对话均结束，隐藏UI");
            EndAllConversations();
        }
        else
        {
            // 如果另一方还没结束，等待对方到最后一张时触发
            Debug.Log($"玩家{justFinishedPlayer}已结束，等待另一方结束。");
        }
    }

    // 隐藏所有对话 UI 并调用下一步逻辑
    private void EndAllConversations()
    {
        HideAll(player1Conversations);
        HideAll(player2Conversations);

        if (scene1UI_Manager != null)
        {
            scene1UI_Manager.OverConversation();
        }
        else
        {
            Debug.LogWarning("scene1UI_Manager未绑定，无法关闭UI。");
        }
    }
}
