using UnityEngine;

public class SpawnPrefabsOnTrigger : MonoBehaviour
{
    [Header("预制体设置")]
    public GameObject prefab1;     // 第一个预制体 
    public GameObject prefab2;     // 第二个预制体 

    [Header("生成位置设置")]
    public Transform spawnPoint1;  // 第一个生成位置的空物体 
    public Transform spawnPoint2;  // 第二个生成位置的空物体 

    [Header("生成设置")]
    public bool spawnOnEnter = true;           // 进入时生成 
    public bool spawnOnExit = false;           // 离开时生成 
    public bool destroyOnExit = false;         // 离开时销毁生成的物体 
    public bool spawnOnlyOnce = true;          // 只生成一次 

    [Header("备用位置（如果没设置空物体）")]
    public Vector3 fallbackPosition1 = Vector3.zero;    // 备用位置1 
    public Vector3 fallbackPosition2 = Vector3.zero;    // 备用位置2 

    private GameObject spawnedObject1;         // 已生成的第一个物体 
    private GameObject spawnedObject2;         // 已生成的第二个物体 
    private bool hasSpawned = false;           // 是否已经生成过 

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

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (spawnOnExit && (!spawnOnlyOnce || !hasSpawned))
            {
                SpawnPrefabs();
                hasSpawned = true;
            }

            if (destroyOnExit)
            {
                DestroySpawnedObjects();
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
            // 如果没有设置空物体，使用备用位置 
            return fallbackPosition;
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

        Debug.Log($"生成了预制体在位置: {actualSpawnPos1} 和 {actualSpawnPos2}");
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

        Debug.Log("已销毁生成的物体");
    }

    /// <summary>
    /// 手动设置生成位置空物体 
    /// </summary>
    public void SetSpawnPoints(Transform point1, Transform point2)
    {
        spawnPoint1 = point1;
        spawnPoint2 = point2;
    }

    /// <summary>
    /// 重置生成状态（允许再次生成）
    /// </summary>
    public void ResetSpawnState()
    {
        hasSpawned = false;
    }

    // 在编辑器中可视化生成位置 
    private void OnDrawGizmosSelected()
    {
        // 绘制触发区域 
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(transform.position + new Vector3(collider.offset.x, collider.offset.y, 0),
                           new Vector3(collider.size.x, collider.size.y, 1));
        }

        // 获取实际生成位置用于可视化 
        Vector3 actualSpawnPos1 = GetSpawnPosition(spawnPoint1, fallbackPosition1);
        Vector3 actualSpawnPos2 = GetSpawnPosition(spawnPoint2, fallbackPosition2);

        // 绘制生成位置 
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(actualSpawnPos1, 0.5f);
        Gizmos.DrawWireSphere(actualSpawnPos2, 0.5f);

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(actualSpawnPos1, 0.3f);
        Gizmos.DrawSphere(actualSpawnPos2, 0.3f);

        // 绘制连线 
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, actualSpawnPos1);
        Gizmos.DrawLine(transform.position, actualSpawnPos2);

        // 绘制从空物体到生成位置的连线（如果有空物体）
        if (spawnPoint1 != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(spawnPoint1.position, actualSpawnPos1);
            if (spawnPoint2 != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(spawnPoint2.position, actualSpawnPos2);
            }
        }
    }
}