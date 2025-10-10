using UnityEngine;

public class ItemInteraction : MonoBehaviour
{
    [SerializeField] private Camera inspectionCamera; // ���븱����ͷ
    [SerializeField] private GameObject inspectionUI; // ������ͷ�����Canvas�������ذ�ť��
    private bool isPlayerNear;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1"))
        {
            isPlayerNear = true;
            Debug.Log("��Hʰȡ��Ʒ");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player1")) isPlayerNear = false;
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.H))
        {
            // �л�����ͷ 
            Camera.main.gameObject.SetActive(false);
            inspectionCamera.gameObject.SetActive(true);
            inspectionUI.SetActive(true);

            // ������Ʒ�������٣�
            gameObject.SetActive(false);
        }
    }

    // �ⲿ���ã��ָ���Ʒ״̬ 
    public void ResetItem()
    {
        gameObject.SetActive(true);
    }
}