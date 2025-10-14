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

    private Vector3 startPosition;
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;
    private bool isFloating;
    private bool isGrabbed = false;

    private void Start()
    {
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

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        StopFloating();
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
        // Don't return to floating automatically after release
        // The ball will now follow physics
    }

    public void ResetToFloating()
    {
        // Call this method if you want to reset the ball to floating state
        isGrabbed = false;
        transform.position = startPosition;
        StartFloating();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}
