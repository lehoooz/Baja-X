using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    [SerializeField] private float dashSpeed = 50f;
    [SerializeField] private float dashDistance = 7f;
    [SerializeField] private float dashCooldown = 1f;

    public bool isDashing { get; private set; }

    private bool canDash = true;
    private PlayerMovement movement;
    private PlayerLedgeWall wall;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        wall = GetComponent<PlayerLedgeWall>();
    }

    private void Start() => movement.Actions.Player.Dash.performed += Dashing;
    private void OnDestroy() => movement.Actions.Player.Dash.performed -= Dashing;

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
        wall.ClearAll();
        movement.WallJumpLockCounter = 0f;

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
}