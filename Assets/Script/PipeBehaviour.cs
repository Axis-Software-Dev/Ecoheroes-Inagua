using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
public class PipeBehavior : MonoBehaviour
{
    enum sectionBehavior
    {
        wheel,
        screw,
        cables
    }
    
    public InputActionReference leftGrip;
    public InputActionReference rightGrip;
    public Transform leftController;
    public Transform rightController;

    [SerializeField]
    private sectionBehavior section;
    private bool leftGrippedPressed = false;
    private bool rightGrippedPressed = false;
    private Quaternion initialLeftControllerAngle;
    private Quaternion currentLeftControllerAngle;
    private Collider colliderObject;
    private Transform currentWheelPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leftGrip.action.performed += ctx => OnLeftGrip();
        rightGrip.action.performed += ctx => OnRightGrip();
        leftGrip.action.started += ctx => leftGrippedPressed=true;
        rightGrip.action.started += ctx => rightGrippedPressed=false;
        leftGrip.action.canceled += ctx => leftGrippedPressed=false;
        rightGrip.action.canceled += ctx => rightGrippedPressed=false;
        currentWheelPosition = this.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (leftGrippedPressed) { 
            currentLeftControllerAngle = getControllerAngle(leftController);
            float angle = getAngle(initialLeftControllerAngle, currentLeftControllerAngle);
            Debug.Log("Angle: " + angle);
            setWheelRotation(angle, initialLeftControllerAngle.eulerAngles.y);
        }
        else
        {

        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Controller"))
        {
            switch (section)
            {
                case sectionBehavior.wheel:
                    wheelBehaviour(other.gameObject);
                    
                    break;
                case sectionBehavior.screw:
                    //screwBehaviour(other.gameObject);
                    break;
                case sectionBehavior.cables:
                    //cablesBehaviour(other.gameObject);
                    break;
                default:
                    break;
            }
        }
    }
    #region ControllerCallbacks
    private void OnLeftGrip()
    {
        initialLeftControllerAngle= getControllerAngle(leftController);
    }
    private void OnRightGrip()
    {
        Debug.Log("Right Grip just Pressed");
    }
   

    #endregion
    #region Behaviors
    #region Wheel
    private Quaternion getControllerAngle(Transform controller)
    {
        return controller.rotation;
    }
    private float getAngle(Quaternion previous, Quaternion current)
    {
        if (previous == Quaternion.identity || current == Quaternion.identity) return 0f;
        Vector3 previousForward = previous * Vector3.forward;
        Vector3 currentForward = current * Vector3.forward;
        previousForward.y = 0;
        currentForward.y = 0;
        float angleDelta = Vector3.SignedAngle(previousForward, currentForward, Vector3.up);
        return angleDelta;
    }
    private void setWheelRotation(float angle, float currentPosition)
    {
        this.transform.rotation = Quaternion.Euler(0, (currentPosition + angle), 0);
    }
    private void wheelBehaviour(GameObject controller)
    {
        Quaternion rotationA = Quaternion.identity, rotationB = Quaternion.identity;
        if (leftGrippedPressed) {
            
        }

        

    }




    #endregion
    #endregion
    #region Helpers
    public void pipeBehaviour()
    {
      
    }
    #endregion
}