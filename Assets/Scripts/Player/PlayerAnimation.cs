using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Dash Visual")]
    [SerializeField] private TrailRenderer dashTrail;

    private Color originalColor;
    private PlayerMovement movement;
    private Animator anim;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        // GetComponentInChildren berjaga-jaga jika Animator ada di child object
        anim = GetComponentInChildren<Animator>();
        dashTrail = GetComponent<TrailRenderer>();

        dashTrail.emitting = false;
    }

    private void Update()
    {
        Vector2 move = movement.MoveInput;

        // Mengirim parameter ke Animator persis seperti kode 2D-mu
        anim.SetBool("isRunning", movement.isMoving);
        anim.SetFloat("animSpeed", movement.Speed / 5f);
        anim.SetBool("isJumping", !movement.isGround);
        //anim.SetBool("isDashing", movement.isDashing);

        HandleVisual();

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
        {
            dashTrail.emitting = movement.isDashing;
        }
    }
}