using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f;

    [Header("Dash Visual")]
    [SerializeField] private TrailRenderer dashTrail;

    [Header("Dash Tint")]
    [SerializeField] private Color dashTint = new Color(1.5f, 2.5f, 3f, 1f); // terang kebiruan
    [SerializeField] private float tintSmooth = 20f;

    private Renderer[] renderers;
    private MaterialPropertyBlock block;
    private Color currentTint = Color.white;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor"); // URP

    private Color originalColor;
    private PlayerMovement movement;
    private Animator anim;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        // GetComponentInChildren berjaga-jaga jika Animator ada di child object
        anim = GetComponentInChildren<Animator>();
        if (dashTrail == null) dashTrail = GetComponentInChildren<TrailRenderer>();
        if (dashTrail != null) dashTrail.emitting = false;
        renderers = GetComponentsInChildren<Renderer>();
        block = new MaterialPropertyBlock();

        dashTrail.emitting = false;
    }

    private void Update()
    {
        Vector2 move = movement.MoveInput;

        // Mengirim parameter ke Animator persis seperti kode 2D-mu
        anim.SetBool("isRunning", movement.isMoving);
        anim.SetFloat("animSpeed", movement.Speed / 5f);
        anim.SetBool("isJumping", !movement.isGround && !movement.isDashing && !movement.isGrabbingLedge && !movement.isWallSliding);
        anim.SetBool("isDashing", movement.isDashing);
        anim.SetBool("isHanging", movement.isGrabbingLedge);
        anim.SetBool("isWallSliding", movement.isWallSliding);

        HandleVisual();
        if (movement.isWallSliding)
        {
            Vector3 away = new Vector3(-move.x, 0f, -move.y);
            if (away != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(away);
            return; // lewati rotasi biasa
        }
        // Menggantikan fungsi sprite.flipX dengan rotasi 3D
        if (movement.isMoving)
        {
            Vector3 lookDirection = new Vector3(move.x, 0f, move.y);
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleVisual()
    {
        if (dashTrail != null)
            dashTrail.emitting = movement.isDashing;

        Color target = movement.isDashing ? dashTint : Color.white;
        currentTint = Color.Lerp(currentTint, target, tintSmooth * Time.deltaTime);

        foreach (var r in renderers)
        {
            if (r is TrailRenderer) continue; // jangan ikut diwarnai
            r.GetPropertyBlock(block);
            block.SetColor(BaseColorId, currentTint);
            r.SetPropertyBlock(block);
        }
    }
}