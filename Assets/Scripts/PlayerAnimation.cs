using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 15f;

    private PlayerMovement movement;
    private Animator anim;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        // GetComponentInChildren berjaga-jaga jika Animator ada di child object
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        Vector2 move = movement.MoveInput;

        // Mengirim parameter ke Animator persis seperti kode 2D-mu
        anim.SetBool("isRunning", movement.isMoving);
        anim.SetFloat("animSpeed", movement.Speed / 5f);
        anim.SetBool("isJumping", !movement.isGround);

        // Menggantikan fungsi sprite.flipX dengan rotasi 3D
        if (movement.isMoving)
        {
            Vector3 lookDirection = new Vector3(move.x, 0f, move.y);
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}