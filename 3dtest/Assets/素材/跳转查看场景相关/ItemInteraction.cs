using UnityEngine;

public class ItemInteraction : MonoBehaviour
{
    [SerializeField] private Camera inspectionCamera; // 拖入副摄像头
    [SerializeField] private GameObject inspectionUI; // 副摄像头界面的Canvas（含返回按钮）
    private bool isPlayerNear;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1"))
        {
            isPlayerNear = true;
            Debug.Log("按H拾取物品");
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
            // 切换摄像头 
            Camera.main.gameObject.SetActive(false);
            inspectionCamera.gameObject.SetActive(true);
            inspectionUI.SetActive(true);

            // 隐藏物品（非销毁）
            gameObject.SetActive(false);
        }
    }

    // 外部调用：恢复物品状态 
    public void ResetItem()
    {
        gameObject.SetActive(true);
    }
}