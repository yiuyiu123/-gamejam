using UnityEngine;

public class HintTrigger : MonoBehaviour
{
    [Header("��ʾͼƬ����")]
    public SpriteRenderer hintSprite;  // ��ʾͼƬ��SpriteRenderer��� 
    public float showDuration = 3f;    // ��ʾ����ʱ�䣨0��ʾһֱ��ʾֱ���뿪��
    public bool hideOnExit = true;     // �뿪����ʱ���� 

    [Header("��������")]
    public bool useFadeEffect = true;  // �Ƿ�ʹ�õ��뵭��Ч�� 
    public float fadeTime = 0.5f;      // ���뵭��ʱ��

    private bool playerInTrigger = false;
    private Coroutine showCoroutine;

    private void Start()
    {
        // ȷ����ʼʱͼƬ�����ص�
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
        // ����Ƿ�����ҽ��� 
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            ShowHint();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // ����Ƿ�������뿪 
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
    /// ��ʾ��ʾͼƬ 
    /// </summary>
    private void ShowHint()
    {
        if (hintSprite == null) return;

        // ����Ѿ�����ʾЭ�������У���ֹͣ�� 
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }

        showCoroutine = StartCoroutine(ShowHintCoroutine());
    }

    private System.Collections.IEnumerator ShowHintCoroutine()
    {
        // ����ͼƬ 
        hintSprite.gameObject.SetActive(true);

        if (useFadeEffect)
        {
            // ����Ч�� 
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

        // �����������ʾʱ�䣬��ʱ���� 
        if (showDuration > 0)
        {
            yield return new WaitForSeconds(showDuration);

            // ֻ��������Ѿ��뿪���߻��������ڵ���Ҫ�Զ�����ʱ���� 
            if (!playerInTrigger || hideOnExit)
            {
                HideHint();
            }
        }
    }

    /// <summary>
    /// ������ʾͼƬ 
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
            // ����Ч�� 
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

        // ����ͼƬ
        hintSprite.gameObject.SetActive(false);

        // ����͸����
        if (useFadeEffect)
        {
            hintSprite.color = new Color(hintSprite.color.r, hintSprite.color.g, hintSprite.color.b, 0);
        }
    }

    // �ڱ༭���п��ӻ���������
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