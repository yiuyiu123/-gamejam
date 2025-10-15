using UnityEngine;

public class DetectLightSwitch : MonoBehaviour
{
    [Header("开关设置")]
    public bool IsSwitchOpen = false; // 与 OpenLight 脚本关联
    public Transform pivot;           // 电闸旋转参考空物体
    public float rotationAngle = 45f; // 打开时旋转角度

    [Header("玩家检测")]
    public string playerTag = "Player";

    private bool playerInRange = false; // 玩家是否在触发范围
    private bool hasRotated = false;    // 防止重复旋转

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
            // 沿 pivot 世界位置向下旋转
            transform.RotateAround(pivot.position, -transform.forward, -rotationAngle);
            hasRotated = true;
        }
        else if (pivot == null)
        {
            Debug.LogWarning("DetectLightSwitch: 请设置 pivot 空物体");
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
