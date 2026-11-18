using UnityEngine;

/// <summary>
/// Mengatur alur game: respawn point, respawn saat pemain mati, stage clear.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Respawn")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform initialSpawnPoint; // boleh kosong, pakai posisi awal pemain

    private Vector3 respawnPoint;
    private Health playerHealth;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        respawnPoint = initialSpawnPoint != null ? initialSpawnPoint.position : player.position;
        playerHealth = player.GetComponent<Health>();
        playerHealth.OnDeath.AddListener(RespawnPlayer);
    }

    public void SetRespawnPoint(Vector3 point)
    {
        respawnPoint = point;
    }

    private void RespawnPlayer()
    {
        // Pindah pemain ke checkpoint + HP penuh
        player.position = respawnPoint;
        playerHealth.ResetHealth();
        // Nanti: reset boss, reset musuh di room, efek fade
    }

    public void StageClear()
    {
        Debug.Log("STAGE CLEAR!");
        // Nanti: tampilkan UI Stage Clear
    }
}
