using System.Collections;
using UnityEngine;

public class CalorInfernalScript : MonoBehaviour
{
    #region Variables
    [Header("References")]
    private Animator calorInfAnimator;
    public PipeBehavior[] pipeSection;  // Asigna en Inspector para evitar Find en runtime
    private Transform _playerTransform;
    private SkinnedMeshRenderer _meshRenderer;  // Cacheado para eficiencia

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
    private Coroutine selectObjectiveCoroutine;  // Referencia para detener
    private Coroutine animationCoroutine;  // Referencia para detener

    [Header("Behaviour Settings")]
    // Constantes para valores mágicos
    public float INITIAL_WAIT = 10f;
    public float OBJECTIVE_INTERVAL = 20f;
    public float INTERACTION_DURATION = 10f;
    public float STOP_ANIMATOR_DELAY = 2.5f;
    public float RESTART_GAME_DELAY = 10f;
    public float ANIMATION_DELAY = 3f;
    public float VALVE_DELAY = 1f;

    // Enum para tipos de objetos (mejor que strings)
    private enum ObjectType { Cables, Screw, Wheel }
    #endregion

    #region Initialization
    private void Awake()
    {
        // Cachear referencias
        calorInfAnimator = GetComponent<Animator>();
        _meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        _playerTransform = Camera.main != null ? Camera.main.transform : GameObject.FindWithTag("MainCamera")?.transform;

        // Inicializar posición
        positionToGo = startPosition;

        // Si no asignas pipeSection en Inspector, descomenta esto (pero es mejor asignarlo manualmente)
        /*
        if (pipeSection == null || pipeSection.Length == 0)
        {
            GameObject[] pipes = GameObject.FindGameObjectsWithTag("Minijuego");
            pipeSection = new PipeBehavior[pipes.Length];
            for (int i = 0; i < pipes.Length; i++)
            {
                pipeSection[i] = pipes[i].GetComponent<PipeBehavior>();
            }
        }
        */

        // Iniciar animación inicial
        calorInfAnimator?.Play("CalorInfernalAnim");
    }

    void Start()
    {
        StartGame();  // Inicia el juego automáticamente o llama desde otro lugar
    }
    #endregion

    #region Update Loop
    void Update()
    {
        if (!isGameStarted) return;  // Early exit si el juego no está activo

        HandleLooking();
        HandleMovement();
    }

    private void HandleLooking()
    {
        Transform target = isLookingAtPlayer ? _playerTransform : pipeSection[randObj]?.transform;
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
    #endregion

    #region Interactions
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
    #endregion

    #region Coroutines
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
        yield return new WaitForSeconds(ANIMATION_DELAY);

        switch (objectType)
        {
            case ObjectType.Cables:
                calorInfAnimator.SetTrigger("Punch");
                yield return new WaitForSeconds(ANIMATION_DELAY);
                pipeSection[randObj]?.activate();
                break;
            case ObjectType.Screw:
                calorInfAnimator.SetTrigger("Crank");
                pipeSection[randObj]?.activate();
                break;
            case ObjectType.Wheel:
                calorInfAnimator.SetTrigger("Valve");
                yield return new WaitForSeconds(VALVE_DELAY);
                pipeSection[randObj]?.activate();
                break;
            default:
                Debug.LogWarning("Tipo de objeto no reconocido: " + objectType);
                break;
        }
    }
    #endregion

    #region Movement and Animation
    private void SetSkinActive() { 
        calorInfAnimator.enabled = true;
    }
    private void LookAtTarget(Transform target)
    {
        Vector3 lookDir = target.position - transform.position;
        if (target.GetComponent<PipeBehavior>() != null)
        {
            switch (target.gameObject.GetComponent<PipeBehavior>().getSectionType())
            {
                case "wheel":
                    lookDir = new Vector3(lookDir.x + 2f, lookDir.y, lookDir.z + 10f); // Ajuste específico para la rueda
                    break;
                default:
                    break;
            }
        }
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
        // Verifica si hay al menos un pipe inactivo
        bool hasInactive = false;
        for (int i = 0; i < pipeSection.Length; i++)
        {
            if (!pipeSection[i].isActive)
            {
                hasInactive = true;
                break;
            }
        }
        if (!hasInactive)
        {
            Debug.Log("Reirse");  // No hay pipes inactivos
            isMoving = false;
            return;
        }
        // Si hay inactivos, selecciona aleatoriamente hasta encontrar uno
        int attempts = 0;
        int maxAttempts = pipeSection.Length;  // Un poco más para asegurar
        do
        {
            randObj = Random.Range(0, pipeSection.Length);
            attempts++;
        } while (pipeSection[randObj].isActive && attempts < maxAttempts);
        Debug.Log("Selected objective: " + randObj + " (" + pipeSection[randObj].getSectionType() + ")");
        positionToGo = pipeSection[randObj].getInfernalPosition();
        isMoving = true;
    }

    private void objectInteract(int objectSelected)
    {
        if (pipeSection == null || objectSelected >= pipeSection.Length) return;

        string typeString = pipeSection[objectSelected].getSectionType();
        ObjectType type = GetObjectTypeFromString(typeString);

        Debug.Log("Interacting with " + typeString);
        animationCoroutine = StartCoroutine(StartAnimationInteraction(type));
        isInteracting = true;
        isLookingAtPlayer = false;

        if (isGameStarted)
        {
            Invoke("stopMoving", INTERACTION_DURATION);
        }
    }

    private ObjectType GetObjectTypeFromString(string type)
    {
        switch (type)
        {
            case "cables": return ObjectType.Cables;
            case "screw": return ObjectType.Screw;
            case "wheel": return ObjectType.Wheel;
            default: return ObjectType.Cables;  // Default
        }
    }

    private void stopMoving()
    {
        calorInfAnimator.SetTrigger("Idle");
        positionToGo = startPosition;
        isLookingAtPlayer = true;
        isInteracting = false;
    }
    #endregion

    #region Game Control
    private void StopGame()
    {
        calorInfAnimator.SetTrigger("FuckOff");
        isGameStarted = false;
        isInteracting = false;
        isLookingAtPlayer = false;
        isMoving = false;

        // Detener Coroutines de manera segura
        if (selectObjectiveCoroutine != null) StopCoroutine(selectObjectiveCoroutine);
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);

        Invoke("stopAnimator", STOP_ANIMATOR_DELAY);
        Debug.Log("Calor Infernal game stopped");
        Invoke("StartGame", RESTART_GAME_DELAY);
    }

    private void StartGame()
    {
        Debug.Log("Calor Infernal game started");
        transform.position = startPosition;
        Invoke("SetSkinActive", 0.5f);
        calorInfAnimator.SetTrigger("Appear");
        if (_meshRenderer != null) _meshRenderer.enabled = true;
        isGameStarted = true;
        isLookingAtPlayer = true;
        selectObjectiveCoroutine = StartCoroutine(SelectObjectiveRoutine());
    }
    public void EndGame()
    {
        isGameStarted = false;
        isInteracting = false;
        isLookingAtPlayer = false;
        isMoving = false;
        calorInfAnimator.SetTrigger("Goodbye");
        Invoke("stopAnimator", STOP_ANIMATOR_DELAY);
        Debug.Log("Calor Infernal game ended");
    }
    private void stopAnimator()
    {
        calorInfAnimator.Rebind();
        calorInfAnimator.Update(0f);
        calorInfAnimator.enabled = false;
        if (_meshRenderer != null) _meshRenderer.enabled = false;
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(startPosition, 0.2f);
    }
    #endregion
}
