using UnityEngine;
using System;
using System.Collections.Generic;
public class FluvioSound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    public float pitch = 1f;
    public float timeToSkip = 0f;
    [Range(0f, 1f)] public float spatialSound = 0f;

    [HideInInspector] public AudioSource source;
}

public class FluvioController : MonoBehaviour
{
    [Header("Audio Settings")]
    public Sound[] sounds;

    [Header("Position Settings")]
    [Tooltip("Target positions (world-space by default). If PositionsAreLocal is true, these are local to PositionsReference.")]
    public Vector3[] toFollowPositions;
    [Tooltip("If true, toFollowPositions are interpreted as local positions relative to PositionsReference.")]
    public bool PositionsAreLocal = false;
    [Tooltip("Reference transform used when PositionsAreLocal is true.")]
    public Transform PositionsReference;

    [Header("Animation Settings")]
    [Tooltip("Animator (will be fetched from children if empty).")]
    public Animator hydroAnimator;
    public SkinnedMeshRenderer[] skinMeshRenderedArray;
    public float speed = 1.5f;
    [Tooltip("A sorted list of timestamps (seconds). Intervals are read as ranges [interval[i], interval[i+1]). The last element is treated as the final stop threshold.")]
    public scriptingAnimation scriptToFollow;
    public float[] interval;

    [Header("Options")]
    public bool startGreetingAnimation = false;
    public float rotationSpeed = 2f;
    public bool drawPositionGizmos = true;

    // internals
    private Dictionary<string, Sound> soundMap;
    private Transform playerTransform;
    private float timer = 0f;
    private float defaultSpeed;
    private bool animationIsPlaying = false;
    private bool animationHasStarted = false;
    private bool allowLookAtPlayer = false;
    private int lastRangeIndex = -1;
    private int activeWalkTarget = -1; // -1 = not walking
    private bool activeWalkShouldLook = false;

