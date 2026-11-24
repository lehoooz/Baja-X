using UnityEngine;

public class BossAttack : MonoBehaviour
{
    private Transform player;
    private float slamRadius;
    private int slamDamage;
    private float slamKnockback;
    private float slamKnockbackPopUp;
    private Projectile bossProjectilePrefab;
    private Transform firePoint;

    public void Setup(float slamRadius, int slamDamage, float slamKnockback, float slamKnockbackPopUp,
                      Projectile bossProjectilePrefab, Transform firePoint)
    {
        this.slamRadius = slamRadius;
        this.slamDamage = slamDamage;
        this.slamKnockback = slamKnockback;
        this.slamKnockbackPopUp = slamKnockbackPopUp;
        this.bossProjectilePrefab = bossProjectilePrefab;
        this.firePoint = firePoint;
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
            player = GameManager.Instance.Player;
    }

    public void DoSlam()
    {
        if (player == null) return;
        float dist = Mathf.Abs(player.position.x - transform.position.x);
        if (dist > slamRadius) return;

        Health hp = player.GetComponent<Health>();
        if (hp != null) hp.TakeDamage(slamDamage);

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            Vector2 dir = new Vector2(Mathf.Sign(player.position.x - transform.position.x), 0f);
            pm.ApplyKnockback(dir, slamKnockback, slamKnockbackPopUp);
        }
    }

    public void DoShoot()
    {
        if (bossProjectilePrefab == null || firePoint == null || player == null) return;
        Vector2 dir = new Vector2(Mathf.Sign(player.position.x - transform.position.x), 0f);
        Vector3 shootDir = new Vector3(dir.x, 0f, 0f);
        Projectile p = Instantiate(bossProjectilePrefab, firePoint.position, Quaternion.LookRotation(shootDir));
        p.Launch(shootDir);
    }
}