using UnityEngine;

public class destinationDetection : MonoBehaviour
{
    public bool isRightInDestination = false;
    public bool isLeftInDestination = false;

    private const string LEFT_CONTROLLER_TAG = "LeftController";
    private const string RIGHT_CONTROLLER_TAG = "RightController";

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if (other.CompareTag(LEFT_CONTROLLER_TAG))
        {
            isLeftInDestination = true;
        }

        if (other.CompareTag(RIGHT_CONTROLLER_TAG))
        {
            isRightInDestination = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;

        if (other.CompareTag(LEFT_CONTROLLER_TAG))
        {
            isLeftInDestination = false;
        }

        if (other.CompareTag(RIGHT_CONTROLLER_TAG))
        {
            isRightInDestination = false;
        }
    }
}
