using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MovingPlatform : MonoBehaviour
{
    public enum MoveAxis { Horizontal, Vertical }

    [Header("Gerakan")]
    [SerializeField] private MoveAxis moveAxis = MoveAxis.Horizontal;
    [SerializeField] private float moveDistance = 4f;
    [SerializeField] private float moveSpeed = 2f;

    public Vector3 Velocity { get; private set; }

    private Rigidbody rb;
    private Vector3 startPos;
    private Vector3 moveDirection;
    private Rigidbody hangingPlayerRb;
    private PlayerLedgeWall hangingPlayerLedge;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        startPos = transform.position;
        moveDirection = moveAxis == MoveAxis.Horizontal ? Vector3.right : Vector3.up;
    }

    private void FixedUpdate()
    {
        float offset = Mathf.PingPong(Time.time * moveSpeed, moveDistance);
        Vector3 targetPos = startPos + moveDirection * offset;
        Vector3 delta = targetPos - rb.position;
        Velocity = delta / Time.fixedDeltaTime;
        rb.MovePosition(targetPos);

        // bawa player yang sedang bergantung ikut bergerak dengan platform
        if (hangingPlayerRb != null && hangingPlayerLedge != null)
        {
            if (hangingPlayerLedge.isGrabbingLedge)
            {
                hangingPlayerRb.MovePosition(hangingPlayerRb.position + delta);
            }
            else
            {
                hangingPlayerRb = null;
                hangingPlayerLedge = null;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryTrackHangingPlayer(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryTrackHangingPlayer(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (hangingPlayerRb != null && collision.rigidbody == hangingPlayerRb)
        {
            hangingPlayerRb = null;
            hangingPlayerLedge = null;
        }
    }

    private void TryTrackHangingPlayer(Collision collision)
    {
        if (hangingPlayerRb != null) return;
        PlayerLedgeWall ledge = collision.gameObject.GetComponent<PlayerLedgeWall>();
        if (ledge != null && ledge.isGrabbingLedge && collision.rigidbody != null)
        {
            hangingPlayerRb = collision.rigidbody;
            hangingPlayerLedge = ledge;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 dir = moveAxis == MoveAxis.Horizontal ? Vector3.right : Vector3.up;
        Vector3 start = Application.isPlaying ? startPos : transform.position;
        Vector3 end = start + dir * moveDistance;
        Gizmos.color = moveAxis == MoveAxis.Horizontal ? Color.green : Color.cyan;
        Gizmos.DrawLine(start, end);
        Vector3 perp = moveAxis == MoveAxis.Horizontal ? Vector3.up : Vector3.right;
        float arrowSize = 0.4f;
        Gizmos.DrawLine(end, end - dir * arrowSize + perp * arrowSize * 0.5f);
        Gizmos.DrawLine(end, end - dir * arrowSize - perp * arrowSize * 0.5f);
        Gizmos.DrawWireSphere(start, 0.25f);
        Gizmos.DrawWireSphere(end, 0.25f);
    }
}