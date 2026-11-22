using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossController : EnemyBase
{
    private enum State { Idle, Telegraph, Charge, Slam, Shoot, Recovery }
    private State state = State.Idle;

    [Header("Aktivasi")]
    [SerializeField] private float activationRange = 12f; // boss mulai aktif kalau pemain sedekat ini
    [SerializeField] private float idleTime = 1.2f; // jeda antar serangan

    [Header("Telegraph")]
    [SerializeField] private float telegraphTime = 0.5f;
    [SerializeField] private Color telegraphColor = Color.red;

    [Header("Charge")]
    [SerializeField] private float chargeSpeed = 14f;
    [SerializeField] private float chargeTime = 0.6f;
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

    private Transform player;
    private Rigidbody rb;
    private Renderer[] rends;
    private Color[] baseColors;
    private Transform model;
    private Vector3 baseScale;

    private float stateTimer;
    private State nextAttack = State.Idle;
    private Vector2 chargeDir = Vector2.right;
    private bool isActive;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        Transform m = transform.Find("Model");
        model = m != null ? m : transform;
        baseScale = model.localScale;

        rends = GetComponentsInChildren<Renderer>();
        baseColors = new Color[rends.Length];
        for (int i = 0; i < rends.Length; i++)
            baseColors[i] = rends[i].material.color;
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
            player = GameManager.Instance.Player;
        stateTimer = idleTime;
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        // Boss berat: peluru tidak mendorongnya, tapi timer knockback tetap dihabiskan
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        float dist = Mathf.Abs(player.position.x - transform.position.x);

        if (!isActive)
        {
            if (dist <= activationRange)
            {
                isActive = true;
                state = State.Idle;
                stateTimer = idleTime;
            }
            return;
        }

        stateTimer -= Time.fixedDeltaTime;

        switch (state)
        {
            case State.Idle: IdleUpdate(); break;
            case State.Telegraph: TelegraphUpdate(); break;
            case State.Charge: ChargeUpdate(); break;
            case State.Slam: SlamUpdate(); break;
            case State.Shoot: ShootUpdate(); break;
            case State.Recovery: RecoveryUpdate(); break;
        }
    }

    private void IdleUpdate()
    {
        FacePlayer();
        if (stateTimer > 0f) return;

        // serangan acak: charge / slam / tembak
        State[] attacks = { State.Charge, State.Slam, State.Shoot };
        nextAttack = attacks[Random.Range(0, attacks.Length)];

        state = State.Telegraph;
        stateTimer = telegraphTime;
        SetColor(telegraphColor);
    }

    private void TelegraphUpdate()
    {
        FacePlayer();
        if (stateTimer > 0f) return;

        RestoreColor();
        switch (nextAttack)
        {
            case State.Charge:
                chargeDir = new Vector2(Mathf.Sign(player.position.x - transform.position.x), 0f);
                state = State.Charge;
                stateTimer = chargeTime;
                break;
            case State.Slam:
                SlamDamage();
                if (model != transform)
                    model.localScale = new Vector3(baseScale.x, baseScale.y * 0.7f, baseScale.z);
                state = State.Slam;
                stateTimer = slamDuration;
                break;
            case State.Shoot:
                ShootProjectile();
                state = State.Shoot;
                stateTimer = 0.1f;
                break;
            default:
                EndAttack();
                break;
        }
    }

    private void ChargeUpdate()
    {
        rb.linearVelocity = new Vector3(chargeDir.x * chargeSpeed, rb.linearVelocity.y, 0f);

        Vector3 origin = transform.position + Vector3.up * 1.6f
            + new Vector3(chargeDir.x * 0.7f, 0f, 0f);
        if (Physics.Raycast(origin, new Vector3(chargeDir.x, 0f, 0f), 0.5f, groundLayer))
        {
            EndAttack();
            return;
        }

        if (stateTimer <= 0f) EndAttack();
    }

    private void SlamUpdate()
    {
        if (stateTimer <= 0f)
        {
            model.localScale = baseScale;
            EndAttack();
        }
    }

    private void ShootUpdate()
    {
        if (stateTimer <= 0f) EndAttack();
    }

    private void RecoveryUpdate()
    {
        if (stateTimer <= 0f)
        {
            state = State.Idle;
            stateTimer = idleTime;
        }
    }

    private void EndAttack()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        state = State.Recovery;
        stateTimer = recoveryTime;
    }

    private void SlamDamage()
    {
        if (player == null) return;
        float dist = Mathf.Abs(player.position.x - transform.position.x);
        if (dist > slamRadius) return;

        Health hp = player.GetComponent<Health>();
        if (hp != null) hp.TakeDamage(slamDamage);

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            Vector2 dir = new Vector2(Mathf.Sign(player.position.x - transform.position.x), 0f);
            pm.ApplyKnockback(dir, slamKnockback, slamKnockbackPopUp);
        }
    }

    private void ShootProjectile()
    {
        if (bossProjectilePrefab == null || firePoint == null) return;
        Vector2 dir = new Vector2(Mathf.Sign(player.position.x - transform.position.x), 0f);
        Vector3 shootDir = new Vector3(dir.x, 0f, 0f);
        Projectile p = Instantiate(bossProjectilePrefab, firePoint.position, Quaternion.LookRotation(shootDir));
        p.Launch(shootDir);
    }

    private void FacePlayer()
    {
        if (player == null) return;
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        if (dir != 0f)
            transform.rotation = Quaternion.Euler(0f, dir > 0f ? 90f : -90f, 0f);
    }

    private void SetColor(Color c)
    {
        if (rends == null) return;
        foreach (Renderer r in rends) r.material.color = c;
    }

    private void RestoreColor()
    {
        if (rends == null) return;
        for (int i = 0; i < rends.Length; i++)
            rends[i].material.color = baseColors[i];
    }
}