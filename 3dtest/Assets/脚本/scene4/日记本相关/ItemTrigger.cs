using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// ��Ʒ����ϵͳ��2D/3D˫ģʽ֧�֣�
/// �����£�2025.10.20 
/// </summary>
public class ItemTrigger : MonoBehaviour
{
    [System.Serializable]
    public class TriggerRule
    {
        [Tooltip("��Ҫƥ�����Ʒ��ǩ�����ִ�Сд��")]
        public string requiredTag = "Untagged";

        [Tooltip("������Ҫɾ����Ŀ������")]
        public GameObject targetObject;

        [Tooltip("�ӳ�ִ��ʱ�䣨�룩")]
        public float delayTime = 0.5f;

        [Tooltip("����ʱ���ŵ���Ƶ")]
        public AudioClip soundEffect;

        [Tooltip("�Զ��崥���¼�")]
        public UnityEvent onTriggered;
    }

    [Header("��������")]
    [SerializeField] private List<TriggerRule> triggerRules = new List<TriggerRule>();
    [SerializeField] private bool debugMode = true;

    [Header("�������")]
    [SerializeField] private float contactOffset = 0.01f;
    [SerializeField] private LayerMask detectableLayers = ~0;

    [Header("��������")]
    [SerializeField] private bool canTriggerMultipleTimes = true;
    [SerializeField] private float cooldownTime = 0f;

    [Header("�����ã����Ŵ���")]
    public int OpenDoorNumber = 0;
    public event Action OpenFirstDoor;
    public event Action OpenSecondDoor;

    private Collider _collider;
    private Collider2D _collider2D;
    private bool _isInCooldown = false;
    private HashSet<GameObject> _processedObjects = new HashSet<GameObject>();

    private void Awake()
    {
        TryGetComponent(out _collider);
        TryGetComponent(out _collider2D);

        // �Զ������������ 
        if (_collider != null)
        {
            // ����3D��ײ������contactOffset 
            if (_collider is BoxCollider boxCollider)
                boxCollider.contactOffset = contactOffset;
            else if (_collider is SphereCollider sphereCollider)
                sphereCollider.contactOffset = contactOffset;
            else if (_collider is CapsuleCollider capsuleCollider)
                capsuleCollider.contactOffset = contactOffset;
            else if (_collider is MeshCollider meshCollider)
                meshCollider.contactOffset = contactOffset;
        }
    }

    // �޸���ȡ��ע��3D��ײ���
    private void OnTriggerEnter(Collider other) => ProcessTrigger(other.gameObject);
    private void OnTriggerEnter2D(Collider2D other) => ProcessTrigger(other.gameObject);

    private void ProcessTrigger(GameObject incomingObject)
    {
        // �����ȴ״̬ 
        if (_isInCooldown)
        {
            if (debugMode) Debug.Log("������������ȴ״̬�����Դ���", this);
            return;
        }

        // ����ظ����� 
        if (!canTriggerMultipleTimes && _processedObjects.Contains(incomingObject))
        {
            if (debugMode) Debug.Log($"��Ʒ [{incomingObject.name}]   �Ѵ������������ظ�����", this);
            return;
        }

        if (!IsObjectValid(incomingObject)) return;

        foreach (var rule in triggerRules)
        {
            if (incomingObject.CompareTag(rule.requiredTag))
            {
                if (debugMode)
                {
                    Debug.Log($"��������ƥ��: ��Ʒ[{incomingObject.name}]    " +
                             $"��ǩ[{rule.requiredTag}]", this);
                }

                // ִ�д����߼� 
                Debug.Log("11111111111111");
                ExecuteTriggerRule(rule);

                // ��¼�Ѵ����Ķ��� 
                if (!canTriggerMultipleTimes)
                {
                    _processedObjects.Add(incomingObject);
                }

                // ������ȴ 
                if (cooldownTime > 0)
                {
                    StartCoroutine(CooldownCoroutine());
                }

                break;
            }
        }
    }

    private bool IsObjectValid(GameObject obj)
    {
        // ���㼶 
        if (((1 << obj.layer) & detectableLayers) == 0)
        {
            if (debugMode) Debug.Log($"���Է�ָ���㼶����: {obj.name}", this);
            return false;
        }

        // ����ǩ�Ƿ�ΪĬ��ֵ����ѡ��飩
        bool hasValidTag = false;
        foreach (var rule in triggerRules)
        {
            if (!string.IsNullOrEmpty(rule.requiredTag) && rule.requiredTag != "Untagged")
            {
                hasValidTag = true;
                break;
            }
        }

        if (!hasValidTag && debugMode)
        {
            Debug.LogWarning("���д�������ı�ǩ����Ĭ��ֵ'Untagged'�������޷���ȷ����", this);
        }

        return true;
    }

    private void ExecuteTriggerRule(TriggerRule rule)
    {
        // ������Ч 
        if (rule.soundEffect != null)
        {
            AudioSource.PlayClipAtPoint(rule.soundEffect, transform.position);
        }

        // ִ���Զ����¼� 
        rule.onTriggered?.Invoke();

        // ����Ŀ������ 
        if (rule.targetObject != null)
        {
            if (debugMode)
            {
                Debug.Log($"����ɾ��Ŀ��: {rule.targetObject.name}    " +
                         $"(�ӳ�{rule.delayTime}   ��)", this);
            }

            if (rule.delayTime > 0)
            {
                StartCoroutine(DestroyWithDelay(rule.targetObject, rule.delayTime));
            }
            else
            {
                Destroy(rule.targetObject);
            }
        }
        //�����ã���¼���Ŵ��� 
        OpenDoorNumber++;
        if (OpenDoorNumber == 1)
        {
            OpenFirstDoor?.Invoke();
        }
        else if (OpenDoorNumber == 2)
        {
            OpenSecondDoor?.Invoke();
        }
    }

    private IEnumerator DestroyWithDelay(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (target != null)
        {
            Destroy(target);
        }
    }

    private IEnumerator CooldownCoroutine()
    {
        _isInCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        _isInCooldown = false;

        if (debugMode) Debug.Log("��������ȴ����", this);
    }

    // �������������ô�����״̬ 
    public void ResetTrigger()
    {
        _processedObjects.Clear();
        _isInCooldown = false;
        StopAllCoroutines();

        if (debugMode) Debug.Log("������״̬������", this);
    }

    // �����������ֶ�����ָ������ 
    public void ManuallyTriggerRule(int ruleIndex)
    {
        if (ruleIndex >= 0 && ruleIndex < triggerRules.Count)
        {
            ExecuteTriggerRule(triggerRules[ruleIndex]);
        }
        else if (debugMode)
        {
            Debug.LogError($"��Ч�Ĺ�������: {ruleIndex}", this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);

        if (_collider != null)
        {
            Gizmos.DrawCube(transform.position, _collider.bounds.size);
        }
        else if (_collider2D != null)
        {
            Gizmos.DrawCube(transform.position, _collider2D.bounds.size);
        }
    }
}