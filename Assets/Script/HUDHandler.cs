using System.Collections;
using TMPro;
using UnityEngine;

public class HUDHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] 
    private TextMeshProUGUI textComponent;
    [SerializeField] 
    private Animator animator;

    [Header("Settings")]
    [SerializeField] 
    private float typewriterSpeed = 0.05f;

    private string currentFullText = "";
    private bool isShowingText = false;
    private bool isHidingText = false;
    private Coroutine activeCoroutine;

    private const float ANIMATION_DELAY = 2f;

    private void Awake()
    {
        if (textComponent == null)
        {
            throw new System.Exception("Text component required to invoke HUD");
        }
    }

    public void ShowText(string text)
    {
        if (isShowingText || activeCoroutine != null || string.IsNullOrEmpty(text))
        {
            return;
        }

        currentFullText = text;
        activeCoroutine = StartCoroutine(TypewriterForward());
    }

    public void HideText()
    {
        if (isHidingText || activeCoroutine != null)
        {
            return;
        }

        activeCoroutine = StartCoroutine(TypewriterReverse());
    }

    private IEnumerator TypewriterForward()
    {
        if (animator != null)
        {
            animator.SetTrigger("open");
        }

        yield return new WaitForSeconds(ANIMATION_DELAY);

        isShowingText = true;
        isHidingText = false;

        if (textComponent != null)
        {
            textComponent.text = "";

            for (int i = 0; i <= currentFullText.Length; i++)
            {
                textComponent.text = currentFullText.Substring(0, i);
                yield return new WaitForSeconds(typewriterSpeed);
            }
        }

        isShowingText = false;
        activeCoroutine = null;
    }

    private IEnumerator TypewriterReverse()
    {
        isHidingText = true;
        isShowingText = false;

        if (textComponent != null)
        {
            currentFullText = textComponent.text;

            for (int i = currentFullText.Length; i >= 0; i--)
            {
                textComponent.text = currentFullText.Substring(0, i);
                yield return new WaitForSeconds(typewriterSpeed);
            }
        }

        isHidingText = false;
        activeCoroutine = null;

        if (animator != null)
        {
            animator.SetTrigger("close");
        }
    }
}
