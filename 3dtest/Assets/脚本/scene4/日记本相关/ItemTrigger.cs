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
    [SerializeField] private GameObject player2Effect; // Player2的特效对象
    [SerializeField] private float effectDisplayTime = 2f; // 特效显示时间
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

        // 初始化特效状态
        InitializeEffect();
    }

    // 初始化特效状态
    private void InitializeEffect()
    {
        if (player2Effect != null)
        {
            player2Effect.SetActive(false);
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

        // 显示Player2特效（物品正确时）
        ShowPlayer2Effect();

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
    /// 显示Player2特效
    /// </summary>
    private void ShowPlayer2Effect()
    {
        if (player2Effect != null)
        {
            // 如果已经有特效在显示，先停止之前的协程
            if (_effectCoroutine != null)
            {
                StopCoroutine(_effectCoroutine);
            }

            // 激活特效
            player2Effect.SetActive(true);

            // 启动隐藏特效的协程
            _effectCoroutine = StartCoroutine(HideEffectAfterDelay());
        }
        else
        {
            if (debugMode) Debug.LogWarning("Player2特效未设置，无法显示特效", this);
        }
    }

    /// <summary>
    /// 延迟隐藏特效
    /// </summary>
    private IEnumerator HideEffectAfterDelay()
    {
        yield return new WaitForSeconds(effectDisplayTime);

        if (player2Effect != null)
        {
            player2Effect.SetActive(false);
        }

        _effectCoroutine = null;
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

        // 重置特效状态
        InitializeEffect();

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
    /// 测试特效显示（用于调试）
    /// </summary>
    [ContextMenu("测试特效显示")]
    public void TestEffect()
    {
        ShowPlayer2Effect();
        if (debugMode) Debug.Log("测试特效显示", this);
    }

    /// <summary>
    /// 设置Player2特效对象
    /// </summary>
    public void SetPlayer2Effect(GameObject effect)
    {
        player2Effect = effect;
        InitializeEffect();
    }

    /// <summary>
    /// 设置特效显示时间
    /// </summary>
    public void SetEffectDisplayTime(float time)
    {
        effectDisplayTime = time;
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