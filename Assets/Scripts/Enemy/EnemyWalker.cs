using UnityEngine;

/// <summary>
/// Musuh Walker: jalan lurus, berbalik arah di tepi platform / dinding / musuh lain.
/// Kalau pemain terlihat di depannya, dia charge dengan cepat.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class WalkerEnemy : EnemyBase
{
    [Header("Gerak")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float wallCheckDistance = 0.5f;
    //[SerializeField] private float edgeCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask enemyLayer = 1 << 6; // layer Enemy — untuk flip kalau ada musuh di depan

    [Header("Charge (kejar pemain)")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float detectionHeight = 1.5f;
    [SerializeField] private float chargeSpeed = 9f;

    private Rigidbody rb;
    private int direction = 1; // 1 = kanan, -1 = kiri
    private Transform player;
    private Animator anim;
    private BoxCollider boxCollider;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        boxCollider = GetComponent<BoxCollider>();
        // Kalau kena tembak, hadap ke arah pemain (bukan karena dengar suara tembakan)
        if (health != null)
            health.OnDamaged.AddListener(OnHitByBullet);
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            player = GameManager.Instance.Player;

    }

    private void FixedUpdate()
    {
        // Saat terpental, pakai velocity knockback dan jangan patrol dulu
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = new Vector3(knockbackVelocity.x, rb.linearVelocity.y, 0f);
            return;
        }

        // Deteksi pemain di depan (seperti melihat)
        float currentSpeed = speed;
        bool isCharging = false;
        if (player != null)
        {
            Vector3 toPlayer = player.position - transform.position;
            bool playerInFront = Mathf.Sign(toPlayer.x) == direction;
            bool playerInRange = Mathf.Abs(toPlayer.x) <= detectionRange;
            bool playerSameHeight = Mathf.Abs(toPlayer.y) <= detectionHeight;
            if (playerInFront && playerInRange && playerSameHeight)
            {
                currentSpeed = chargeSpeed;
                isCharging = true;
            }
        }

        // Update animasi: FoundEnemies saat charge, Walk saat patrol
        if (anim != null)
            anim.SetBool("isAlerted", isCharging);

        // Jalan
        rb.linearVelocity = new Vector3(direction * currentSpeed, rb.linearVelocity.y, 0f);

        // Setengah lebar collider di sumbu X dunia. bounds.extents.x otomatis
        // mengikuti rotasi Flip — kalau collider diubah lagi, ray ikut menyesuaikan.
        float halfWidth = boxCollider != null ? boxCollider.bounds.extents.x : 0.5f;

        // Cek musuh di depan — kalau ada musuh lain, berbalik arah (jangan mendorongnya)
        Vector3 wallOrigin = transform.position + new Vector3(direction * (halfWidth + 0.1f), 0.5f, 0f);
        bool hitEnemy = Physics.Raycast(wallOrigin, new Vector3(direction, 0f, 0f), wallCheckDistance, enemyLayer);

        // Cek dinding di depan — kalau kena, berbalik arah
        bool hitWall = Physics.Raycast(wallOrigin, new Vector3(direction, 0f, 0f), wallCheckDistance, groundLayer);

        // Cek tepi di depan — kalau kosong (tidak ada tanah), langsung berbalik arah
        Vector3 edgeOrigin = transform.position + new Vector3(direction * (halfWidth + 0.1f), 0.5f, 0f);
        bool groundAhead = Physics.Raycast(edgeOrigin, Vector3.down, 1.5f, groundLayer);

        if (hitWall || hitEnemy || !groundAhead)
            Flip();
    }

    private void Flip()
    {
        direction *= -1;
        // Putar model menghadap arah jalan (sesuaikan dengan model nanti)
        transform.rotation = Quaternion.Euler(0f, direction > 0 ? 90f : -90f, 0f);
    }

    /// <summary> Hadap ke arah posisi sumber (misal saat mendengar tembakan). </summary>
    public void FaceTowards(Vector3 sourcePosition)
    {
        int newDir = sourcePosition.x >= transform.position.x ? 1 : -1;
        if (newDir != direction)
            Flip();
    }

    /// <summary> Dipanggil saat kena damage (misal kena peluru pemain) — berbalik menghadap pemain. </summary>
    private void OnHitByBullet(int damage)
    {
        if (player != null)
            FaceTowards(player.position);
    }

    private void OnDrawGizmosSelected()
    {
        // Garis jangkauan deteksi (merah) di depan walker
        Gizmos.color = Color.red;
        Vector3 from = transform.position + Vector3.up * 0.5f;
        Vector3 to = from + new Vector3(direction * detectionRange, 0f, 0f);
        Gizmos.DrawLine(from, to);
    }
}
