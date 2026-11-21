using UnityEngine;

/// <summary>
/// Musuh Turret: diam di tempat, menembak lurus satu arah.
/// Kalau pemain masuk ke zona trigger di depannya, dia ancang-ancang
/// (berubah merah) lalu menembak satu peluru lurus ke arah tembak.
/// 
/// Cara pakai: pilih Arah di Inspector (Kanan/Kiri). Zona trigger,
/// arah tembak, dan posisi FirePoint diatur otomatis.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class EnemyTurret : EnemyBase
{
    public enum TurretDirection { Kanan, Kiri }

    [Header("Arah & Zona")]
    [SerializeField] private TurretDirection direction = TurretDirection.Kanan;
    [SerializeField] private float triggerRange = 10f;
    [SerializeField] private float triggerHeight = 3f;

    [Header("Serang")]
    [SerializeField] private float cooldownTime = 2f;
    [SerializeField] private float telegraphTime = 0.4f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    private Vector2 fireDirection = Vector2.right;
    private Renderer[] rends;
    private Color[] baseColors;
    private Animator anim;
    private float cooldownTimer;
    private float telegraphTimer;
    private bool isTelegraphing;
    private bool playerInTrigger;
    private Transform model;    // turret-rigged (objek Animator)
    private float modelYaw = -90f; // Y rotasi target model: +90 Kanan, -90 Kiri

    protected override void Awake()
    {
        base.Awake();
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        ApplyDirection();
        anim = GetComponentInChildren<Animator>();
    }

    // Dipanggil Unity setiap kali ada nilai berubah di Inspector.
    // Jadi zona trigger & FirePoint langsung menyesuaikan saat kamu ganti Arah.
    private void OnValidate()
    {
        ApplyDirection();
    }

    private void ApplyDirection()
    {
        float sign = direction == TurretDirection.Kanan ? 1f : -1f;
        fireDirection = new Vector2(sign, 0f);

        // Cari collider trigger yang sudah ada, kalau belum ada buat baru
        BoxCollider trigger = null;
        foreach (BoxCollider col in GetComponents<BoxCollider>())
        {
            if (col.isTrigger) { trigger = col; break; }
        }
        if (trigger == null)
        {
            trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
        }
        trigger.center = new Vector3(sign * triggerRange * 0.5f, 0.5f, 0f);
        trigger.size = new Vector3(triggerRange, triggerHeight, 1f);

        // Cari / buat FirePoint, posisikan di sisi arah tembak
        if (firePoint == null)
        {
            Transform found = transform.Find("FirePoint");
            if (found != null)
                firePoint = found;
            else
            {
                GameObject fp = new GameObject("FirePoint");
                fp.transform.SetParent(transform, false);
                firePoint = fp.transform;
            }
        }
        firePoint.localPosition = new Vector3(sign * 0.6f, 0.2f, 0f);

        // Putar model menghadap arah tembak (otomatis saat ganti Arah di Inspector)
        float yaw = direction == TurretDirection.Kanan ? 90f : -90f;
        modelYaw = yaw;
        Animator m = GetComponentInChildren<Animator>();
        if (m != null)
        {
            model = m.transform;
            Vector3 e = model.localEulerAngles;
            model.localEulerAngles = new Vector3(e.x, yaw, e.z);
        }
    }

    private void Start()
    {
        // Model sekarang jadi child (FBX), jadi cari semua Renderer di bawah root.
        // GetComponent<Renderer>() lama hanya mencari di root — tidak ketemu
        // kalau modelnya anak, dan telegraph merah tidak akan muncul.
        rends = GetComponentsInChildren<Renderer>();
        baseColors = new Color[rends.Length];
        for (int i = 0; i < rends.Length; i++)
            baseColors[i] = rends[i].material.color;
    }

    /// <summary> Ubah warna semua bagian model (untuk telegraph merah). </summary>
    private void SetColor(Color c)
    {
        if (rends == null) return;
        foreach (Renderer r in rends)
            r.material.color = c;
    }

    /// <summary> Kembalikan warna asli tiap bagian model. </summary>
    private void RestoreColor()
    {
        if (rends == null) return;
        for (int i = 0; i < rends.Length; i++)
            rends[i].material.color = baseColors[i];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerMovement>() != null)
            playerInTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerMovement>() != null)
            playerInTrigger = false;
    }

    private void Update()
    {
        if (anim != null)
            anim.SetBool("isAlerted", isTelegraphing);
        // Pemain di luar zona trigger: reset, tidak menyerang
        if (!playerInTrigger)
        {
            isTelegraphing = false;
            cooldownTimer = 0f;
            RestoreColor();
            return;
        }

        if (isTelegraphing)
        {
            telegraphTimer -= Time.deltaTime;
            if (telegraphTimer <= 0f)
            {
                Fire();
                isTelegraphing = false;
                cooldownTimer = cooldownTime;
                RestoreColor();
            }
        }
        else
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isTelegraphing = true;
                telegraphTimer = telegraphTime;
                SetColor(Color.red);
            }
        }
    }

    private void Fire()
    {
        if (projectilePrefab == null || firePoint == null)
            return;

        if (anim != null)
            anim.SetTrigger("Shoot");
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile p = bullet.GetComponent<Projectile>();
        if (p == null)
            return;

        // Tembak lurus satu arah (tidak mengejar pemain)
        Vector3 dir = new Vector3(fireDirection.x, fireDirection.y, 0f);
        p.Launch(dir.normalized);
    }
    private void LateUpdate()
    {
        // Animator menimpa rotasi model jadi konstan tiap frame (kurva di FBX).
        // Kembalikan Y ke arah tembak supaya model tetap menghadap direction yang dipilih.
        if (model != null)
        {
            Vector3 e = model.localEulerAngles;
            model.localEulerAngles = new Vector3(e.x, modelYaw, e.z);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Garis arah tembak (merah)
        Gizmos.color = Color.red;
        Vector3 dir = new Vector3(fireDirection.x, fireDirection.y, 0f).normalized;
        Gizmos.DrawLine(transform.position, transform.position + dir * 3f);
    }
}
