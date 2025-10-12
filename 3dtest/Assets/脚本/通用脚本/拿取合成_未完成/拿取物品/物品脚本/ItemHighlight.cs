using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Renderer))]
public class ItemHighlight : MonoBehaviour
{
    [Header("�߹�����")]
    public float highlightRange = 5f;                    // �߹ⴥ����Χ
    public float highlightIntensity = 2f;               // �߹�ǿ��
    public Color highlightColor = Color.yellow;         // �߹���ɫ
    public float fadeInTime = 0.3f;                     // ����ʱ��
    public float fadeOutTime = 0.5f;                    // ����ʱ��
    public bool pulseEffect = true;                     // �Ƿ���������Ч��
    public float pulseSpeed = 2f;                       // �����ٶ�

    [Header("�������")]
    public string playerTag = "Player";                 // ��ұ�ǩ
    public LayerMask obstacleMask = -1;                 // �ϰ��������

    [Header("����ѡ��")]
    public bool showDebugGizmos = true;                 // ��ʾ����ͼ��
    public bool enableHighlight = true;                 // �Ƿ����ø߹�

    private Material[] originalMaterials;               // ԭʼ����
    private Material[] highlightMaterials;             // �߹����
    private Renderer itemRenderer;                     // ��Ʒ��Ⱦ��
    private bool isHighlighted = false;                // �Ƿ����ڸ߹�
    private float currentIntensity = 0f;               // ��ǰǿ��
    private Coroutine highlightCoroutine;              // �߹�Э��

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
            Debug.LogWarning($"��Ʒ {gameObject.name} û��Renderer������߹�Ч����������");
            enableHighlight = false;
            return;
        }

        // ����ԭʼ����
        originalMaterials = itemRenderer.materials;

        // �����߹���ʸ���
        highlightMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            highlightMaterials[i] = new Material(originalMaterials[i]);
            highlightMaterials[i].EnableKeyword("_EMISSION");
        }

        Debug.Log($"�߹�ϵͳ��ʼ�����: {gameObject.name}");
    }

    void CheckForPlayers()
    {
        // ���ҷ�Χ�ڵ����
        Collider[] players = Physics.OverlapSphere(transform.position, highlightRange);
        bool playerInRange = false;

        foreach (Collider col in players)
        {
            if (col.CompareTag(playerTag))
            {
                // ����Ƿ����ϰ����赲
                if (!IsObstructed(col.transform))
                {
                    playerInRange = true;
                    break;
                }
            }
        }

        // ��������Ƿ��ڷ�Χ���л��߹�״̬
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
            // ������߻��еĲ�����ң�˵�����ϰ����赲
            return !hit.collider.CompareTag(playerTag);
        }

        return false;
    }

    void StartHighlight()
    {
        isHighlighted = true;

        // ֹ֮ͣǰ��Э��
        if (highlightCoroutine != null)
            StopCoroutine(highlightCoroutine);

        // ��ʼ����Ч��
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

        // Ӧ�ø߹����
        if (currentIntensity > 0)
        {
            itemRenderer.materials = highlightMaterials;

            float finalIntensity = currentIntensity;
            if (pulseEffect && isHighlighted)
            {
                // �������Ч��
                finalIntensity *= 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.2f;
            }

            // ���÷�����ɫ��ǿ��
            Color emissionColor = highlightColor * finalIntensity;
            foreach (Material mat in highlightMaterials)
            {
                mat.SetColor("_EmissionColor", emissionColor);
            }
        }
        else
        {
            // �ָ�ԭʼ����
            itemRenderer.materials = originalMaterials;
        }
    }

    // �����������ֶ����ø߹�
    public void EnableHighlight(bool enable)
    {
        enableHighlight = enable;
        if (!enable)
        {
            StopHighlight();
        }
    }

    // ����������ǿ����ʾ�߹�
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

    // �������������ø߹ⷶΧ
    public void SetHighlightRange(float newRange)
    {
        highlightRange = newRange;
    }

    // �������������ø߹���ɫ
    public void SetHighlightColor(Color newColor)
    {
        highlightColor = newColor;
    }

    void OnDestroy()
    {
        // �������ĸ߹����
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

        // ��ʾ�߹ⷶΧ
        Gizmos.color = isHighlighted ? Color.yellow : Color.gray;
        Gizmos.DrawWireSphere(transform.position, highlightRange);

        // ��ʾ�߹�״̬
        GUIStyle style = new GUIStyle();
        style.normal.textColor = isHighlighted ? Color.yellow : Color.white;
#if UNITY_EDITOR
        string status = $"�߹�: {(isHighlighted ? "����" : "�ر�")}\nǿ��: {currentIntensity:F2}";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, status, style);
#endif
    }
}
