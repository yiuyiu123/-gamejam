using UnityEngine;

public class SpawnPrefabsOnTrigger : MonoBehaviour
{
    [Header("Ԥ��������")]
    public GameObject prefab1;     // ��һ��Ԥ���� 
    public GameObject prefab2;     // �ڶ���Ԥ���� 

    [Header("����λ������")]
    public Transform spawnPoint1;  // ��һ������λ�õĿ����� 
    public Transform spawnPoint2;  // �ڶ�������λ�õĿ����� 

    [Header("��������")]
    public bool spawnOnEnter = true;           // ����ʱ���� 
    public bool spawnOnExit = false;           // �뿪ʱ���� 
    public bool destroyOnExit = false;         // �뿪ʱ�������ɵ����� 
    public bool spawnOnlyOnce = true;          // ֻ����һ�� 

    [Header("����λ�ã����û���ÿ����壩")]
    public Vector3 fallbackPosition1 = Vector3.zero;    // ����λ��1 
    public Vector3 fallbackPosition2 = Vector3.zero;    // ����λ��2 

    private GameObject spawnedObject1;         // �����ɵĵ�һ������ 
    private GameObject spawnedObject2;         // �����ɵĵڶ������� 
    private bool hasSpawned = false;           // �Ƿ��Ѿ����ɹ� 

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
    /// ��ȡʵ������λ�� 
    /// </summary>
    private Vector3 GetSpawnPosition(Transform spawnPoint, Vector3 fallbackPosition)
    {
        if (spawnPoint != null)
        {
            return spawnPoint.position;
        }
        else
        {
            // ���û�����ÿ����壬ʹ�ñ���λ�� 
            return fallbackPosition;
        }
    }

    /// <summary>
    /// ����Ԥ���� 
    /// </summary>
    public void SpawnPrefabs()
    {
        // ��ȡʵ������λ�� 
        Vector3 actualSpawnPos1 = GetSpawnPosition(spawnPoint1, fallbackPosition1);
        Vector3 actualSpawnPos2 = GetSpawnPosition(spawnPoint2, fallbackPosition2);

        // ���ɵ�һ��Ԥ���� 
        if (prefab1 != null)
        {
            // ����Ѿ����ڣ������� 
            if (spawnedObject1 != null)
                Destroy(spawnedObject1);

            spawnedObject1 = Instantiate(prefab1, actualSpawnPos1, Quaternion.identity);
            spawnedObject1.name = prefab1.name + "_Spawned";
        }

        // ���ɵڶ���Ԥ���� 
        if (prefab2 != null)
        {
            // ����Ѿ����ڣ������� 
            if (spawnedObject2 != null)
                Destroy(spawnedObject2);

            spawnedObject2 = Instantiate(prefab2, actualSpawnPos2, Quaternion.identity);
            spawnedObject2.name = prefab2.name + "_Spawned";
        }

        Debug.Log($"������Ԥ������λ��: {actualSpawnPos1} �� {actualSpawnPos2}");
    }

    /// <summary>
    /// ���������ɵ����� 
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

        Debug.Log("���������ɵ�����");
    }

    /// <summary>
    /// �ֶ���������λ�ÿ����� 
    /// </summary>
    public void SetSpawnPoints(Transform point1, Transform point2)
    {
        spawnPoint1 = point1;
        spawnPoint2 = point2;
    }

    /// <summary>
    /// ��������״̬�������ٴ����ɣ�
    /// </summary>
    public void ResetSpawnState()
    {
        hasSpawned = false;
    }

    // �ڱ༭���п��ӻ�����λ�� 
    private void OnDrawGizmosSelected()
    {
        // ���ƴ������� 
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(transform.position + new Vector3(collider.offset.x, collider.offset.y, 0),
                           new Vector3(collider.size.x, collider.size.y, 1));
        }

        // ��ȡʵ������λ�����ڿ��ӻ� 
        Vector3 actualSpawnPos1 = GetSpawnPosition(spawnPoint1, fallbackPosition1);
        Vector3 actualSpawnPos2 = GetSpawnPosition(spawnPoint2, fallbackPosition2);

        // ��������λ�� 
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(actualSpawnPos1, 0.5f);
        Gizmos.DrawWireSphere(actualSpawnPos2, 0.5f);

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(actualSpawnPos1, 0.3f);
        Gizmos.DrawSphere(actualSpawnPos2, 0.3f);

        // �������� 
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, actualSpawnPos1);
        Gizmos.DrawLine(transform.position, actualSpawnPos2);

        // ���ƴӿ����嵽����λ�õ����ߣ�����п����壩
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