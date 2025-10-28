using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 物品触发系统（2D/3D双模式支持）
/// 最后更新：2025.10.20 
/// </summary>
public class ItemTrigger : MonoBehaviour
{
    [System.Serializable]
    public class TriggerRule
    {
        [Tooltip("需要匹配的物品标签（区分大小写）")]
        public string requiredTag = "Untagged";

        [Tooltip("触发后要删除的目标物体")]
        public GameObject targetObject;

        [Tooltip("延迟执行时间（秒）")]
        public float delayTime = 0.5f;

        [Tooltip("触发时播放的音频")]
        public AudioClip soundEffect;

        [Tooltip("自定义触发事件")]
        public UnityEvent onTriggered;
    }

    [Header("核心配置")]
    [SerializeField] private List<TriggerRule> triggerRules = new List<TriggerRule>();
    [SerializeField] private bool debugMode = true;

    [Header("物理参数")]
    [SerializeField] private float contactOffset = 0.01f;
    [SerializeField] private LayerMask detectableLayers = ~0;

    [Header("触发限制")]
    [SerializeField] private bool canTriggerMultipleTimes = true;
    [SerializeField] private float cooldownTime = 0f;

    [Header("张奕忻：开门次数")]
    public int OpenDoorNumber = 0;
    public event Action OpenFirstDoor;
    public event Action OpenSecondDoor;

    [Header("特效系统 - Player2专用")]
    [SerializeField] private ParticleSystem player2Effect; // 粒子系统特效
    [SerializeField] private float effectDuration = 2f; // 特效持续时间
    [SerializeField] private string correctItemSound = "正确物品音效"; // 正确物品的音效组ID

    private Collider _collider;
    private Collider2D _collider2D;
    private bool _isInCooldown = false;
    private HashSet<GameObject> _processedObjects = new HashSet<GameObject>();
    private Coroutine _effectCoroutine; // 特效协程引用

    private void Awake()
    {
        TryGetComponent(out _collider);
        TryGetComponent(out _collider2D);

        // 自动配置物理参数 
        if (_collider != null)
        {
            // 对于3D碰撞器设置contactOffset 
            if (_collider is BoxCollider boxCollider)
                boxCollider.contactOffset = contactOffset;
            else if (_collider is SphereCollider sphereCollider)
                sphereCollider.contactOffset = contactOffset;
            else if (_collider is CapsuleCollider capsuleCollider)
                capsuleCollider.contactOffset = contactOffset;
            else if (_collider is MeshCollider meshCollider)
                meshCollider.contactOffset = contactOffset;
        }

        // 确保特效对象是激活状态
        if (player2Effect != null)
        {
            player2Effect.gameObject.SetActive(true);
        }
    }

    // 修复：取消注释3D碰撞检测
    private void OnTriggerEnter(Collider other) => ProcessTrigger(other.gameObject);
    private void OnTriggerEnter2D(Collider2D other) => ProcessTrigger(other.gameObject);

    private void ProcessTrigger(GameObject incomingObject)
    {
        // 检查冷却状态 
        if (_isInCooldown)
        {
            if (debugMode) Debug.Log("触发器处于冷却状态，忽略触发", this);
            return;
        }

        // 检查重复触发 
        if (!canTriggerMultipleTimes && _processedObjects.Contains(incomingObject))
        {
            if (debugMode) Debug.Log($"物品 [{incomingObject.name}]   已触发过，忽略重复触发", this);
            return;
        }

        if (!IsObjectValid(incomingObject)) return;

        bool ruleMatched = false;

        foreach (var rule in triggerRules)
        {
            if (incomingObject.CompareTag(rule.requiredTag))
            {
                if (debugMode)
                {
                    Debug.Log($"触发规则匹配: 物品[{incomingObject.name}]" + $"标签[{rule.requiredTag}]", this);
                }

                // 执行触发逻辑 
                Debug.Log("11111111111111");
                ExecuteTriggerRule(rule);
                ruleMatched = true;

                // 记录已触发的对象 
                if (!canTriggerMultipleTimes)
                {
                    _processedObjects.Add(incomingObject);
                }

                // 启动冷却 
                if (cooldownTime > 0)
                {
                    StartCoroutine(CooldownCoroutine());
                }

                break;
            }
        }

        // 如果没有匹配任何规则，说明物品不正确
        if (!ruleMatched)
        {
            if (debugMode) Debug.Log($"物品 [{incomingObject.name}] 不匹配任何规则，不触发特效", this);
        }
    }

