using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    [Header("���������")]
    public float mouseSensitivity = 2f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Update()
    {
        // ��ȡ������� 
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationX -= mouseY;
        rotationY += mouseX;

        // ���ƴ�ֱ�Ƕ� 
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        // Ӧ����ת 
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }
}