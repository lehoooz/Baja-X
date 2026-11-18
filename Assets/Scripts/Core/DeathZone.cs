using UnityEngine;

/// <summary>
/// Zona kematian: saat pemain masuk (misal jatuh ke jurang), langsung mati.
/// Letakkan sebagai trigger besar di bawah level.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Hanya bereaksi ke pemain
        PlayerMovement pm = other.GetComponentInParent<PlayerMovement>();
        if (pm == null) return;

        Health health = other.GetComponentInParent<Health>();
        if (health == null) return;

        health.Kill(); // mati langsung, abaikan i-frames
    }
}