    private bool IsObjectValid(GameObject obj)
    {
        // 检查层级 
        if (((1 << obj.layer) & detectableLayers) == 0)
        {
            if (debugMode) Debug.Log($"忽略非指定层级物体: {obj.name}", this);
            return false;
        }

        // 检查标签是否为默认值（可选检查）
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
            Debug.LogWarning("所有触发规则的标签都是默认值'Untagged'，可能无法正确触发", this);
        }

        return true;
    }

    private void ExecuteTriggerRule(TriggerRule rule)
    {
        // 播放音效 
        if (rule.soundEffect != null)
        {
            AudioSource.PlayClipAtPoint(rule.soundEffect, transform.position);
        }

        // 播放正确物品音效（通过AudioManager）
        if (!string.IsNullOrEmpty(correctItemSound) && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot(
                correctItemSound,
                -1,
                false, 0f,
                false, 0f,
                false,  // Player2使用右声道
                true    // 3D音效
            );
        }

        // 执行自定义事件 
        rule.onTriggered?.Invoke();

        // 销毁目标物体 
        if (rule.targetObject != null)
        {
            if (debugMode)
            {
                Debug.Log($"正在删除目标: {rule.targetObject.name}    " +
                         $"(延迟{rule.delayTime}   秒)", this);
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

        // 播放Player2特效（物品正确时）
        PlayPlayer2Effect();

        //张奕忻：记录开门次数 
        OpenDoorNumber++;
        if (OpenDoorNumber == 1)
        {
            OpenFirstDoor?.Invoke();
        }
        else if (OpenDoorNumber >= 2)
        {
            Debug.Log($"销毁第二扇门一次");
            OpenSecondDoor?.Invoke();
        }
    }

    /// <summary>
    /// 播放Player2特效
    /// </summary>
    private void PlayPlayer2Effect()
    {
        if (player2Effect != null)
        {
            // 如果已经有特效在播放，先停止之前的协程
            if (_effectCoroutine != null)
            {
                StopCoroutine(_effectCoroutine);
            }

            // 直接播放粒子系统特效
            player2Effect.Play();

            // 启动特效消失协程
            _effectCoroutine = StartCoroutine(StopEffectAfterDelay());

            if (debugMode) Debug.Log("播放Player2特效", this);
        }
        else
        {
            if (debugMode) Debug.LogWarning("Player2特效未设置，无法播放特效", this);
        }
    }

    /// <summary>
    /// 延迟停止特效
    /// </summary>
    private IEnumerator StopEffectAfterDelay()
    {
        yield return new WaitForSeconds(effectDuration);

        if (player2Effect != null)
        {
            player2Effect.Stop();
        }

        _effectCoroutine = null;

        if (debugMode) Debug.Log("Player2特效已停止", this);
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

        if (debugMode) Debug.Log("触发器冷却结束", this);
    }

    // 公共方法：重置触发器状态 
    public void ResetTrigger()
    {
        _processedObjects.Clear();
        _isInCooldown = false;
        StopAllCoroutines();

        // 停止特效
        if (player2Effect != null)
        {
            player2Effect.Stop();
        }
        _effectCoroutine = null;

        if (debugMode) Debug.Log("触发器状态已重置", this);
    }

    // 公共方法：手动触发指定规则 
    public void ManuallyTriggerRule(int ruleIndex)
    {
        if (ruleIndex >= 0 && ruleIndex < triggerRules.Count)
        {
            ExecuteTriggerRule(triggerRules[ruleIndex]);
        }
        else if (debugMode)
        {
            Debug.LogError($"无效的规则索引: {ruleIndex}", this);
        }
    }

    /// <summary>
    /// 测试特效播放（用于调试）
    /// </summary>
    [ContextMenu("测试特效播放")]
    public void TestEffect()
    {
        PlayPlayer2Effect();
        if (debugMode) Debug.Log("测试特效播放", this);
    }

    /// <summary>
    /// 设置Player2特效
    /// </summary>
    public void SetPlayer2Effect(ParticleSystem effect)
    {
        player2Effect = effect;

        // 确保新设置的特效对象是激活状态
        if (player2Effect != null)
        {
            player2Effect.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 设置特效持续时间
    /// </summary>
    public void SetEffectDuration(float duration)
    {
        effectDuration = duration;
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