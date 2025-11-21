using UnityEngine;

public class CanvasOrbitController : MonoBehaviour
{
    [Tooltip("Camera to follow (usually Main Camera in XROrigin).")]
    public Transform targetCamera;

    [Tooltip("Distance in front of the camera.")]
    public float distance = 2f;

    [Tooltip("Height offset relative to camera (optional).")]
    public float heightOffset = 0f;

    [Tooltip("Smooth follow speed.")]
    public float followSpeed = 5f;

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 forward = targetCamera.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 targetPosition = targetCamera.position + forward * distance + Vector3.up * heightOffset;

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
        transform.LookAt(targetCamera);
    }
}
