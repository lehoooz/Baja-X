using UnityEngine;

public class PlayerLedgeWall : MonoBehaviour
{
    [Header("Ledge Grab (bisa diubah2 sesuai bentuk)")]
    [SerializeField] private float ledgeForwardOffset = 0.14f;
    [SerializeField] private float wallCheckHeight = 2.46f;
    [SerializeField] private float ledgeCheckHeight = 2.89f;
    [SerializeField] private float ledgeCheckRadius = 0.2f;
    [SerializeField] private float ledgeGrabCooldown = 0.25f;

    [Header("Wall Slide")]
    [SerializeField] private float wallSlideSpeed = 3f;
    [SerializeField] private float wallCoyoteTime = 0.15f;

    public bool isGrabbingLedge { get; private set; }
    public bool isWallSliding { get; private set; }
    public Vector2 WallDir { get; private set; }
    public float WallCoyoteCounter { get; private set; }
    public float WallSlideSpeed => wallSlideSpeed;

    private float ledgeCooldownCounter;
    private PlayerMovement movement;
    private PlayerDash dash;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        dash = GetComponent<PlayerDash>();
    }

    // ---------- Dipanggil script lain ----------
    public void ReleaseLedge()
    {
        isGrabbingLedge = false;
        ledgeCooldownCounter = ledgeGrabCooldown;
    }

    public void ReleaseWall()
    {
        isWallSliding = false;
        WallCoyoteCounter = 0f;
        ledgeCooldownCounter = ledgeGrabCooldown;
    }

    public void ClearAll()
    {
        isGrabbingLedge = false;
        isWallSliding = false;
        WallCoyoteCounter = 0f;
    }

    // ---------- Dipanggil PlayerMovement.Update ----------
    public void UpdateLedge()
    {
        isWallSliding = false;

        if (movement.isGround) WallCoyoteCounter = 0f;
        else if (WallCoyoteCounter > 0f) WallCoyoteCounter -= Time.deltaTime;

        if (ledgeCooldownCounter > 0f) ledgeCooldownCounter -= Time.deltaTime;

        // Sedang menggantung: tekan S / bawah untuk melepas
        if (isGrabbingLedge)
        {
            if (movement.isGround || movement.RawMoveInput.y < -0.5f)
            {
                ReleaseLedge();
                return;
            }
            // kalau dinding/ledge sudah tidak ada lagi (platform bergerak / hancur), lepas supaya tidak melayang di udara
            Vector3 hangCheckPos = transform.position + Vector3.up * wallCheckHeight;
            bool hangTouchingWall = Physics.CheckSphere(hangCheckPos, ledgeCheckRadius + ledgeForwardOffset, movement.GroundLayer);
            if (!hangTouchingWall)
            {
                ReleaseLedge();
                return;
            }
            return;
        }

        Rigidbody rb = movement.Rb;
        if (movement.isGround || dash.isDashing || rb.linearVelocity.y > 0.1f || ledgeCooldownCounter > 0f) return;

        Vector2 dir = movement.LastMoveDirection;
        Vector3 facingDir = new Vector3(dir.x, 0f, dir.y).normalized;

        Vector3 wallCheckPos = transform.position + facingDir * ledgeForwardOffset + Vector3.up * wallCheckHeight;
        Vector3 ledgeCheckPos = transform.position + facingDir * ledgeForwardOffset + Vector3.up * ledgeCheckHeight;

        bool isTouchingWall = Physics.CheckSphere(wallCheckPos, ledgeCheckRadius, movement.GroundLayer);
        bool isTouchingLedgeTop = Physics.CheckSphere(ledgeCheckPos, ledgeCheckRadius, movement.GroundLayer);

        // Sensor bawah kena, sensor atas kosong = ujung platform
        if (isTouchingWall && !isTouchingLedgeTop)
        {
            isGrabbingLedge = true;
            WallCoyoteCounter = 0f;
            rb.linearVelocity = Vector3.zero;
        }
        else if (isTouchingWall && movement.isMoving)
        {
            isWallSliding = true;
            WallCoyoteCounter = wallCoyoteTime;
            WallDir = dir;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 dir = Vector2.right;
        if (movement != null) dir = movement.LastMoveDirection;

        Vector3 facingDir = new Vector3(dir.x, 0f, dir.y).normalized;
        if (facingDir == Vector3.zero) facingDir = Vector3.right;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + facingDir * ledgeForwardOffset + Vector3.up * wallCheckHeight, ledgeCheckRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + facingDir * ledgeForwardOffset + Vector3.up * ledgeCheckHeight, ledgeCheckRadius);
    }
}