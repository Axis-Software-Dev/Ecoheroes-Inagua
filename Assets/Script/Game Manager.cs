using Fluvio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Serializable]
    public class BackgroundSound
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
        [NonSerialized] 
        public AudioSource source;
    }

    [Header("Audio")]
    public BackgroundSound[] sounds;

    [Header("Game Settings")]
    [SerializeField]
    public int POINTS_TO_WIN = 3;
    public float timeBeforeReturnToMenu = 120f;

    [Header("References")]
    public static GameManager Instance;
    public FluvioController fluvio;
    public CalorInfernalScript calorInfernal;
    public GameObject[] Heroes;
    public GameObject unSelectedHeroe;
    public Button[] buttons;
    public MeshRenderer externalTool;
    public GameObject Mirror;
    public Canvas teleportCanvas;
    public Canvas ThankYou;
    public GameObject tpCanvas;

    [Header("Game State")]
    public int minijuegosCompletados = 0;

    private persistanceData data;
    private Dictionary<string, BackgroundSound> _soundMap;
    private int lastPointScore = 0;
    private LoadingScreen sceneManager;

    private const string TERRAIN_NAME = "TerrenoPlain";
    private const string XR_RIG_NAME = "XR Rig";
    private const string SCENE_MANAGER_NAME = "SceneManager";
    private const string EVENT_SYSTEM_NAME = "EventSystem";
    private const string XR_INTERACTION_MANAGER_NAME = "XR Interaction Manager";
    private const string GAME_MANAGER_NAME = "--GameManager";

    private const float BGM_FADE_SPEED = 0.2f;
    private const float AUDIO_SOURCE_DESTROY_DELAY = 9f;
    private const float FLUVIO_ANIMATION_DELAY = 5f;
    private const float BUTTONS_ENABLE_DELAY = 37f;
    private const float CANVAS_ENABLE_DELAY = 61f;
    private const float MENU_RETURN_DELAY = 10f;

    private void Awake()
    {
        if (Mirror != null) Mirror.SetActive(false);
        if (ThankYou != null) ThankYou.enabled = false;
        
        GameObject sceneManagerObj = GameObject.Find(SCENE_MANAGER_NAME);
        if (sceneManagerObj != null)
        {
            sceneManager = sceneManagerObj.GetComponent<LoadingScreen>();
        }

        if (externalTool != null) externalTool.enabled = false;

        data = Resources.Load<persistanceData>("persistanceData");
        
        if (data != null)
        {
            string selectedChar = data.getSelectedCharacter();
            switch (selectedChar)
            {
                case "aguita":
                    if (Heroes != null && Heroes.Length > 0)
                    {
                        unSelectedHeroe = Heroes[0];
                    }
                    externalTool = null;
                    break;
                case "lluvia":
                    if (Heroes != null && Heroes.Length > 1)
                    {
                        unSelectedHeroe = Heroes[1];
                    }
                    break;
                default:
                    if (Heroes != null && Heroes.Length > 0)
                    {
                        unSelectedHeroe = Heroes[0];
                    }
                    break;
            }
        }

        if (calorInfernal != null && calorInfernal.pipeSection != null)
        {
            minijuegosCompletados = 0 - calorInfernal.pipeSection.Length;
        }

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(SetButtons(false, 0f));
        
        if (teleportCanvas != null) teleportCanvas.enabled = false;
        if (tpCanvas != null) tpCanvas.SetActive(false);

        _soundMap = new Dictionary<string, BackgroundSound>(StringComparer.OrdinalIgnoreCase);
        
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
        StartCoroutine(PlayBGM("Evil Loop", 9.5f));
    }

    private void Update()
    {
        if (minijuegosCompletados == POINTS_TO_WIN && lastPointScore == POINTS_TO_WIN - 1)
        {
            EndGame();
        }
        lastPointScore = minijuegosCompletados;
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

    public void ShowMirror()
    {
        if (Mirror != null)
        {
            Mirror.SetActive(true);
        }
    }

    public IEnumerator PlayBGM(string audioName, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (string.IsNullOrEmpty(audioName) || _soundMap == null) yield break;
        
        if (_soundMap.TryGetValue(audioName, out var s) && s?.source != null)
        {
            if (!s.source.loop)
            {
                s.source.loop = true;
            }
            PlayAudio(audioName);
        }
        else
        {
            Debug.LogWarning($"BGM: music '{audioName}' not found on {name}");
        }
    }

    private IEnumerator StopBGM(string audioName)
    {
        if (string.IsNullOrEmpty(audioName) || _soundMap == null) yield break;
        
        if (_soundMap.TryGetValue(audioName, out var s) && s?.source != null)
        {
            while (s.source.isPlaying && s.source.volume > 0f)
            {
                s.source.volume -= Time.deltaTime * BGM_FADE_SPEED;
                yield return null;
            }
        }
    }

    public void StartFluvioAnimation()
    {
        if (fluvio != null)
        {
            fluvio.StartAnimationOnFlag = true;
        }
    }

    private IEnumerator DestroyAudioSource(AudioSource audioToDestroy)
    {
        yield return new WaitForSeconds(AUDIO_SOURCE_DESTROY_DELAY);
        if (audioToDestroy != null)
        {
            Destroy(audioToDestroy);
        }
    }

    public void EndGame()
    {
        if (calorInfernal != null)
        {
            calorInfernal.EndGame();
        }

        Invoke(nameof(StartFluvioAnimation), FLUVIO_ANIMATION_DELAY);
        StartCoroutine(SetButtons(true, BUTTONS_ENABLE_DELAY));
        StartCoroutine(StopBGM("Evil Loop"));
        Invoke(nameof(SetCanvasOn), CANVAS_ENABLE_DELAY);
        Invoke(nameof(EndExploringPhase), timeBeforeReturnToMenu + CANVAS_ENABLE_DELAY);
    }

    private void SetCanvasOn()
    {
        if (teleportCanvas != null)
        {
            teleportCanvas.enabled = true;
        }
    }

    private void EndExploringPhase()
    {
        if (ThankYou != null) ThankYou.enabled = true;
        if (tpCanvas != null) tpCanvas.SetActive(false);

        DestroyAllExceptTerrain();
        Invoke(nameof(GoToMenu), MENU_RETURN_DELAY);
    }

    private void DestroyAllExceptTerrain()
    {
        GameObject[] allRootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject rootObj in allRootObjects)
        {
            if (!ShouldPreserveObject(rootObj.name))
            {
                Destroy(rootObj);
            }
        }
    }

    private bool ShouldPreserveObject(string objectName)
    {
        return objectName == TERRAIN_NAME ||
               objectName == XR_RIG_NAME ||
               objectName == SCENE_MANAGER_NAME ||
               objectName == EVENT_SYSTEM_NAME ||
               objectName == XR_INTERACTION_MANAGER_NAME ||
               objectName == GAME_MANAGER_NAME;
    }

    public void ShowTablet()
    {
        if (tpCanvas != null)
        {
            tpCanvas.SetActive(true);
        }
    }

    private void GoToMenu()
    {
        if (sceneManager != null)
        {
            sceneManager.LoadScene(0);
        }
    }

    public void AddMinigamePoint()
    {
        minijuegosCompletados++;
    }

    public IEnumerator SetButtons(bool state, float time)
    {
        yield return new WaitForSeconds(time);
        
        if (buttons != null)
        {
            foreach (Button button in buttons)
            {
                if (button != null)
                {
                    button.interactable = state;
                }
            }
        }
    }
}
