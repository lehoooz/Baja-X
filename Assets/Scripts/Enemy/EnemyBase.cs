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

    [Header("Knockback (saat kena tembak)")]
    [SerializeField] protected float hitKnockbackForce = 5f;
    [SerializeField] protected float hitKnockbackDuration = 0.2f;

    protected Health health;
    protected Vector3 knockbackVelocity;
    protected float knockbackTimer;

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

    private void OnCollisionEnter(Collision collision)
    {
        // Hanya damage pemain (yang punya PlayerMovement)
        PlayerMovement pm = collision.collider.GetComponentInParent<PlayerMovement>();
        if (pm == null) return;

        Health playerHealth = collision.collider.GetComponentInParent<Health>();
        if (playerHealth == null) return;

        int hpBefore = playerHealth.CurrentHealth;
        playerHealth.TakeDamage(contactDamage);

        // Knockback hanya kalau damage benar-benar masuk (tidak ketahan i-frames)
        if (playerHealth.CurrentHealth < hpBefore)
        {
            Vector3 dir = collision.transform.position - transform.position;
            pm.ApplyKnockback(dir, knockbackForce, knockbackPopUp);
        }
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
