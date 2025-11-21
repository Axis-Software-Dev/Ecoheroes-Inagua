using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Fluvio
{
    [Serializable]
    public class FluvioSound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] 
        public float volume = 1f;
        public float pitch = 1f;
        public float timeToSkip = 0f;
        [Range(0f, 1f)] 
        public float spatialSound = 0f;

        [NonSerialized] 
        public AudioSource source;
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
        public string stringArg;
        public int intArg;
        public float floatArg;
        public bool boolArg;
        public Vector3 vectorArg;
        public UnityEvent unityEvent;
    }

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

        private Dictionary<string, FluvioSound> _soundMap;
        private Transform _playerTransform;
        private float _timer = 0f;
        private float _defaultSpeed;
        private bool _animationIsPlaying = false;
        private bool _animationHasStarted = false;
        private bool _allowLookAtPlayer;
        private int _lastRangeIndex = -1;

        private bool _activeWalkShouldLook = false;
        private bool _useCurve = false;
        private float _walkProgress = 0f;
        private Vector3 _activeWalkTarget;
        private persistanceData _persistanceData;

        private Vector3 _curveStart, _curveControl, _curveEnd;

        private readonly Dictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>(StringComparer.Ordinal);

        private const float LOOK_ROTATION_SPEED = 2f;
        private const float DISTANCE_THRESHOLD = 0.01f;
        private const float VICTORY_INITIAL_DELAY = 3f;
        private const float VICTORY_MID_DELAY = 6.5f;
        private const float VICTORY_AFTER_PANIC = 2f;
        private const float VICTORY_FINAL_DELAY = 9f;
        private const int VICTORY_SCENE_ID = 3;

        #region Unity Callbacks
        private void Awake()
        {
            _soundMap = new Dictionary<string, FluvioSound>(StringComparer.OrdinalIgnoreCase);
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

            if (Animator == null)
            {
                Animator = GetComponentInChildren<Animator>();
            }

            _defaultSpeed = speed;

            _playerTransform = Camera.main != null ? Camera.main.transform : GameObject.FindWithTag("MainCamera")?.transform;

            SetSkinsActive(false);
            _persistanceData = Resources.Load<persistanceData>("persistanceData");
        }

        private void Update()
        {
            if (StartAnimationOnFlag && !_animationHasStarted)
            {
                StartAnimation();
            }
            _animationHasStarted = StartAnimationOnFlag;

            if (!_animationIsPlaying) return;

            if (interval == null || interval.Length < 2) return;

            _timer += Time.deltaTime;

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

            if (_useCurve) 
            {
                MoveAlongCurve();
            }
            else 
            {
                MoveTowardsActiveTarget();
            }
        }

        private void LateUpdate()
        {
            if (_allowLookAtPlayer && _playerTransform != null)
            {
                Vector3 lookDir = _playerTransform.position - transform.position;
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, LOOK_ROTATION_SPEED * Time.deltaTime);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawPositionGizmos || actions == null) return;

            Gizmos.color = positionGizmosColor;
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null && actions[i].vectorArg != Vector3.zero)
                {
                    Vector3 world = actions[i].vectorArg;
                    Gizmos.DrawWireSphere(world, 0.12f);
#if UNITY_EDITOR
                    Handles.Label(world + Vector3.up * 0.15f, $"P[{i}]");
#endif
                }
            }
        }
        #endregion

        #region Range Logic
        private int GetRangeIndexForTime(float t)
        {
            if (interval == null) return -1;
            
            for (int i = 0; i < interval.Length - 1; i++)
            {
                if (t >= interval[i] && t < interval[i + 1]) 
                {
                    return i;
                }
            }
            return -1;
        }

        private void OnEnterRange(int rangeIndex)
        {
            if (actions != null)
            {
                foreach (var entry in actions)
                {
                    if (entry != null && entry.intArg == rangeIndex)
                    {
                        ExecuteAction(entry);
                    }
                }
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
                    if (!string.IsNullOrEmpty(entry.stringArg)) 
                    {
                        PlayAudio(entry.stringArg);
                    }
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
                    {
                        Animator.SetBool(entry.stringArg, entry.boolArg);
                    }
                    break;
                case ActionEntry.ActionType.SetAnimatorTrigger:
                    if (Animator != null && !string.IsNullOrEmpty(entry.stringArg))
                    {
                        Animator.SetTrigger(entry.stringArg);
                    }
                    break;
                case ActionEntry.ActionType.InvokeLocalMethod:
                    if (!string.IsNullOrEmpty(entry.stringArg))
                    {
                        InvokeLocalMethodSafe(entry.stringArg);
                    }
                    break;
                case ActionEntry.ActionType.AllowLookAtPlayer:
                    _allowLookAtPlayer = entry.boolArg;
                    break;
                case ActionEntry.ActionType.InvokeUnityEvent:
                    entry.unityEvent?.Invoke();
                    break;
                case ActionEntry.ActionType.playVarAudio:
                    if (_persistanceData != null)
                    {
                        string selectedChar = _persistanceData.getSelectedCharacter();
                        if (!string.IsNullOrEmpty(selectedChar) && selectedChar != "none")
                        {
                            playVarAudio(selectedChar);
                        }
                    }
                    break;
            }
        }
        #endregion

        #region Audio
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

        private void playVarAudio(string characterSelected)
        {
            PlayAudio(characterSelected);
        }
        #endregion

        #region Movement
        public void StartWalkingTo(Vector3 target, bool lookTowardsTarget)
        {
            _activeWalkTarget = target;
            _activeWalkShouldLook = lookTowardsTarget;
            _useCurve = false;
        }

        public void StartWalkingCurve(Vector3 target, bool lookTowardsTarget, string curveDirection, float arcHeight = 1f)
        {
            _activeWalkShouldLook = lookTowardsTarget;
            _walkProgress = 0f;
            _curveStart = transform.position;
            _curveEnd = target;
            _useCurve = true;

            Vector3 midPoint = (_curveStart + _curveEnd) * 0.5f;

            switch (curveDirection.ToLower())
            {
                case "left":
                    _curveControl = midPoint - transform.right * arcHeight;
                    break;
                case "right":
                    _curveControl = midPoint + transform.right * arcHeight;
                    break;
                case "up":
                    _curveControl = midPoint + Vector3.up * arcHeight;
                    break;
                default:
                    Debug.LogWarning($"Unknown curve direction '{curveDirection}', defaulting to up.");
                    _curveControl = midPoint + Vector3.up * arcHeight;
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

            if (Vector3.Distance(transform.position, _activeWalkTarget) < DISTANCE_THRESHOLD)
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

        #region Helpers
        private void SetSkinsActive(bool active)
        {
            if (skinMeshRenderedArray == null) return;
            
            foreach (var s in skinMeshRenderedArray)
            {
                if (s != null)
                {
                    s.enabled = active;
                }
            }
        }

        public void StartAnimation()
        {
            StartAnimationOnFlag = true;
            _timer = 0f;
            _lastRangeIndex = -1;
            _animationIsPlaying = true;
            _useCurve = false;
        }

        public void PlayVictorySequence()
        {
            StartCoroutine(VictorySequenceCoroutine());
        }

        private IEnumerator VictorySequenceCoroutine()
        {
            Debug.Log("Starting victory sequence");
            yield return new WaitForSeconds(VICTORY_INITIAL_DELAY);

            PlayAudio("14");

            if (Animator != null)
            {
                Animator.SetTrigger("mb");
            }

            yield return new WaitForSeconds(VICTORY_MID_DELAY);

            PlayAudio("alarm");
            
            if (Animator != null)
            {
                Animator.SetTrigger("Panico1");
            }

            yield return new WaitForSeconds(VICTORY_AFTER_PANIC);

            PlayAudio("15");
            yield return new WaitForSeconds(VICTORY_FINAL_DELAY);

            GameObject sceneManagerObj = GameObject.Find("SceneManager");
            if (sceneManagerObj != null)
            {
                LoadingScreen loadingScreen = sceneManagerObj.GetComponent<LoadingScreen>();
                if (loadingScreen != null)
                {
                    loadingScreen.LoadScene(VICTORY_SCENE_ID);
                }
            }
        }

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
