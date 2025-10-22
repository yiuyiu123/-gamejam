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

    private Rigidbody2D rb;
    private Vector3 velocity = Vector3.zero;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void FixedUpdate()
    {
        // 地面检测（圆形检测优化）
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundedRadius, groundLayer);

        // 修改为JL键控制移动（J=左，L=右）
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) moveInput = -1f; // J键向左 
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;  // L键向右 

        // 平滑移动（保留抗抖动逻辑）
        Vector3 targetVelocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref velocity, movementSmoothing);

        // 跳跃（空格键保持不变）
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundedRadius);
    }
}