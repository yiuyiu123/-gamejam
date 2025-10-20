using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("基础跟踪")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -10f);
    [SerializeField][Range(0.01f, 1f)] private float smoothTime = 0.15f;

    [Header("高级设置")]
    [SerializeField] private bool enableBounds = true;
    [SerializeField] private Vector2 minBounds = new Vector2(-10, -5);
    [SerializeField] private Vector2 maxBounds = new Vector2(10, 5);
    [SerializeField] private bool lookAhead = true;
    [SerializeField] private float lookAheadMultiplier = 0.5f;

    [Header("抗抖动")]
    [SerializeField] private bool pixelPerfect = false;
    [SerializeField] private float pixelsPerUnit = 32f;

    private Vector3 velocity = Vector3.zero;
    private float originalZ;

    private void Start()
    {
        originalZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 计算目标位置（含偏移和预判）
        Vector3 targetPosition = target.position + offset;
        if (lookAhead && target.TryGetComponent<Rigidbody2D>(out var rb))
        {
            targetPosition += (Vector3)rb.velocity * lookAheadMultiplier * Time.deltaTime;
        }

        // 边界限制 
        if (enableBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        }

        // 平滑跟踪（帧率补偿）
        targetPosition.z = originalZ;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime * Mathf.Max(1, Time.deltaTime * 60)
        );

        // 可选像素对齐 
        if (pixelPerfect)
        {
            float pixelSize = 1f / pixelsPerUnit;
            transform.position = new Vector3(
                Mathf.Round(transform.position.x / pixelSize) * pixelSize,
                Mathf.Round(transform.position.y / pixelSize) * pixelSize,
                originalZ
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (enableBounds)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawCube((minBounds + maxBounds) / 2, maxBounds - minBounds);
        }
    }
}