using UnityEngine;
using UnityEngine.UI;

public class Conversation_Manager : MonoBehaviour
{
    [Header("���1�Ի�")]
    public GameObject[] player1Conversations;
    [Header("���2�Ի�")]
    public GameObject[] player2Conversations;

    [Header("���õ�UI�����������ڽ���ʱ����UI")]
    public scene1UI_Manager scene1UI_Manager;

    [Header("���밴������")]
    public KeyCode player1Key = KeyCode.F;
    public KeyCode player2Key = KeyCode.H;

    private int player1Index = 0;
    private int player2Index = 0;

    public bool player1Finished = false;
    public bool player2Finished = false;

    public void StartConversation()
    {
        // ��ʼ�����������жԻ�ͼƬ
        InitConversation(player1Conversations);
        InitConversation(player2Conversations);

        // ��ʾ���Եĵ�һ��
        ShowConversation(player1Conversations, 0);
        ShowConversation(player2Conversations, 0);
    }

    void Update()
    {
        // ���1��F�л�
        if (!player1Finished && Input.GetKeyDown(player1Key))
        {
            HandleNext(player: 1);
        }

        // ���2��H�л�
        if (!player2Finished && Input.GetKeyDown(player2Key))
        {
            HandleNext(player: 2);
        }
    }

    // ͳһ�ĶԻ���ʼ��
    private void InitConversation(GameObject[] convArray)
    {
        foreach (var img in convArray)
        {
            if (img != null)
                img.SetActive(false);
        }
    }

    // ��ʾָ�������ָ���±�
    private void ShowConversation(GameObject[] convArray, int index)
    {
        var ui = FindObjectOfType<scene1UI_Manager>();
        
        if (convArray == null || convArray.Length == 0) 
            return;
        if (index >= 0 && index < convArray.Length)
            convArray[index].SetActive(true);
    }

    // ����ָ�����������
    private void HideAll(GameObject[] convArray)
    {
        foreach (var img in convArray)
        {
            if (img != null)
                img.SetActive(false);
        }
    }

    // ��ҵ����һ���߼�
    // ��ҵ����һ���߼�
    public void HandleNext(int player)
    {
        if (player == 1)
        {
            // ����ǰһ�ţ��������һ��ʱ������
            if (player1Index < player1Conversations.Length)
                HideConversation(player1Conversations, player1Index);

            player1Index++;

            if (player1Index < player1Conversations.Length)
            {
                ShowConversation(player1Conversations, player1Index);
            }
            else
            {
                // �����һ���ˣ������أ�������
                player1Finished = true;
                Debug.Log("���1�Ի��������");

                // ���˫���Ƿ񶼽���
                CheckBothFinished(1);
            }
        }
        else if (player == 2)
        {
            if (player2Index < player2Conversations.Length)
                HideConversation(player2Conversations, player2Index);

            player2Index++;

            if (player2Index < player2Conversations.Length)
            {
                ShowConversation(player2Conversations, player2Index);
            }
            else
            {
                player2Finished = true;
                Debug.Log("���2�Ի��������");

                CheckBothFinished(2);
            }
        }
    }

    // ���ص����Ի�ͼƬ����Ӱ�����һ�ţ�
    private void HideConversation(GameObject[] convArray, int index)
    {
        if (convArray == null || index < 0 || index >= convArray.Length) return;

        // ��������һ�žͲ�����
        if (index < convArray.Length - 1)
            convArray[index].SetActive(false);
    }


    // ���˫���Ƿ����
    private void CheckBothFinished(int justFinishedPlayer)
    {
        if (player1Finished && player2Finished)
        {
            Debug.Log("˫���Ի�������������UI");
            EndAllConversations();
        }
        else
        {
            // �����һ����û�������ȴ��Է������һ��ʱ����
            Debug.Log($"���{justFinishedPlayer}�ѽ������ȴ���һ��������");
        }
    }

    // �������жԻ� UI ��������һ���߼�
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
            Debug.LogWarning("scene1UI_Managerδ�󶨣��޷��ر�UI��");
        }
    }
}
