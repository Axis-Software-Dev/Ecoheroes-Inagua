using System.Collections;
using UnityEngine;

public class TaladroAnimationScript : MonoBehaviour
{
    public GameObject dirtParticle;
    public GameObject rockParticle;
    public Animator drillAnimator;
    public Animator cableAnimator;
    public float[] intervals;
    public AudioSource drillSound;
    public AudioSource rockSound;

    private const int FIRST_INTERVAL_INDEX = 0;
    private const int SECOND_INTERVAL_INDEX = 1;

    private void Start()
    {
        if (drillAnimator != null)
        {
            drillAnimator.enabled = false;
        }

        if (cableAnimator != null)
        {
            cableAnimator.enabled = false;
        }

        if (dirtParticle != null)
        {
            dirtParticle.SetActive(false);
        }

        if (rockParticle != null)
        {
            rockParticle.SetActive(false);
        }
    }

    public void PlayDrillAnimation()
    {
        if (intervals == null || intervals.Length < 2)
        {
            Debug.LogWarning("Intervals array not properly configured.");
            return;
        }

        StartCoroutine(SetDrillAnimation());
    }

    private IEnumerator SetDrillAnimation()
    {
        yield return new WaitForSeconds(intervals[FIRST_INTERVAL_INDEX]);

        if (drillSound != null && !drillSound.isPlaying)
        {
            drillSound.Play();
        }

        if (drillAnimator != null)
        {
            drillAnimator.enabled = true;
        }

        if (cableAnimator != null)
        {
            cableAnimator.enabled = true;
        }

        yield return new WaitForSeconds(intervals[SECOND_INTERVAL_INDEX]);

        if (rockSound != null && !rockSound.isPlaying)
        {
            rockSound.Play();
        }

        if (dirtParticle != null)
        {
            dirtParticle.SetActive(true);
        }

        if (rockParticle != null)
        {
            rockParticle.SetActive(true);
        }
    }
}
