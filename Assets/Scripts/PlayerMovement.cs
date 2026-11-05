using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.15f;
    [Tooltip("Centang jika hanya gerak Kiri-Kanan (2.5D). Matikan jika ingin gerak 3D bebas (WASD).")]
    [SerializeField] private bool is25DPlatformer = true;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    // Menggunakan Vector2 agar reusable untuk 2.5D maupun 3D penuh
    public Vector2 MoveInput { get; private set; }
    public float Speed => speed;
    public bool isGround { get; private set; }
    public bool isMoving { get; private set; }

    private bool wasGround;
    private float coyoteTimeCounter;
    private InputSystem_Actions actions;
    private Rigidbody rb;
    private int maxJump = 2;

    private void Awake()
    {
        actions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        actions.Player.Enable();
        actions.Player.Jump.performed += Jumping;
    }

    private void OnDisable()
    {
        actions.Player.Jump.performed -= Jumping;
        actions.Player.Disable();
    }

    private void Update()
    {
        Vector2 rawInput = actions.Player.Move.ReadValue<Vector2>();

        // Jika 2.5D, kunci sumbu Y input (depan-belakang) menjadi 0
        MoveInput = is25DPlatformer ? new Vector2(rawInput.x, 0f) : rawInput;

        isMoving = MoveInput.sqrMagnitude > 0.01f;

        GroundState();
    }

    private void FixedUpdate()
    {
        Moving();
    }

    private void Jumping(InputAction.CallbackContext ctx)
    {
        if (coyoteTimeCounter > 0f || maxJump > 0)
        {
            // Di Unity 6, gunakan rb.linearVelocity dengan Vector3 untuk 3D
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            maxJump--;
            coyoteTimeCounter = 0f;
        }
    }

    private void Moving()
    {
        // Menggerakkan sumbu X (kiri-kanan) dan sumbu Z (maju-mundur)
        rb.linearVelocity = new Vector3(MoveInput.x * speed, rb.linearVelocity.y, MoveInput.y * speed);
    }

    private void GroundState()
    {
        if (groundCheckTransform == null) return;

        // Versi 3D dari Physics2D.OverlapCircle
        isGround = Physics.CheckSphere(groundCheckTransform.position, groundCheckRadius, groundLayer);

        if (isGround && !wasGround)
        {
            maxJump = 2;
        }

        if (isGround)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        wasGround = isGround;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckTransform == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
    }
}