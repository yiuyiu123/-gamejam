using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlayerTransparency : MonoBehaviour
{
    [Header("透明效果设置")]
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
            Debug.LogError("SimplePlayerTransparency: 没有找到SpriteRenderer组件！");
            enabled = false;
            return;
        }

        originalColor = spriteRenderer.color;
        FindPlayerCamera();
    }

    void FindPlayerCamera()
    {
        // 查找对应的摄像机
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
            Debug.LogWarning($"SimplePlayerTransparency: 无法找到 {gameObject.name} 的摄像机");
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
        // 计算玩家到摄像机的距离
        float distance = Vector3.Distance(transform.position, playerCamera.transform.position);

        // 计算目标透明度
        float targetAlpha = 1f;
        if (distance < transparencyDistance)
        {
            targetAlpha = Mathf.Lerp(minAlpha, 1f, distance / transparencyDistance);
        }

        // 平滑过渡透明度
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

        // 应用透明度
        Color newColor = originalColor;
        newColor.a = currentAlpha;
        spriteRenderer.color = newColor;
    }

    void OnDestroy()
    {
        // 恢复原始颜色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    // 调试信息
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