using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public GameObject LoadingPanel;

    private void Start()
    {
        StartCoroutine(FadeFromWhite());
    }

    public void LoadScene(int sceneId)
    {
        StartCoroutine(LoadSceneAsync(sceneId));
    }

    IEnumerator LoadSceneAsync(int sceneId)
    {
        yield return StartCoroutine(FadeToWhite());
        LoadingPanel.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);

        while (!operation.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator FadeToWhite()
    {
        const float fadeDuration = 2f;
        float elapsed = 0f;
        Color fadeColor = Color.white;

        LoadingPanel.SetActive(true);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            LoadingPanel.GetComponent<Image>().color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        LoadingPanel.GetComponent<Image>().color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        LoadingPanel.SetActive(false);
    }

    private IEnumerator FadeFromWhite()
    {
        const float fadeDuration = 2f;
        float elapsed = 0f;
        Color fadeColor = Color.white;

        LoadingPanel.SetActive(true);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            LoadingPanel.GetComponent<Image>().color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        LoadingPanel.GetComponent<Image>().color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        LoadingPanel.SetActive(false);

    }

}
