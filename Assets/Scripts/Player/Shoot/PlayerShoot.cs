using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.25f; // jeda antar tembakan (detik)

    private float fireTimer;
    private PlayerMovement movement;
    public bool isShooting { get; private set; }


    private void Awake() => movement = GetComponent<PlayerMovement>();

    private void Update()
    {
        fireTimer -= Time.deltaTime;

        isShooting = movement.Actions.Player.Attack.IsPressed()
            && !movement.isDashing
            && !movement.isGrabbingLedge;

        if (!isShooting) return;

        // Action "Attack" sudah ada di Input Actions bawaan (klik kiri mouse)
        if (fireTimer <= 0f)
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

        // Beri tahu semua walker dari mana tembakan berasal (mereka menoleh ke arah pemain)
        foreach (WalkerEnemy walker in FindObjectsByType<WalkerEnemy>())
            walker.FaceTowards(transform.position);
    }

}