using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements.Experimental;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float coyoteTime = 0.15f;
    [Tooltip("Centang jika hanya gerak Kiri-Kanan (2.5D). Matikan jika ingin gerak 3D bebas (WASD).")]
    [SerializeField] private bool is25DPlatformer = true;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 50f;
    [SerializeField] private float dashDistance = 7f;
    [SerializeField] private float dashCooldown = 1f;


    [Header("Gravity")] // BARU
    [SerializeField] private float riseGravity = 45f;   // gravitasi saat naik
    [SerializeField] private float fallGravity = 45f;   // gravitasi saat turun (makin besar = makin berat)
    [SerializeField] private float maxFallSpeed = 30f;  // batas kecepatan jatuh

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ledge Grab (bisa diubah2 sesuai bentuk)")]
    [SerializeField] private float ledgeForwardOffset = 0.5f;  // Jarak sensor ke depan badan
    [SerializeField] private float wallCheckHeight = 0.5f;     // Tinggi sensor Bawah (Dada)
    [SerializeField] private float ledgeCheckHeight = 1.2f;    // Tinggi sensor Atas (Kepala)
    [SerializeField] private float ledgeCheckRadius = 0.2f;    // Ukuran bola sensor
    [SerializeField] private float ledgeGrabCooldown = 0.25f;  // Jeda agar tidak langsung nempel lagi saat lepas

    [Header("Wall Slide")]
    [SerializeField] private float wallSlideSpeed = 3f;
    [SerializeField] private float wallJumpForceX = 12f;    // BARU: Kekuatan pantul menjauhi dinding
    [SerializeField] private float wallJumpLockTime = 0.5f; // BARU: Durasi kunci belok setelah pantul

    public Vector2 MoveInput { get; private set; }
    public float Speed => speed;
    public bool isGround { get; private set; }
    public bool isMoving { get; private set; }
    public bool isDashing { get; private set; }
    public bool isGrabbingLedge { get; private set; }
    public bool isWallSliding { get; private set; }

    private bool wasGround;
    private bool canDash = true;
    private Vector2 lastMoveDirection = Vector2.right;
    private Vector2 rawMoveInput;
    private float ledgeCooldownCounter;
    private float wallJumpLockCounter; // BARU: Timer pengunci gerak setelah wall jump
    private float coyoteTimeCounter;
    private InputSystem_Actions actions;
    private Rigidbody rb;
    //private int maxJump = 2;

    private void Awake()
    {
        actions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // BARU: matikan gravitasi bawaan
    }

    private void OnEnable()
    {
        actions.Player.Enable();
        actions.Player.Jump.performed += Jumping;
        actions.Player.Dash.performed += Dashing;
    }

    private void OnDisable()
    {
        actions.Player.Jump.performed -= Jumping;
        actions.Player.Dash.performed -= Dashing;
        actions.Player.Disable();
    }

    private void Update()
    {
        rawMoveInput = actions.Player.Move.ReadValue<Vector2>();

        // BARU: Saat baru memantul dari dinding, kunci input & arah hadap menjauhi dinding
        if (wallJumpLockCounter > 0f)
        {
            wallJumpLockCounter -= Time.deltaTime;
            MoveInput = lastMoveDirection;
            isMoving = true;
        }
        else
        {
            MoveInput = is25DPlatformer ? new Vector2(rawMoveInput.x, 0f) : rawMoveInput;
            isMoving = MoveInput.sqrMagnitude > 0.01f;

            if (Mathf.Abs(rawMoveInput.x) > 0.01f)
            {
                lastMoveDirection = new Vector2(Mathf.Sign(rawMoveInput.x), 0f);
            }
        }

        GroundState();
        LedgeState();
    }

    private void FixedUpdate()
    {
        if (isDashing || isGrabbingLedge) return;

        Moving();
        ApplyGravity(); // BARU
    }

    private void Jumping(InputAction.CallbackContext ctx)
    {
        if (isDashing) return;

        // 1. BARU: Jika melompat dari posisi Wall Slide (Pantul Menjauhi Dinding)
        if (isWallSliding)
        {
            isWallSliding = false;
            ledgeCooldownCounter = ledgeGrabCooldown;
            wallJumpLockCounter = wallJumpLockTime; // Aktifkan kunci input 0.2 detik

            // Balikkan arah hadap karakter menjauhi dinding
            lastMoveDirection = -lastMoveDirection;

            // Lontarkan karakter miring ke atas dan menjauhi dinding
            rb.linearVelocity = new Vector3(lastMoveDirection.x * wallJumpForceX, jumpForce, lastMoveDirection.y * wallJumpForceX);
            coyoteTimeCounter = 0f;
        }
        // 2. TETAP SAMA: Lompat biasa dari tanah atau Ledge Grab
        else if (coyoteTimeCounter > 0f || isGrabbingLedge)
        {
            isGrabbingLedge = false;
            ledgeCooldownCounter = ledgeGrabCooldown;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            coyoteTimeCounter = 0f;
        }
    }

    private void Moving()
    {
        if (wallJumpLockCounter > 0f) return; // BARU: Jangan timpa kecepatan lontaran wallJumpForceX
        rb.linearVelocity = new Vector3(MoveInput.x * speed, rb.linearVelocity.y, MoveInput.y * speed);
    }

    private void Dashing(InputAction.CallbackContext ctx)
    {
        if ( canDash && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        // Fase 1: Mulai Dash & Kunci Status
        canDash = false;
        isDashing = true;
        isGrabbingLedge = false;
        isWallSliding = false;     // BARU
        wallJumpLockCounter = 0f;  // BARU

        // Fase 2: Tentukan Arah & Berikan Kecepatan (DIUBAH)
        bool hasRawInput = rawMoveInput.sqrMagnitude > 0.01f;
        Vector2 dashDir = hasRawInput ? rawMoveInput.normalized : lastMoveDirection;

        if (is25DPlatformer)
        {
            // Mode 2.5D: W/S (dashDir.y) dimasukkan ke sumbu Y (Atas/Bawah)
            rb.linearVelocity = new Vector3(dashDir.x * dashSpeed, dashDir.y * dashSpeed, 0f);
        }
        else
        {
            // Mode 3D Bebas: W/S dimasukkan ke sumbu Z (Maju/Mundur)
            rb.linearVelocity = new Vector3(dashDir.x * dashSpeed, 0f, dashDir.y * dashSpeed);
        }

        // Fase 3: Tunggu Durasi Dash Selesai
        float dashDuration = dashDistance / dashSpeed;
        yield return new WaitForSeconds(dashDuration);

        // OPSIONAL: Rem sedikit kecepatan ke atas setelah dash selesai
        rb.linearVelocity = Vector3.zero;

        isDashing = false;

        // Fase 4: Tunggu Cooldown Selesai
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // BARU
    private void ApplyGravity()
    {
        float g = rb.linearVelocity.y > 0 ? riseGravity : fallGravity;
        rb.AddForce(Vector3.down * g, ForceMode.Acceleration);

        float currentMaxFall = isWallSliding ? wallSlideSpeed : maxFallSpeed;

        if (rb.linearVelocity.y < -currentMaxFall)
        {
            Vector3 v = rb.linearVelocity;
            v.y = -currentMaxFall;
            rb.linearVelocity = v;
        }
    }

    private void GroundState()
    {

        if (groundCheckTransform == null) return;

        isGround = Physics.CheckSphere(groundCheckTransform.position, groundCheckRadius, groundLayer);

        //if (isGround && !wasGround)
        //{
        //    maxJump = 2;
        //}

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

    private void LedgeState()
    {
        isWallSliding = false;

        // Kurangi timer cooldown jika masih ada
        if (ledgeCooldownCounter > 0f)
        {
            ledgeCooldownCounter -= Time.deltaTime;
        }

        // Jika sedang menggantung, tekan S / Arah Bawah untuk melepas pegangan
        if (isGrabbingLedge)
        {
            if (isGround || rawMoveInput.y < -0.5f)
            {
                isGrabbingLedge = false;
                ledgeCooldownCounter = ledgeGrabCooldown;
            }
            return;
        }

        // Syarat dasar: Hanya cari ujung platform saat di udara, sedang jatuh, tidak dash, & cooldown habis
        if (isGround || isDashing || rb.linearVelocity.y > 0.1f || ledgeCooldownCounter > 0f) return;

        // Tentukan arah depan berdasarkan arah hadap terakhir (lastMoveDirection)
        Vector3 facingDir = new Vector3(lastMoveDirection.x, 0f, lastMoveDirection.y).normalized;

        // Hitung titik koordinat Sensor Bawah (Dada) dan Sensor Atas (Kepala)
        Vector3 wallCheckPos = transform.position + (facingDir * ledgeForwardOffset) + (Vector3.up * wallCheckHeight);
        Vector3 ledgeCheckPos = transform.position + (facingDir * ledgeForwardOffset) + (Vector3.up * ledgeCheckHeight);

        // Cek apakah sensor menyentuh groundLayer
        bool isTouchingWall = Physics.CheckSphere(wallCheckPos, ledgeCheckRadius, groundLayer);
        bool isTouchingLedgeTop = Physics.CheckSphere(ledgeCheckPos, ledgeCheckRadius, groundLayer);

        // LOGIKA UTAMA: Sensor Bawah KENA dinding, tapi Sensor Atas KOSONG (udara)
        if (isTouchingWall && !isTouchingLedgeTop)
        {
            isGrabbingLedge = true;
            rb.linearVelocity = Vector3.zero; // Hentikan kecepatan jatuh seketika
        }

        else if(isTouchingWall && isMoving)
        {
            isWallSliding = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
        }

        // BARU: Visualisasi 2 Sensor Ledge Grab
        Vector3 facingDir = new Vector3(lastMoveDirection.x, 0f, lastMoveDirection.y).normalized;
        if (facingDir == Vector3.zero) facingDir = Vector3.right;

        Vector3 wallCheckPos = transform.position + (facingDir * ledgeForwardOffset) + (Vector3.up * wallCheckHeight);
        Vector3 ledgeCheckPos = transform.position + (facingDir * ledgeForwardOffset) + (Vector3.up * ledgeCheckHeight);

        Gizmos.color = Color.yellow; // Kuning = Sensor Bawah (Dada)
        Gizmos.DrawWireSphere(wallCheckPos, ledgeCheckRadius);

        Gizmos.color = Color.green;  // Hijau = Sensor Atas (Kepala)
        Gizmos.DrawWireSphere(ledgeCheckPos, ledgeCheckRadius);
    }
}