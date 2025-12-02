using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public enum AccelerationType
{
    Linear,
    Bezier,
    Logarithmic,
    Exponential,
    Curve
}

public class WaypointMover : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public Vector3[] waypoints;
    public float speed = 2f;
    public bool loopPath = false;
    public bool startOnAwake = true;

    [Header("Acceleration Type")]
    public AccelerationType accelerationType = AccelerationType.Linear;
    public AnimationCurve customCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Animation Settings")]
    public Animator animator;
    public string movementAnimationName = "Walk";
    public string interactionAnimationName = "Wave";

    [Header("Interaction Settings")]
    public float interactionDuration = 3f;
    public bool facePlayerOnInteraction = true;

    [Header("Gizmo Settings")]
    public Color waypointColor = Color.green;
    public Color lineColor = Color.yellow;
    public float gizmoRadius = 0.3f;
    public bool showTimeLabels = true;

    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private bool isPaused = false;
    private Transform playerTransform;
    private Unity.XR.CoreUtils.XROrigin xrOrigin;
    private Coroutine movementCoroutine;
    private Quaternion originalRotation;

    private const float ROTATION_SPEED = 5f;
    private const float WAYPOINT_THRESHOLD = 0.1f;

    private void Start()
    {
        xrOrigin = FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null)
        {
            playerTransform = xrOrigin.Camera.transform;
        }

        if (startOnAwake && waypoints != null && waypoints.Length > 0)
        {
            StartMovement();
        }
    }

    public void StartMovement()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints assigned!");
            return;
        }

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        currentWaypointIndex = 0;
        isMoving = true;
        movementCoroutine = StartCoroutine(MoveAlongWaypoints());
    }

    public void StopMovement()
    {
        isMoving = false;
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
    }

    public void OnInteractorActivate()
    {
        if (isMoving && !isPaused)
        {
            StartCoroutine(HandleInteraction());
        }
    }

    private IEnumerator HandleInteraction()
    {
        isPaused = true;
        originalRotation = transform.rotation;

        if (animator != null && !string.IsNullOrEmpty(interactionAnimationName))
        {
            animator.SetTrigger(interactionAnimationName);
        }

        // Rotate to Face Player
        if (facePlayerOnInteraction && playerTransform != null)
        {
            float elapsedTime = 0f;
            Quaternion startRotation = transform.rotation;

            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0; // Keep rotation flat
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

            while (elapsedTime < 0.5f)
            {
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime * 2f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.rotation = targetRotation;
        }

        // Wait for Interaction
        Debug.Log("Waiting for interaction...");
        yield return new WaitForSeconds(interactionDuration);
        Debug.Log("Interaction completed!");

        // Rotate Back to Original Position (The Fix)
        // We check facePlayerOnInteraction again to see if we actually moved
        if (facePlayerOnInteraction)
        {
            float elapsedTime = 0f;
            Quaternion currentRotation = transform.rotation;

            while (elapsedTime < 0.5f)
            {
                transform.rotation = Quaternion.Slerp(currentRotation, originalRotation, elapsedTime * 2f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.rotation = originalRotation;
        }

        // 5. Resume
        if (animator != null && !string.IsNullOrEmpty(movementAnimationName))
        {
            animator.SetTrigger(movementAnimationName);
        }

        isPaused = false;
    }

    private IEnumerator MoveAlongWaypoints()
    {
        while (isMoving)
        {
            if (currentWaypointIndex >= waypoints.Length)
            {
                if (loopPath)
                {
                    currentWaypointIndex = 0;
                }
                else
                {
                    isMoving = false;
                    yield break;
                }
            }

            Vector3 targetPosition = waypoints[currentWaypointIndex];
            yield return StartCoroutine(MoveToWaypoint(targetPosition));

            currentWaypointIndex++;
        }
    }

    private IEnumerator MoveToWaypoint(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float journeyTime = distance / speed;
        float elapsedTime = 0f;

        Vector3 direction = (targetPosition - startPosition).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }

        while (elapsedTime < journeyTime)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / journeyTime);
            float interpolatedT = GetInterpolatedValue(t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, interpolatedT);

            yield return null;
        }

        transform.position = targetPosition;
    }

    private float GetInterpolatedValue(float t)
    {
        switch (accelerationType)
        {
            case AccelerationType.Linear:
                return t;

            case AccelerationType.Bezier:
                return t * t * (3f - 2f * t);

            case AccelerationType.Logarithmic:
                return Mathf.Log10(1 + t * 9f);

            case AccelerationType.Exponential:
                return t * t;

            case AccelerationType.Curve:
                return customCurve.Evaluate(t);

            default:
                return t;
        }
    }

    private float CalculateTimeToWaypoint(Vector3 from, Vector3 to)
    {
        float distance = Vector3.Distance(from, to);
        return distance / speed;
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Vector3 previousPosition = transform.position;

        for (int i = 0; i < waypoints.Length; i++)
        {
            Gizmos.color = waypointColor;
            Gizmos.DrawWireSphere(waypoints[i], gizmoRadius);

            Gizmos.color = lineColor;
            Gizmos.DrawLine(previousPosition, waypoints[i]);

#if UNITY_EDITOR
            if (showTimeLabels)
            {
                float timeToWaypoint = CalculateTimeToWaypoint(previousPosition, waypoints[i]);
                UnityEditor.Handles.Label(
                    waypoints[i] + Vector3.up * 0.5f,
                    $"#{i}\n{timeToWaypoint:F2}s",
                    new GUIStyle()
                    {
                        normal = new GUIStyleState() { textColor = Color.white },
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 12
                    }
                );
            }
#endif

            previousPosition = waypoints[i];
        }

        if (loopPath && waypoints.Length > 0)
        {
            Gizmos.color = lineColor;
            Gizmos.DrawLine(waypoints[waypoints.Length - 1], waypoints[0]);
        }
    }
}
