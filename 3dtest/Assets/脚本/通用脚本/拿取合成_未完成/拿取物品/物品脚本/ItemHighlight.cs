using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Renderer))]
public class ItemHighlight : MonoBehaviour
{
    [Header("高光设置")]
    public float highlightRange = 5f;                    // 高光触发范围
    public float highlightIntensity = 2f;               // 高光强度
    public Color highlightColor = Color.yellow;         // 高光颜色
    public float fadeInTime = 0.3f;                     // 淡入时间
    public float fadeOutTime = 0.5f;                    // 淡出时间
    public bool pulseEffect = true;                     // 是否启用脉冲效果
    public float pulseSpeed = 2f;                       // 脉冲速度

    [Header("玩家设置")]
    public string playerTag = "Player";                 // 玩家标签
    public LayerMask obstacleMask = -1;                 // 障碍物层掩码

    [Header("调试选项")]
    public bool showDebugGizmos = true;                 // 显示调试图形
    public bool enableHighlight = true;                 // 是否启用高光

    private Material[] originalMaterials;               // 原始材质
    private Material[] highlightMaterials;             // 高光材质
    private Renderer itemRenderer;                     // 物品渲染器
    private bool isHighlighted = false;                // 是否正在高光
    private float currentIntensity = 0f;               // 当前强度
    private Coroutine highlightCoroutine;              // 高光协程

    void Start()
    {
        InitializeHighlight();
    }

    void Update()
    {
        if (!enableHighlight) return;

        CheckForPlayers();
        UpdateHighlightEffect();
    }

    void InitializeHighlight()
    {
        itemRenderer = GetComponent<Renderer>();
        if (itemRenderer == null)
        {
            Debug.LogWarning($"物品 {gameObject.name} 没有Renderer组件，高光效果将不可用");
            enableHighlight = false;
            return;
        }

        // 保存原始材质
        originalMaterials = itemRenderer.materials;

        // 创建高光材质副本
        highlightMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            highlightMaterials[i] = new Material(originalMaterials[i]);
            highlightMaterials[i].EnableKeyword("_EMISSION");
        }

        Debug.Log($"高光系统初始化完成: {gameObject.name}");
    }

    void CheckForPlayers()
    {
        // 查找范围内的玩家
        Collider[] players = Physics.OverlapSphere(transform.position, highlightRange);
        bool playerInRange = false;

        foreach (Collider col in players)
        {
            if (col.CompareTag(playerTag))
            {
                // 检查是否有障碍物阻挡
                if (!IsObstructed(col.transform))
                {
                    playerInRange = true;
                    break;
                }
            }
        }

        // 根据玩家是否在范围内切换高光状态
        if (playerInRange && !isHighlighted)
        {
            StartHighlight();
        }
        else if (!playerInRange && isHighlighted)
        {
            StopHighlight();
        }
    }

    bool IsObstructed(Transform player)
    {
        Vector3 direction = player.position - transform.position;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, direction.normalized, out hit, highlightRange, obstacleMask))
        {
            // 如果射线击中的不是玩家，说明有障碍物阻挡
            return !hit.collider.CompareTag(playerTag);
        }

        return false;
    }

    void StartHighlight()
    {
        isHighlighted = true;

        // 停止之前的协程
        if (highlightCoroutine != null)
            StopCoroutine(highlightCoroutine);

        // 开始淡入效果
        highlightCoroutine = StartCoroutine(FadeHighlight(true));
    }

    void StopHighlight()
    {
        isHighlighted = false;

        if (highlightCoroutine != null)
            StopCoroutine(highlightCoroutine);

        highlightCoroutine = StartCoroutine(FadeHighlight(false));
    }

    IEnumerator FadeHighlight(bool fadeIn)
    {
        float startIntensity = currentIntensity;
        float targetIntensity = fadeIn ? highlightIntensity : 0f;
        float fadeTime = fadeIn ? fadeInTime : fadeOutTime;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            currentIntensity = Mathf.Lerp(startIntensity, targetIntensity, elapsedTime / fadeTime);
            yield return null;
        }

        currentIntensity = targetIntensity;
    }

    void UpdateHighlightEffect()
    {
        if (itemRenderer == null || highlightMaterials == null) return;

        // 应用高光材质
        if (currentIntensity > 0)
        {
            itemRenderer.materials = highlightMaterials;

            float finalIntensity = currentIntensity;
            if (pulseEffect && isHighlighted)
            {
                // 添加脉冲效果
                finalIntensity *= 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.2f;
            }

            // 设置发射颜色和强度
            Color emissionColor = highlightColor * finalIntensity;
            foreach (Material mat in highlightMaterials)
            {
                mat.SetColor("_EmissionColor", emissionColor);
            }
        }
        else
        {
            // 恢复原始材质
            itemRenderer.materials = originalMaterials;
        }
    }

    // 公共方法：手动启用高光
    public void EnableHighlight(bool enable)
    {
        enableHighlight = enable;
        if (!enable)
        {
            StopHighlight();
        }
    }

    // 公共方法：强制显示高光
    public void ForceHighlight(float duration = 3f)
    {
        if (!enableHighlight) return;

        StartHighlight();
        if (duration > 0)
        {
            StartCoroutine(ForceHighlightDuration(duration));
        }
    }

    IEnumerator ForceHighlightDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (isHighlighted)
        {
            StopHighlight();
        }
    }

    // 公共方法：设置高光范围
    public void SetHighlightRange(float newRange)
    {
        highlightRange = newRange;
    }

    // 公共方法：设置高光颜色
    public void SetHighlightColor(Color newColor)
    {
        highlightColor = newColor;
    }

    void OnDestroy()
    {
        // 清理创建的高光材质
        if (highlightMaterials != null)
        {
            foreach (Material mat in highlightMaterials)
            {
                if (mat != null)
                    DestroyImmediate(mat);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // 显示高光范围
        Gizmos.color = isHighlighted ? Color.yellow : Color.gray;
        Gizmos.DrawWireSphere(transform.position, highlightRange);

        // 显示高光状态
        GUIStyle style = new GUIStyle();
        style.normal.textColor = isHighlighted ? Color.yellow : Color.white;
#if UNITY_EDITOR
        string status = $"高光: {(isHighlighted ? "开启" : "关闭")}\n强度: {currentIntensity:F2}";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, status, style);
#endif
    }
}
