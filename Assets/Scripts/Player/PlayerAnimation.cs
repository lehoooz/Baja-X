using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float shootSmothAnimation = 5f;

    private PlayerMovement movement;
    private Animator anim;
    private PlayerShoot shoot;
    private int shootLayerIndex;
    private float shootWeight;



    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        anim = GetComponentInChildren<Animator>();
        shoot = GetComponent<PlayerShoot>();
        shootLayerIndex = anim.GetLayerIndex("Shoot");

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

        if (shoot != null && shootLayerIndex >= 0)
        {
            float targetWeight = shoot.isShooting ? 1f : 0f;
            shootWeight = Mathf.MoveTowards(shootWeight, targetWeight, Time.deltaTime * shootSmothAnimation);
            anim.SetLayerWeight(shootLayerIndex, shootWeight);
        }


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