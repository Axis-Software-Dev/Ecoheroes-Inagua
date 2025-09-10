using System.Collections;
using UnityEngine;
using TMPro;

public class SquirrelTrigger : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Animation and Audio")]
    public Animator squirrelAnimator;
    public string animationTriggerName = "Sad";

    [Header("Dialogue Content")]
    [TextArea(3, 10)]
    public string dialogueMessage = "Ayúdanos, nos quedamos sin agua en el estado y no sobreviviremos las plantas y animales si siguen así";
    public float dialogueDisplayTime = 5f;

    private bool hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
            hasBeenTriggered = true;
            StartCoroutine(SquirrelSequence());
        }
    }

    private IEnumerator SquirrelSequence()
    {
        if (squirrelAnimator != null)
        {
            squirrelAnimator.SetTrigger(animationTriggerName);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            dialogueText.text = dialogueMessage;
        }

        yield return new WaitForSeconds(dialogueDisplayTime);

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
}