    #region Unity callbacks
    private void Awake()
    {
        // Audio map
        soundMap = new Dictionary<string, Sound>(StringComparer.OrdinalIgnoreCase);
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
                    if (!soundMap.ContainsKey(s.name)) soundMap.Add(s.name, s);
                    else Debug.LogWarning($"Duplicate sound name '{s.name}' in {name}");
                }
            }
        }

        if (hydroAnimator == null)
            hydroAnimator = GetComponentInChildren<Animator>();

        // find main camera transform safely
        playerTransform = Camera.main != null ? Camera.main.transform :
            GameObject.FindWithTag("MainCamera")?.transform;

        defaultSpeed = speed;

        // disable skins by default (maintains old behaviour)
        SetSkinsActive(false);
    }

    private void Start()
    {
        if (toFollowPositions != null && toFollowPositions.Length > 0)
        {
            Vector3 startPos = GetWorldPosition(0);
            transform.position = startPos;
            // keep current rotation or optionally set to face next position; preserving old behaviour by setting to position[0] rotation:
            if (toFollowPositions.Length > 1)
            {
                Vector3 dir = GetWorldPosition(1) - transform.position;
                if (dir.sqrMagnitude > 0.0001f)
                    transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }

    private void Update()
    {
        // update look vector
        if (playerTransform != null)
        {
            Vector3 look = playerTransform.position - transform.position;
            // only set allow look if permitted
            if (allowLookAtPlayer && look.sqrMagnitude > 0.0001f)
            {
                // LateUpdate handles slerp to avoid jittering transform during physics updates
            }
        }

#if UNITY_EDITOR
        if (drawPositionGizmos) DrawDebugRayToNext();
#endif

        // timer & animation start
        if (startGreetingAnimation && !animationHasStarted)
            StartAnimation();

        animationHasStarted = startGreetingAnimation;

        if (!animationIsPlaying) return; // nothing to do if stopped

        // safety: need at least two interval values for ranges
        if (interval == null || interval.Length < 2) return;

        timer += Time.deltaTime;

        // if timer surpasses final stop threshold -> stop
        if (timer >= interval[interval.Length - 1])
        {
            StopTimer();
            speed = defaultSpeed;
            return;
        }

        // find the active range index: interval[i] <= timer < interval[i+1]
        int currentRange = GetRangeIndexForTime(timer);

        if (currentRange != lastRangeIndex)
        {
            // entered a new range
            OnEnterRange(currentRange);
            Debug.Log("Executing step " + currentRange + " at " + interval[currentRange]);
            lastRangeIndex = currentRange;
        }

        // perform continuous behaviors for current range (walk, etc.)
        DoRangeBehavior(currentRange);
    }

    private void LateUpdate()
    {
        // smooth look at player if allowed
        if (allowLookAtPlayer && playerTransform != null)
        {
            Vector3 lookDir = playerTransform.position - transform.position;
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawPositionGizmos || toFollowPositions == null) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < toFollowPositions.Length; i++)
        {
            Vector3 world = (Application.isPlaying) ? GetWorldPosition(i) : (PositionsAreLocal && PositionsReference != null ? PositionsReference.TransformPoint(toFollowPositions[i]) : toFollowPositions[i]);
            Gizmos.DrawWireSphere(world, 0.1f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(world + Vector3.up * 0.15f, $"P[{i}]");
#endif
        }
    }
    #endregion

    #region Range & behavior logic
    private int GetRangeIndexForTime(float t)
    {
        // linear search; interval is expected sorted ascending
        for (int i = 0; i < interval.Length - 1; i++)
        {
            if (t >= interval[i] && t < interval[i + 1]) return i;
        }
        return -1;
    }

    private void OnEnterRange(int rangeIndex)
    {
        // map the original behaviour into enter-range actions
        // NOTE: these indices follow the original code: range 0 -> interval[0..1], range 1 -> interval[1..2], ...

        foreach(var actionSet in scriptToFollow.listOfActions)
        {
            if(actionSet.Order == rangeIndex)
            {
                actionSet.Actions.Invoke();
            }
        }



























        /*switch (rangeIndex)
    {
        case 0:
            SetSkinsActive(true);
            hydroAnimator.SetTrigger("Idle");
            AudioPlay("Teleport1");
            activeWalkTarget = -1;
            break;

        case 1:
            allowLookAtPlayer = true;
            AudioPlay("Saludo");
            hydroAnimator.SetTrigger("Saludo");
            break;

        case 2:
            StartWalkingTo(1, lookTowardsTarget: false);
            break;

        case 3:
            AudioPlay("Alarma");
            hydroAnimator.SetTrigger("Panico");
            break;

        case 4:
            hydroAnimator.SetTrigger("Aviso");
            AudioPlay("Ayuda");
            break;

        case 5:
            hydroAnimator.SetTrigger("Apuntando");
            break;

        case 6:
            speed = 1f;
            allowLookAtPlayer = false;
            StartWalkingTo(2, lookTowardsTarget: true);
            Debug.Log("Ya me voy");
            break;

        case 7:
            AudioPlay("Teleport2");
            Debug.Log("Fluvio uso TP");
            SetSkinsActive(false);
            StopWalking();
            break;

        default:
            // ranges outside defined mapping: do nothing on enter
            // que loopee aviso, que se lleve las manos a la cabeza en la alarma, tu en tu
            break;
    }*/
    }

    private void DoRangeBehavior(int rangeIndex)
    {
        // continuous behaviors while inside a range
        if (activeWalkTarget >= 0)
        {
            MoveTowardsActiveTarget();
        }
    }
    #endregion

    #region Walking / movement
    public void StartWalkingTo(int index)
    {
        if (toFollowPositions == null || index < 0 || index >= toFollowPositions.Length)
        {
            Debug.LogWarning($"StartWalkingTo: invalid index {index}. toFollowPositions length = {(toFollowPositions == null ? 0 : toFollowPositions.Length)}");
            activeWalkTarget = -1;
            return;
        }

        activeWalkTarget = index;
        
    }
    public void setLookToTarget(bool lookTowardsTarget)
    {
        activeWalkShouldLook = lookTowardsTarget;
    }

    public void StopWalking()
    {
        activeWalkTarget = -1;
        activeWalkShouldLook = false;
    }

    private void MoveTowardsActiveTarget()
    {
        if (activeWalkTarget < 0 || toFollowPositions == null || activeWalkTarget >= toFollowPositions.Length) return;

        Vector3 target = GetWorldPosition(activeWalkTarget);
        float moveStep = speed * Time.deltaTime;

        // optionally rotate towards movement direction
        if (activeWalkShouldLook)
        {
            Vector3 dir = target - transform.position;
            if (dir.sqrMagnitude > 0.00001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }

        // move
        transform.position = Vector3.MoveTowards(transform.position, target, moveStep);
    }
    #endregion

    #region Audio
    public void AudioPlay(string audioName)
    {
        if (string.IsNullOrEmpty(audioName)) return;
        if (soundMap == null) return;
        if (soundMap.TryGetValue(audioName, out var sound))
        {
            if (sound?.source != null && !sound.source.isPlaying)
                sound.source.Play();
        }
        else
        {
            Debug.LogWarning($"AudioPlay: sound '{audioName}' not found (check names).");
        }
    }
    #endregion

    #region Helpers
    private Vector3 GetWorldPosition(int index)
    {
        if (toFollowPositions == null || index < 0 || index >= toFollowPositions.Length) return transform.position;
        if (PositionsAreLocal)
        {
            if (PositionsReference != null)
                return PositionsReference.TransformPoint(toFollowPositions[index]);
            else
                return transform.TransformPoint(toFollowPositions[index]); // fallback to this transform as reference
        }
        else
        {
            return toFollowPositions[index];
        }
    }

    public void SetSkinsActive(bool active)
    {
        if (skinMeshRenderedArray == null) return;
        foreach (var s in skinMeshRenderedArray)
            if (s != null) s.enabled = active;
    }
    public void setAnimationTrigger(string triggerName)
    {
        hydroAnimator.SetTrigger(triggerName);
    }
    public void setActiveWalkTarget(int index)
    {
      activeWalkTarget = index;
    }
    public void setAllowLookPlayer(bool active)
    {
        allowLookAtPlayer = active;
    }
    public void setSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    public void writeDevug(string Message)
    {
       Debug.Log(Message);
    }
    [ContextMenu("Populate positions from child transforms (world)")]
    private void PopulatePositionsFromChildrenWorld()
    {
        List<Vector3> list = new List<Vector3>();
        foreach (Transform child in transform)
            list.Add(child.position);
        toFollowPositions = list.ToArray();
        PositionsAreLocal = false;
        Debug.Log($"Populated {toFollowPositions.Length} positions from children (world-space).");
    }

    [ContextMenu("Populate positions from child transforms (local to this)")]
    private void PopulatePositionsFromChildrenLocal()
    {
        List<Vector3> list = new List<Vector3>();
        foreach (Transform child in transform)
            list.Add(transform.InverseTransformPoint(child.position));
        toFollowPositions = list.ToArray();
        PositionsAreLocal = true;
        PositionsReference = transform;
        Debug.Log($"Populated {toFollowPositions.Length} positions from children (local to this).");
    }

    private void DrawDebugRayToNext()
    {
        if (toFollowPositions == null || toFollowPositions.Length == 0) return;
        Vector3 next = GetWorldPosition(0);
        Debug.DrawLine(transform.position, next, Color.green);
    }

    public void StartAnimation()
    {
        // public convenience method to start
        timer = 0f;
        lastRangeIndex = -1;
        animationIsPlaying = true;
        activeWalkTarget = -1;
        allowLookAtPlayer = false;
    }

    private void StopTimer()
    {
        animationIsPlaying = false;
        activeWalkTarget = -1;
        allowLookAtPlayer = false;
        // restore speed if needed
        speed = defaultSpeed;
    }
    #endregion
}
