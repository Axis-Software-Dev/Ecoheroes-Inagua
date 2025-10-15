using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using System;
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
    public float screwMinHeight,screwMaxHeight;

    [SerializeField]
    private sectionBehavior section;
    private bool leftGrippedPressed = false;
    private bool rightGrippedPressed = false;
    private Quaternion initialLeftControllerAngle;
    private Quaternion currentLeftControllerAngle;
    private Quaternion currentRightControllerAngle;
    private Quaternion initialRightControllerAngle;
    private Collider colliderObject;
    private Transform currentWheelPosition;
    private bool isLeftInPipe = false;
    private bool isRightInPipe = false;
    #region Unity Callbacks
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
       
       switch (section)
        {
            case sectionBehavior.wheel:
                wheelBehaviour();
                break;
            case sectionBehavior.screw:
                screwBehaviour();
                break;
            case sectionBehavior.cables:
                pipeBehaviour();
                break;
            default:
                break;
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("LeftController"))
        {
            isLeftInPipe = true;
        }
        else if (other.CompareTag("RightController"))
        {
            isRightInPipe = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LeftController"))
        {
            isLeftInPipe = false;
        } else if (other.CompareTag("RightController"))
        {
            isRightInPipe = false;
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Controller")) // Tag your controllers
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            Vector3 pushDir = collision.contacts[0].normal * -1; // Push away from contact
            rb.AddForce(pushDir * 0.1f, ForceMode.Force);
        }
    }
    #endregion
    #region ControllerCallbacks
    private void OnLeftGrip()
    {
        initialLeftControllerAngle= getControllerAngle(leftController);
    }
    private void OnRightGrip()
    {
        initialRightControllerAngle = getControllerAngle(rightController);
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
    private void wheelBehaviour()
    {
        if (isLeftInPipe)
        {
            if (leftGrippedPressed)
            {
                currentLeftControllerAngle = getControllerAngle(leftController);
                float angle = getAngle(initialLeftControllerAngle, currentLeftControllerAngle);
                angle = Mathf.Clamp(angle, -90f, 90f);
                Debug.Log("Angle: " + angle);

                setWheelRotation(angle, initialLeftControllerAngle.eulerAngles.y);
            }
        }
        if (isRightInPipe)
        {
            if (rightGrippedPressed)
            {
                currentRightControllerAngle = getControllerAngle(rightController);
                float angle = getAngle(initialRightControllerAngle, currentRightControllerAngle);
                angle = Mathf.Clamp(angle, -90f, 90f);
                Debug.Log("Angle: " + angle);

                setWheelRotation(angle, initialRightControllerAngle.eulerAngles.y);
            }
        }



    }




    #endregion
    #region Screw
    private void screwBehaviour()
    {
        this.transform.position = new Vector3(this.transform.position.x, Mathf.Clamp(this.transform.position.y,screwMinHeight,screwMaxHeight), this.transform.position.z);
    }
    #endregion
    #endregion
    #region Helpers
    public void pipeBehaviour()
    {
      
    }
    #endregion
}