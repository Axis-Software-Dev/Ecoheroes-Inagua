using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreditsController : MonoBehaviour
{
    [Header("UI References")]
    public Button creditsButton;
    public TextMeshProUGUI creditsText;
    public CanvasGroup buttonCanvasGroup;
    public CanvasGroup creditsCanvasGroup;

    [Header("Animation Settings")]
    public float fadeDuration = 1f;
    public float scrollSpeed = 50f;
    public float scrollDistance = 2000f;

    [Header("Credits Content")]
    [TextArea(10, 30)]
    public string creditsContent = "ECOHÉROES\n\n\nDesarrollado por:\nTu Equipo\n\n\nDiseño:\nEquipo de Diseño\n\n\nProgramación:\nEquipo de Programación\n\n\nAgradecimientos especiales:\nA todos los que hicieron posible este proyecto";

    private RectTransform creditsRectTransform;
    private bool isScrolling = false;

    private void Awake()
    {
        if (creditsText != null)
        {
            creditsRectTransform = creditsText.GetComponent<RectTransform>();
        }
        
        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(OnCreditsButtonClicked);
        }

        if (creditsText != null && !string.IsNullOrEmpty(creditsContent))
        {
            creditsText.text = creditsContent;
        }

        if (creditsCanvasGroup != null)
        {
            creditsCanvasGroup.alpha = 0f;
            creditsCanvasGroup.gameObject.SetActive(false);
        }

        if (buttonCanvasGroup != null)
        {
            buttonCanvasGroup.alpha = 1f;
        }
    }

    public void SetReferences(Button button, TextMeshProUGUI text, CanvasGroup btnGroup, CanvasGroup creditsGroup, string content)
    {
        creditsButton = button;
        creditsText = text;
        buttonCanvasGroup = btnGroup;
        creditsCanvasGroup = creditsGroup;
        creditsContent = content;

        if (creditsText != null)
        {
            creditsRectTransform = creditsText.GetComponent<RectTransform>();
        }
        
        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(OnCreditsButtonClicked);
        }

        if (creditsText != null && !string.IsNullOrEmpty(creditsContent))
        {
            creditsText.text = creditsContent;
        }

        if (creditsCanvasGroup != null)
        {
            creditsCanvasGroup.alpha = 0f;
            creditsCanvasGroup.gameObject.SetActive(false);
        }

        if (buttonCanvasGroup != null)
        {
            buttonCanvasGroup.alpha = 1f;
        }
    }

    private void OnCreditsButtonClicked()
    {
        if (!isScrolling)
        {
            StartCoroutine(PlayCreditsSequence());
        }
    }

    private IEnumerator PlayCreditsSequence()
    {
        isScrolling = true;

        yield return StartCoroutine(FadeOut(buttonCanvasGroup));
        
        buttonCanvasGroup.gameObject.SetActive(false);

        creditsRectTransform.anchoredPosition = Vector2.zero;
        creditsCanvasGroup.gameObject.SetActive(true);

        yield return StartCoroutine(FadeIn(creditsCanvasGroup));

        yield return StartCoroutine(ScrollCredits());

        yield return StartCoroutine(FadeOut(creditsCanvasGroup));
        
        creditsCanvasGroup.gameObject.SetActive(false);

        buttonCanvasGroup.gameObject.SetActive(true);
        
        yield return StartCoroutine(FadeIn(buttonCanvasGroup));

        isScrolling = false;
    }

    private IEnumerator FadeIn(CanvasGroup canvasGroup)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut(CanvasGroup canvasGroup)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
    }

    private IEnumerator ScrollCredits()
    {
        float distanceScrolled = 0f;
        Vector2 currentPosition = creditsRectTransform.anchoredPosition;

        while (distanceScrolled < scrollDistance)
        {
            float scrollAmount = scrollSpeed * Time.deltaTime;
            currentPosition.y += scrollAmount;
            creditsRectTransform.anchoredPosition = currentPosition;
            distanceScrolled += scrollAmount;
            
            yield return null;
        }
    }
}
