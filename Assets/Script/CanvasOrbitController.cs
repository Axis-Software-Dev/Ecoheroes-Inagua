using UnityEngine;
using UnityEngine.XR;

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

        // Desired position: always in front of the camera
        Vector3 forward = targetCamera.forward;
        forward.y = 0; // keep level, so it doesn’t tilt up/down weirdly
        forward.Normalize();

        Vector3 targetPosition = targetCamera.position + forward * distance + Vector3.up * heightOffset;

        // Smooth move
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        // Always face the camera
        transform.LookAt(targetCamera);
        transform.rotation = Quaternion.LookRotation(transform.position - targetCamera.position);
    }
}
