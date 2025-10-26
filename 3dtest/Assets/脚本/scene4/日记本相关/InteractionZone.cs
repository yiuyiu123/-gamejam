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
    private bool hasInteracted = false; // 新增：标记是否已经交互过 

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
        // 如果已经交互过，直接返回
        if (hasInteracted) return;

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
        // 如果已经交互过，直接返回
        if (hasInteracted) return;

        if (other.CompareTag("Player2"))
        {
            playerInZone = true;
            ShowUIHint();
        }
    }

    void OnTriggerExit(Collider other)
    {
        // 如果已经交互过，直接返回 
        if (hasInteracted) return;

        if (other.CompareTag("Player2"))
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

    void SpawnInteractableItem()
    {
        // 如果已经交互过，直接返回 
        if (hasInteracted) return;

        // 如果已经存在物品，先销毁 
        DestroySpawnedItem();

        if (interactableItemPrefab != null)
        {
            // 在区域中心生成物品 
            spawnedItem = Instantiate(interactableItemPrefab,
                transform.position,
                Quaternion.identity);

            Debug.Log("交互物品已生成！");

            // 隐藏UI提示 
            HideUIHint();

            // 标记为已交互 
            hasInteracted = true;

            // 禁用脚本 
            this.enabled = false;

            Debug.Log("脚本已禁用，UI提示将永久隐藏");
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

    // 可选：如果需要重置交互状态，可以添加这个方法
    public void ResetInteraction()
    {
        hasInteracted = false;
        this.enabled = true;
        playerInZone = false;
        HideUIHint();
        DestroySpawnedItem();
    }
}