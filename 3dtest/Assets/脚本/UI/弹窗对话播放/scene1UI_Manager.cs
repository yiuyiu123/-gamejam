using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class scene1UI_Manager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject UI_Conversation;
    public GameObject UI_Settings;
    public bool noInputAnything=false;//ֻ�ܵ��F H

    private bool conversationStarted = false; // �Ƿ��Ѿ�����Ի��׶�
    private Conversation_Manager conversation;

    void Start()
    {
        UI_Settings.SetActive(false);
        UI_Conversation.SetActive(false);

        conversation = FindObjectOfType<Conversation_Manager>();
        //FindObjectOfType<SceneBGM>().PlayBGM();
    }

    private void Update()
    {
        if (!conversation.player1Finished)
        {
            if (!conversationStarted && Input.anyKeyDown)
            {
                UI_Conversation.SetActive(true);
                conversationStarted = true;
                noInputAnything = true;

                if (conversation != null)
                    conversation.StartConversation();
            }
            if (conversation != null && conversationStarted && noInputAnything)
            {
                if (Input.anyKeyDown && Input.GetKeyDown(KeyCode.F) == false && Input.GetKeyDown(KeyCode.H) == false)
                {
                    Debug.Log("���뱻����");
                }
            }
        }
    }

    public void OverConversation()
    {
        UI_Conversation.SetActive(false);
        noInputAnything =false;
        conversationStarted = false;//�ָ�����
        //OnSettings();
        // ���� "���" ���0��Ԫ�ص����Ч
        //AudioManager.Instance.PlayClick("���", volume: 1f, index: 0);
    }

    #region ����ת����������
    /*public void ClickGameButton()
    {
        SceneLoader.LoadSceneWithLoading(
            "FEI",
            "�������̻�ʹ�����ķ�ɫ�α�ڣ����ӷΰ����ա�",
            "��Ц�����ŷβ����ٽ�����������������������",
            7.5f
            );


        // ���� "���" ���0��Ԫ�ص����Ч
        AudioManager.Instance.PlayClick("���", volume: 1f, index: 0);

    }*/
    #endregion

    /*private void OnSettings()
    {
        TowUI(UI_Settings, UI_Conversation);
    }*/

    private void TowUI(GameObject show, GameObject off)
    {
        show.SetActive(true);
        off.SetActive(false);
        noInputAnything = true;
        // ���� "���" ���0��Ԫ�ص����Ч
        //AudioManager.Instance.PlayClick("���", volume: 1f, index: 0);
    }

    private void ToggleUI(GameObject showObj)
    {
        showObj.SetActive(!showObj.activeSelf);
        // ���� "���" ���0��Ԫ�ص����Ч
        //AudioManager.Instance.PlayClick("���", volume: 1f, index: 0);
    }
}