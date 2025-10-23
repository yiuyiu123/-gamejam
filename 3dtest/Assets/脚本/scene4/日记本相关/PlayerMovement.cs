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

    [Header("���������")]
    [SerializeField] private Camera playerCamera; // ��ק������������� 

    private Rigidbody2D rb;
    private Vector3 velocity = Vector3.zero;
    private bool isGrounded;
    private bool isFacingRight = true; // Ĭ���泯�� 

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // ȷ����ʼʱ�ű�״̬�������һ�� 
        if (playerCamera != null)
        {
            this.enabled = playerCamera.enabled;
        }
    }

    private void FixedUpdate()
    {
        // �������������ã�ֱ�ӷ��� 
        if (playerCamera != null && !playerCamera.enabled)
        {
            rb.velocity = Vector2.zero;  // ֹͣ�ƶ� 
            return;
        }

        // ������ 
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundedRadius, groundLayer);

        // �޸�ΪAD�������ƶ���A=��D=�ң�
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) moveInput = -1f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;

        // ƽ���ƶ� 
        Vector3 targetVelocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref velocity, movementSmoothing);

        // �����ƶ�����ת��ɫ 
        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }

        // ��Ծ 
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
    }

    // ��ɫ��ת���� 
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // �������״̬�ı�ʱ���� 
    private void OnCameraStateChanged(bool isEnabled)
    {
        this.enabled = isEnabled;
    }

    // ע��/ȡ��ע�������״̬�仯�¼� 
    private void OnEnable()
    {
        if (playerCamera != null)
        {
            // ���������������Զ����¼���ʵ�ʿ�����Ҫ������ʽ���� 
            // ����ʹ��UnityEvent��������ű����ֶ����� 
        }
    }

    private void OnDisable()
    {
        // �����¼�ע�� 
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundedRadius);
    }
}