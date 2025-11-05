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
        cables,
        bigWheel
    }

    [Header("Type of pipe Minigame")]
    [SerializeField] private sectionBehavior section;

    [Header("General Settings")]
    public InputActionReference leftGrip;
    public InputActionReference rightGrip;
    public Transform leftController;
    public Transform rightController;
    private bool leftGrippedPressed = false;
    private bool rightGrippedPressed = false;
    private bool isLeftInPipe = false;
    private bool isRightInPipe = false;
    public bool isActive = false;
    public Vector3 InfernalPosition;
    private Vector3 worldPosition;
    public SphereCollider infernalCollider;

    [Header("Wheel Settings")]
    [SerializeField] private float currentWheelPosition = 0;
    private Quaternion initialLeftControllerAngle;
    private Quaternion currentLeftControllerAngle;
    private Quaternion currentRightControllerAngle;
    private Quaternion initialRightControllerAngle;
    private float[] rotationRanges = { 0f, 60f, 120f, 180f, 240f, 300f, 360f };
    [SerializeField] private const int MAX_CHECKPOINTS = 5;
    private int checkPoints = 0;
    private int lastCheckpoint = 0;
    public float colliderRadiius = 0.3f;


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
    public GameObject indicator;





    private void Awake()
    {
        //infernalCollider.radius = colliderRadiius;
        worldPosition = transform.TransformPoint(InfernalPosition);
        infernalCollider.transform.position = worldPosition;
    }


    #region Unity Callbacks
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leftGrip.action.performed += ctx => OnLeftGrip();
        rightGrip.action.performed += ctx => OnRightGrip();
        leftGrip.action.started += ctx => leftGrippedPressed = true;
        rightGrip.action.started += ctx => rightGrippedPressed = true;
        leftGrip.action.canceled += ctx => leftGrippedPressed = false;
        rightGrip.action.canceled += ctx => rightGrippedPressed = false;

        cableRenderer = this.GetComponent<LineRenderer>();

        if (cableRenderer != null)
        {

            cableRenderer.enabled = false;
            endDestination = destination?.GetComponent<destinationDetection>();

        }


        deactivate();
    }

    // Update is called once per frame
    void Update()
    {
       
        switch (section)
        {
            case sectionBehavior.wheel:
            case sectionBehavior.bigWheel:
                if (isActive) wheelBehaviour();
                break;
            case sectionBehavior.screw:
                 screwBehaviour();
                break;
            case sectionBehavior.cables:
                if (isActive) cableBehaviour();
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
        }
        else if (other.CompareTag("RightController"))
        {
            isRightInPipe = false;
        }
    }
    #endregion
    #region ControllerCallbacks
    private void OnLeftGrip()
    {
        initialLeftControllerAngle = getControllerAngle(leftController);
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
        // Evita errores con identity
        if (previous == Quaternion.identity || current == Quaternion.identity)
            return 0f;

        // Proyectamos en el plano XY (ignoramos Z)
        Vector3 prevDir = (previous * Vector3.forward);
        Vector3 currDir = (current * Vector3.forward);

        // Proyectar al plano XY: ponemos Z = 0 y normalizamos
        prevDir.z = 0f;
        currDir.z = 0f;

        if (prevDir == Vector3.zero || currDir == Vector3.zero)
            return 0f;

        prevDir.Normalize();
        currDir.Normalize();

        // Usamos Vector3.forward (Z) como eje de rotación
        float angleDelta = Vector3.SignedAngle(prevDir, currDir, Vector3.forward);

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

                setWheelRotation(angle, currentWheelPosition);
            }

        }
        if (isRightInPipe)
        {
            if (rightGrippedPressed)
            {
                currentRightControllerAngle = getControllerAngle(rightController);
                float angle = getAngle(initialRightControllerAngle, currentRightControllerAngle);

                setWheelRotation(angle, currentWheelPosition);
            }
            else
            {
                currentWheelPosition = transform.rotation.eulerAngles.y;
            }

        }
        int currentRangeIndex = GetCurrentRangeIndex();  // Obtiene el indice del rango actual
        UpdateCheckPoints(currentRangeIndex);  // Funcion para manejar checkpoints
        if (checkPoints == MAX_CHECKPOINTS && lastCheckpoint == MAX_CHECKPOINTS - 1)
        {


            deactivate();

        }
        lastCheckpoint = checkPoints;


    }

    private int GetCurrentRangeIndex()
    {
        float angleY = transform.rotation.eulerAngles.y;  // Obtiene el angulo Y
        for (int i = 0; i < rotationRanges.Length - 1; i++)
        {
            if (angleY >= rotationRanges[i] && angleY < rotationRanges[i + 1])
            {

                return i;  // Retorna el indice del rango (0-5)
            }
        }
        return 5;  // Si esta entre 300-360, retorna 5
    }
    private void UpdateCheckPoints(int currentRangeIndex)
    {
        int expectedCheckPoint = currentRangeIndex;  // El rango actual deberia coincidir con el checkpoint
        if (checkPoints == expectedCheckPoint + 1)
        {
            checkPoints--;
        }
        else if (checkPoints == expectedCheckPoint - 1)
        {
            checkPoints++;
        }
        
    }



    #endregion
    #region Screw
    private void screwBehaviour()
    {
        if (isActive)
        {
            // Calcula la nueva posicion Y con SmoothDamp
            float newY = Mathf.SmoothDamp(transform.position.y, screwMaxHeight, ref speedValue, screwSpeed);

            // Aplica el clamp inmediatamente para evitar sobrepasos
            newY = Mathf.Clamp(newY, screwMinHeight, screwMaxHeight);

            // Actualiza la posicion con el valor clamped
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // Verifica si ha llegado cerca del minimo para desactivar (ajusta el umbral si es necesario)
            if (newY <= screwMinHeight + 0.01f)
            {
                deactivate();
                transform.position = new Vector3(transform.position.x, screwMinHeight, transform.position.z);
            }
        }
        else
        {
            // Si no esta activo, asegurate de que la posicion este clamped (por si acaso)
            transform.position = new Vector3(transform.position.x,  screwMinHeight, transform.position.z);
        }
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

                // Detecta transicion de cable conectado
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

            // Detecta transicion de cable conectado
            if (previousCableState && !rightGrippedPressed)
            {
                Debug.Log("Cable connected");
                if (isActive) deactivate();
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
            indicator?.SetActive(true);
        }
        else
        {
            cableRenderer.enabled = false;
            indicator.SetActive(false);
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
                transform.position = new Vector3(transform.position.x, screwMinHeight+0.05f, transform.position.z);


                break;
            case sectionBehavior.wheel:
                checkPoints = 0;
                lastCheckpoint = 0;
                break;
        }


    }
    public void deactivate()
    {
        isActive = false;
        GameManager.Instance.AddMinigamePoint();
        switch (section)
        {
            case sectionBehavior.cables:
                if (cableMesh != null) cableMesh.enabled = true;
                break;
            case sectionBehavior.screw:
                transform.position = new Vector3(transform.position.x, screwMinHeight, transform.position.z);
                break;
            case sectionBehavior.wheel:
                checkPoints = 0;
                lastCheckpoint = 0;
                Debug.Log("Wheel spinned 1 time");
                break;

        }
    }
    public string getSectionType()
    {
        return section.ToString();
    }
    public Vector3 getInfernalPosition()
    {
        
        return transform.TransformPoint(InfernalPosition);
    }

    private void OnDrawGizmos()
    {
        
        Gizmos.color = Color.red;
        Vector3 newWorldPosition=transform.TransformPoint(InfernalPosition);

        Gizmos.DrawWireSphere(newWorldPosition, 0.3f);

    }
    #endregion
}
