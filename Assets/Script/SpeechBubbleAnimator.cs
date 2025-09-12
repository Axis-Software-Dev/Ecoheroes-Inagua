using System.Collections;
using UnityEngine;

public class SpeechBubbleAnimator : MonoBehaviour
{
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private void Awake()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
    }
    [Header("Animation Settings")]
    public Vector3 growScale = new Vector3(0.003f, 0.003f, 0.003f);
    public float growDuration = 1f;
    public float shakeDuration = 1f;
    public float shakeIntensity = 0.005f;
    public float shrinkDuration = 1f;



    public void AnimateIn()
    {
        StopAllCoroutines();
        StartCoroutine(GrowAndShake());
    }

    public void AnimateOut()
    {
        StopAllCoroutines();
        StartCoroutine(Shrink());
    }

    private IEnumerator GrowAndShake()
    {

        float timer = 0f;
        while (timer < growDuration)
        {
            float progress = timer / growDuration;
            transform.localScale = Vector3.Lerp(originalScale, growScale, progress);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = growScale;


        float shakeTimer = 0f;
        while (shakeTimer < shakeDuration)
        {
            transform.localPosition += Random.insideUnitSphere * shakeIntensity;
            shakeTimer += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPosition;
    }

    private IEnumerator Shrink()
    {
        float timer = 0f;
        while (timer < shrinkDuration)
        {
            float progress = timer / shrinkDuration;
            transform.localScale = Vector3.Lerp(growScale, Vector3.zero, progress);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector3.zero;
    }
}