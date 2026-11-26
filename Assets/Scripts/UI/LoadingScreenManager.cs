using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenManager : MonoBehaviour
{
    public static string sceneToLoad;
    public Slider progressBar;
    public TextMeshProUGUI progressText;

    public static void LoadScene(string sceneName)
    {
        sceneToLoad = sceneName;
        SceneManager.LoadScene("Loading");
    }

    void Start()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
            sceneToLoad = "Level1";
        StartCoroutine(LoadAsync());
    }

    IEnumerator LoadAsync()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);
        op.allowSceneActivation = false;
        while (!op.isDone)
        {
            float p = Mathf.Clamp01(op.progress / 0.9f);
            progressBar.value = p;
            progressText.text = (p * 100f).ToString("F0") + "%";
            if (op.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.5f);
                op.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}