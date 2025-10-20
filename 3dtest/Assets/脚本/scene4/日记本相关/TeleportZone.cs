using UnityEngine;

public class TeleportZone : MonoBehaviour
{
    [Header("��������")]
    [Tooltip("��קһ����������Ϊ����Ŀ���")]
    public Transform targetTransform; // ��ΪTransform���� 

    [Tooltip("����ʱ�Ƿ�����ҵ�ǰ�ٶ�")]
    public bool preserveVelocity = false;

    [Tooltip("���ͺ��Ƿ����ö����޵�ʱ��(��)")]
    public float invincibleDuration = 0.5f;

    // ��ѡ��������Ч 
    public GameObject teleportEffectPrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && targetTransform != null) // ���ӿ����ü�� 
        {
            TeleportPlayer(other.gameObject);
        }
        else if (targetTransform == null)
        {
            Debug.LogWarning("����Ŀ��δ���ã�", this);
        }
    }

    private void TeleportPlayer(GameObject player)
    {
        // ���Ŵ�����Ч������У�
        if (teleportEffectPrefab != null)
        {
            Instantiate(teleportEffectPrefab, player.transform.position, Quaternion.identity);
        }

        // ��ȡ��ҵ�ǰ�ٶȣ������Ҫ������
        Vector2 storedVelocity = Vector2.zero;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null && preserveVelocity)
        {
            storedVelocity = rb.velocity;
        }

        // ִ�д��ͣ�ʹ��Transform��λ�ã�
        player.transform.position = targetTransform.position;

        // �ָ��ٶȣ������Ҫ��
        if (rb != null && preserveVelocity)
        {
            rb.velocity = storedVelocity;
        }

        // �����޵�ʱ�䣨��������ã�
        if (invincibleDuration > 0)
        {
            StartCoroutine(InvincibilityCoroutine(player));
        }

        // ���ͺ���Ч������У�
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