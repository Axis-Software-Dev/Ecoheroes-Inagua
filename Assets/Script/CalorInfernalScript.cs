using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalorInfernalScript : MonoBehaviour
{
    [Serializable]
    public class InfernalSound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] 
        public float volume = 1f;
        [Range(0f, 3f)]
        public float pitch = 1f;
        public float timeToSkip = 0f;
        [Range(0f, 1f)]
        public float spatialSound = 0f;
        [NonSerialized] 
        public AudioSource source;
    }

    public InfernalSound[] sounds;

    [Header("References")]
    private Animator calorInfAnimator;
    public PipeBehavior[] pipeSection;
    private Transform _playerTransform;
    private SkinnedMeshRenderer _meshRenderer;
    private Rigidbody _calorInfernalRB;

    [Header("Settings")]
    public bool isLookingAtPlayer = false;
    public bool isMoving = false;
    public float moveSpeed = 1f;
    public Vector3 startPosition = Vector3.zero;
    public bool isGameStarted = false;

    [Header("Internal State")]
    private int randObj = 0;
    private bool isInteracting = false;
    private Vector3 positionToGo;
    private Coroutine selectObjectiveCoroutine;
    private Coroutine animationCoroutine;

    [Header("Behaviour Settings")]
    public float INITIAL_WAIT = 10f;
    public float OBJECTIVE_INTERVAL = 20f;
    public float INTERACTION_DURATION = 10f;
    public float STOP_ANIMATOR_DELAY = 2.5f;
    public float RESTART_GAME_DELAY = 10f;
    public float ANIMATION_DELAY = 3f;
    public float VALVE_DELAY = 1f;
    public float START_GAME_DELAY = 9.5f;
    public float SKIN_ACTIVE_DELAY = 1f;
    public float END_GAME_DELAY = 5f;
    public float BGM_DELAY = 2f;
    
    private enum ObjectType { Cables, Screw, Wheel, BigWheel }
    private Dictionary<string, InfernalSound> _soundMap;

    private const float WHEEL_LOOK_OFFSET_X = -10f;
    private const float WHEEL_LOOK_OFFSET_Z = -2f;
    private const float SCREW_LOOK_OFFSET_X = -2f;
    private const float SCREW_LOOK_OFFSET_Z = -10f;
    private const float SCREW_ACTIVATION_HEIGHT_OFFSET = 0.05f;
    private const float SCREW_DEACTIVATION_THRESHOLD = 0.01f;

    private void Awake()
    {
        calorInfAnimator = GetComponent<Animator>();
        _meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        _playerTransform = Camera.main != null ? Camera.main.transform : GameObject.FindWithTag("MainCamera")?.transform;
        _calorInfernalRB = GetComponent<Rigidbody>();
        
        positionToGo = startPosition;

        if (calorInfAnimator != null)
        {
            calorInfAnimator.Play("CalorInfernalAnim");
        }
        
        _soundMap = new Dictionary<string, InfernalSound>(StringComparer.OrdinalIgnoreCase);
        
        if (sounds != null)
        {
            foreach (var s in sounds)
            {
                if (s == null) continue;

                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.time = s.timeToSkip;
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
        if (_meshRenderer != null)
        {
            _meshRenderer.enabled = false;
        }
        
        if (calorInfAnimator != null)
        {
            calorInfAnimator.enabled = false;
        }

        Invoke(nameof(StartGame), START_GAME_DELAY);
    }

    private void Update()
    {
        if (!isGameStarted) return;

        HandleLooking();
        HandleMovement();
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

    private void HandleLooking()
    {
        Transform target = isLookingAtPlayer ? _playerTransform : (pipeSection != null && randObj < pipeSection.Length && pipeSection[randObj] != null ? pipeSection[randObj].transform : null);
        
        if (target != null)
        {
            LookAtTarget(target);
        }
    }

    private void HandleMovement()
    {
        if (isMoving)
        {
            MoveToPipe(positionToGo);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Minijuego") && isGameStarted)
        {
            Debug.Log("Entered to: " + other.name);
            isLookingAtPlayer = false;
            objectInteract(randObj);
        }
        else if ((other.CompareTag("LeftController") || other.CompareTag("RightController")) && isInteracting)
        {
            StopGame();
        }
    }

    private IEnumerator SelectObjectiveRoutine()
    {
        yield return new WaitForSeconds(INITIAL_WAIT);
        
        while (isGameStarted)
        {
            SelectObjective();
            yield return new WaitForSeconds(OBJECTIVE_INTERVAL);
        }
    }

    private IEnumerator StartAnimationInteraction(ObjectType objectType)
    {
        if (!isGameStarted) yield return null;
        yield return new WaitForSeconds(ANIMATION_DELAY);

        if (pipeSection == null || randObj >= pipeSection.Length || pipeSection[randObj] == null)
        {
            yield break;
        }

        switch (objectType)
        {
            case ObjectType.Cables:
                if (calorInfAnimator != null)
                {
                    calorInfAnimator.SetTrigger("Punch");
                }
                PlayAudio("Punch");
                yield return new WaitForSeconds(ANIMATION_DELAY);
                pipeSection[randObj].activate();
                break;
            case ObjectType.Screw:
                if (calorInfAnimator != null)
                {
                    calorInfAnimator.SetTrigger("Valve");
                }
                pipeSection[randObj].activate();
                break;
            case ObjectType.Wheel:
                if (calorInfAnimator != null)
                {
                    calorInfAnimator.SetTrigger("Valve");
                }
                yield return new WaitForSeconds(VALVE_DELAY);
                pipeSection[randObj].activate();
                break;
            case ObjectType.BigWheel:
                if (calorInfAnimator != null)
                {
                    calorInfAnimator.SetTrigger("Crank");
                }
                yield return new WaitForSeconds(VALVE_DELAY);
                pipeSection[randObj].activate();
                break;
            default:
                Debug.LogWarning("Tipo de objeto no reconocido: " + objectType);
                break;
        }
    }

    private void SetSkinActive()
    {
        if (calorInfAnimator != null)
        {
            calorInfAnimator.enabled = true;
        }
    }

    private void LookAtTarget(Transform target)
    {
        Vector3 lookDir = target.position - transform.position;
        
        PipeBehavior pipeBehavior = target.GetComponent<PipeBehavior>();
        if (pipeBehavior != null)
        {
            string sectionType = pipeBehavior.getSectionType();
            switch (sectionType)
            {
                case "wheel":
                    lookDir = new Vector3(lookDir.x + WHEEL_LOOK_OFFSET_X, lookDir.y, lookDir.z + WHEEL_LOOK_OFFSET_Z);
                    break;
                case "screw":
                    lookDir = new Vector3(lookDir.x + SCREW_LOOK_OFFSET_X, lookDir.y, lookDir.z + SCREW_LOOK_OFFSET_Z);
                    break;
            }
        }
        
        lookDir.y = 0f;
        
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime);
        }
    }

    private void MoveToPipe(Vector3 position)
    {
        transform.position = Vector3.Slerp(transform.position, position, moveSpeed * Time.deltaTime);
    }

    private void SelectObjective()
    {
        Debug.Log("Selecting new objective...");
        
        if (pipeSection == null || pipeSection.Length == 0) return;

        bool hasInactive = false;
        for (int i = 0; i < pipeSection.Length; i++)
        {
            if (pipeSection[i] != null && !pipeSection[i].isActive)
            {
                hasInactive = true;
                break;
            }
        }

        if (!hasInactive)
        {
            Debug.Log("No inactive pipes available");
            isMoving = false;
            return;
        }

        int attempts = 0;
        int maxAttempts = pipeSection.Length * 2;

        do
        {
            randObj = UnityEngine.Random.Range(0, pipeSection.Length);
            attempts++;
            
            if (attempts >= maxAttempts || (pipeSection[randObj] != null && !pipeSection[randObj].isActive))
            {
                break;
            }
        } 
        while (pipeSection[randObj] == null || pipeSection[randObj].isActive);

        if (pipeSection[randObj] != null)
        {
            Debug.Log("Selected objective: " + randObj + " (" + pipeSection[randObj].getSectionType() + ")");
            positionToGo = pipeSection[randObj].getInfernalPosition();
            isMoving = true;
        }
    }

    private void objectInteract(int objectSelected)
    {
        if (pipeSection == null || objectSelected >= pipeSection.Length || pipeSection[objectSelected] == null) return;

        string typeString = pipeSection[objectSelected].getSectionType();
        ObjectType type = GetObjectTypeFromString(typeString);

        Debug.Log("Interacting with " + typeString);
        animationCoroutine = StartCoroutine(StartAnimationInteraction(type));
        isInteracting = true;
        isLookingAtPlayer = false;

        if (isGameStarted)
        {
            Invoke(nameof(stopMoving), INTERACTION_DURATION);
        }
    }

    private ObjectType GetObjectTypeFromString(string type)
    {
        switch (type?.ToLower())
        {
            case "cables": 
                return ObjectType.Cables;
            case "screw": 
                return ObjectType.Screw;
            case "wheel": 
                return ObjectType.Wheel;
            case "bigwheel": 
                return ObjectType.BigWheel;
            default: 
                return ObjectType.Cables;
        }
    }

    private void stopMoving()
    {
        if (calorInfAnimator != null)
        {
            calorInfAnimator.SetTrigger("Idle");
        }
        
        positionToGo = startPosition;
        
        if (_calorInfernalRB != null)
        {
            _calorInfernalRB.linearVelocity = Vector3.zero;
        }
        
        isLookingAtPlayer = true;
        isInteracting = false;
    }

    private void StopGame()
    {
        if (calorInfAnimator != null)
        {

            calorInfAnimator.SetTrigger("FuckOff");
            calorInfAnimator.SetBool("isGameStarted", false);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMinigamePoint();
        }

        PlayAudio("FuckOff");
        isGameStarted = false;
        isInteracting = false;
        isLookingAtPlayer = false;
        isMoving = false;

        if (selectObjectiveCoroutine != null) 
        {
            StopCoroutine(selectObjectiveCoroutine);
        }
        if (animationCoroutine != null) 
        {
            StopCoroutine(animationCoroutine);
        }
        if (GameManager.Instance.minijuegosCompletados!=GameManager.Instance.POINTS_TO_WIN)
        {
            Invoke(nameof(stopAnimator), STOP_ANIMATOR_DELAY);
            Debug.Log("Calor Infernal game stopped");
            Invoke(nameof(StartGame), RESTART_GAME_DELAY);
            
        }
        
        
    }

    private void StartGame()
    {
        PlayAudio("Laugh");
        
        if (calorInfAnimator != null)
        {
            calorInfAnimator.enabled = true;
        }

        Debug.Log("Calor Infernal game started");
        transform.position = startPosition;
        
        if (_calorInfernalRB != null)
        {
            _calorInfernalRB.linearVelocity = Vector3.zero;
        }

        Invoke(nameof(SetSkinActive), SKIN_ACTIVE_DELAY);
        
        if (calorInfAnimator != null)
        {
            calorInfAnimator.SetTrigger("Appear");
            calorInfAnimator.SetBool("isGameStarted", true);
        }

        if (_meshRenderer != null)
        {
            _meshRenderer.enabled = true;
        }

        isGameStarted = true;
        isLookingAtPlayer = true;
        selectObjectiveCoroutine = StartCoroutine(SelectObjectiveRoutine());
    }

    public void EndGame()
    {
        isGameStarted = false;
        isInteracting = false;
        isLookingAtPlayer = true;
        isMoving = false;
        

        if (calorInfAnimator != null)
        {
            calorInfAnimator.SetTrigger("Goodbye");
        }

        Invoke(nameof(stopAnimator), END_GAME_DELAY);
        Invoke(nameof(DisableCL), END_GAME_DELAY);
        Invoke(nameof(PlayNewBGM), END_GAME_DELAY + BGM_DELAY);
        
        Debug.Log("Calor Infernal game ended");

        if (_calorInfernalRB != null)
        {
            _calorInfernalRB.linearVelocity = Vector3.zero;
        }

        PlayAudio("Scream");

        if (pipeSection != null)
        {
            foreach (var pipe in pipeSection)
            {
                if (pipe != null)
                {
                    pipe.deactivate();
                }
            }
        }
    }

    private void stopAnimator()
    {
        if (calorInfAnimator != null)
        {
            calorInfAnimator.Rebind();
            calorInfAnimator.Update(0f);
            calorInfAnimator.enabled = false;
        }

        if (_meshRenderer != null)
        {
            _meshRenderer.enabled = false;
        }
    }

    private void DisableCL()
    {
        gameObject.SetActive(false);
    }

    private void PlayNewBGM()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayAudio("Chill");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(startPosition, 0.2f);
    }
}
