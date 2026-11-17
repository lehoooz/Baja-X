using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.25f; // jeda antar tembakan (detik)

    private float fireTimer;
    private PlayerMovement movement;

    private void Awake() => movement = GetComponent<PlayerMovement>();

    private void Update()
    {
        fireTimer -= Time.deltaTime;

        if (movement.isDashing || movement.isGrabbingLedge) return;

        // Action "Attack" sudah ada di Input Actions bawaan (klik kiri mouse)
        if (movement.Actions.Player.Attack.IsPressed() && fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireRate;
        }
    }

    private void Shoot()
    {
        Vector2 dir = movement.LastMoveDirection;
        Vector3 shootDir = new Vector3(dir.x, 0f, dir.y).normalized;

        Vector3 pos = firePoint != null ? firePoint.position : transform.position + Vector3.up;
        Projectile p = Instantiate(projectilePrefab, pos, Quaternion.LookRotation(shootDir));
        p.Launch(shootDir);
    }
}