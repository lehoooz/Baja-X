using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(PlayerDash), typeof(PlayerLedgeWall))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float coyoteTime = 0.15f;
    [Tooltip("Centang jika hanya gerak Kiri-Kanan (2.5D). Matikan jika ingin gerak 3D bebas (WASD).")]
    [SerializeField] private bool is25DPlatformer = true;

    [Header("Gravity")]
    [SerializeField] private float riseGravity = 45f;
    [SerializeField] private float fallGravity = 45f;
    [SerializeField] private float maxFallSpeed = 30f;

    [Header("Wall Jump")]
    [SerializeField] private float wallJumpForceX = 12f;
    [SerializeField] private float wallJumpLockTime = 0.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 0.25f;


    // ===== Data yang dibaca script lain =====
    public InputSystem_Actions Actions { get; private set; }
    public Rigidbody Rb { get; private set; }
    public Vector2 MoveInput { get; private set; }
    public Vector2 RawMoveInput { get; private set; }
    public Vector2 LastMoveDirection { get; set; } = Vector2.right;
    public float WallJumpLockCounter { get; set; }
    public LayerMask GroundLayer => groundLayer;
    public bool Is25DPlatformer => is25DPlatformer;
    public float Speed => speed;
    public bool isGround { get; private set; }
    public bool isMoving { get; private set; }

    // Diteruskan dari script lain, supaya PlayerAnimation tidak perlu diubah
    public bool isDashing => dash.isDashing;
    public bool isGrabbingLedge => wall.isGrabbingLedge;
    public bool isWallSliding => wall.isWallSliding;

    private float coyoteTimeCounter;
    private PlayerDash dash;
    private PlayerLedgeWall wall;
    private Vector3 knockbackVelocity;
    private float knockbackTimer;

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        Actions = new InputSystem_Actions();
        Rb.useGravity = false;
        dash = GetComponent<PlayerDash>();
        wall = GetComponent<PlayerLedgeWall>();
    }

    private void OnEnable()
    {
        Actions.Player.Enable();
        Actions.Player.Jump.performed += Jumping;
    }

    private void OnDisable()
    {
        Actions.Player.Jump.performed -= Jumping;
        Actions.Player.Disable();
    }

    private void Update()
    {
        RawMoveInput = Actions.Player.Move.ReadValue<Vector2>();

        if (WallJumpLockCounter > 0f)
        {
            WallJumpLockCounter -= Time.deltaTime;
            MoveInput = LastMoveDirection;
            isMoving = true;
        }
        else
        {
            MoveInput = is25DPlatformer ? new Vector2(RawMoveInput.x, 0f) : RawMoveInput;
            isMoving = MoveInput.sqrMagnitude > 0.01f;

            if (Mathf.Abs(RawMoveInput.x) > 0.01f)
                LastMoveDirection = new Vector2(Mathf.Sign(RawMoveInput.x), 0f);
        }

        GroundState();
        wall.UpdateLedge(); // urutan tetap: ground dulu, baru ledge
    }

    private void FixedUpdate()
    {
        if (isDashing || isGrabbingLedge) return;

        Moving();
        ApplyGravity();
    }

    // ---------- Lompat ----------
    private void Jumping(InputAction.CallbackContext ctx)
    {
        if (isDashing) return;

        if (wall.isWallSliding || wall.WallCoyoteCounter > 0f)
        {
            WallJump();
        }
        else if (coyoteTimeCounter > 0f || wall.isGrabbingLedge)
        {
            wall.ReleaseLedge();
            Rb.linearVelocity = new Vector3(Rb.linearVelocity.x, jumpForce, Rb.linearVelocity.z);
            coyoteTimeCounter = 0f;
        }
    }

    private void WallJump()
    {
        wall.ReleaseWall();
        WallJumpLockCounter = wallJumpLockTime;

        // Hadap menjauhi dinding terakhir
        LastMoveDirection = -wall.WallDir;

        Rb.linearVelocity = new Vector3(LastMoveDirection.x * wallJumpForceX, jumpForce, LastMoveDirection.y * wallJumpForceX);
        coyoteTimeCounter = 0f;
    }

    // ---------- Gerak ----------
    private void Moving()
    {
        if (WallJumpLockCounter > 0f) return;

        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            Rb.linearVelocity = new Vector3(knockbackVelocity.x, Rb.linearVelocity.y, knockbackVelocity.z);
            return;
        }

        Rb.linearVelocity = new Vector3(MoveInput.x * speed, Rb.linearVelocity.y, MoveInput.y * speed);
    }

    // ---------- Knockback ----------
    public void ApplyKnockback(Vector3 direction, float force, float popUp = 10f)
    {
        direction.y = 0f;
        knockbackVelocity = direction.normalized * force;
        knockbackTimer = knockbackDuration;

        // Pop ke atas sekali
        Vector3 v = Rb.linearVelocity;
        v.y = popUp;
        Rb.linearVelocity = v;
    }


    // ---------- Gravitasi ----------
    private void ApplyGravity()
    {
        float g = Rb.linearVelocity.y > 0 ? riseGravity : fallGravity;
        Rb.AddForce(Vector3.down * g, ForceMode.Acceleration);

        float currentMaxFall = isWallSliding ? wall.WallSlideSpeed : maxFallSpeed;

        if (Rb.linearVelocity.y < -currentMaxFall)
        {
            Vector3 v = Rb.linearVelocity;
            v.y = -currentMaxFall;
            Rb.linearVelocity = v;
        }
    }

    // ---------- Ground ----------
    private void GroundState()
    {
        if (groundCheckTransform == null) return;

        isGround = Physics.CheckSphere(groundCheckTransform.position, groundCheckRadius, groundLayer);

        if (isGround) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckTransform == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
    }
}