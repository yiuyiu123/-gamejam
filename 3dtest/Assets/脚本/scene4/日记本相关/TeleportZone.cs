using UnityEngine;

public class TeleportZone : MonoBehaviour
{
    [Header("传送设置")]
    [Tooltip("拖拽一个空物体作为传送目标点")]
    public Transform targetTransform; // 改为Transform类型 

    [Tooltip("传送时是否保留玩家当前速度")]
    public bool preserveVelocity = false;

    [Tooltip("传送后是否启用短暂无敌时间(秒)")]
    public float invincibleDuration = 0.5f;

    // 可选：传送特效 
    public GameObject teleportEffectPrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && targetTransform != null) // 增加空引用检查 
        {
            TeleportPlayer(other.gameObject);
        }
        else if (targetTransform == null)
        {
            Debug.LogWarning("传送目标未设置！", this);
        }
    }

    private void TeleportPlayer(GameObject player)
    {
        // 播放传送特效（如果有）
        if (teleportEffectPrefab != null)
        {
            Instantiate(teleportEffectPrefab, player.transform.position, Quaternion.identity);
        }

        // 获取玩家当前速度（如果需要保留）
        Vector2 storedVelocity = Vector2.zero;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null && preserveVelocity)
        {
            storedVelocity = rb.velocity;
        }

        // 执行传送（使用Transform的位置）
        player.transform.position = targetTransform.position;

        // 恢复速度（如果需要）
        if (rb != null && preserveVelocity)
        {
            rb.velocity = storedVelocity;
        }

        // 触发无敌时间（如果有设置）
        if (invincibleDuration > 0)
        {
            StartCoroutine(InvincibilityCoroutine(player));
        }

        // 传送后特效（如果有）
        if (teleportEffectPrefab != null)
        {
            Instantiate(teleportEffectPrefab, targetTransform.position, Quaternion.identity);
        }
    }

    private System.Collections.IEnumerator InvincibilityCoroutine(GameObject player)
    {
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        bool originalColliderState = playerCollider.enabled;
        playerCollider.enabled = false;

        SpriteRenderer sprite = player.GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;
        float elapsedTime = 0f;

        while (elapsedTime < invincibleDuration)
        {
            sprite.color = Color.Lerp(originalColor, new Color(1, 1, 1, 0.5f), Mathf.PingPong(elapsedTime * 10f, 1));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        sprite.color = originalColor;
        playerCollider.enabled = originalColliderState;
    }
}