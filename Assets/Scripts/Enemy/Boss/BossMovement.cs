using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Transform player;
    private float walkSpeed;

    public void Setup(float walkSpeed)
    {
        this.walkSpeed = walkSpeed;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
            player = GameManager.Instance.Player;
    }

    public void FacePlayer()
    {
        if (player == null) return;
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        if (dir != 0f)
            transform.rotation = Quaternion.Euler(0f, dir > 0f ? 90f : -90f, 0f);
    }

    public void WalkTowardPlayer()
    {
        if (player == null) return;
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        if (dir != 0f)
            rb.linearVelocity = new Vector3(dir * walkSpeed, rb.linearVelocity.y, 0f);
        else
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }
}