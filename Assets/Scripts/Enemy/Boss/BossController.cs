using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BossMovement))]
[RequireComponent(typeof(BossCharge))]
[RequireComponent(typeof(BossAttack))]
[RequireComponent(typeof(BossAnimation))]
public class BossController : EnemyBase
{
    private enum State { Approach, Telegraph, Charge, Slam, Shoot, Recovery }
    private enum Phase { Charge, Shoot, Slam }
    private State state = State.Telegraph;
    private Phase phase = Phase.Charge;

    [Header("Mendekati Player")]
    [SerializeField] private float walkSpeed = 3f; // kecepatan jalan mendekati player (fase slam)
    [SerializeField] private float slamAttackRange = 3.5f; // jarak slam

    [Header("Fase Serangan")]
    [SerializeField] private int chargeAttackCount = 2; // jumlah charge di fase charge
    [SerializeField] private int shootAttackCount = 3;  // jumlah tembakan di fase tembak
    [SerializeField] private int slamAttackCount = 2;   // jumlah slam di fase slam

    [Header("Telegraph")]
    [SerializeField] private float telegraphTime = 0.5f;
    

    [Header("Charge")]
    [SerializeField] private float chargeSpeed = 14f;
    [SerializeField] private float chargeTime = 5f; // batas aman saja, charge berhenti kalau kena tembok / ujung platform
    [SerializeField] private LayerMask groundLayer;

    [Header("Ground Slam")]
    [SerializeField] private float slamRadius = 2.5f;
    [SerializeField] private int slamDamage = 2;
    [SerializeField] private float slamDuration = 0.4f;
    [SerializeField] private float slamKnockback = 10f;
    [SerializeField] private float slamKnockbackPopUp = 5f;

    [Header("Projectile Besar")]
    [SerializeField] private Projectile bossProjectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Recovery")]
    [SerializeField] private float recoveryTime = 0.8f;

    [Header("Kematian")]
    [SerializeField] private float deathDelay = 1.0f; // jeda sebelum boss hilang, samakan dengan panjang clip Died

    private Transform player;
    private Rigidbody rb;
    private BossMovement movement;
    private BossCharge charge;
    private BossAttack attack;
    private BossAnimation bossAnim;
    private bool isDead;

    private float stateTimer;
    private State nextAttack = State.Charge;
    private Vector2 chargeDir = Vector2.right;
    private bool isActive;
    private int phaseAttackDone; // jumlah serangan yang sudah dilakukan di fase ini

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        movement = GetComponent<BossMovement>();
        charge = GetComponent<BossCharge>();
        attack = GetComponent<BossAttack>();
        bossAnim = GetComponent<BossAnimation>();

