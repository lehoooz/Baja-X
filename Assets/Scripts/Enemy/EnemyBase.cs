using UnityEngine;

/// <summary>
/// Base class semua musuh. Pegang Health, hancurkan diri saat HP habis,
/// dan beri damage saat menyentuh pemain.
/// </summary>
[RequireComponent(typeof(Health))]
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Contact Damage")]
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackPopUp = 5f;

    [Header("Serangan Berulang (saat menempel pemain)")]
    [SerializeField] private float attackInterval = 1f; // jeda antar hit (detik)

    [Header("Knockback (saat kena tembak)")]
    [SerializeField] protected float hitKnockbackForce = 5f;
    [SerializeField] protected float hitKnockbackDuration = 0.2f;

    protected Health health;
    protected Vector3 knockbackVelocity;
    protected float knockbackTimer;

    // Waktu (Time.time) kapan boleh hit berikutnya. Pakai timestamp supaya
    // tidak butuh Update — Turret punya Update sendiri yang akan menutupi
    // Update base class kalau pakai hitung mundur.
    private float nextAttackTime;

    protected virtual void Awake()
    {
        health = GetComponent<Health>();
        health.OnDeath.AddListener(Die);
    }

    protected virtual void Die()
    {
        // Nanti: efek ledakan / drop item di sini
        Destroy(gameObject);
    }

    // Hit pertama: langsung saat bersentuhan
    private void OnCollisionEnter(Collision collision)
    {
        if (TryHitPlayer(collision))
            nextAttackTime = Time.time + attackInterval;
    }

    // Hit berikutnya: tiap attackInterval selama masih bersentuhan
    private void OnCollisionStay(Collision collision)
    {
        if (Time.time < nextAttackTime) return;
        if (TryHitPlayer(collision))
            nextAttackTime = Time.time + attackInterval;
    }

    /// <summary> Coba beri damage ke pemain. Return true hanya kalau damage
    /// benar-benar masuk. Kalau ketahan i-frames, return false — jadi
    /// OnCollisionStay akan mencoba lagi tiap langkah fisika sampai
    /// i-frames habis, lalu hit masuk tepat saat pemain rentan lagi. </summary>
    private bool TryHitPlayer(Collision collision)
    {
        // Hanya damage pemain (yang punya PlayerMovement)
        PlayerMovement pm = collision.collider.GetComponentInParent<PlayerMovement>();
        if (pm == null) return false;

        Health playerHealth = collision.collider.GetComponentInParent<Health>();
        if (playerHealth == null) return false;

        int hpBefore = playerHealth.CurrentHealth;
        playerHealth.TakeDamage(contactDamage);

        // Damage ketahan i-frames (atau pemain sudah mati) — bukan hit
        if (playerHealth.CurrentHealth >= hpBefore) return false;

        // Knockback hanya kalau damage benar-benar masuk
        Vector3 dir = collision.transform.position - transform.position;
        pm.ApplyKnockback(dir, knockbackForce, knockbackPopUp);
        return true;
    }

    /// <summary> Dorong musuh ke arah tertentu (dipanggil proyektil saat mengenai musuh). </summary>
    public void ApplyKnockback(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;
        direction.Normalize();
        knockbackVelocity = direction * hitKnockbackForce;
        knockbackTimer = hitKnockbackDuration;
    }
}