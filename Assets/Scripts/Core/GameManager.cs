using UnityEngine;

/// <summary>
/// Mengatur alur game: respawn point, game over saat pemain mati, stage clear saat boss mati.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Respawn")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform initialSpawnPoint; // boleh kosong, pakai posisi awal pemain

    [Header("UI")]
    [SerializeField] private UIManager uiManager;

    [Header("Win / Lose")]
    [SerializeField] private float winDelay = 1.5f;  // jeda sebelum panel Stage Clear muncul
    [SerializeField] private float loseDelay = 1f;   // jeda sebelum panel Game Over muncul

    public Transform Player => player;

    private Vector3 respawnPoint;
    private Health playerHealth;
    private bool isGameEnded; // true kalau sudah menang/kalah, supaya panel tidak dobel

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
        playerHealth.OnDeath.AddListener(GameOver);
    }

    public void SetRespawnPoint(Vector3 point)
    {
        respawnPoint = point;
    }

    public void RespawnPlayer()
    {
        // Pindah pemain ke checkpoint + HP penuh
        player.position = respawnPoint;
        playerHealth.ResetHealth();
        isGameEnded = false;
        SetPlayerControl(true);
        // Nanti: reset boss, reset musuh di room, efek fade
    }

    private void GameOver()
    {
        if (isGameEnded) return;
        isGameEnded = true;
        SetPlayerControl(false);
        StartCoroutine(ShowGameOverAfterDelay());
    }

    private System.Collections.IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSeconds(loseDelay);
        if (uiManager != null) uiManager.ShowGameOver();
        else Debug.LogError("[GameManager] UIManager belum di-drag di Inspector GameManager!");
    }

    public void StageClear()
    {
        if (isGameEnded) return;
        isGameEnded = true;
        Debug.Log("STAGE CLEAR!");
        StartCoroutine(ShowStageClearAfterDelay());
    }

    private System.Collections.IEnumerator ShowStageClearAfterDelay()
    {
        yield return new WaitForSeconds(winDelay);
        if (uiManager != null) uiManager.ShowStageClear();
        else Debug.LogError("[GameManager] UIManager belum di-drag di Inspector GameManager!");
    }

    private void SetPlayerControl(bool enabled)
    {
        var movement = player.GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = enabled;
        var shoot = player.GetComponent<PlayerShoot>();
        if (shoot != null) shoot.enabled = enabled;
        var dash = player.GetComponent<PlayerDash>();
        if (dash != null) dash.enabled = enabled;
        var ledgeWall = player.GetComponent<PlayerLedgeWall>();
        if (ledgeWall != null) ledgeWall.enabled = enabled;
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            // Gravitasi pemain itu custom dari PlayerMovement (useGravity = false).
            // Waktu PlayerMovement dimatikan, gravitasi ikut mati dan pemain melayang.
            // Jadi selama pemain mati, nyalakan gravitasi bawaan Unity supaya jatuh ke tanah.
            // Waktu hidup lagi, matikan lagi supaya gravitasi custom yang bekerja.
            rb.useGravity = !enabled;
        }
    }
}