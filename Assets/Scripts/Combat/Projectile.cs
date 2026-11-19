using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 30f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private int damage = 1; // damage per kena
    [SerializeField] private LayerMask hitLayers; // layer yang membuat peluru hancur

    [Header("Knockback ke Player")]
    [SerializeField] private float playerKnockbackForce = 8f;
    [SerializeField] private float playerKnockbackPopUp = 5f;


    public int Damage => damage;

    public void Launch(Vector3 direction)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = direction * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & hitLayers) == 0) return;

        // Cari Health di objek yang kena (atau di parent-nya, kalau collider ada di child)
        Health health = other.GetComponent<Health>();
        if (health == null) health = other.GetComponentInParent<Health>();
        if (health != null) health.TakeDamage(damage);

        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy == null) enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            Rigidbody prb = GetComponent<Rigidbody>();
            Vector3 dir = prb != null ? prb.linearVelocity : Vector3.right;
            enemy.ApplyKnockback(dir);
        }

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null) player = other.GetComponentInParent<PlayerMovement>();
        if (player != null)
        {
            Rigidbody prb = GetComponent<Rigidbody>();
            Vector3 dir = prb != null ? prb.linearVelocity : Vector3.right;
            player.ApplyKnockback(dir, playerKnockbackForce, playerKnockbackPopUp);
        }


        Destroy(gameObject);
    }
}
