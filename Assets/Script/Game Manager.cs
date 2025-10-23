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
    public class BackgroundSound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public float pitch = 1f;
        public float timeToSkip = 0f;
        [Range(0f, 1f)] public float spatialSound = 0f;

        [NonSerialized] public AudioSource source;
    }
    public BackgroundSound[] sounds;
    public static GameManager Instance;
    public FluvioController fluvio;
    public CalorInfernalScript calorInfernal;
    public int minijuegosCompletados = 0;
    public Canvas teleportCanvas;
    [SerializeField]
    public int POINTS_TO_WIN = 3;
    public GameObject[] Heroes;
    public GameObject unSelectedHeroe;
    public Button[] buttons;
    public MeshRenderer externalTool;
    persistanceData data;
    private Dictionary<string, BackgroundSound> _soundMap;
    private int lastPointScore=0;
    private void Awake()
    {
        externalTool.enabled = false;
        data = Resources.Load<persistanceData>("persistanceData");
        switch (data.getSelectedCharacter())
        {
            case "aguita":
                unSelectedHeroe=Heroes[0];
                externalTool = null;
                break;
            case "lluvia":
                unSelectedHeroe=Heroes[1];
                
                break;
            default:
                unSelectedHeroe=Heroes[0];
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

        StartCoroutine(SetButtons(false,0f));
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

        StartCoroutine(PlayStartingAudio());
    }

    void Update()
    {
        if (minijuegosCompletados == POINTS_TO_WIN && lastPointScore == POINTS_TO_WIN - 1) EndGame();
        lastPointScore = minijuegosCompletados;
    }

    public IEnumerator PlayBGM(string audioName, float LoopDelay)
    {
        
        if (string.IsNullOrEmpty(audioName) || _soundMap == null) yield return null;
        if (_soundMap.TryGetValue(audioName, out var s) && s?.source != null)
        {
            if (!s.source.isPlaying) s.source.Play();
        }
        else
        {
            Debug.LogWarning($"PlayAudio: sound '{audioName}' not found on {name}");
        }
        yield return new WaitForSeconds(LoopDelay);
        PlayBGM(audioName, LoopDelay);
    }
    private IEnumerator PlayStartingAudio()
    {
        yield return new WaitForSeconds(9.5f);
        StartCoroutine(PlayBGM("Evil Loop",6f));
    }

    public void StartFluvioAnimation()
    {
        fluvio.StartAnimationOnFlag = true;

    }
    public void EndGame()
    {
        calorInfernal.EndGame();
        Invoke("StartFluvioAnimation",5f);
        StartCoroutine(SetButtons(true,37f));
        Invoke("SetCanvasOn", 61f);
    }
    private void SetCanvasOn() {
        teleportCanvas.enabled = true;
    }


    public void AddMinigamePoint()
    {
        minijuegosCompletados++;
    }
    public IEnumerator SetButtons(bool State,float Time)
    {

        yield return new WaitForSeconds(Time);
        foreach (Button button in buttons)
        {
          button.interactable = State;
        }
        
    }
}
