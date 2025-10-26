using UnityEngine;

public class SpawnPrefabsOnTrigger : MonoBehaviour
{
    [Header("预制体设置")]
    public GameObject prefab1;
    public GameObject prefab2;

    [Header("生成位置设置")]
    public Transform spawnPoint1;
    public Transform spawnPoint2;

    [Header("生成设置")]
    public bool spawnOnEnter = true;
    public bool spawnOnlyOnce = true;

    [Header("备用位置（如果没设置空物体）")]
    public Vector3 fallbackPosition1 = Vector3.zero;
    public Vector3 fallbackPosition2 = Vector3.zero;

    [Header("需要显隐的物体")]
    public GameObject objectToShow1;
    public GameObject objectToShow2;

    private GameObject spawnedObject1;
    private GameObject spawnedObject2;
    private bool hasSpawned = false;

    private void Start()
    {
        // 初始时隐藏两个物体 
        if (objectToShow1 != null) objectToShow1.SetActive(false);
        if (objectToShow2 != null) objectToShow2.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (spawnOnEnter && other.CompareTag("Player"))
        {
            if (!spawnOnlyOnce || !hasSpawned)
            {
                SpawnPrefabs();
                hasSpawned = true;
            }
        }
    }

    /// <summary>
    /// 获取实际生成位置 
    /// </summary>
    private Vector3 GetSpawnPosition(Transform spawnPoint, Vector3 fallbackPosition)
    {
        if (spawnPoint != null)
        {
            return spawnPoint.position;
        }
        else
        {
            return transform.position + fallbackPosition;
        }
    }

    /// <summary>
    /// 生成预制体 
    /// </summary>
    public void SpawnPrefabs()
    {
        // 获取实际生成位置 
        Vector3 actualSpawnPos1 = GetSpawnPosition(spawnPoint1, fallbackPosition1);
        Vector3 actualSpawnPos2 = GetSpawnPosition(spawnPoint2, fallbackPosition2);

        // 生成第一个预制体 
        if (prefab1 != null)
        {
            // 如果已经存在，先销毁 
            if (spawnedObject1 != null)
                Destroy(spawnedObject1);

            spawnedObject1 = Instantiate(prefab1, actualSpawnPos1, Quaternion.identity);
            spawnedObject1.name = prefab1.name + "_Spawned";
        }

        // 生成第二个预制体 
        if (prefab2 != null)
        {
            // 如果已经存在，先销毁 
            if (spawnedObject2 != null)
                Destroy(spawnedObject2);

            spawnedObject2 = Instantiate(prefab2, actualSpawnPos2, Quaternion.identity);
            spawnedObject2.name = prefab2.name + "_Spawned";
        }

        // 显示两个需要显隐的物体 
        ShowHiddenObjects();

        Debug.Log($"生成了预制体在位置: {actualSpawnPos1} 和 {actualSpawnPos2}");
    }

    /// <summary>
    /// 显示被隐藏的物体 
    /// </summary>
    private void ShowHiddenObjects()
    {
        if (objectToShow1 != null) objectToShow1.SetActive(true);
        if (objectToShow2 != null) objectToShow2.SetActive(true);
    }

    /// <summary>
    /// 销毁已生成的物体 
    /// </summary>
    public void DestroySpawnedObjects()
    {
        if (spawnedObject1 != null)
        {
            Destroy(spawnedObject1);
            spawnedObject1 = null;
        }

        if (spawnedObject2 != null)
        {
            Destroy(spawnedObject2);
            spawnedObject2 = null;
        }
    }

    /// <summary>
    /// 重置生成状态（允许再次生成）
    /// </summary>
    public void ResetSpawnState()
    {
        hasSpawned = false;
    }
}