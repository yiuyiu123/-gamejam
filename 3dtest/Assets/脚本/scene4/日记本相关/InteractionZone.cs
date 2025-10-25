using UnityEngine;

public class InteractionZone : MonoBehaviour
{
    [Header("摄像机引用")]
    public Camera playerCamera;

    [Header("UI提示预制体")]
    public GameObject uiHintPrefab;

    [Header("交互物品预制体")]
    public GameObject interactableItemPrefab;

    [Header("UI提示偏移")]
    public Vector3 uiOffset = new Vector3(0, 2f, 0);

    private GameObject currentUIHint;
    private bool playerInZone = false;
    private GameObject spawnedItem;

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

        // 检测H键按下 
        if (playerInZone && Input.GetKeyDown(KeyCode.H))
        {
            SpawnInteractableItem();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player2"))
        {
            playerInZone = true;
            ShowUIHint();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player2"))
        {
            playerInZone = false;
            HideUIHint();
            // 移除了DestroySpawnedItem()调用，物品不会消失
        }
    }

    void ShowUIHint()
    {
        if (uiHintPrefab != null && currentUIHint == null)
        {
            currentUIHint = Instantiate(uiHintPrefab,
                transform.position + uiOffset,
                Quaternion.identity);
        }
    }

    void HideUIHint()
    {
        if (currentUIHint != null)
        {
            Destroy(currentUIHint);
            currentUIHint = null;
        }
    }

    void SpawnInteractableItem()
    {
        // 如果已经存在物品，先销毁 
        DestroySpawnedItem();

        if (interactableItemPrefab != null)
        {
            // 在区域中心生成物品
            spawnedItem = Instantiate(interactableItemPrefab,
                transform.position,
                Quaternion.identity);

            Debug.Log("交互物品已生成！");
        }
    }

    void DestroySpawnedItem()
    {
        if (spawnedItem != null)
        {
            Destroy(spawnedItem);
            spawnedItem = null;
        }
    }
}