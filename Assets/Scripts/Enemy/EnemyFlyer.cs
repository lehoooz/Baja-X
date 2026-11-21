using UnityEngine;

/// <summary>
/// Musuh Flyer: patroli di udara dalam jarak tertentu, berbalik di ujung.
/// Kalau pemain masuk kotak deteksi di bawah depannya, dia ancang-ancang
/// lalu menukik ke arah pemain, lalu kembali ke tempat terbangnya.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class FlyerEnemy : EnemyBase
{
    [Header("Patroli")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float bobAmplitude = 0.5f;
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float patrolRange = 4f;

    [Header("Deteksi (kotak di bawah depan)")]
    [SerializeField] private float detectionWidth = 4f;
    [SerializeField] private float detectionHeight = 5f;

    [Header("Telegraph (ancang-ancang)")]
    [SerializeField] private float telegraphTime = 0.6f;

    [Header("Menukik")]
    [SerializeField] private float diveSpeed = 12f;
    [SerializeField] private float maxDiveTime = 1f;

    [Header("Kembali")]
    [SerializeField] private float returnSpeed = 4f;

    private enum State { Patrol, Telegraph, Dive, Return }
    private State state = State.Patrol;

    private Rigidbody rb;
    private int direction = 1; // 1 = kanan, -1 = kiri
    private Transform player;
    private Vector3 homePosition;
    private Vector3 diveTarget;
    private float stateTimer;
    private Animator anim;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        homePosition = transform.position;
        if (GameManager.Instance != null)
            player = GameManager.Instance.Player;
        transform.rotation = Quaternion.Euler(0f, direction > 0 ? 90f : -90f, 0f);
    }

    private void FixedUpdate()
    {

        if (anim != null)
            anim.SetBool("isAlerted", state == State.Telegraph);
        // Saat terpental, pakai velocity knockback dan jangan lakukan yang lain
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = new Vector3(knockbackVelocity.x, rb.linearVelocity.y, 0f);
            return;
        }

        switch (state)
        {
            case State.Patrol: PatrolUpdate(); break;
            case State.Telegraph: TelegraphUpdate(); break;
            case State.Dive: DiveUpdate(); break;
            case State.Return: ReturnUpdate(); break;
        }
    }

    private void PatrolUpdate()
    {
        // Berbalik di ujung jarak patroli (dari titik spawn)
        // Hanya flip kalau sedang bergerak menjauhi home (biar tidak flip bolak-balik di ujung)
        float distFromHome = transform.position.x - homePosition.x;
        if (Mathf.Abs(distFromHome) >= patrolRange && Mathf.Sign(distFromHome) == direction)
            Flip();

        // Terbang naik-turun (sinus)
        float vy = Mathf.Cos(Time.time * bobFrequency) * bobAmplitude * bobFrequency;
        rb.linearVelocity = new Vector3(direction * speed, vy, 0f);

        // Cek pemain di kotak deteksi (bawah depan)
        if (player != null && IsPlayerInDetectionBox())
        {
            state = State.Telegraph;
            stateTimer = telegraphTime;
            rb.linearVelocity = Vector3.zero;
        }
    }


    private bool IsPlayerInDetectionBox()
    {
        Vector3 toPlayer = player.position - transform.position;
        bool inFront = Mathf.Sign(toPlayer.x) == direction;
        bool inWidth = Mathf.Abs(toPlayer.x) <= detectionWidth;
        bool below = toPlayer.y <= 0f && toPlayer.y >= -detectionHeight;
        return inFront && inWidth && below;
    }

    private void TelegraphUpdate()
    {
        // Bergetar di tempat (ancang-ancang, pemain bisa menghindar)
        rb.linearVelocity = new Vector3(Mathf.Sin(Time.time * 40f) * 0.8f, 0f, 0f);

        stateTimer -= Time.fixedDeltaTime;
        if (stateTimer <= 0f)
        {
            // Tangkap posisi pemain TERKINI saat ancang-ancang selesai,
            // bukan posisi 0.6 detik lalu.
            if (player != null)
                diveTarget = player.position;
            state = State.Dive;
            stateTimer = maxDiveTime;
            if (anim != null)
                anim.SetTrigger("Attack");
        }
    }

    private void DiveUpdate()
    {
        // Menukik ke posisi pemain saat ancang-ancang selesai (bukan mengikuti terus)
        Vector3 dir = (diveTarget - transform.position).normalized;
        rb.linearVelocity = dir * diveSpeed;

        stateTimer -= Time.fixedDeltaTime;
        float dist = Vector3.Distance(transform.position, diveTarget);
        if (dist < 0.3f || stateTimer <= 0f)
            state = State.Return;
    }

    private void ReturnUpdate()
    {
        // Kembali ke tempat terbang (home)
        Vector3 dir = (homePosition - transform.position).normalized;
        rb.linearVelocity = dir * returnSpeed;

        if (Vector3.Distance(transform.position, homePosition) < 0.3f)
            state = State.Patrol;
    }

    private void Flip()
    {
        direction *= -1;
        transform.rotation = Quaternion.Euler(0f, direction > 0 ? 90f : -90f, 0f);
    }

    /// <summary> Hadap ke arah posisi sumber (misal saat mendengar tembakan). </summary>
    public void FaceTowards(Vector3 sourcePosition)
    {
        int newDir = sourcePosition.x >= transform.position.x ? 1 : -1;
        if (newDir != direction)
            Flip();
    }


    private void OnDrawGizmosSelected()
    {
        // Kotak deteksi (kuning) di bawah depan
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + new Vector3(direction * detectionWidth * 0.5f, -detectionHeight * 0.5f, 0f);
        Gizmos.DrawWireCube(center, new Vector3(detectionWidth, detectionHeight, 0.5f));
        // Garis jarak patroli (cyan)
        Gizmos.color = Color.cyan;
        Vector3 home = Application.isPlaying ? homePosition : transform.position;
        Gizmos.DrawLine(home + Vector3.left * patrolRange, home + Vector3.right * patrolRange);
    }
}
