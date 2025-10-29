using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public GameObject LoadingPanel;

    private Image loadingImage;
    private Color fadeColor = Color.white;

    private void Awake()
    {
        if (LoadingPanel != null)
        {
            loadingImage = LoadingPanel.GetComponent<Image>();
            if (loadingImage == null)
            {
                Debug.LogError("LoadingPanel is missing Image component!");
            }
        }
        else
        {
            Debug.LogError("LoadingPanel is not assigned!");
        }
    }

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

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            yield return null;
        }

        yield return Resources.UnloadUnusedAssets();

        System.GC.Collect();
    }

    private IEnumerator FadeToWhite()
    {
        const float fadeDuration = 2f;
        float elapsed = 0f;

        if (loadingImage == null) yield break;

        LoadingPanel.SetActive(true);
        loadingImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            loadingImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        loadingImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
    }

    private IEnumerator FadeFromWhite()
    {
        const float fadeDuration = 2f;
        float elapsed = 0f;

        if (loadingImage == null) yield break;

        LoadingPanel.SetActive(true);
        loadingImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            loadingImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        loadingImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        LoadingPanel.SetActive(false);
    }
}
