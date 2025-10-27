using UnityEngine;

public class uicome : MonoBehaviour
{
    [Header("摄像机引用")]
    public Camera playerCamera;

    [Header("UI提示预制体")]
    public GameObject uiHintPrefab;

    [Header("UI提示偏移")]
    public Vector3 uiOffset = new Vector3(0, 2f, 0);

    private GameObject currentUIHint;
    private bool playerInZone = false;

    void Start()
    {
        // 确保有碰撞器 
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
            GetComponent<Collider>().isTrigger = true;
        }
    }

    void Update()
    {
        // 更新UI朝向摄像机 
        if (currentUIHint != null && playerCamera != null)
        {
            currentUIHint.transform.LookAt(currentUIHint.transform.position +
                playerCamera.transform.rotation * Vector3.forward,
                playerCamera.transform.rotation * Vector3.up);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1"))
        {
            playerInZone = true;
            ShowUIHint();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player1"))
        {
            playerInZone = false;
            HideUIHint();
        }
    }

    void ShowUIHint()
    {
        if (uiHintPrefab != null && currentUIHint == null)
        {
            currentUIHint = Instantiate(uiHintPrefab,
                transform.position + uiOffset,
                Quaternion.identity);
            Debug.Log("UI提示已显示");
        }
    }

    void HideUIHint()
    {
        if (currentUIHint != null)
        {
            Debug.Log("正在销毁UI提示");
            Destroy(currentUIHint);
            currentUIHint = null;
            Debug.Log("UI提示已彻底隐藏");
        }
    }
}