using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Komponen HP generik. Dipakai pemain, musuh, dan boss.
/// Cara pakai: pasang di GameObject, atur Max Health di Inspector.
/// Beri damage dengan TakeDamage(jumlah).
/// Reaksi saat mati: subscribe ke event OnDeath.
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 16;
    [SerializeField] private float invincibilityTime = 0f; // i-frames (detik). 0 = tidak ada. Pemain: 1.

    [SerializeField] private int currentHealth; // tampil di Inspector biar gampang debug

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;

    /// <summary> dipanggil tiap HP berubah (current, max) — untuk HUD nanti </summary>
    public UnityEvent<int, int> OnHealthChanged;
    /// <summary> dipanggil saat terkena damage (jumlah damage) — untuk knockback / flash nanti </summary>
    public UnityEvent<int> OnDamaged;
    /// <summary> dipanggil saat HP habis </summary>
    public UnityEvent OnDeath;

    private float invincibilityTimer;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (invincibilityTimer > 0f)
            invincibilityTimer -= Time.deltaTime;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        if (invincibilityTimer > 0f) return; // masih i-frames, abaikan damage

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        OnDamaged?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (invincibilityTime > 0f)
            invincibilityTimer = invincibilityTime;

        if (IsDead)
            OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary> Kembalikan HP ke penuh (dipakai saat respawn). </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        invincibilityTimer = 0f;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary> Bunuh langsung, abaikan i-frames (dipakai DeathZone / jatuh ke jurang). </summary>
    public void Kill()
    {
        if (IsDead) return;
        currentHealth = 0;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDeath?.Invoke();
    }

    /// <summary> Beri i-frames sementara (dipakai dash). Tidak memotong i-frames yang sedang berjalan. </summary>
    public void GrantInvincibility(float duration)
    {
        if (duration > invincibilityTimer)
            invincibilityTimer = duration;
    }


}
