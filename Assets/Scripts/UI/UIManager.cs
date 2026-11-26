using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject stageClearPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Tombol Stage Clear")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Tombol Game Over")]
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button retryButton;

    private void Awake()
    {
        if (stageClearPanel != null) stageClearPanel.SetActive(false);
        else Debug.LogError("[UI] StageClearPanel belum di-drag ke UIManager!");

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        else Debug.LogError("[UI] GameOverPanel belum di-drag ke UIManager!");

        if (restartButton != null) restartButton.onClick.AddListener(RestartScene);
        else Debug.LogError("[UI] RestartButton belum di-drag ke UIManager!");

        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        else Debug.LogError("[UI] QuitButton belum di-drag ke UIManager!");

        if (respawnButton != null) respawnButton.onClick.AddListener(RespawnAtCheckpoint);
        else Debug.LogError("[UI] RespawnButton belum di-drag ke UIManager!");

        if (retryButton != null) retryButton.onClick.AddListener(RestartScene);
        else Debug.LogError("[UI] RetryButton belum di-drag ke UIManager!");
    }

    public void ShowStageClear()
    {
        if (stageClearPanel == null) return;
        stageClearPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ShowGameOver()
    {
        if (gameOverPanel == null) return;
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RespawnAtCheckpoint()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        Time.timeScale = 1f;
        if (GameManager.Instance != null) GameManager.Instance.RespawnPlayer();
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}