using UnityEngine;
using UnityEngine.UI;

public class TutorialScript : MonoBehaviour
{
    public RawImage tutorialImage;
    public SpeechBubbleAnimator speechBubbleAnimator;
    public GameObject tutorialPanel;

    private bool isTutorialActive = false;
    private Transform playerCamera;

    private const float SHOW_IMAGE_DELAY = 1f;

    private void Start()
    {
        if (tutorialImage != null)
        {
            tutorialImage.enabled = false;
        }

        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (isTutorialActive)
        {
            LookAtPlayer();
        }
    }

    public void ActivateTutorial()
    {
        isTutorialActive = true;

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }

        if (speechBubbleAnimator != null)
        {
            speechBubbleAnimator.AnimateIn();
        }

        Invoke(nameof(ShowTutorialImage), SHOW_IMAGE_DELAY);
    }

    public void DeactivateTutorial()
    {
        isTutorialActive = false;

        if (speechBubbleAnimator != null)
        {
            speechBubbleAnimator.AnimateOut();
        }

        if (tutorialImage != null)
        {
            tutorialImage.enabled = false;
        }
    }

    private void LookAtPlayer()
    {
        if (playerCamera == null)
        {
            if (Camera.main != null)
            {
                playerCamera = Camera.main.transform;
            }
            else
            {
                return;
            }
        }

        Vector3 direction = transform.position - playerCamera.position;
        
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void ShowTutorialImage()
    {
        if (tutorialImage != null)
        {
            tutorialImage.enabled = true;
        }
    }
}
