using UnityEngine;

public class HintTrigger : MonoBehaviour
{
    [Header("提示图片设置")]
    public SpriteRenderer hintSprite;  // 提示图片的SpriteRenderer组件 
    public float showDuration = 3f;    // 显示持续时间（0表示一直显示直到离开）
    public bool hideOnExit = true;     // 离开区域时隐藏 

    [Header("动画设置")]
    public bool useFadeEffect = true;  // 是否使用淡入淡出效果 
    public float fadeTime = 0.5f;      // 淡入淡出时间

    private bool playerInTrigger = false;
    private Coroutine showCoroutine;

    private void Start()
    {
        // 确保开始时图片是隐藏的
        if (hintSprite != null)
        {
            if (useFadeEffect)
            {
                hintSprite.color = new Color(hintSprite.color.r, hintSprite.color.g, hintSprite.color.b, 0);
            }
            hintSprite.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否是玩家进入 
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            ShowHint();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 检查是否是玩家离开 
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;

            if (hideOnExit)
            {
                HideHint();
            }
        }
    }

    /// <summary>
    /// 显示提示图片 
    /// </summary>
    private void ShowHint()
    {
        if (hintSprite == null) return;

        // 如果已经有显示协程在运行，先停止它 
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }

        showCoroutine = StartCoroutine(ShowHintCoroutine());
    }

    private System.Collections.IEnumerator ShowHintCoroutine()
    {
        // 激活图片 
        hintSprite.gameObject.SetActive(true);

        if (useFadeEffect)
        {
            // 淡入效果 
            float timer = 0f;
            while (timer < fadeTime)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(0, 1, timer / fadeTime);
                hintSprite.color = new Color(hintSprite.color.r, hintSprite.color.g, hintSprite.color.b, alpha);
                yield return null;
            }
            hintSprite.color = new Color(hintSprite.color.r, hintSprite.color.g, hintSprite.color.b, 1);
        }

        // 如果设置了显示时间，定时隐藏 
        if (showDuration > 0)
        {
            yield return new WaitForSeconds(showDuration);

            // 只有在玩家已经离开或者还在区域内但需要自动隐藏时隐藏 
            if (!playerInTrigger || hideOnExit)
            {
                HideHint();
            }
        }
    }

    /// <summary>
    /// 隐藏提示图片 
    /// </summary>
    private void HideHint()
    {
        if (hintSprite == null) return;

        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }

        showCoroutine = StartCoroutine(HideHintCoroutine());
    }

    private System.Collections.IEnumerator HideHintCoroutine()
    {
        if (useFadeEffect)
        {
            // 淡出效果 
            float timer = 0f;
            Color startColor = hintSprite.color;

            while (timer < fadeTime)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0, timer / fadeTime);
                hintSprite.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }

        // 禁用图片
        hintSprite.gameObject.SetActive(false);

        // 重置透明度
        if (useFadeEffect)
        {
            hintSprite.color = new Color(hintSprite.color.r, hintSprite.color.g, hintSprite.color.b, 0);
        }
    }

    // 在编辑器中可视化触发区域
    private void OnDrawGizmos()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(transform.position + new Vector3(collider.offset.x, collider.offset.y, 0),
                           new Vector3(collider.size.x, collider.size.y, 1));
        }
    }
}