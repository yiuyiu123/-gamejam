using UnityEngine;

public class DetectLightSwitch : MonoBehaviour
{
    [Header("��������")]
    public bool IsSwitchOpen = false; // �� OpenLight �ű�����
    public Transform pivot;           // ��բ��ת�ο�������
    public float rotationAngle = 45f; // ��ʱ��ת�Ƕ�

    [Header("��Ҽ��")]
    public string playerTag = "Player";

    private bool playerInRange = false; // ����Ƿ��ڴ�����Χ
    private bool hasRotated = false;    // ��ֹ�ظ���ת

    void Update()
    {
        if (!IsSwitchOpen && playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            OpenSwitch();
        }
    }

    private void OpenSwitch()
    {
        IsSwitchOpen = true;

        if (pivot != null && !hasRotated)
        {
            // �� pivot ����λ��������ת
            transform.RotateAround(pivot.position, -transform.forward, -rotationAngle);
            hasRotated = true;
        }
        else if (pivot == null)
        {
            Debug.LogWarning("DetectLightSwitch: ������ pivot ������");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            playerInRange = false;
    }
}
