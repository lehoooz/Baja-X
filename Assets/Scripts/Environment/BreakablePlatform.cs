using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BreakablePlatform : MonoBehaviour
{
    [Header("Hancur")]
    [SerializeField] private float breakDelay = 0.8f;
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private float shakeAmount = 0.05f;

    private Collider col;
    private Renderer rend;
    private Vector3 startPos;
    private float breakTimer;
    private float respawnTimer;
    private bool isPlayerOn;
    private bool isBroken;

    private void Awake()
    {
        col = GetComponent<Collider>();
        rend = GetComponentInChildren<Renderer>();
        startPos = transform.position;
    }

    private void Update()
    {
        if (isBroken)
        {
            if (respawnDelay > 0f)
            {
                respawnTimer -= Time.deltaTime;
                if (respawnTimer <= 0f)
                    Respawn();
            }
            return;
        }

        if (isPlayerOn)
        {
            breakTimer += Time.deltaTime;
            Vector3 shake = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * shakeAmount * (breakTimer / breakDelay);
            transform.position = startPos + shake;
            if (breakTimer >= breakDelay)
                Break();
        }
        else
        {
            breakTimer = 0f;
            transform.position = startPos;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsPlayerFromTopOrSide(collision))
            isPlayerOn = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (IsPlayerFromTopOrSide(collision))
            isPlayerOn = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerMovement>() != null)
            isPlayerOn = false;
    }

    private bool IsPlayerFromTopOrSide(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerMovement>() == null) return false;
        if (collision.contacts.Length == 0) return false;
        return collision.contacts[0].normal.y > -0.5f;
    }

    private void Break()
    {
        isBroken = true;
        isPlayerOn = false;
        col.enabled = false;
        if (rend != null) rend.enabled = false;
        transform.position = startPos;
        respawnTimer = respawnDelay;
    }

    private void Respawn()
    {
        isBroken = false;
        breakTimer = 0f;
        col.enabled = true;
        if (rend != null) rend.enabled = true;
    }
}