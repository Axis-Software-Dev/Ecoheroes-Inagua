using System.Collections;
using UnityEngine;


//If someone reads this. i want to say i needed to do this script due to the awful way this animation is orginnized
public class TaladroAnimationScript : MonoBehaviour
{
    public GameObject dirtParticle;
    public GameObject rockParticle;
    public Animator drillAnimator;
    public Animator cableAnimator;
    public float[] intervals;
    public AudioSource drillSound;
    public AudioSource rockSound;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if(drillAnimator!=null)drillAnimator.enabled = false;
        if (cableAnimator != null) cableAnimator.enabled = false;
        dirtParticle?.SetActive(false);
        rockParticle?.SetActive(false); 
    }

    public void PlayDrillAnimation()
    {
        StartCoroutine(SetDrillAnimation(intervals));
    }

    private IEnumerator SetDrillAnimation(float[] DelayIntervals)
    {
        yield return new WaitForSeconds(intervals[0]);
        if(!drillSound.isPlaying)drillSound?.Play();
        if (drillAnimator != null) drillAnimator.enabled = true;
        if (cableAnimator != null) cableAnimator.enabled = true;
        yield return new WaitForSeconds(intervals[1]);
        if(!rockSound.isPlaying)rockSound?.Play();
        dirtParticle?.SetActive(true);
        rockParticle?.SetActive(true);
    }
}
