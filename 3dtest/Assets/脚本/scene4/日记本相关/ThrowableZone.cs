using System.Collections;
using UnityEngine;

public class ThrowableZone : MonoBehaviour
{
    [Header("抛掷设置")]
    public Transform throwTarget;
    public float throwHeight = 3f;
    public float throwDuration = 0.8f;

    [Header("区域设置")]
    public float detectionRadius = 3f;

    [Header("调试选项")]
    public bool showDebugGUI = false;

    private void Start()
    {
        EnsureColliderSize();
        Debug.Log($"抛掷区域 {name} 已初始化");
    }

    void EnsureColliderSize()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = detectionRadius;
        }
        else if (!collider.isTrigger)
        {
            collider.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        InteractableItem item = other.GetComponent<InteractableItem>();
        if (item != null && !item.isBeingHeld && !item.isInExchangeProcess)
        {
            // 自动抛掷物品到区域中心
            StartCoroutine(ThrowItemCoroutine(item));
        }
    }

    // 抛掷物品到区域中心 
    public IEnumerator ThrowItemCoroutine(InteractableItem item)
    {
        Debug.Log($"开始抛掷物品: {item.itemName}  到区域 {name}");

        item.isInExchangeProcess = true;
        if (item.Rb != null)
        {
            item.Rb.isKinematic = true;
            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
        }

        Vector3 startPosition = item.transform.position;
        Vector3 targetPosition = throwTarget != null ? throwTarget.position : transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < throwDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / throwDuration;
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, progress);
            currentPos.y += Mathf.Sin(progress * Mathf.PI) * throwHeight;
            item.transform.position = currentPos;
            yield return null;
        }

        item.transform.position = targetPosition;
        item.isInExchangeProcess = false;
        if (item.Rb != null) item.Rb.isKinematic = false;

        Debug.Log($"物品 {item.itemName}  抛掷完成到区域 {name}");
    }

    // 手动抛掷物品到区域
    public void ThrowItemToZone(InteractableItem item)
    {
        StartCoroutine(ThrowItemCoroutine(item));
    }

    [ContextMenu("测试抛掷动画")]
    public void TestThrowAnimation()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider collider in colliders)
        {
            InteractableItem item = collider.GetComponent<InteractableItem>();
            if (item != null && !item.isBeingHeld)
            {
                StartCoroutine(ThrowItemCoroutine(item));
                break;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Vector3 targetPos = throwTarget != null ? throwTarget.position : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPos, 0.5f);

        // 绘制抛掷路径示意 
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPos, 0.3f);
        }
    }
}