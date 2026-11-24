using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BreakablePlatform : MonoBehaviour
{
    [Header("Hancur")]
    [SerializeField] private float breakDelay = 0.8f;
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private Color warningColor = Color.red;

    private Collider col;
    private Renderer rend;
    private Material mat;
    private Color originalColor;
    private float breakTimer;
    private float respawnTimer;
    private bool isPlayerOn;
    private bool isBroken;

    private void Awake()
    {
        col = GetComponent<Collider>();
        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
            originalColor = mat.color;
        }
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
            // peringatan: warna berubah makin merah saat mendekati hancur
            if (mat != null)
                mat.color = Color.Lerp(originalColor, warningColor, breakTimer / breakDelay);
            if (breakTimer >= breakDelay)
                Break();
        }
        else
        {
            breakTimer = 0f;
            if (mat != null)
                mat.color = originalColor;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerMovement>() != null)
            isPlayerOn = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerMovement>() != null)
            isPlayerOn = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerMovement>() != null)
            isPlayerOn = false;
    }

    private void Break()
    {
        isBroken = true;
        isPlayerOn = false;
        col.enabled = false;
        if (rend != null) rend.enabled = false;
        respawnTimer = respawnDelay;
    }

    private void Respawn()
    {
        isBroken = false;
        breakTimer = 0f;
        col.enabled = true;
        if (rend != null) rend.enabled = true;
        if (mat != null) mat.color = originalColor;
    }
}