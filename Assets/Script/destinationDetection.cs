using UnityEngine;

public class destinationDetection : MonoBehaviour
{
    public bool isRightInDestination=false;
    public bool isLeftInDestination=false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("LeftController")) isLeftInDestination = true;
        if(other.CompareTag("RightController")) isRightInDestination = true;

    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("LeftController")) isLeftInDestination = false;
        if(other.CompareTag("RightController")) isRightInDestination = false;
    }
}
