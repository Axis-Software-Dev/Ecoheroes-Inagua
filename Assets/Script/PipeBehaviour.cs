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

    [Header("Type of pipe Minigame")]
    [SerializeField]private sectionBehavior section;
    
    [Header("General Settings")]
    public InputActionReference leftGrip;
    public InputActionReference rightGrip;
    public Transform leftController;
    public Transform rightController;
    private bool leftGrippedPressed = false;
    private bool rightGrippedPressed = false;
    private bool isLeftInPipe = false;
    private bool isRightInPipe = false;
    [SerializeField] private bool isActive = false;

    [Header("Wheel Settings")]
    [SerializeField]private Transform currentWheelPosition;
    private Quaternion initialLeftControllerAngle;
    private Quaternion currentLeftControllerAngle;
    private Quaternion currentRightControllerAngle;
    private Quaternion initialRightControllerAngle;

    [Header("Screw Settings")]
    public float screwMinHeight;
    public float screwMaxHeight;
    [SerializeField] private float screwSpeed = 0f;
    private static float speedValue = 0f;

    [Header("Cable Settings")]
    public GameObject destination;
    public MeshRenderer cableMesh;
    private LineRenderer cableRenderer;
    [SerializeField] private bool cableGrabed;
    private destinationDetection endDestination;
    private bool previousCableState = false;








    #region Unity Callbacks
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leftGrip.action.performed += ctx => OnLeftGrip();
        rightGrip.action.performed += ctx => OnRightGrip();
        leftGrip.action.started += ctx => leftGrippedPressed=true;
        rightGrip.action.started += ctx => rightGrippedPressed=true;
        leftGrip.action.canceled += ctx => leftGrippedPressed=false;
        rightGrip.action.canceled += ctx => rightGrippedPressed=false;
        currentWheelPosition = this.transform;
        cableRenderer = this.GetComponent<LineRenderer>();

        if (cableRenderer != null)
        {

            cableRenderer.enabled = false;
            endDestination=destination?.GetComponent<destinationDetection>();

        }


        activate();
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
                cableBehaviour();
                break;
            default:
                break;
        }

    }

    private void OnTriggerEnter(Collider other)
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
        if (isActive) transform.position = new Vector3(transform.position.x, Mathf.SmoothDamp(transform.position.y, screwMaxHeight, ref speedValue, screwSpeed * Time.deltaTime), transform.position.z);
        if (transform.position.y <= 0.01f&& isActive) deactivate();
    }
    #endregion
    #region Cable

    private void cableBehaviour()
    {
        rightControllerLogic();
        

    
        
    }

    private void leftControllerLogic()
    {
      

        if (isLeftInPipe)
        {
            if (leftGrippedPressed)
            {
                cableGrabed = true;
            }
        }

        if (!leftGrippedPressed)
        {
            cableGrabed = false;
        }

        if (cableGrabed)
        {
            cableRenderer.enabled = true;
            cableRenderer.SetPosition(0, transform.position);
            cableRenderer.SetPosition(1, leftController.position);
            if (endDestination?.isLeftInDestination == true)
            {

                // Detecta transición de cable conectado
                if (previousCableState && !leftGrippedPressed)
                {
                    Debug.Log("Cable connected");
                }
            }
        }
        else
        {
            cableRenderer.enabled = false;
        }

        previousCableState = leftGrippedPressed;
    }
    private void rightControllerLogic()
    {
        if (endDestination?.isRightInDestination == true)
        {

            // Detecta transición de cable conectado
            if (previousCableState && !rightGrippedPressed)
            {
                Debug.Log("Cable connected");
                if (isActive)deactivate();
            }
        }

        if (isRightInPipe)
        {
            if (rightGrippedPressed)
            {
                cableGrabed = true;
            }
        }

        if (!rightGrippedPressed)
        {
            cableGrabed = false;
        }

        if (cableGrabed)
        {
            cableRenderer.enabled = true;
            cableRenderer.SetPosition(0, transform.position);
            cableRenderer.SetPosition(1, rightController.position);
        }
        else
        {
            cableRenderer.enabled = false;
        }
        previousCableState = rightGrippedPressed;
    }
    #endregion
    #endregion
    #region Helpers
    public void activate()
    {
        isActive = true;

        switch (section)
        {
            case sectionBehavior.cables:
                if (cableMesh != null) cableMesh.enabled = false;
                break;
            case sectionBehavior.screw:
                transform.position=new Vector3(transform.position.x, 0.02f, transform.position.z);

               
                break;
            case sectionBehavior.wheel:
                //wheelLogic
                break;
        }

        
    }
    public void deactivate()
    {
        isActive = false;
        switch (section)
        { 
            case sectionBehavior.cables:
                if (cableMesh != null) cableMesh.enabled = true;
                break;
            case sectionBehavior.screw:
                    transform.position = new Vector3(transform.position.x,screwMinHeight,transform.position.z);
                break;
            case sectionBehavior.wheel:
                    //wheelLogic
                break;

        }
    }
    #endregion
}