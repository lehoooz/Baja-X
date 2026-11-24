using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    private BossController boss;

    private void Awake()
    {
        boss = GetComponentInParent<BossController>();
        if (boss == null)
            Debug.LogError("[BossTrigger] BossController tidak ditemukan di parent!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>() != null)
            boss?.Activate();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>() != null)
            boss?.Deactivate();
    }
}