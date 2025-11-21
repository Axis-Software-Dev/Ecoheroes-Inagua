using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PipeBehavior : MonoBehaviour
{
    private enum SectionBehavior
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
        [Range(0f, 1f)] 
        public float volume = 1f;
        public float pitch = 1f;
        public float timeToSkip = 0f;
        [Range(0f, 1f)] 
        public float spatialSound = 0f;
        public bool loop = false;
        public GameObject soundObject = null;
        [NonSerialized] 
        public AudioSource source;
    }

    [Header("Type of pipe Minigame")]
    [SerializeField] 
    private SectionBehavior section;

    [Header("General Settings")]
    public InputActionReference leftGrip;
    public InputActionReference rightGrip;
    public Transform leftController;
    public Transform rightController;
    public bool isActive = false;
    public Vector3 InfernalPosition;
    public SphereCollider infernalCollider;
    public BackgroundSFX[] sounds;
    public MeshRenderer arrowMesh;
    public Animator waterAnimator;
    public TutorialScript tutorialScript;

    [Header("Wheel Settings")]
    [SerializeField] 
    private float currentWheelPosition = 0;
    private float[] rotationRanges = { 0f, 60f, 120f, 180f, 240f, 300f, 360f };
    private const int MAX_CHECKPOINTS = 5;
    private int checkPoints = 0;
    private int lastCheckpoint = 0;
    public float colliderRadius = 0.3f;

    [Header("Screw Settings")]
    public float screwMinHeight;
    public float screwMaxHeight;
    [SerializeField] 
    private float screwSpeed = 0f;
    private static float speedValue = 0f;
    private const float SCREW_ACTIVATION_HEIGHT_OFFSET = 0.05f;
    private const float SCREW_DEACTIVATION_THRESHOLD = 0.01f;

    [Header("Cable Settings")]
    public GameObject destination;
    public MeshRenderer cableMesh;
    public MeshRenderer brokenCable;
    public GameObject Chispas;

    private bool leftGrippedPressed = false;
    private bool rightGrippedPressed = false;
    private bool isLeftInPipe = false;
    private bool isRightInPipe = false;
    private Vector3 worldPosition;
    private Dictionary<string, BackgroundSFX> _soundMap;
    private Animator arrowAnimator;
    private Ray initialLeftControllerRay;
    private Ray currentLeftControllerRay;
    private Ray currentRightControllerRay;
    private Ray initialRightControllerRay;
    private LineRenderer cableRenderer;
    private bool cableGrabed;
    private destinationDetection endDestination;
    private bool previousCableState = false;

    private const float RAY_DEBUG_LENGTH = 1f;
    private const float GIZMO_SPHERE_RADIUS = 0.3f;

    private void Awake()
    {
        if (Chispas != null)
        {
            Chispas.SetActive(false);
        }

        if (arrowMesh != null)
        {
            arrowMesh.enabled = false;
            arrowAnimator = arrowMesh.gameObject.GetComponent<Animator>();
        }

        if (arrowAnimator != null)
        {
            arrowAnimator.enabled = false;
        }

        worldPosition = transform.TransformPoint(InfernalPosition);
        
        if (infernalCollider != null)
        {
            infernalCollider.transform.position = worldPosition;
        }

        if (brokenCable != null)
        {
            brokenCable.enabled = false;
        }

        _soundMap = new Dictionary<string, BackgroundSFX>(StringComparer.OrdinalIgnoreCase);
        
        if (sounds != null)
        {
            foreach (var s in sounds)
            {
                if (s == null) continue;

                if (s.soundObject == null)
                {
                    s.source = gameObject.AddComponent<AudioSource>();
                }
                else
                {
                    s.source = s.soundObject.AddComponent<AudioSource>();
                }

                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.time = s.timeToSkip;
                s.source.loop = s.loop;
                s.source.spatialBlend = s.spatialSound;

                if (!string.IsNullOrEmpty(s.name))
                {
                    if (!_soundMap.ContainsKey(s.name))
                    {
                        _soundMap.Add(s.name, s);
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate sound name '{s.name}' on {name}.");
                    }
                }
            }
        }
    }

    private void Start()
    {
        if (leftGrip != null && leftGrip.action != null)
        {
            leftGrip.action.performed += ctx => OnLeftGrip();
            leftGrip.action.started += ctx => leftGrippedPressed = true;
            leftGrip.action.canceled += ctx => leftGrippedPressed = false;
        }

        if (rightGrip != null && rightGrip.action != null)
        {
            rightGrip.action.performed += ctx => OnRightGrip();
            rightGrip.action.started += ctx => rightGrippedPressed = true;
            rightGrip.action.canceled += ctx => rightGrippedPressed = false;
        }

        cableRenderer = GetComponent<LineRenderer>();

        if (cableRenderer != null)
        {
            cableRenderer.enabled = false;
            
            if (destination != null)
            {
                endDestination = destination.GetComponent<destinationDetection>();
            }
        }

        deactivate();
    }

    private void Update()
    {
        switch (section)
        {
            case SectionBehavior.wheel:
            case SectionBehavior.bigWheel:
                if (isActive)
                {
                    wheelBehaviour();
                }
                break;
            case SectionBehavior.screw:
                screwBehaviour();
                break;
            case SectionBehavior.cables:
                if (isActive)
                {
                    cableBehaviour();
                }
                break;
        }
    }

    public void PlayAudio(string audioName)
    {
        if (string.IsNullOrEmpty(audioName) || _soundMap == null) return;
        
        if (_soundMap.TryGetValue(audioName, out var s) && s?.source != null)
        {
            if (!s.source.isPlaying)
            {
                s.source.Play();
            }
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
            if (s.source.isPlaying)
            {
                s.source.Stop();
            }
        }
        else
        {
            Debug.LogWarning($"StopAudio: sound '{audioName}' not found on {name}");
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

    private void OnLeftGrip()
    {
        if (leftController != null)
        {
            initialLeftControllerRay = GetRay(transform.position, getControllerPosition(leftController));
        }
        
        if (section == SectionBehavior.wheel && isActive)
        {
            PlayAudio("ValveSFX");
        }
    }

    private void OnRightGrip()
    {
        if (rightController != null)
        {
            initialRightControllerRay = GetRay(transform.position, getControllerPosition(rightController));
        }
        
        if (section == SectionBehavior.wheel && isActive)
        {
            PlayAudio("ValveSFX");
        }
    }

    private Vector3 getControllerPosition(Transform controller)
    {
        return controller != null ? controller.position : Vector3.zero;
    }

    private Ray GetRay(Vector3 rayOrigin, Vector3 rayDirectionObject)
    {
        return new Ray(rayOrigin, (rayDirectionObject - rayOrigin));
    }

    private float getAngle(Ray previous, Ray current)
    {
        Vector3 prevDir = previous.direction.normalized;
        Vector3 currDir = current.direction.normalized;

        prevDir.y = 0;
        currDir.y = 0;
        prevDir.Normalize();
        currDir.Normalize();

        float angle = Vector3.SignedAngle(prevDir, currDir, Vector3.up);

        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;

        return angle;
    }

    private void setWheelRotation(float angle, float currentPosition)
    {
        transform.rotation = Quaternion.Euler(0, currentPosition + angle, 0);
    }

    private void wheelBehaviour()
    {
        if (isLeftInPipe && leftGrippedPressed && leftController != null)
        {
            currentLeftControllerRay = GetRay(transform.position, getControllerPosition(leftController));
            Debug.DrawRay(currentLeftControllerRay.origin, currentLeftControllerRay.direction * RAY_DEBUG_LENGTH, Color.blue);
            Debug.DrawRay(initialLeftControllerRay.origin, initialLeftControllerRay.direction * RAY_DEBUG_LENGTH, Color.green);
            
            float angle = getAngle(initialLeftControllerRay, currentLeftControllerRay);
            setWheelRotation(angle, currentWheelPosition);
        }

        if (isRightInPipe && rightController != null)
        {
            if (rightGrippedPressed)
            {
                currentRightControllerRay = GetRay(transform.position, getControllerPosition(rightController));
                Debug.DrawRay(currentRightControllerRay.origin, currentRightControllerRay.direction * RAY_DEBUG_LENGTH, Color.blue);
                Debug.DrawRay(initialRightControllerRay.origin, initialRightControllerRay.direction * RAY_DEBUG_LENGTH, Color.green);

                float angle = getAngle(initialRightControllerRay, currentRightControllerRay);
                setWheelRotation(angle, currentWheelPosition);
            }
            else
            {
                currentWheelPosition = transform.rotation.eulerAngles.y;
            }
        }

        int currentRangeIndex = GetCurrentRangeIndex();
        UpdateCheckPoints(currentRangeIndex);

        if (checkPoints == MAX_CHECKPOINTS && lastCheckpoint == MAX_CHECKPOINTS - 1)
        {
            deactivate();
        }

        lastCheckpoint = checkPoints;
    }

    private int GetCurrentRangeIndex()
    {
        float angleY = transform.rotation.eulerAngles.y;
        
        for (int i = 0; i < rotationRanges.Length - 1; i++)
        {
            if (angleY >= rotationRanges[i] && angleY < rotationRanges[i + 1])
            {
                return i;
            }
        }
        
        return 5;
    }

    private void UpdateCheckPoints(int currentRangeIndex)
    {
        int expectedCheckPoint = currentRangeIndex;
        
        if (checkPoints == expectedCheckPoint + 1)
        {
            checkPoints--;
        }
        else if (checkPoints == expectedCheckPoint - 1)
        {
            checkPoints++;
        }
    }

    private void screwBehaviour()
    {
        if (isActive)
        {
            float newY = Mathf.SmoothDamp(transform.position.y, screwMaxHeight, ref speedValue, screwSpeed);
            newY = Mathf.Clamp(newY, screwMinHeight, screwMaxHeight);

            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            if (newY <= screwMinHeight + SCREW_DEACTIVATION_THRESHOLD)
            {
                deactivate();
                transform.position = new Vector3(transform.position.x, screwMinHeight, transform.position.z);
            }
        }
        else
        {
            transform.position = new Vector3(transform.position.x, screwMinHeight, transform.position.z);
        }
    }

    private void cableBehaviour()
    {
        rightControllerLogic();
    }

    private void rightControllerLogic()
    {
        if (endDestination != null && endDestination.isRightInDestination)
        {
            if (previousCableState && !rightGrippedPressed)
            {
                Debug.Log("Cable connected");
                if (isActive)
                {
                    deactivate();
                }
            }
        }

        if (isRightInPipe && rightGrippedPressed)
        {
            cableGrabed = true;
        }

        if (!rightGrippedPressed)
        {
            cableGrabed = false;
        }

        if (cableGrabed && cableRenderer != null && rightController != null)
        {
            cableRenderer.enabled = true;
            cableRenderer.SetPosition(0, transform.position);
            cableRenderer.SetPosition(1, rightController.position);
        }
        else if (cableRenderer != null)
        {
            cableRenderer.enabled = false;
        }

        previousCableState = rightGrippedPressed;
    }

    public void activate()
    {
        isActive = true;
        
        if (arrowMesh != null)
        {
            arrowMesh.enabled = true;
        }
        
        if (arrowAnimator != null)
        {
            arrowAnimator.enabled = true;
        }

        switch (section)
        {
            case SectionBehavior.cables:
                if (cableMesh != null) cableMesh.enabled = false;
                if (brokenCable != null) brokenCable.enabled = true;
                PlayAudio("CableSFX");
                if (Chispas != null) Chispas.SetActive(true);
                break;
            case SectionBehavior.screw:
                transform.position = new Vector3(transform.position.x, screwMinHeight + SCREW_ACTIVATION_HEIGHT_OFFSET, transform.position.z);
                PlayAudio("ScrewSFX");
                break;
            case SectionBehavior.wheel:
            case SectionBehavior.bigWheel:
                if (tutorialScript != null)
                {
                    tutorialScript.ActivateTutorial();
                }
                checkPoints = 0;
                lastCheckpoint = 0;
                PlayAudio("WaterSFX");
                if (waterAnimator != null)
                {
                    waterAnimator.SetTrigger("Open");
                }
                break;
        }
    }

    public void deactivate()
    {
        isActive = false;
        
        if (arrowMesh != null)
        {
            arrowMesh.enabled = false;
        }
        
        if (arrowAnimator != null)
        {
            arrowAnimator.enabled = false;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMinigamePoint();
        }

        switch (section)
        {
            case SectionBehavior.cables:
                if (Chispas != null) Chispas.SetActive(false);
                if (cableMesh != null) cableMesh.enabled = true;
                if (brokenCable != null) brokenCable.enabled = false;
                StopAudio("CableSFX");
                break;
            case SectionBehavior.screw:
                transform.position = new Vector3(transform.position.x, screwMinHeight, transform.position.z);
                StopAudio("ScrewSFX");
                break;
            case SectionBehavior.wheel:
            case SectionBehavior.bigWheel:
                if (tutorialScript != null)
                {
                    tutorialScript.DeactivateTutorial();
                }
                StopAudio("WaterSFX");
                checkPoints = 0;
                lastCheckpoint = 0;
                Debug.Log("Wheel spinned 1 time");
                if (waterAnimator != null)
                {
                    waterAnimator.SetTrigger("Close");
                }
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
        Vector3 newWorldPosition = transform.TransformPoint(InfernalPosition);
        Gizmos.DrawWireSphere(newWorldPosition, GIZMO_SPHERE_RADIUS);
    }
}
