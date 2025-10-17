using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class InspectableItem : MonoBehaviour
{
    [Header("��������")]
    public string itemName;
    public string inspectSceneName = "InspectScene";

    [Header("�������")]
    public float interactDistance = 2f;
    public KeyCode interactKey = KeyCode.F;

    private bool isPlayerInRange = false;
    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player2");
    }

    void Update()
    {
        // �������Ƿ��ڽ�����Χ��
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            isPlayerInRange = distance <= interactDistance;

            // ��ⰴ������
            if (isPlayerInRange && Input.GetKeyDown(interactKey))
            {
                EnterInspectMode();
            }
        }

        // ��ʾ/���ؽ�����ʾ
        UpdateInteractionUI();
    }

    void UpdateInteractionUI()
    {
        // ����������UI��ʾ��������ʾ"��F�鿴"
        if (isPlayerInRange)
        {
            // ��ʾ������ʾUI
            ShowInteractionPrompt();
        }
        else
        {
            // ���ؽ�����ʾUI
            HideInteractionPrompt();
        }
    }

    void EnterInspectMode()
    {
        // ��ȡ����������
        SceneTransitionManager sceneManager = FindObjectOfType<SceneTransitionManager>();
        if (sceneManager == null)
        {
            // ���û���ҵ�����������������һ��
            GameObject managerObj = new GameObject("SceneTransitionManager");
            sceneManager = managerObj.AddComponent<SceneTransitionManager>();
        }

        // ���浱ǰ������Ϣ�������ڲ鿴��������ʾ��
        sceneManager.SetCurrentInspectItem(this);

        // ����鿴ģʽ
        sceneManager.LoadInspectScene(inspectSceneName);
    }

    void ShowInteractionPrompt()
    {
        // ʵ����ʾ������ʾUI���߼�
        // ���磺��ʾCanvas�ı���UIͼ��
    }

    void HideInteractionPrompt()
    {
        // ʵ�����ؽ�����ʾUI���߼�
    }

    // ���ӻ�������Χ����Scene��ͼ����ʾ��
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}