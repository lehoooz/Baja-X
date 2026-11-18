using UnityEngine;

/// <summary>
/// Musuh Walker: jalan lurus, berbalik arah di tepi platform / dinding.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class WalkerEnemy : EnemyBase
{
    [Header("Gerak")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private float edgeCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private int direction = 1; // 1 = kanan, -1 = kiri

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // Jalan
        rb.linearVelocity = new Vector3(direction * speed, rb.linearVelocity.y, 0f);

        // Cek dinding di depan
        Vector3 wallOrigin = transform.position + new Vector3(direction * 0.4f, 0.5f, 0f);
        bool hitWall = Physics.Raycast(wallOrigin, new Vector3(direction, 0f, 0f), wallCheckDistance, groundLayer);

        // Cek tepi di depan (apakah masih ada tanah di bawah depan)
        Vector3 edgeOrigin = transform.position + new Vector3(direction * edgeCheckDistance, 0f, 0f);
        bool groundAhead = Physics.Raycast(edgeOrigin, Vector3.down, 1.5f, groundLayer);

        if (hitWall || !groundAhead)
            Flip();
    }

    private void Flip()
    {
        direction *= -1;
        // Putar model menghadap arah jalan (sesuaikan dengan model nanti)
        transform.rotation = Quaternion.Euler(0f, direction > 0 ? 90f : -90f, 0f);
    }
}
