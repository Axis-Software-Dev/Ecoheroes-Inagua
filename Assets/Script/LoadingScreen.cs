using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public GameObject LoadingPanel;

    private Image loadingImage;
    private const Color FADE_COLOR = Color.white;
    private const float FADE_DURATION = 2f;

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

    private IEnumerator LoadSceneAsync(int sceneId)
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
        if (loadingImage == null) yield break;

        LoadingPanel.SetActive(true);
        loadingImage.color = new Color(FADE_COLOR.r, FADE_COLOR.g, FADE_COLOR.b, 0f);

        float elapsed = 0f;
        while (elapsed < FADE_DURATION)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / FADE_DURATION);
            loadingImage.color = new Color(FADE_COLOR.r, FADE_COLOR.g, FADE_COLOR.b, alpha);
            yield return null;
        }

        loadingImage.color = new Color(FADE_COLOR.r, FADE_COLOR.g, FADE_COLOR.b, 1f);
    }

    private IEnumerator FadeFromWhite()
    {
        if (loadingImage == null) yield break;

        LoadingPanel.SetActive(true);
        loadingImage.color = new Color(FADE_COLOR.r, FADE_COLOR.g, FADE_COLOR.b, 1f);

        float elapsed = 0f;
        while (elapsed < FADE_DURATION)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / FADE_DURATION);
            loadingImage.color = new Color(FADE_COLOR.r, FADE_COLOR.g, FADE_COLOR.b, alpha);
            yield return null;
        }

        loadingImage.color = new Color(FADE_COLOR.r, FADE_COLOR.g, FADE_COLOR.b, 0f);
        LoadingPanel.SetActive(false);
    }
}
