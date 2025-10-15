using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FloatingInteractableObject : MonoBehaviour
{
    [Header("Floating Settings")]
    public bool startFloating = true;
    public float floatHeight = 0.2f;
    public float floatSpeed = 1f;
    public float bobIntensity = 0.1f;

    [Header("Rotation Settings")]
    public bool enableFloatingRotation = true;
    public Vector3 rotationSpeed = new Vector3(0, 30, 0);

    [Header("Appearance Animation")]
    [Tooltip("Duration of the appearance transition")]
    public float appearanceDuration = 1.5f;
    [Tooltip("Default height offset for appearance animation")]
    public float defaultAppearanceHeight = 5f;
    [Tooltip("Animation curve for smooth appearance")]
    public AnimationCurve appearanceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 startPosition;
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;
    private bool isFloating;
    private bool isGrabbed = false;

    private void Start()
    {
        gameObject.SetActive(false);

        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        startPosition = transform.position;

        if (startFloating)
        {
            StartFloating();
        }

        // Subscribe to grab events
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void Update()
    {
        if (isFloating && !isGrabbed)
        {
            HandleFloating();
        }
    }

    private void HandleFloating()
    {
        // Bobbing motion
        float newY = startPosition.y + floatHeight + Mathf.Sin(Time.time * floatSpeed) * bobIntensity;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Gentle rotation
        if (enableFloatingRotation)
        {
            transform.Rotate(rotationSpeed * Time.deltaTime);
        }
    }

    public void StartFloating()
    {
        if (rb != null)
        {
            isFloating = true;
            rb.useGravity = false;
            Debug.Log($"{gameObject.name} started floating");
        }
    }

    public void StopFloating()
    {
        if (rb != null)
        {
            isFloating = false;
            rb.useGravity = true;
            Debug.Log($"{gameObject.name} stopped floating");
        }
    }

    public void AppearFrom()
    {
        AppearFrom(defaultAppearanceHeight);
    }

    public void AppearFrom(float heightOffset)
    {
        AppearFrom(heightOffset, appearanceDuration);
    }

    public void AppearFrom(float heightOffset, float duration)
    {
        gameObject.SetActive(true);
        StartCoroutine(AppearanceTransition(heightOffset, duration));
    }

    private IEnumerator AppearanceTransition(float heightOffset, float duration)
    {
        Vector3 targetPosition = startPosition;
        Vector3 startPos = targetPosition + Vector3.up * heightOffset;

        transform.position = startPos;
        rb.isKinematic = true;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            float curveValue = appearanceCurve.Evaluate(progress);

            transform.position = Vector3.Lerp(startPos, targetPosition, curveValue);

            yield return null;
        }

        transform.position = targetPosition;

        if (startFloating)
        {
            rb.isKinematic = false;
            StartFloating();
        }

        Debug.Log($"{gameObject.name} appeared at position {targetPosition}");
    }


    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        StopFloating();
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }

    public void ResetToFloating()
    {
        isGrabbed = false;
        transform.position = startPosition;
        StartFloating();
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}
