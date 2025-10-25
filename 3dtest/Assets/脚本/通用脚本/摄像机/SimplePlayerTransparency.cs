using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlayerTransparency : MonoBehaviour
{
    [Header("͸��Ч������")]
    public float transparencyDistance = 3f;
    public float minAlpha = 0.3f;
    public float fadeSpeed = 5f;

    private SpriteRenderer spriteRenderer;
    private Camera playerCamera;
    private Color originalColor;
    private float currentAlpha = 1f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SimplePlayerTransparency: û���ҵ�SpriteRenderer�����");
            enabled = false;
            return;
        }

        originalColor = spriteRenderer.color;
        FindPlayerCamera();
    }

    void FindPlayerCamera()
    {
        // ���Ҷ�Ӧ�������
        string playerName = gameObject.name.ToLower();
        if (playerName.Contains("player1") || playerName.Contains("1"))
        {
            playerCamera = GameObject.Find("Player1Camera")?.GetComponent<Camera>();
        }
        else if (playerName.Contains("player2") || playerName.Contains("2"))
        {
            playerCamera = GameObject.Find("Player2Camera")?.GetComponent<Camera>();
        }

        if (playerCamera == null)
        {
            Debug.LogWarning($"SimplePlayerTransparency: �޷��ҵ� {gameObject.name} �������");
            enabled = false;
        }
    }

    void Update()
    {
        if (spriteRenderer == null || playerCamera == null) return;

        UpdateTransparency();
    }

    void UpdateTransparency()
    {
        // ������ҵ�������ľ���
        float distance = Vector3.Distance(transform.position, playerCamera.transform.position);

        // ����Ŀ��͸����
        float targetAlpha = 1f;
        if (distance < transparencyDistance)
        {
            targetAlpha = Mathf.Lerp(minAlpha, 1f, distance / transparencyDistance);
        }

        // ƽ������͸����
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

        // Ӧ��͸����
        Color newColor = originalColor;
        newColor.a = currentAlpha;
        spriteRenderer.color = newColor;
    }

    void OnDestroy()
    {
        // �ָ�ԭʼ��ɫ
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    // ������Ϣ
    void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawLine(transform.position, playerCamera.transform.position);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, transparencyDistance);
        }
    }
}