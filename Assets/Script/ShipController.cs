using UnityEngine;
using System.Collections;

public class SpaceshipController : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip arrivalSound;
    public AudioClip departureSound;

    [Header("Movement")]
    public Transform arrivalPoint;   // where the ship arrives
    public Transform departurePoint; // where it leaves
    public float moveSpeed = 5f;

    [Header("Hover/Shake")]
    public float hoverTime = 3f;
    public float shakeIntensity = 0.1f;
    public float shakeSpeed = 20f;

    private Vector3 originalPosition;

    private void Start()
    {
        // Start the whole sequence
        StartCoroutine(SpaceshipSequence());
    }

    private IEnumerator SpaceshipSequence()
    {
        // Play arrival sound
        if (audioSource && arrivalSound)
        {
            audioSource.clip = arrivalSound;
            audioSource.Play();
        }

        // Arrive
        yield return MoveTo(arrivalPoint.position);

        // First shake
        yield return Shake(1f);

        // Hover for a while
        originalPosition = transform.position;
        yield return new WaitForSeconds(hoverTime);

        // Second shake
        yield return Shake(1f);

        // Play departure sound
        if (audioSource && departureSound)
        {
            audioSource.clip = departureSound;
            audioSource.Play();
        }

        // Leave
        yield return MoveTo(departurePoint.position);

        Debug.Log("Spaceship sequence finished 🚀");
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }
    }

    private IEnumerator Shake(float duration)
    {
        float elapsed = 0f;
        Vector3 basePos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
            float offsetY = Mathf.Cos(Time.time * shakeSpeed) * shakeIntensity;

            transform.position = basePos + new Vector3(offsetX, offsetY, 0);
            yield return null;
        }

        // Reset
        transform.position = basePos;
    }
}
