using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField][Range(0, 0.3f)] private float movementSmoothing = 0.05f;

    [Header("地面检测")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    private const float groundedRadius = 0.2f;

    [Header("摄像机控制")]
    [SerializeField] private Camera playerCamera;

    [Header("动画控制")]
    [SerializeField] private Animator animator;
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private string runAnimationName = "Run";

    [Header("移动音效设置")]
    [SerializeField] private string moveSoundGroupID = "玩家12D脚步声";
    [SerializeField] private float moveSoundInterval = 0.5f;
    [SerializeField] private float moveSoundFadeInTime = 0.1f;

    private Rigidbody2D rb;
    private Vector3 velocity = Vector3.zero;
    private bool isGrounded;
    private bool isFacingRight = true;
    private string currentAnimation;
    private float moveInput;

    // 移动音效相关变量
    private bool isMoving = false;
    private float soundTimer = 0f;
    private Vector3 lastPosition;
    private bool actuallyMoving = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 确保初始时脚本状态与摄像机一致 
        if (playerCamera != null)
        {
            this.enabled = playerCamera.enabled;
        }

        // 自动获取Animator组件（如果未手动赋值）
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // 初始化位置记录
        lastPosition = transform.position;
    }

    private void Start()
    {
        // 初始播放待机动画 
        PlayAnimation(idleAnimationName);
    }

    private void Update()
    {
        // 在Update中处理输入检测 
        moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) moveInput = -1f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;

        // 根据移动状态播放动画 
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            PlayAnimation(runAnimationName);
            isMoving = true;
        }
        else
        {
            PlayAnimation(idleAnimationName);
            isMoving = false;
        }

        // 跳跃检测（也在Update中）
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }

        // 更新移动音效
        UpdateMovementSound();
    }

    private void FixedUpdate()
    {
        // 如果摄像机被禁用，直接返回 
        if (playerCamera != null && !playerCamera.enabled)
        {
            rb.velocity = Vector2.zero;    // 停止移动 
            actuallyMoving = false;
            return;
        }

        // 地面检测 
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundedRadius, groundLayer);

        // 检测实际移动（基于位置变化）
        Vector3 currentPos = transform.position;
        float distance = Vector3.Distance(currentPos, lastPosition);
        actuallyMoving = distance > 0.01f && isGrounded && isMoving;
        lastPosition = currentPos;

        // 平滑移动 
        Vector3 targetVelocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref velocity, movementSmoothing);

        // 根据移动方向翻转角色 
        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void UpdateMovementSound()
    {
        // 更新音效计时器
        if (soundTimer > 0f)
        {
            soundTimer -= Time.deltaTime;
        }

        // 播放移动音效的条件：
        // 1. 脚本启用（在2D场景中）
        // 2. 实际在移动
        // 3. 音效计时器归零
        if (this.enabled && actuallyMoving && soundTimer <= 0f)
        {
            PlayMoveSound();
            soundTimer = moveSoundInterval;
        }
    }

    private void PlayMoveSound()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(moveSoundGroupID))
        {
            // 2D场景专用音效：使用左声道，2D音效模式
            AudioManager.Instance.PlayOneShot(
                moveSoundGroupID,
                -1,                    // 随机选择音效
                true,                  // 淡入
                moveSoundFadeInTime,
                false,                 // 不淡出
                0f,
                true,                  // 玩家1 - 左声道
                false                  // 2D音效，不受空间影响
            );

#if UNITY_EDITOR
            Debug.Log($"播放2D移动音效: {moveSoundGroupID}");
#endif
        }
        else
        {
#if UNITY_EDITOR
            if (AudioManager.Instance == null)
                Debug.LogWarning("AudioManager实例不存在");
            if (string.IsNullOrEmpty(moveSoundGroupID))
                Debug.LogWarning("移动音效组ID未设置");
#endif
        }
    }

    // 播放动画方法 
    private void PlayAnimation(string animationName)
    {
        if (animator != null && currentAnimation != animationName)
        {
            // 使用CrossFade确保动画平滑过渡并持续播放 
            animator.CrossFade(animationName, 0.1f);
            currentAnimation = animationName;
        }
    }

    // 角色翻转方法 
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // 当摄像机状态改变时调用 
    private void OnCameraStateChanged(bool isEnabled)
    {
        this.enabled = isEnabled;

        // 摄像机状态改变时重置移动状态
        if (!isEnabled)
        {
            actuallyMoving = false;
            isMoving = false;
        }
    }

    // 注册/取消注册摄像机状态变化事件 
    private void OnEnable()
    {
        // 启用时重置位置记录
        lastPosition = transform.position;

        if (playerCamera != null)
        {
            // 这里假设摄像机有自定义事件，实际可能需要其他方式监听 
            // 或者使用UnityEvent在摄像机脚本中手动触发 
        }
    }

    private void OnDisable()
    {
        // 禁用时停止移动状态
        actuallyMoving = false;
        isMoving = false;

        // 清理事件注册 
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundedRadius);
    }

    /// <summary>
    /// 设置移动音效组ID（可选，用于动态切换音效）
    /// </summary>
    public void SetMoveSoundGroupID(string newGroupID)
    {
        moveSoundGroupID = newGroupID;
    }

    /// <summary>
    /// 设置移动锁定状态（供外部调用）
    /// </summary>
    public void SetMovementLock(bool locked)
    {
        // 如果锁定移动，停止移动状态
        if (locked)
        {
            actuallyMoving = false;
            isMoving = false;
        }
    }
}