        if (movement == null) Debug.LogError("[Boss] BossMovement belum dipasang di Boss!");
        if (charge == null) Debug.LogError("[Boss] BossCharge belum dipasang di Boss!");
        if (attack == null) Debug.LogError("[Boss] BossAttack belum dipasang di Boss!");
        if (bossAnim == null) Debug.LogError("[Boss] BossAnimation belum dipasang di Boss!");
    }

    private void Start()
    {
        player = GameManager.Instance.Player;
        movement.Setup(walkSpeed);
        charge.Setup(chargeSpeed, groundLayer);
        attack.Setup(slamRadius, slamDamage, slamKnockback, slamKnockbackPopUp, bossProjectilePrefab, firePoint);
        phase = Phase.Charge;
        phaseAttackDone = 0;
        isActive = false; // boss diam dulu, tunggu player masuk trigger
        bossAnim?.PlayWalk(false);
    }
    public void Activate()
    {
        if (isDead || isActive) return;
        isActive = true;
        phase = Phase.Charge;
        phaseAttackDone = 0;
        Debug.Log("[Boss] Aktif, player masuk trigger");
        BeginTelegraph();
    }

    public void Deactivate()
    {
        if (isDead || !isActive) return;
        isActive = false;
        rb.linearVelocity = Vector3.zero;
        bossAnim?.PlayWalk(false);
        bossAnim?.PlayRecover();
        state = State.Recovery;
        stateTimer = 0f;
        Debug.Log("[Boss] Nonaktif, player keluar trigger");
    }

    private void FixedUpdate()
    {
        if (player == null || !isActive) return;

        // Boss berat: peluru tidak mendorongnya, jadi timer knockback
        // cuma dihabiskan saja — state machine tetap jalan walau ditembak
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
        }

        stateTimer -= Time.fixedDeltaTime;

        switch (state)
        {
            case State.Approach: ApproachUpdate(); break;
            case State.Telegraph: TelegraphUpdate(); break;
            case State.Charge: ChargeUpdate(); break;
            case State.Slam: SlamUpdate(); break;
            case State.Shoot: ShootUpdate(); break;
            case State.Recovery: RecoveryUpdate(); break;
        }
    }

    private void BeginTelegraph()
    {
        state = State.Telegraph;
        stateTimer = telegraphTime;
        bossAnim?.PlayWalk(false);
        bossAnim?.PlayTelegraph();
        Debug.Log("[Boss] Telegraph -> serangan: " + nextAttack + " (fase " + phase + ", " + (phaseAttackDone + 1) + "/" + PhaseTarget(phase) + ")");
    }

    private void ApproachUpdate()
    {
        movement.FacePlayer();
        movement.WalkTowardPlayer();

        float dist = Mathf.Abs(player.position.x - transform.position.x);
        if (dist <= slamAttackRange)
        {
            nextAttack = State.Slam;
            BeginTelegraph();
        }
    }

    private void TelegraphUpdate()
    {
        movement.FacePlayer();
        if (stateTimer > 0f) return;

        switch (nextAttack)
        {
            case State.Charge:
                chargeDir = new Vector2(Mathf.Sign(player.position.x - transform.position.x), 0f);
                Debug.Log("[Boss] Charge!");
                state = State.Charge;
                stateTimer = chargeTime;
                bossAnim?.PlayCharge();
                break;
            case State.Slam:
                attack.DoSlam();
                Debug.Log("[Boss] Slam!");
                state = State.Slam;
                stateTimer = slamDuration;
                bossAnim?.PlaySlam();
                break;
            case State.Shoot:
                attack.DoShoot();
                Debug.Log("[Boss] Shoot!");
                state = State.Shoot;
                stateTimer = 0.3f;
                bossAnim?.PlayShoot();
                break;
            default:
                EndAttack();
                break;
        }
    }

    private void ChargeUpdate()
    {
        if (charge.RunCharge(chargeDir, stateTimer))
            EndAttack();
    }

    private void SlamUpdate()
    {
        if (stateTimer <= 0f) EndAttack();
    }

    private void ShootUpdate()
    {
        if (stateTimer <= 0f) EndAttack();
    }

    private void RecoveryUpdate()
    {
        if (stateTimer <= 0f)
        {
            phaseAttackDone++;
            if (phaseAttackDone >= PhaseTarget(phase))
            {
                phase = NextPhase(phase);
                phaseAttackDone = 0;
                Debug.Log("[Boss] Ganti fase: " + phase);
            }

            if (phase == Phase.Slam)
            {
                state = State.Approach;
                bossAnim?.PlayWalk(true);
                Debug.Log("[Boss] Approach (fase Slam)");
            }
            else
            {
                nextAttack = (phase == Phase.Charge) ? State.Charge : State.Shoot;
                BeginTelegraph();
            }
        }
    }

    private void EndAttack()
    {
        Debug.Log("[Boss] Recovery");
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        state = State.Recovery;
        stateTimer = recoveryTime;
        bossAnim?.StopChargeLayer();
        bossAnim?.PlayRecover();

    }

    private int PhaseTarget(Phase p)
    {
        switch (p)
        {
            case Phase.Charge: return chargeAttackCount;
            case Phase.Shoot: return shootAttackCount;
            default: return slamAttackCount;
        }
    }

    private Phase NextPhase(Phase p)
    {
        switch (p)
        {
            case Phase.Charge: return Phase.Shoot;
            case Phase.Shoot: return Phase.Slam;
            default: return Phase.Charge;
        }
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;
        isActive = false;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true; // matikan fisika supaya tidak jatuh menembus tanah
        GetComponent<Collider>().enabled = false;
        bossAnim?.PlayDie();
        Destroy(gameObject, deathDelay);
    }
}