using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.Splines;

namespace Fluvio
{
    [Serializable]
    public class FluvioSound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public float pitch = 1f;
        public float timeToSkip = 0f;
        [Range(0f, 1f)] public float spatialSound = 0f;

        [NonSerialized] public AudioSource source;
    }

    [Serializable]
    public class ActionEntry
    {
        public enum ActionType
        {
            PlayAudio,
            ToggleSkins,
            StartWalkStraight,
            StartWalkCurve,
            SetAnimatorBool,
            SetAnimatorTrigger,
            InvokeLocalMethod,
            AllowLookAtPlayer,
            playVarAudio,
            InvokeUnityEvent
        }

        public ActionType Type;

        // Common fields used by various actions
        public string stringArg;          // e.g., audio name, method name, animator param
        public int intArg;                // e.g., target index
        public float floatArg;            // e.g., speed or misc
        public bool boolArg;              // toggle value
        public Vector3 vectorArg;         // control point or offset (for curve)
        public UnityEvent unityEvent;     // designer assigned event
    }

    /// <summary>
    /// Robust FluvioController: safe runtime script with data-driven actions and both straight and curved walking.
    /// Replace older scripts with this; wire ActionEntry lists in inspector (or call StartWalkingTo/StartWalkingCurve from code).
    /// </summary>
    public class FluvioController : MonoBehaviour
    {
        [Header("Audio Settings")]
        public FluvioSound[] sounds;

        [Header("Animation Settings")]
        public Animator Animator;
        public SkinnedMeshRenderer[] skinMeshRenderedArray;
        public float speed = 1.5f;
        [Tooltip("Sorted timestamps. Ranges are [interval[i], interval[i+1]). Last entry is stop threshold.")]
        public float[] interval;

        [Header("Options")]
        public bool StartAnimationOnFlag = false;
        public float rotationSpeed = 2f;
        public bool drawPositionGizmos = true;
        public Color positionGizmosColor = Color.cyan;

        [Header("Action mapping (data driven)")]
        [Tooltip("List of actions that can be executed by an external caller or by OnEnterRange.")]
        public ActionEntry[] actions;

        // internals
        private Dictionary<string, FluvioSound> _soundMap;
        private Transform _playerTransform;
        private float _timer = 0f;
        private float _defaultSpeed;
        private bool _animationIsPlaying = false;
        private bool _animationHasStarted = false;
        private bool _allowLookAtPlayer;
        private int _lastRangeIndex = -1;

        // walking internals
        private bool _activeWalkShouldLook = false;
        private bool _useCurve = false;
        private float _walkProgress = 0f;
        private Vector3 _activeWalkTarget;
        private persistanceData _persistanceData;

        // curve internals
        private Vector3 _curveStart, _curveControl, _curveEnd;

        // reflection cache for invoking local methods
        private readonly Dictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>(StringComparer.Ordinal);

        #region Unity callbacks
        private void Awake()
        {
            // Build audio map
            _soundMap = new Dictionary<string, FluvioSound>(StringComparer.OrdinalIgnoreCase);
            if (sounds != null)
            {
                foreach (var s in sounds)
                {
                    if (s == null) continue;
                    // create audio source for each sound (small projects okay). Consider pooling if many sounds.
                    s.source = gameObject.AddComponent<AudioSource>();
                    s.source.clip = s.clip;
                    s.source.volume = s.volume;
                    s.source.pitch = s.pitch;
                    s.source.time = s.timeToSkip;
                    s.source.spatialBlend = s.spatialSound;

                    if (!string.IsNullOrEmpty(s.name))
                    {
                        if (!_soundMap.ContainsKey(s.name)) _soundMap.Add(s.name, s);
                        else Debug.LogWarning($"Duplicate sound name '{s.name}' on {name}.");
                    }
                }
            }

            if (Animator == null)
                Animator = GetComponentInChildren<Animator>();

            _defaultSpeed = speed;

            _playerTransform = Camera.main != null ? Camera.main.transform : GameObject.FindWithTag("MainCamera")?.transform;

            SetSkinsActive(false);
            _persistanceData = Resources.Load<persistanceData>("persistanceData");
        }

        private void Update()
        {
#if UNITY_EDITOR
#endif
            // timer & animation start flag handling
            if (StartAnimationOnFlag && !_animationHasStarted)
            {
                StartAnimation();
            }
            _animationHasStarted = StartAnimationOnFlag;

            if (!_animationIsPlaying) return;

            if (interval == null || interval.Length < 2) return;

            _timer += Time.deltaTime;

            // stop if we've passed the final threshold
            if (_timer >= interval[interval.Length - 1])
            {
                StopTimer();
                speed = _defaultSpeed;
                return;
            }

            int currentRange = GetRangeIndexForTime(_timer);
            if (currentRange != _lastRangeIndex)
            {
                if (currentRange >= 0)
                {
                    Debug.Log($"Executing step {currentRange} at {interval[currentRange]:F2}");
                    OnEnterRange(currentRange);
                }
                _lastRangeIndex = currentRange;
            }

            if (_playerTransform != null)
            {
                Vector3 look = _playerTransform.position - transform.position;
            }

            if (_useCurve) MoveAlongCurve();
            else MoveTowardsActiveTarget();
        }

        private void LateUpdate()
        {
            if (_allowLookAtPlayer && _playerTransform != null)
            {
                Vector3 lookDir = _playerTransform.position - transform.position;
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 2 * Time.deltaTime);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawPositionGizmos) return;

            Gizmos.color = positionGizmosColor;
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i].vectorArg != null && actions[i].vectorArg != Vector3.zero)
                {
                    Vector3 world = actions[i].vectorArg;
                    Gizmos.DrawWireSphere(world, 0.12f);
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(world + Vector3.up * 0.15f, $"P[{i}]");
#endif
                }
            }
        }
        #endregion

        #region Range logic
        private int GetRangeIndexForTime(float t)
        {
            if (interval == null) return -1;
            for (int i = 0; i < interval.Length - 1; i++)
            {
                if (t >= interval[i] && t < interval[i + 1]) return i;
            }
            return -1;
        }

        private void OnEnterRange(int rangeIndex)
        {
            if (actions != null)
            {
                foreach (var entry in actions)
                {
                    if (entry.intArg == rangeIndex)
                    {
                        ExecuteAction(entry);
                    }
                }
            }

            switch (rangeIndex)
            {
                default:
                    break;
            }
        }
        #endregion

        #region Actions
        private void ExecuteAction(ActionEntry entry)
        {
            if (entry == null) return;
            switch (entry.Type)
            {
                case ActionEntry.ActionType.PlayAudio:
                    if (!string.IsNullOrEmpty(entry.stringArg)) PlayAudio(entry.stringArg);
                    break;
                case ActionEntry.ActionType.ToggleSkins:
                    SetSkinsActive(entry.boolArg);
                    break;
                case ActionEntry.ActionType.StartWalkStraight:
                    StartWalkingTo(entry.vectorArg, entry.boolArg);
                    break;
                case ActionEntry.ActionType.StartWalkCurve:
                    StartWalkingCurve(entry.vectorArg, entry.boolArg, entry.stringArg, entry.floatArg);
                    break;
                case ActionEntry.ActionType.SetAnimatorBool:
                    if (Animator != null && !string.IsNullOrEmpty(entry.stringArg))
                        Animator.SetBool(entry.stringArg, entry.boolArg);
                    break;
                case ActionEntry.ActionType.SetAnimatorTrigger:
                    if (Animator != null && !string.IsNullOrEmpty(entry.stringArg))
                        Animator.SetTrigger(entry.stringArg);
                    break;
                case ActionEntry.ActionType.InvokeLocalMethod:
                    if (!string.IsNullOrEmpty(entry.stringArg))
                        InvokeLocalMethodSafe(entry.stringArg);
                    break;
                case ActionEntry.ActionType.AllowLookAtPlayer:
                    _allowLookAtPlayer = entry.boolArg;
                    break;
                case ActionEntry.ActionType.InvokeUnityEvent:
                    entry.unityEvent?.Invoke();
                    break;
                case ActionEntry.ActionType.playVarAudio:
                    if(_persistanceData?.getSelectedCharacter()!=null||
                        _persistanceData?.getSelectedCharacter()!="none")
                        playVarAudio(_persistanceData.getSelectedCharacter());
                    break;
            }
        }
        #endregion

        #region Audio helpers
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
        private void playVarAudio(string characterSelected)
        {
            PlayAudio(characterSelected);
        }
        #endregion

        #region Walking / movement (public API)
        /// <summary>Begin straight-line walk toward the indexed follow position.</summary>
        public void StartWalkingTo(Vector3 target, bool lookTowardsTarget)
        {
            if (target == null)
            {
                Debug.LogWarning($"StartWalkingTo: invalid target {target}.");
                _activeWalkTarget = transform.position;
                _activeWalkShouldLook = false;
                _useCurve = false;
                return;
            }

            _activeWalkTarget = target;
            _activeWalkShouldLook = lookTowardsTarget;
            _useCurve = false;
        }

        /// <summary>Begin curved walk using a quadratic Bezier curve (controlPoint is world-space).</summary>
        public void StartWalkingCurve(Vector3 target, bool lookTowardsTarget, string curveDirection, float arcHeight = 1f)
        {
            if (target == null)
            {
                Debug.LogWarning($"StartWalkingCurve: invalid target {target}.");
                _useCurve = false;
                return;
            }

            _activeWalkShouldLook = lookTowardsTarget;
            _walkProgress = 0f;
            _curveStart = transform.position;
            _curveEnd = target;

            _useCurve = true;
            switch (curveDirection.ToLower())
            {
                case "left":
                    _curveControl = ((_curveStart + _curveEnd) * .5f) - transform.right * arcHeight;
                    break;
                case "right":
                    _curveControl = ((_curveStart + _curveEnd) * .5f) + transform.right * arcHeight;
                    break;
                case "up":
                    _curveControl = ((_curveStart + _curveEnd) * .5f) + Vector3.up * arcHeight;
                    break;
                default:
                    Debug.LogWarning($"Unknown curve direction '{curveDirection}', defaulting to up.");
                    _curveControl = ((_curveStart + _curveEnd) * .5f) + Vector3.up * arcHeight;
                    break;
            }
        }

        public void StopWalking()
        {
            _activeWalkTarget = transform.position;
            _useCurve = false;
        }

        private void MoveTowardsActiveTarget()
        {
            if (_activeWalkTarget == Vector3.zero) return;
            float moveStep = speed * Time.deltaTime;

            if (_activeWalkShouldLook)
            {
                Vector3 dir = _activeWalkTarget - transform.position;
                if (dir.sqrMagnitude > 0.00001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
                }
            }

            transform.position = Vector3.MoveTowards(transform.position, _activeWalkTarget, moveStep);

            if (Vector3.Distance(transform.position, _activeWalkTarget) < 0.01f)
            {
                StopWalking();
            }
        }

        private void MoveAlongCurve()
        {
            float denom = Vector3.Distance(_curveStart, _curveEnd);
            if (denom <= 0.0001f) denom = 0.0001f;
            _walkProgress += Time.deltaTime * (speed / denom);
            _walkProgress = Mathf.Clamp01(_walkProgress);

            // Quadratic Bezier: (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
            float t = _walkProgress;
            float u = 1f - t;
            Vector3 pos = u * u * _curveStart + 2f * u * t * _curveControl + t * t * _curveEnd;

            if (_activeWalkShouldLook)
            {
                Vector3 dir = _curveEnd - transform.position;
                if (dir.sqrMagnitude > 0.00001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
                }
            }

            transform.position = pos;

            if (_walkProgress >= 1f - Mathf.Epsilon)
            {
                StopWalking();
            }
        }
        #endregion

        #region Helpers & utilities

        private void SetSkinsActive(bool active)
        {
            if (skinMeshRenderedArray == null) return;
            foreach (var s in skinMeshRenderedArray) if (s != null) s.enabled = active;
        }

        /// <summary>Public: start the controller animation sequence.</summary>
        public void StartAnimation()
        {
            StartAnimationOnFlag = true;
            _timer = 0f;
            _lastRangeIndex = -1;
            _animationIsPlaying = true;
            _useCurve = false;
        }

        /// <summary>Public: stop the timed animation sequence.</summary>
        public void StopTimer()
        {
            _animationIsPlaying = false;
            _useCurve = false;
            speed = _defaultSpeed;
        }

        private void InvokeLocalMethodSafe(string methodName)
        {
            if (string.IsNullOrEmpty(methodName)) return;

            if (!_methodCache.TryGetValue(methodName, out var mi))
            {
                mi = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mi == null)
                {
                    Debug.LogWarning($"InvokeLocalMethodSafe: method '{methodName}' not found on {name}.");
                    return;
                }
                _methodCache[methodName] = mi;
            }

            try
            {
                mi.Invoke(this, null);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error invoking method '{methodName}' on {name}: {ex}");
            }
        }
        #endregion
    }
}
