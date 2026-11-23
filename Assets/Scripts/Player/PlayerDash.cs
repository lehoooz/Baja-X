using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    [SerializeField] private float dashSpeed = 50f;
    [SerializeField] private float dashDistance = 7f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Dash Damage")]
    [SerializeField] private int dashDamage = 1; // damage ke musuh saat dash menembus mereka

    public bool isDashing { get; private set; }

    private bool canDash = true;
    private PlayerMovement movement;
    private PlayerLedgeWall wall;
    private Health health;
    private PlayerIntangibility intangibility;
    private CapsuleCollider playerCollider;
    private readonly HashSet<EnemyBase> dashHitEnemies = new HashSet<EnemyBase>();

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        wall = GetComponent<PlayerLedgeWall>();
        health = GetComponent<Health>();
        intangibility = GetComponent<PlayerIntangibility>();
        playerCollider = GetComponent<CapsuleCollider>();
    }

    private void Start() => movement.Actions.Player.Dash.performed += Dashing;
    private void OnDestroy() => movement.Actions.Player.Dash.performed -= Dashing;

    private void FixedUpdate()
    {
        // selama dash, cek musuh yang ditembus dan beri damage (sekali per musuh per dash)
        if (isDashing)
            CheckDashDamage();
    }

    private void Dashing(InputAction.CallbackContext ctx)
    {
        if (canDash && !isDashing)
            StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        // Fase 1: Mulai & kunci status
        canDash = false;
        isDashing = true;
        movement.ClearKnockback();
        health.GrantInvincibility(dashDistance / dashSpeed);
        wall.ClearAll();
        movement.WallJumpLockCounter = 0f;

        // dash menembus musuh (tidak tertahan / terjepit)
        dashHitEnemies.Clear();
        if (intangibility != null)
            intangibility.BeginIntangibility(dashDistance / dashSpeed);
        else
            Debug.LogWarning("[Dash] PlayerIntangibility belum dipasang di Player — dash tidak menembus musuh.");

        // Fase 2: Arah & kecepatan
        Vector2 raw = movement.RawMoveInput;
        Vector2 dashDir = raw.sqrMagnitude > 0.01f ? raw.normalized : movement.LastMoveDirection;

        if (movement.Is25DPlatformer)
            movement.Rb.linearVelocity = new Vector3(dashDir.x * dashSpeed, dashDir.y * dashSpeed, 0f);
        else
            movement.Rb.linearVelocity = new Vector3(dashDir.x * dashSpeed, 0f, dashDir.y * dashSpeed);

        // Fase 3: Tunggu dash selesai
        yield return new WaitForSeconds(dashDistance / dashSpeed);

        movement.Rb.linearVelocity = Vector3.zero;
        isDashing = false;

        // Fase 4: Cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // cek musuh yang bersentuhan dengan kapsul player selama dash.
    // tiap musuh kena damage sekali per dash. Boss TIDAK kena damage —
    // dash hanya menembus boss, tidak melukai dia.
    private void CheckDashDamage()
    {
        if (playerCollider == null) return;

        Vector3 center = transform.TransformPoint(playerCollider.center);
        float halfHeight = playerCollider.height * 0.5f - playerCollider.radius;
        Vector3 top = center + Vector3.up * halfHeight;
        Vector3 bottom = center - Vector3.up * halfHeight;

        Collider[] hits = Physics.OverlapCapsule(top, bottom, playerCollider.radius);
        foreach (Collider hit in hits)
        {
            EnemyBase enemy = hit.GetComponentInParent<EnemyBase>();
            if (enemy == null) continue;
            if (enemy is BossController) continue; // boss tidak kena damage dash
            if (dashHitEnemies.Contains(enemy)) continue;

            dashHitEnemies.Add(enemy);
            Health hp = enemy.GetComponent<Health>();
            if (hp != null)
            {
                hp.TakeDamage(dashDamage);
                Debug.Log("[Dash] Kena " + enemy.name + ", damage " + dashDamage);
            }
        }
    }
}
