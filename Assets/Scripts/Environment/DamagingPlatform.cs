using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamagingPlatform : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageInterval = 1f;

    private float damageTimer;

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerMovement>() == null) return;
        Health hp = collision.gameObject.GetComponent<Health>();
        if (hp == null) return;
        damageTimer -= Time.fixedDeltaTime;
        if (damageTimer > 0f) return;
        hp.TakeDamage(damageAmount);
        damageTimer = damageInterval;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerMovement>() == null) return;
        damageTimer = 0f;
    }
}