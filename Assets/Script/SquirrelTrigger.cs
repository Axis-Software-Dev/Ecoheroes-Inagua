using System.Collections;
using UnityEngine;
using TMPro;

public class SquirrelTrigger : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public GameObject interactableIndicatorPanel;
    public TextMeshProUGUI dialogueText;

    [Header("Animation and Audio")]
    public Animator squirrelAnimator;
    public string animationTriggerName = "Sad";

    [Header("Dialogue Content")]
    [TextArea(3, 10)]
    public string dialogueMessage = "Ayúdanos, nos quedamos sin agua en el estado y no sobreviviremos las plantas y animales si siguen así";
    public float dialogueDisplayTime = 5f;
    public float typingSpeed = 0.05f;

    [Header("Bubble Animation")]
    public SpeechBubbleAnimator speechBubbleAnimator;
    [Header("Alien slave")]
    public ShipController shipController;
    private bool isTyping = false;
    private Vector3 indicatorOriginalPosition;
    private bool isIndicatorActive = false;
    private AudioSource cry;

    private void Start()
    {
        if (interactableIndicatorPanel != null)
        {
            indicatorOriginalPosition = interactableIndicatorPanel.transform.localPosition;
            interactableIndicatorPanel.SetActive(false);
            StartCoroutine(ShowIndicatorAfterDelay());
        }
        cry = GetComponent<AudioSource>();
        cry.Pause();
    }

    private void Update()
    {
        if (isIndicatorActive && interactableIndicatorPanel != null)
        {
            float newY = indicatorOriginalPosition.y + Mathf.Sin(Time.time * 2) * .1f;
            interactableIndicatorPanel.transform.localPosition = new Vector3(
                indicatorOriginalPosition.x,
                newY,
                indicatorOriginalPosition.z
            );
        }
    }

    private IEnumerator ShowIndicatorAfterDelay()
    {
        yield return new WaitForSeconds(30f);

        if (interactableIndicatorPanel != null)
        {
            interactableIndicatorPanel.SetActive(true);
            isIndicatorActive = true;
        }
        if (cry) cry.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (interactableIndicatorPanel != null && isIndicatorActive)
        {
            interactableIndicatorPanel.SetActive(false);
            isIndicatorActive = false;
        }

        if (!isTyping)
        {
            Destroy(cry);
            isTyping = true;
            StartCoroutine(SquirrelSequence());
            Debug.Log("Squirrel triggered, starting coroutine");
        }
    }

    private IEnumerator SquirrelSequence()
    {
        squirrelAnimator.SetTrigger(animationTriggerName);
        Debug.Log("Trigger " + animationTriggerName + " set");

        dialoguePanel.SetActive(true);
        speechBubbleAnimator.AnimateIn();
        yield return StartCoroutine(TypeText(dialogueMessage));

        yield return new WaitForSeconds(dialogueDisplayTime);

        if (speechBubbleAnimator != null)
        {
            yield return StartCoroutine(UntypeText());
            speechBubbleAnimator.AnimateOut();
        }

        yield return new WaitForSeconds(speechBubbleAnimator.shrinkDuration);

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            Debug.Log("Dialog hidden");
            squirrelAnimator.SetTrigger("Idle");
            isTyping = false;
        }
        shipController.startAnimation = true;

        yield return new WaitForSeconds(4);

        squirrelAnimator.SetTrigger("asustado");
    }
    private IEnumerator TypeText(string textToType)
    {
        dialogueText.text = "";
        yield return new WaitForSeconds(2f);
        foreach (char letter in textToType.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator UntypeText()
    {
        while (dialogueText.text.Length > 0)
        {
            dialogueText.text = dialogueText.text.Substring(0, dialogueText.text.Length - 1);
            yield return new WaitForSeconds(typingSpeed / 4);
        }
    }
}