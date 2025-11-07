using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using static GameManager;
public class PipeBehavior : MonoBehaviour
{
    enum sectionBehavior
    {
        wheel,
        screw,
        cables,
        bigWheel
    }
    [Serializable]
    public class BackgroundSFX
    {
        public string name;
        public AudioClip clip;
        [UnityEngine.Range(0f, 1f)] public float volume = 1f;
        public float pitch = 1f;
        public float timeToSkip = 0f;
        [UnityEngine.Range(0f, 1f)] public float spatialSound = 0f;
        public bool loop = false;
        public GameObject soundObject=null;
        [NonSerialized] public AudioSource source;
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
    public BackgroundSFX[] sounds;
    private Dictionary<string, BackgroundSFX> _soundMap;
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
    public Animator waterAnimator;

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
    public MeshRenderer brokenCable;





    private void Awake()
    {
        //infernalCollider.radius = colliderRadiius;
        worldPosition = transform.TransformPoint(InfernalPosition);
        infernalCollider.transform.position = worldPosition;
        if(brokenCable!=null)brokenCable.enabled = false;
        _soundMap = new Dictionary<string, BackgroundSFX>(StringComparer.OrdinalIgnoreCase);
        if (sounds != null)
        {
            foreach (var s in sounds)
            {
                if (s == null) continue;
                // create audio source for each sound (small projects okay). Consider pooling if many sounds.
                
                if(s.soundObject==null)
                    s.source = gameObject.AddComponent<AudioSource>();
                else
                    s.source = s.soundObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.time = s.timeToSkip;
                s.source.loop = s.loop;
                s.source.spatialBlend = s.spatialSound;

                if (!string.IsNullOrEmpty(s.name))
                {
                    if (!_soundMap.ContainsKey(s.name)) _soundMap.Add(s.name, s);
                    else Debug.LogWarning($"Duplicate sound name '{s.name}' on {name}.");
                }
            }
        }
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
    public void PlayAudio(string audioName)
    {
        if (string.IsNullOrEmpty(audioName) || _soundMap == null) return;
        if (_soundMap.TryGetValue(audioName, out var s) && s?.source != null)
        {
            if (!s.source.isPlaying) s.source.Play();
        }
        else
        {
            Debug.LogWarning($"PlayAudio: sound '{audioName}' not found on {name}");
        }
    }
    public void StopAudio(string audioName)
    {
        if (string.IsNullOrEmpty(audioName) || _soundMap == null) return;
        if (_soundMap.TryGetValue(audioName, out var s) && s?.source != null)
        {
            if (s.source.isPlaying) s.source.Stop();
        }
        else
        {
            Debug.LogWarning($"PlayAudio: sound '{audioName}' not found on {name}");
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
        if(section==sectionBehavior.wheel&&isActive)PlayAudio("ValveSFX");
    }
    private void OnRightGrip()
    {
        initialRightControllerAngle = getControllerAngle(rightController);
        if (section == sectionBehavior.wheel&&isActive) PlayAudio("ValveSFX");
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
        if (previous == Quaternion.identity || current == Quaternion.identity)
            return 0f;

        Quaternion delta = current * Quaternion.Inverse(previous);
        float angle = delta.eulerAngles.y;

        // Normalizar a -180..180
        if (angle > 180f) angle -= 360f;

        return angle;
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
            //Show indicator to grab cable
        }
        else
        {
            cableRenderer.enabled = false;
            //Hide indicator to grab cable
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
                if (brokenCable != null) brokenCable.enabled = true;
                PlayAudio("CableSFX");
                break;
            case sectionBehavior.screw:
                transform.position = new Vector3(transform.position.x, screwMinHeight+0.05f, transform.position.z);
                PlayAudio("ScrewSFX");

                break;
            case sectionBehavior.wheel:
            case sectionBehavior.bigWheel:
                checkPoints = 0;
                lastCheckpoint = 0;
                PlayAudio("WaterSFX");
                if(waterAnimator!=null)waterAnimator.SetTrigger("Open");
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
                if(brokenCable!=null)brokenCable.enabled = false;
                StopAudio("CableSFX");
                break;
            case sectionBehavior.screw:
                transform.position = new Vector3(transform.position.x, screwMinHeight, transform.position.z);
                StopAudio("ScrewSFX");
                break;
            case sectionBehavior.wheel:
            case sectionBehavior.bigWheel:
                StopAudio("WaterSFX");
                checkPoints = 0;
                lastCheckpoint = 0;
                Debug.Log("Wheel spinned 1 time");
                if (waterAnimator != null) waterAnimator.SetTrigger("Close");
                break;

        }
    }
    public string getSectionType()
    {
        return section.ToString();
    }
    public Vector3 getInfernalPosition()
    {
        
        return worldPosition;
    }

    private void OnDrawGizmos()
    {
        
        Gizmos.color = Color.red;
        Vector3 newWorldPosition=transform.TransformPoint(InfernalPosition);

        Gizmos.DrawWireSphere(newWorldPosition, 0.3f);

    }
    #endregion
}
