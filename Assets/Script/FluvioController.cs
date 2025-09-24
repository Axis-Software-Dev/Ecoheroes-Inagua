using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class FluvioController : MonoBehaviour
{
    [Header("Audio Settings")]
    public Sound[] sounds;

    [Header("Animation Settings")]
    public Animator hydroAnimator;
    public GameObject[] toFollowPosition;
    public SkinnedMeshRenderer[] skinMeshRenderedArray;
    public float speed = 1.5f;
    public float[] interval;

    [Header("Flags")]
    public bool startGreetingAnimation = false;

    // Internals
    private float timer = 0f;
    private float step, defaultSpeed;
    private bool activeTimer = false, animationHasStarted = false, animationIsPlaying = false, allowLookAtPlayer = false;

    private GameObject player;
    private Vector3 lookToPlayer;
    private Quaternion saveRotation;

    // Audio system
    private void Awake()
    {
        // Audio initialization
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.time = s.timeToSkip;
            s.source.spatialBlend = s.spatialSound;
        }

        // Animation initialization
        saveRotation = this.transform.rotation;
        foreach (GameObject position in toFollowPosition)
        {
            position.GetComponent<MeshRenderer>().enabled = false;
        }
        foreach (SkinnedMeshRenderer skin in skinMeshRenderedArray)
        {
            skin.enabled = false;
        }

        hydroAnimator = GetComponentInChildren<Animator>();
        player = GameObject.FindWithTag("MainCamera");
        defaultSpeed = speed;
    }

    private void Start()
    {
        transform.position = toFollowPosition[0].transform.position;
        transform.rotation = toFollowPosition[0].transform.rotation;
    }

    private void Update()
    {
        // Look direction vector
        lookToPlayer = player.transform.position - this.transform.position;
        Debug.DrawRay(this.transform.position, lookToPlayer, Color.blue);

        step = speed * Time.deltaTime;

        // Timer control
        if (activeTimer == true)
        {
            timer += Time.deltaTime;
        }

        // Greeting animation trigger
        if (startGreetingAnimation && !animationHasStarted)
        {
            startAnimation();
        }
        animationHasStarted = startGreetingAnimation;

        // Play animation sequence
        if (animationIsPlaying)
        {
            if (timer >= interval[0] && timer <= interval[1])
            {
                foreach (SkinnedMeshRenderer skin in skinMeshRenderedArray)
                {
                    skin.enabled = true;
                }
                audioPlay("Teleport1");
            }
            else if (timer >= interval[1] && timer <= interval[2])
            {
                allowLookAtPlayer = true;
                hydroAnimator.SetBool("StartAnimation", true);
                audioPlay("Saludo");
            }
            else if (timer >= interval[2] && timer <= interval[3])
            {
                Walk(1, false);
            }
            else if (timer >= interval[3] && timer <= interval[4])
            {
                // Reserved step
            }
            else if (timer >= interval[4] && timer <= interval[5])
            {
                hydroAnimator.SetBool("stayIdle", false);
            }
            else if (timer >= interval[5] && timer <= interval[6])
            {
                audioPlay("Ayuda");
            }
            else if (timer >= interval[6] && timer <= interval[7])
            {
                hydroAnimator.SetBool("stayAviso", false);
                hydroAnimator.SetBool("stayIdle", true);
            }
            else if (timer >= interval[7] && timer <= interval[8])
            {
                // Pause
            }
            else if (timer >= interval[8] && timer <= interval[9])
            {
                speed = 1f;
                allowLookAtPlayer = false;
                Walk(2, true);
            }
            else if (timer >= interval[9] && timer <= interval[10])
            {
                audioPlay("Teleport2");
                foreach (SkinnedMeshRenderer skin in skinMeshRenderedArray)
                {
                    skin.enabled = false;
                }
            }

            if (timer >= interval[interval.Length - 1])
            {
                stopTimer();
                speed = defaultSpeed;
            }
        }
    }

    private void LateUpdate()
    {
        if (allowLookAtPlayer)
        {
            LookAtPlayer();
        }
    }

    // ----------------------
    // Audio functions
    // ----------------------
    public void audioPlay(string audioName)
    {
        Sound toPlaySound = Array.Find(sounds, sound => sound.name == audioName);
        if (toPlaySound != null && !toPlaySound.source.isPlaying)
        {
            toPlaySound.source.Play();
        }
    }

    // ----------------------
    // Animation functions
    // ----------------------
    void startTimer()
    {
        activeTimer = true;
        animationIsPlaying = true;
    }

    void stopTimer()
    {
        activeTimer = false;
        animationIsPlaying = false;
    }

    void startAnimation()
    {
        startTimer();
    }

    void LookAtPlayer()
    {
        this.transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(lookToPlayer),
            2 * Time.deltaTime
        );
    }

    void Walk(int positionToMove, bool lookToDirection)
    {
        if (lookToDirection)
        {
            this.transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(toFollowPosition[positionToMove].transform.position - this.transform.position),
                2 * Time.deltaTime
            );
        }
        transform.position = Vector3.MoveTowards(
            transform.position,
            toFollowPosition[positionToMove].transform.position,
            step
        );
    }
}
