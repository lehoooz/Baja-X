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
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (restartButton != null) restartButton.onClick.AddListener(RestartScene);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

        // Tombol di panel Game Over sekarang me-restart scene dari awal
        // (bukan teleport ke checkpoint) — sama seperti tombol restart di Stage Clear.
        if (respawnButton != null) respawnButton.onClick.AddListener(RestartScene);
        if (retryButton != null) retryButton.onClick.AddListener(RestartScene);
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