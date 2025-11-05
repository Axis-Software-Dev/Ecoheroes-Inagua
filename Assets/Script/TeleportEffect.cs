using UnityEngine;
using System.Collections;

public class TeleportEffect : MonoBehaviour
{
    private Vector3 originalScale;
    [HideInInspector]
    public Vector3 originalPosition;
    private float cylinderHeight;

    void Start()
    {
        gameObject.SetActive(false);
        originalScale = transform.localScale;
        originalPosition = transform.position;
        cylinderHeight = originalScale.y;
    }

    public void PlayCurtainEffect()
    {
       
        gameObject.SetActive(true);
        StartCoroutine(CurtainAnimation());
    }
    public void PlayCurtainEffectOnPosition()
    {
        if(transform.parent!=null)originalPosition = transform.parent.transform.position;
        gameObject.SetActive(true);
        StartCoroutine(CurtainAnimation());
    }
    private IEnumerator CurtainAnimation()
    {
        float appearDuration = .2f;
        float stayDuration = .7f;
        float disappearDuration = .2f;

        transform.localScale = new Vector3(originalScale.x, 0f, originalScale.z);
        transform.position = originalPosition + Vector3.up * (cylinderHeight / 2f);

        yield return StartCoroutine(AppearFromTop(appearDuration));

        yield return new WaitForSeconds(stayDuration);

        yield return StartCoroutine(DisappearFromBottom(disappearDuration));
    }

    private IEnumerator AppearFromTop(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.localScale = new Vector3(
                originalScale.x,
                originalScale.y * t,
                originalScale.z
            );

            transform.position = originalPosition + Vector3.up * (cylinderHeight / 2f) * (1f - t);

            yield return null;
        }

        transform.localScale = originalScale;
        transform.position = originalPosition;
    }

    private IEnumerator DisappearFromBottom(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.localScale = new Vector3(
                originalScale.x,
                originalScale.y * (1f - t),
                originalScale.z
            );

            transform.position = originalPosition + Vector3.up * (cylinderHeight / 2f) * t;

            yield return null;
        }

        transform.localScale = new Vector3(originalScale.x, 0f, originalScale.z);
        gameObject.SetActive(false);
    }
}
