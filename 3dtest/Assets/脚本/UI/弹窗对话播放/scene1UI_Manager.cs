using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class scene1UI_Manager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject UI_Conversation;
    public GameObject UI_Settings;
    public Button GameButton;

    void Start()
    {
        UI_Settings.SetActive(false);
        UI_Conversation.SetActive(false);
        //FindObjectOfType<SceneBGM>().PlayBGM();
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            UI_Conversation.SetActive(true);
        }
    }

    public void OverConversation()
    {
        OnSettings();
        // ���� "���" ���0��Ԫ�ص����Ч
        AudioManager.Instance.PlayClick("���", volume: 1f, index: 0);
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

    private void OnSettings()
    {
        TowUI(UI_Settings, UI_Conversation);
    }

    private void TowUI(GameObject show, GameObject off)
    {
        show.SetActive(true);
        off.SetActive(false);
        // ���� "���" ���0��Ԫ�ص����Ч
        AudioManager.Instance.PlayClick("���", volume: 1f, index: 0);
    }

    private void ToggleUI(GameObject showObj)
    {
        showObj.SetActive(!showObj.activeSelf);
        // ���� "���" ���0��Ԫ�ص����Ч
        AudioManager.Instance.PlayClick("���", volume: 1f, index: 0);
    }
}