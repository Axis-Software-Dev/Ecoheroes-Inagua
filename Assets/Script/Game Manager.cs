
using Fluvio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Serializable]
    public class BackgroundSound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public float pitch = 1f;
        public float timeToSkip = 0f;
        [Range(0f, 1f)] public float spatialSound = 0f;
        public bool loop = false;
        [NonSerialized] public AudioSource source;
    }
    public BackgroundSound[] sounds;
    public float timeBeforeReturnToMenu = 120f;
    public static GameManager Instance;
    public FluvioController fluvio;
    public CalorInfernalScript calorInfernal;
    public int minijuegosCompletados = 0;
    public Canvas teleportCanvas;
    public Canvas ThankYou;
    [SerializeField]
    public int POINTS_TO_WIN = 3;
    public GameObject[] Heroes;
    public GameObject unSelectedHeroe;
    public Button[] buttons;
    public MeshRenderer externalTool;
    public GameObject Mirror;
    persistanceData data;
    private Dictionary<string, BackgroundSound> _soundMap;
    private int lastPointScore = 0;
    private bool isPlayingLoop = true;
    private LoadingScreen sceneManager;
    [SerializeField]
    private bool hasMirrorShowed = false;
    private void Awake()
    {
        Mirror.SetActive(false);
        if (ThankYou!=null)ThankYou.enabled = false;
        sceneManager = GameObject.Find("SceneManager").GetComponent<LoadingScreen>();
        externalTool.enabled = false;
        data = Resources.Load<persistanceData>("persistanceData");
        switch (data.getSelectedCharacter())
        {
            case "aguita":
                unSelectedHeroe = Heroes[0];
                externalTool = null;
                break;
            case "lluvia":
                unSelectedHeroe = Heroes[1];

                break;
            default:
                unSelectedHeroe = Heroes[0];
                break;
        }

        minijuegosCompletados = (0 - calorInfernal.pipeSection.Length);
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        StartCoroutine(SetButtons(false, 0f));
        teleportCanvas.enabled = false;

        _soundMap = new Dictionary<string, BackgroundSound>(StringComparer.OrdinalIgnoreCase);
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

    void Start()
    {

        StartCoroutine(PlayBGM("Evil Loop", 9.5f));
    }

    void Update()
    {
        if (minijuegosCompletados == POINTS_TO_WIN && lastPointScore == POINTS_TO_WIN - 1) EndGame();
        lastPointScore = minijuegosCompletados;
       
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
    public void ShowMirror()
    {
        Mirror.SetActive(true);
    }
    public IEnumerator PlayBGM(string audioName, float Delay)
    {
       
        yield return new WaitForSeconds(Delay);
        if (string.IsNullOrEmpty(audioName) || _soundMap == null) yield return null;
        if (_soundMap.TryGetValue(audioName, out var s) && s?.source != null)
        {
            if (!s.source.loop==false) s.source.loop = true;
            PlayAudio(audioName);
        }
        else
        {
            Debug.LogWarning($"BGM: music '{audioName}' not found on {name}");
        }
        

    }
    private IEnumerator StopBGM(string audioName)
    {
        if (string.IsNullOrEmpty(audioName) || _soundMap == null) yield return null;
        if (_soundMap.TryGetValue(audioName, out var s) && s?.source != null)
        {
            while (s.source.isPlaying&&s.source.volume>0f)
            {
                s.source.volume -= Time.deltaTime * 0.1f;
                yield return null;
            }
        }
        

    }

    public void StartFluvioAnimation()
    {
        fluvio.StartAnimationOnFlag = true;

    }
    private IEnumerator DestroyAudioSource(AudioSource AudioToDestroy)
    {
        yield return new WaitForSeconds(9f);    
        Destroy(AudioToDestroy);
    }
    public void EndGame()
    {
        calorInfernal.EndGame();
        Invoke("StartFluvioAnimation", 5f);
        StartCoroutine(SetButtons(true, 37f));
        StartCoroutine(StopBGM("Evil Loop"));
        Invoke("SetCanvasOn", 61f);
        Invoke("EndExploringPhase", timeBeforeReturnToMenu+61f);
    }
    private void SetCanvasOn()
    {
        teleportCanvas.enabled = true;
    }

    private void EndExploringPhase()
    {
        if(ThankYou!=null)ThankYou.enabled = true;
        Invoke("GoToMenu", 10f);
    }

    private void GoToMenu()
    {
        sceneManager.LoadScene(0);
    }

    public void AddMinigamePoint()
    {
        minijuegosCompletados++;
    }
    public IEnumerator SetButtons(bool State, float Time)
    {

        yield return new WaitForSeconds(Time);
        foreach (Button button in buttons)
        {
            button.interactable = State;
        }

    }
}
