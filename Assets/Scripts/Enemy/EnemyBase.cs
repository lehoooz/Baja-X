using UnityEngine;

/// <summary>
/// Base class semua musuh. Pegang Health, dan hancurkan diri saat HP habis.
/// Varian (Walker / Turret / Flyer) tinggal turun dari class ini.
/// </summary>
[RequireComponent(typeof(Health))]
public abstract class EnemyBase : MonoBehaviour
{
    protected Health health;

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
}
