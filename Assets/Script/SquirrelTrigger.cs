using System.Collections;
using UnityEngine;
using TMPro;

public class SquirrelTrigger : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel; // Drag UI Panel here
    public TextMeshProUGUI dialogueText; // Drag TextMeshPro component here

    [Header("Animation and Audio")]
    public Animator squirrelAnimator; // Drag the Animator component here
    public string animationTriggerName = "Sad"; // Name of the animation trigger

    [Header("Dialogue Content")]
    [TextArea(3, 10)]
    public string dialogueMessage = "Ayúdanos, nos quedamos sin agua en el estado y no sobreviviremos las plantas y animales si siguen así";
    public float dialogueDisplayTime = 5f;
    public float typingSpeed = 0.05f;

    [Header("Bubble Animation")]
    public SpeechBubbleAnimator speechBubbleAnimator;

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(SquirrelSequence());
    }

    // This is a coroutine that handles the timed events.
    private IEnumerator SquirrelSequence()
    {
        // Step 1: Play the animation
        if (squirrelAnimator != null)
        {
            squirrelAnimator.SetTrigger(animationTriggerName);
        }

        // Step 2: Show the dialogue text
        if (dialoguePanel != null && speechBubbleAnimator != null)
        {
            dialoguePanel.SetActive(true);
            speechBubbleAnimator.AnimateIn();
            yield return StartCoroutine(TypeText(dialogueMessage));
        }

        // Step 3: Wait for a few seconds
        yield return new WaitForSeconds(dialogueDisplayTime);

        if (speechBubbleAnimator != null)
        {
            yield return StartCoroutine(UntypeText());
            speechBubbleAnimator.AnimateOut();
        }

        yield return new WaitForSeconds(speechBubbleAnimator.shrinkDuration);

        // Step 4: Hide the dialogue panel
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            squirrelAnimator.SetTrigger("Idle");
        }

    }
    private IEnumerator TypeText(string textToType)
    {
        dialogueText.text = "";
        yield return new WaitForSeconds(.3f);
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