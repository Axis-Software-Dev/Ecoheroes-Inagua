using UnityEngine;
using UnityEngine.UI;

public class TutorialScript : MonoBehaviour
{
    public RawImage tutorialImage;
    public SpeechBubbleAnimator speechBubbleAnimator;
    public GameObject tutorialPanel;
    private bool isTutorialActive = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     tutorialImage.enabled = false;
    }
    
    // Update is called once per frame
    void Update()
    {
        if(isTutorialActive)LookAtPlayer();
    }

    public void ActivateTutorial()
    {
        isTutorialActive = true;
        tutorialPanel?.SetActive(true);
        speechBubbleAnimator.AnimateIn();
        Invoke("ShowGif", 1f);
    }
    public void DeactivateTutorial()
    {
        isTutorialActive = false;
        speechBubbleAnimator.AnimateOut();
        tutorialImage.enabled = false;
    }
    private void LookAtPlayer()
    {
       transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }
    private void ShowGif()
    {
        tutorialImage.enabled = true;
    }
    
}
