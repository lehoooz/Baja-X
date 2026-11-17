using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f;

    private PlayerMovement movement;
    private Animator anim;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        Vector2 move = movement.MoveInput;

        anim.SetBool("isRunning", movement.isMoving);
        anim.SetFloat("animSpeed", movement.Speed / 5f);
        anim.SetBool("isJumping", !movement.isGround && !movement.isDashing && !movement.isGrabbingLedge && !movement.isWallSliding);
        anim.SetBool("isDashing", movement.isDashing);
        anim.SetBool("isHanging", movement.isGrabbingLedge);
        anim.SetBool("isWallSliding", movement.isWallSliding);

        if (movement.isWallSliding)
        {
            Vector3 away = new Vector3(-move.x, 0f, -move.y);
            if (away != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(away);
            return;
        }

        if (movement.isMoving)
        {
            Vector3 lookDirection = new Vector3(move.x, 0f, move.y);
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}