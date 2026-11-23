using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossController : EnemyBase
{
    private enum State { Approach, Telegraph, Charge, Slam, Shoot, Recovery }
    private State state = State.Approach;

    [Header("Mendekati Player")]
    [SerializeField] private float walkSpeed = 3f; // kecepatan jalan mendekati player
    [SerializeField] private float approachMinTime = 0.5f; // jalan dulu segini sebelum boleh menyerang

    [Header("Jarak Serangan")]
    [SerializeField] private float slamAttackRange = 3.5f; // dekat -> slam
    [SerializeField] private float attackRange = 15f; // dalam jarak ini boss menyerang (charge / shot acak)

    [Header("Telegraph")]
    [SerializeField] private float telegraphTime = 0.5f;
    [SerializeField] private Color telegraphColor = Color.red;

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

    private Transform player;
    private Rigidbody rb;
    private Renderer[] rends;
    private Color[] baseColors;
    private Transform model;
    private Vector3 baseScale;

    private float stateTimer;
    private float approachTimer;
    private State nextAttack = State.Approach;
    private Vector2 chargeDir = Vector2.right;
    private bool isActive;

    // anti pengulangan: serangan yang sama tidak boleh lebih dari 2x berturut-turut
    private State lastAttack = State.Approach;
    private int sameAttackCount = 0;

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

        // boss langsung aktif dari awal scene, tidak menunggu player dekat
        isActive = true;
        state = State.Approach;
        approachTimer = approachMinTime;
        Debug.Log("[Boss] Aktif dari awal scene.");
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

    private void ApproachUpdate()
    {
        FacePlayer();

        // jalan mendekati player
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        if (dir != 0f)
            rb.linearVelocity = new Vector3(dir * walkSpeed, rb.linearVelocity.y, 0f);
        else
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        // jalan dulu sebentar sebelum boleh menyerang, biar boss kelihatan mendekat
        if (approachTimer > 0f)
        {
            approachTimer -= Time.fixedDeltaTime;
            return;
        }

        float dist = Mathf.Abs(player.position.x - transform.position.x);

        // dekat -> slam (tetap berdasarkan jarak, bukan acak)
        if (dist <= slamAttackRange)
        {
            nextAttack = State.Slam;
            lastAttack = State.Slam;
            sameAttackCount = 0;
        }
        // dalam jangkauan serang -> charge / shot dipilih ACAK
        else if (dist <= attackRange)
        {
            State[] attacks = { State.Charge, State.Shoot };
            State pick = attacks[Random.Range(0, attacks.Length)];

            // serangan yang sama tidak boleh lebih dari 2x berturut-turut
            if (pick == lastAttack)
            {
                sameAttackCount++;
                if (sameAttackCount > 2)
                {
                    pick = (pick == State.Charge) ? State.Shoot : State.Charge;
                    sameAttackCount = 1;
                }
            }
            else
            {
                sameAttackCount = 1;
            }

            lastAttack = pick;
            nextAttack = pick;
        }
        else
        {
            return; // terlalu jauh, terus jalan mendekat
        }

        Debug.Log("[Boss] Telegraph -> serangan: " + nextAttack + " (jarak: " + dist.ToString("F1") + ")");
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
                Debug.Log("[Boss] Charge! Arah: " + chargeDir.x);
                state = State.Charge;
                stateTimer = chargeTime;
                break;
            case State.Slam:
                SlamDamage();
                Debug.Log("[Boss] Slam! Radius: " + slamRadius);
                if (model != transform)
                    model.localScale = new Vector3(baseScale.x, baseScale.y * 0.7f, baseScale.z);
                state = State.Slam;
                stateTimer = slamDuration;
                break;
            case State.Shoot:
                ShootProjectile();
                Debug.Log("[Boss] Shoot!");
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

        // berhenti kalau nabrak tembok
        Vector3 wallOrigin = transform.position + Vector3.up * 1.6f
            + new Vector3(chargeDir.x * 0.7f, 0f, 0f);
        if (Physics.Raycast(wallOrigin, new Vector3(chargeDir.x, 0f, 0f), 1f, groundLayer))
        {
            Debug.Log("[Boss] Charge nabrak tembok, berhenti.");
            EndAttack();
            return;
        }

        // berhenti kalau sudah di ujung platform (tidak ada tanah di depan)
        // biar charge sampai ujung, bukan jatuh dari platform
        Vector3 edgeOrigin = transform.position + new Vector3(chargeDir.x * 0.8f, 0.2f, 0f);
        bool groundAhead = Physics.Raycast(edgeOrigin, Vector3.down, 2.5f, groundLayer);
        if (!groundAhead)
        {
            Debug.Log("[Boss] Charge sampai ujung platform, berhenti.");
            EndAttack();
            return;
        }

        // batas aman saja (5 detik), biar tidak charge selamanya kalau kedua cek di atas lolos
        if (stateTimer <= 0f)
        {
            Debug.Log("[Boss] Charge selesai (batas waktu), berhenti.");
            EndAttack();
        }
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
            Debug.Log("[Boss] Approach");
            state = State.Approach;
            approachTimer = approachMinTime;
        }
    }

    private void EndAttack()
    {
        Debug.Log("[Boss] Recovery");
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
