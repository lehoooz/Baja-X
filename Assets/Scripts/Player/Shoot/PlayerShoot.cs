using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.25f; // jeda antar tembakan (detik)
    [SerializeField] private float burstWindow = 0.6f; // interval waktu — 3 tembakan beruntun dalam interval ini triger jeda
    [SerializeField] private float reloadTime = 1f; // jeda setelah 3 tembakan cepat (detik)

    private float fireTimer;
    private float reloadTimer;
    private bool isReloading;
    private float lastShotTime = -100f;
    private int rapidCount;
    private PlayerMovement movement;
    public bool isShooting { get; private set; }
    public bool IsReloading => isReloading;


    private void Awake() => movement = GetComponent<PlayerMovement>();

    private void Update()
    {
        fireTimer -= Time.deltaTime;

        // Lagi reload — hitung mundur, selesai reload bisa tembak lagi
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                isReloading = false;
                rapidCount = 0;
            }
            isShooting = false;
            return;
        }

        isShooting = movement.Actions.Player.Attack.IsPressed()
            && !movement.isDashing
            && !movement.isGrabbingLedge;

        if (!isShooting) return;

        // Action "Attack" sudah ada di Input Actions bawaan (klik kiri mouse)
        if (fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireRate;

            // Hitung tembakan beruntun: kalau jeda dari tembakan sebelumnya
            // masih dalam burstWindow, ini tembakan cepat beruntun.
            // Kalau jedanya sudah lewat, mulai hitung baru dari 1.
            if (Time.time - lastShotTime <= burstWindow)
                rapidCount++;
            else
                rapidCount = 1;
            lastShotTime = Time.time;

            // 3 tembakan cepat beruntun — triger jeda (reload)
            if (rapidCount >= 3)
            {
                isReloading = true;
                reloadTimer = reloadTime;
                rapidCount = 0;
            }
        }
    }


    private void Shoot()
    {
        Vector2 dir = movement.LastMoveDirection;
        Vector3 shootDir = new Vector3(dir.x, 0f, dir.y).normalized;

        Vector3 pos = firePoint != null ? firePoint.position : transform.position + Vector3.up;
        Projectile p = Instantiate(projectilePrefab, pos, Quaternion.LookRotation(shootDir));
        p.Launch(shootDir);
        AudioManager.Instance?.PlayShoot();
    }

}