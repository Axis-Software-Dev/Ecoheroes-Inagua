using UnityEngine;
using System;
using System.Collections.Generic;
public class ShipSound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    public float pitch = 1f;
    public float timeToSkip = 0f;
    [Range(0f, 1f)] public float spatialSound = 0f;
    public AudioSource source;
}

public class ShipController : MonoBehaviour
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
    public Animator animator;
    public SkinnedMeshRenderer[] skinMeshRenderedArray;
    public float speed = 1.5f;
    [Tooltip("A sorted list of timestamps (seconds). Intervals are read as ranges [interval[i], interval[i+1]). The last element is treated as the final stop threshold.")]
    public float[] interval;

    [Header("Options")]
    public bool startAnimation = false;
    public float rotationSpeed = 2f;
    public bool drawPositionGizmos = true;
    public Color positionGizmosColor = Color.red;
    [Header("Fluvio")]
    public FluvioController fluvioController;

    // internals
    private Dictionary<string, Sound> soundMap;
    private float timer = 0f;
    private float defaultSpeed;
    private bool animationIsPlaying = false;
    private bool animationHasStarted = false;
    private int lastRangeIndex = -1;
    private int activeWalkTarget = -1;
    private bool activeWalkShouldLook = false;
    // --- Curve walking internals ---
    private bool useCurve = false;
    private float walkProgress = 0f;
    private Vector3 curveStart;
    private Vector3 curveControl;
    private Vector3 curveEnd;

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

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        defaultSpeed = speed;

        SetSkinsActive(false);
    }

    private void Start()
    {
        if (toFollowPositions != null && toFollowPositions.Length > 0)
        {
            Vector3 startPos = GetWorldPosition(0);
            transform.position = startPos;
            // keep current rotation or optionally set to face next position:
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
#if UNITY_EDITOR
        if (drawPositionGizmos) DrawDebugRayToNext();
#endif

        // timer & animation start
        if (startAnimation && !animationHasStarted)
            StartAnimation();

        animationHasStarted = startAnimation;

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

    private void OnDrawGizmos()
    {
        if (!drawPositionGizmos || toFollowPositions == null) return;

        Gizmos.color = positionGizmosColor;
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
        switch (rangeIndex)
        {
            case 0:
                animator.SetTrigger("Arrival");
                SetSkinsActive(true);
                activeWalkTarget = -1;
                AudioPlay("land");
                break;

            case 1:

                fluvioController.startGreetingAnimation = true;
                break;

            case 2:
                AudioPlay("depart");
                break;

            case 3:

                break;

            case 4:

                break;

            case 5:

                break;

            case 6:

                break;

            case 7:

                break;

            default:
                // ranges outside defined mapping: do nothing on enter
                break;
        }
    }

    private void DoRangeBehavior(int rangeIndex)
    {
        // continuous behaviors while inside a range
        if (activeWalkTarget >= 0)
        {
            if (useCurve)
                MoveAlongCurve();
            else
                MoveTowardsActiveTarget();
        }
    }
    #endregion

    #region Walking / movement
    private void StartWalkingTo(int index, bool lookTowardsTarget)
    {
        if (toFollowPositions == null || index < 0 || index >= toFollowPositions.Length)
        {
            Debug.LogWarning($"StartWalkingTo: invalid index {index}. toFollowPositions length = {(toFollowPositions == null ? 0 : toFollowPositions.Length)}");
            activeWalkTarget = -1;
            return;
        }

        activeWalkTarget = index;
        activeWalkShouldLook = lookTowardsTarget;
    }
    private void StartWalkingCurve(int index, Vector3 controlPoint, bool lookTowardsTarget)
    {
        if (toFollowPositions == null || index < 0 || index >= toFollowPositions.Length)
        {
            Debug.LogWarning($"StartWalkingCurve: invalid index {index}. toFollowPositions length = {(toFollowPositions == null ? 0 : toFollowPositions.Length)}");
            activeWalkTarget = -1;
            return;
        }

        activeWalkTarget = index;
        activeWalkShouldLook = lookTowardsTarget;
        walkProgress = 0f;

        curveStart = transform.position;
        curveEnd = GetWorldPosition(index);
        curveControl = controlPoint;
        useCurve = true;
    }

    private void StopWalking()
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
    private void MoveAlongCurve()
    {
        walkProgress += Time.deltaTime * (speed / Vector3.Distance(curveStart, curveEnd));
        walkProgress = Mathf.Clamp01(walkProgress);

        // Quadratic Bezier interpolation
        Vector3 pos = Mathf.Pow(1 - walkProgress, 2) * curveStart +
                      2 * (1 - walkProgress) * walkProgress * curveControl +
                      Mathf.Pow(walkProgress, 2) * curveEnd;

        if (activeWalkShouldLook)
        {
            Vector3 dir = curveEnd - transform.position;
            if (dir.sqrMagnitude > 0.00001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }

        transform.position = pos;

        if (walkProgress >= 1f)
        {
            activeWalkTarget = -1;
            useCurve = false;
        }
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

    private void SetSkinsActive(bool active)
    {
        if (skinMeshRenderedArray == null) return;
        foreach (var s in skinMeshRenderedArray)
            if (s != null) s.enabled = active;
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
    }

    private void StopTimer()
    {
        animationIsPlaying = false;
        activeWalkTarget = -1;
        // restore speed if needed
        speed = defaultSpeed;
    }
    #endregion
}
