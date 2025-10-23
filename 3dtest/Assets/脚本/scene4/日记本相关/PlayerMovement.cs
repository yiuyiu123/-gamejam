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
    [SerializeField] private Camera playerCamera; // 拖拽主摄像机到这里 

    private Rigidbody2D rb;
    private Vector3 velocity = Vector3.zero;
    private bool isGrounded;
    private bool isFacingRight = true; // 默认面朝右 

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
    }

    private void FixedUpdate()
    {
        // 如果摄像机被禁用，直接返回 
        if (playerCamera != null && !playerCamera.enabled)
        {
            rb.velocity = Vector2.zero;  // 停止移动 
            return;
        }

        // 地面检测 
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundedRadius, groundLayer);

        // 修改为AD键控制移动（A=左，D=右）
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) moveInput = -1f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;

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

        // 跳跃 
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
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
    }

    // 注册/取消注册摄像机状态变化事件 
    private void OnEnable()
    {
        if (playerCamera != null)
        {
            // 这里假设摄像机有自定义事件，实际可能需要其他方式监听 
            // 或者使用UnityEvent在摄像机脚本中手动触发 
        }
    }

    private void OnDisable()
    {
        // 清理事件注册 
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundedRadius);
    }
}