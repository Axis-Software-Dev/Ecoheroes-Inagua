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

    private bool hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the player and the event hasn't happened yet.
        // We'll assume the player's XR Rig has a "Player" tag.
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
            // Set the flag so the event doesn't trigger again.
            hasBeenTriggered = true;
            Debug.Log("Player entered the squirrel's trigger area.");

            // Start the sequence of events. Coroutines are perfect for timed sequences.
            StartCoroutine(SquirrelSequence());
        }
    }

    // This is a coroutine that handles the timed events.
    private IEnumerator SquirrelSequence()
    {
        // Step 1: Play the animation
        if (squirrelAnimator != null)
        {
            squirrelAnimator.SetTrigger(animationTriggerName);
            Debug.Log("Playing squirrel animation: " + animationTriggerName);
        }

        // Step 2: Show the dialogue text
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            dialogueText.text = dialogueMessage;
            Debug.Log("Displaying dialogue: " + dialogueMessage);
        }

        // Step 3: Wait for a few seconds
        yield return new WaitForSeconds(dialogueDisplayTime);

        // Step 4: Hide the dialogue panel
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            Debug.Log("Hiding dialogue panel.");
        }

    }
}