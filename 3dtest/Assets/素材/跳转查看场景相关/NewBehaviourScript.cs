using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    [Header("鼠标灵敏度")]
    public float mouseSensitivity = 2f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Update()
    {
        // 获取鼠标输入 
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationX -= mouseY;
        rotationY += mouseX;

        // 限制垂直角度 
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        // 应用旋转 
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }
}