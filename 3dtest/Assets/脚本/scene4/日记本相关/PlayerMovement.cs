using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("�ƶ�����")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField][Range(0, 0.3f)] private float movementSmoothing = 0.05f;

    [Header("������")]
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
        // �����⣨Բ�μ���Ż���
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundedRadius, groundLayer);

        // �޸�ΪJL�������ƶ���J=��L=�ң�
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) moveInput = -1f; // J������ 
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;  // L������ 

        // ƽ���ƶ��������������߼���
        Vector3 targetVelocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref velocity, movementSmoothing);

        // ��Ծ���ո�����ֲ��䣩
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