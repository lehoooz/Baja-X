using UnityEngine;

/// <summary>
/// Mengatur alur game: game over saat pemain mati, stage clear saat boss mati.
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

    private void GameOver()
    {
        if (isGameEnded) return;
        isGameEnded = true;
        // Pemain langsung hilang begitu mati — tidak melayang, tidak ada animasi.
        // Aman karena tombol restart me-reload scene dan player dibuat ulang.
        if (player != null) Destroy(player.gameObject);
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
}