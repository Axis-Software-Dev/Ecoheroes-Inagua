using UnityEngine;

public class hydrolitoExedraScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Animator hydroAnimator;
    public GameObject[] toFollowPosition;
    public float speed = 1.5f,defaultSpeed;
    public float[] interval;
    hydrolitoAudioManager hydroAudio;

    [SerializeField] float timer = 0f;
    float step;
    bool activeTimer = false, animationHasStarted = false, animationIsPlaying = false, allowLookAtPlayer = false;
    public bool startGreetingAnimation = false;
    [SerializeField] GameObject player;
    Vector3 lookToPlayer;
    private void Awake()
    {
        foreach (GameObject position in toFollowPosition)
        {
            position.GetComponent<MeshRenderer>().enabled = false;
        }
        hydroAnimator = GetComponentInChildren<Animator>();
        hydroAudio = GetComponentInChildren<hydrolitoAudioManager>();
        player = GameObject.FindWithTag("Player");
        defaultSpeed = speed;
    }

    void Start()
    {
        transform.position = toFollowPosition[0].transform.position;
        transform.rotation = toFollowPosition[0].transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        lookToPlayer = player.transform.position - this.transform.position;
        Debug.DrawRay(this.transform.position, lookToPlayer, Color.blue);

        step = speed * Time.deltaTime;
        if (activeTimer == true)
        {
            timer += Time.deltaTime;
        }
        if (startGreetingAnimation && !animationHasStarted)
        {
            startAnimation();
        }
        animationHasStarted = startGreetingAnimation;

        if (animationIsPlaying)
        {
            if (timer >= interval[0] && timer <= interval[1])
            {
                Walk( 1);
            }
            else if (timer >= interval[1] && timer <= interval[2])
            {
                allowLookAtPlayer = true;
            }
            else if (timer >= interval[2] && timer <= interval[3])
            {
                hydroAnimator.SetBool("stayIdle", false);
            }
            else if (timer >= interval[3] && timer <= interval[4])
            {

                hydroAudio.audioPlay("Ayuda");
            }
            else if (timer >= interval[4] && timer <= interval[5])
            {
                hydroAnimator.SetBool("stayAviso", false);
                hydroAnimator.SetBool("stayIdle", true);

            }
            else if (timer >= interval[5] && timer <= interval[6])
            {
                //Pause

            }
            else if (timer >= interval[6] && timer <= interval[7])
            {
                speed = 2f;
                allowLookAtPlayer = false;
                Walk(2);

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
        hydroAnimator.SetBool("StartAnimation", true);
        hydroAudio.audioPlay("Saludo");

    }
    void LookAtPlayer()
    {
        this.transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookToPlayer), 2 * Time.deltaTime);
    }
    void Walk(int positionToMove)
    {
       
        this.transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toFollowPosition[positionToMove].transform.position - this.transform.position), 2 * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, toFollowPosition[positionToMove].transform.position, step);


    }
}
