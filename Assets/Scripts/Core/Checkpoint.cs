using UnityEngine;

/// <summary>
/// Checkpoint: saat pemain menyentuh, jadikan titik ini respawn point.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private bool healOnTouch = true;

    private bool activated;

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;

        // Hanya bereaksi ke pemain (cek komponen PlayerMovement)
        PlayerMovement pm = other.GetComponentInParent<PlayerMovement>();
        if (pm == null) return;

        activated = true;
        GameManager.Instance.SetRespawnPoint(transform.position);

        if (healOnTouch)
        {
            Health h = other.GetComponentInParent<Health>();
            if (h != null) h.Heal(h.MaxHealth);
        }
        // Nanti: ubah visual (nyala) saat aktif
    }
}
