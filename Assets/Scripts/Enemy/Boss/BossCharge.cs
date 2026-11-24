using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossCharge : MonoBehaviour
{
    private Rigidbody rb;
    private BoxCollider box;
    private float chargeSpeed;
    private LayerMask groundLayer;

    public void Setup(float chargeSpeed, LayerMask groundLayer)
    {
        this.chargeSpeed = chargeSpeed;
        this.groundLayer = groundLayer;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        box = GetComponent<BoxCollider>();
        if (box == null) Debug.LogError("[Boss] BossCharge butuh BoxCollider di Boss!");
    }

    // jalankan charge ke arah dir. return true kalau charge harus berhenti.
    public bool RunCharge(Vector2 dir, float stateTimer)
    {
        rb.linearVelocity = new Vector3(dir.x * chargeSpeed, rb.linearVelocity.y, 0f);

        // muka depan collider (arah X dunia) — pakai bounds supaya ikut ukuran collider
        float frontX = dir.x > 0f ? box.bounds.max.x : box.bounds.min.x;
        float rayY = box.bounds.min.y + 1.6f;

        // berhenti kalau nabrak tembok
        Vector3 wallOrigin = new Vector3(frontX + dir.x * 0.1f, rayY, transform.position.z);
        if (Physics.Raycast(wallOrigin, new Vector3(dir.x, 0f, 0f), 1f, groundLayer))
        {
            Debug.Log("[Boss] Charge nabrak tembok, berhenti.");
            return true;
        }

        // berhenti kalau sudah di ujung platform (tidak ada tanah di depan)
        Vector3 edgeOrigin = new Vector3(frontX + dir.x * 0.1f, box.bounds.min.y + 0.2f, transform.position.z);
        bool groundAhead = Physics.Raycast(edgeOrigin, Vector3.down, 2.5f, groundLayer);
        if (!groundAhead)
        {
            Debug.Log("[Boss] Charge sampai ujung platform, berhenti.");
            return true;
        }

        // batas aman saja, biar tidak charge selamanya kalau kedua cek di atas lolos
        if (stateTimer <= 0f)
        {
            Debug.Log("[Boss] Charge selesai (batas waktu), berhenti.");
            return true;
        }

        return false;
    }
}