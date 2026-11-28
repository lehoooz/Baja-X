using UnityEngine;

/// <summary>
/// Benda lingkungan yang bisa dihancurkan (tong, peti, penghalang).
/// Tidak menyerang dan tidak memberi damage saat disentuh — hanya
/// menghalangi jalan sampai dihancurkan dengan tembakan atau dash.
/// Taruh di layer Enemy (atau Ground) supaya peluru bisa mengenainya.
/// </summary>
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Collider))]
public class DestructibleObject : MonoBehaviour
{
    [Header("Efek Hancur (opsional)")]
    [SerializeField] private GameObject destroyEffectPrefab; // mis. partikel, kosong = tidak ada efek

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        health.OnDeath.AddListener(Die);
    }

    private void Die()
    {
        if (destroyEffectPrefab != null)
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        AudioManager.Instance?.PlayExplosion();
        Destroy(gameObject);
    }
}