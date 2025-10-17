using System.Collections;
using TMPro;
using UnityEngine;

public class HUDHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private Animator animator;

    [Header("Settings")]
    [SerializeField] private float typewriterSpeed = 0.05f;

    private string currentFullText = "";
    private bool isShowingText = false;
    private bool isHidingText = false;
    private Coroutine activeCoroutine;

    private void Awake()
    {
        if (textComponent == null) throw new System.Exception("Text component required to invoke HUD");
    }

    public void ShowText(string text)
    {
        if (isShowingText) return;

        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        currentFullText = text;
        activeCoroutine = StartCoroutine(TypewriterForward());
    }

    public void HideText()
    {
        if (isHidingText) return;

        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(TypewriterReverse());
    }

    private IEnumerator TypewriterForward()
    {
        Debug.Log("TypewriterForward coroutine started");
        animator.SetTrigger("open");
        new WaitForSeconds(2);

        isShowingText = true;
        isHidingText = false;

        textComponent.text = "";

        for (int i = 0; i <= currentFullText.Length; i++)
        {
            textComponent.text = currentFullText.Substring(0, i);
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isShowingText = false;
        activeCoroutine = null;
    }

    private IEnumerator TypewriterReverse()
    {
        Debug.Log("TypewriterReverse coroutine started");

        isHidingText = true;
        isShowingText = false;

        currentFullText = textComponent.text;

        for (int i = currentFullText.Length; i >= 0; i--)
        {
            textComponent.text = currentFullText.Substring(0, i);
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isHidingText = false;
        activeCoroutine = null;

        animator.SetTrigger("close");
    }
}
