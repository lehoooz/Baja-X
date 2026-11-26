using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject controlsPanel;

    public void PlayGame()
    {
        LoadingScreenManager.LoadScene("Level1");
    }

    public void OpenCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void ShowControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(true);
        else
            Debug.Log("Panel Controls belum dibuat");
    }

    public void HideControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
    }
z
    public void QuitGame()
    {
        Debug.Log("Keluar dari game");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}