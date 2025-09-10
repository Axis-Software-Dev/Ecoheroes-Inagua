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
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            dialogueText.text = dialogueMessage;
        }

        // Step 3: Wait for a few seconds
        yield return new WaitForSeconds(dialogueDisplayTime);

        // Step 4: Hide the dialogue panel
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            squirrelAnimator.SetTrigger("Idle");
        }

    }
}