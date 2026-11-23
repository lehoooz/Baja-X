using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bikin player bisa menembus musuh selama i-frames (kebal) dan selama dash.
/// Dipasang di Player (satu GameObject dengan Health + Collider).
///
/// Cara kerja: saat player kena damage (Health.OnDamaged) atau saat dash
/// (PlayerDash memanggil BeginIntangibility), matikan tumbukan fisik antara
/// Collider player dan semua Collider solid musuh selama durasi yang diminta.
/// Setelah selesai, tumbukan dinyalakan lagi.
/// Ini mencegah player terjepit di antara boss dan tembok saat boss ngecharge —
/// player bisa bergerak menembus boss untuk kabur.
///
/// Catatan: hanya Collider solid yang diabaikan. Trigger (mis. deteksi
/// turret, peluru) tidak disentuh, jadi tembak-menembak tetap jalan.
/// </summary>
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Collider))]
public class PlayerIntangibility : MonoBehaviour
{
    private Health health;
    private Collider playerCollider;
    private readonly List<Collider> ignoredColliders = new List<Collider>();
    private float intangibleEndTime = 0f;
    private bool isIntangible = false;

    private void Awake()
    {
        health = GetComponent<Health>();
        playerCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDamaged.AddListener(OnPlayerDamaged);
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDamaged.RemoveListener(OnPlayerDamaged);
        RestoreCollisions();
        isIntangible = false;
        intangibleEndTime = 0f;
    }

    private void Update()
    {
        if (isIntangible && Time.time >= intangibleEndTime)
        {
            RestoreCollisions();
            isIntangible = false;
            Debug.Log("[Player] Tembus musuh OFF.");
        }
    }

    // dipanggil tiap player kena damage — tembus selama waktu kebal
    private void OnPlayerDamaged(int damage)
    {
        BeginIntangibility(health.InvincibilityTime);
    }

    // Bisa dipanggil dari script lain (mis. PlayerDash) — tembus selama durasi tertentu.
    // Kalau sudah tembus, durasi diperpanjang (pakai yang paling lama),
    // jadi dash tidak memotong masa tembus dari damage, begitu pula sebaliknya.
    public void BeginIntangibility(float duration)
    {
        float endTime = Time.time + duration;
        if (endTime > intangibleEndTime)
            intangibleEndTime = endTime;

        if (!isIntangible)
        {
            IgnoreEnemyCollisions(true);
            isIntangible = true;
            Debug.Log("[Player] Tembus musuh ON.");
        }
    }

    private void IgnoreEnemyCollisions(bool ignore)
    {
        if (ignore)
        {
            ignoredColliders.Clear();
            // cari semua musuh (Walker, Flyer, Turret, Boss) lalu matikan
            // tumbukan fisik player dengan Collider solid mereka saja.
            // Collider trigger (mis. jangkauan deteksi turret) tidak disentuh.
            EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
            foreach (EnemyBase enemy in enemies)
            {
                Collider[] cols = enemy.GetComponentsInChildren<Collider>();
                foreach (Collider c in cols)
                {
                    if (c.isTrigger) continue;
                    if (c == playerCollider) continue;
                    Physics.IgnoreCollision(playerCollider, c, true);
                    ignoredColliders.Add(c);
                }
            }
        }
        else
        {
            RestoreCollisions();
        }
    }

    private void RestoreCollisions()
    {
        foreach (Collider c in ignoredColliders)
        {
            if (c != null && playerCollider != null)
                Physics.IgnoreCollision(playerCollider, c, false);
        }
        ignoredColliders.Clear();
    }
}
