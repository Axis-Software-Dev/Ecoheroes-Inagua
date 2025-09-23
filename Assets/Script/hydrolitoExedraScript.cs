using UnityEngine;

public class hydrolitoExedraScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Animator hydroAnimator;
    public GameObject[] toFollowPosition;
    public SkinnedMeshRenderer[] skinMeshRenderedArray;
    public float speed = 1.5f,defaultSpeed;
    public float[] interval;
    hydrolitoAudioManager hydroAudio;
    
    [SerializeField] float timer = 0f;
    float step;
    bool activeTimer = false, animationHasStarted = false, animationIsPlaying = false, allowLookAtPlayer = false;
    public bool startGreetingAnimation = false;
    [SerializeField] GameObject player;
    Vector3 lookToPlayer;
    Quaternion saveRotation;
    public GameObject nave;
    [Range(0f,50f)]
    public float Offsetx,Offsety,Offsetz;
    private void Awake()
    {
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
        hydroAudio = GetComponentInChildren<hydrolitoAudioManager>();
        player = GameObject.FindWithTag("MainCamera");
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
                foreach (SkinnedMeshRenderer skin in skinMeshRenderedArray)
                {
                    skin.enabled = true;
                }
                Walk(1);
            }
            else if (timer >= interval[1] && timer <= interval[2])
            {
                speed = 2f;
                allowLookAtPlayer=true;
                hydroAnimator.SetBool("StartAnimation", true);
                hydroAudio.audioPlay("Saludo");
            }
            else if (timer >= interval[2] && timer <= interval[3])
            {
                
                Walk(2);
            }
            else if (timer >= interval[3] && timer <= interval[4])
            {
                
            }
            else if (timer >= interval[4] && timer <= interval[5])
            {
                
                hydroAnimator.SetBool("stayIdle", false);
            }
            else if (timer >= interval[5] && timer <= interval[6])
            {

                hydroAudio.audioPlay("Ayuda");
            }
            else if (timer >= interval[6] && timer <= interval[7])
            {
                hydroAnimator.SetBool("stayAviso", false);
                hydroAnimator.SetBool("stayIdle", true);

            }
            else if (timer >= interval[7] && timer <= interval[8])
            {
                //Pausa
            }
            else if (timer >= interval[8] && timer <= interval[9])
            {
                
                speed = 2.5f;
                allowLookAtPlayer = false;
                Walk(1);

            }
            else if (timer >= interval[9] && timer <= interval[10])
            {
                
                Walk(0);

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

        nave.transform.localPosition = new Vector3(nave.transform.localPosition.x + 2f, nave.transform.localPosition.y, nave.transform.localPosition.z);
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
