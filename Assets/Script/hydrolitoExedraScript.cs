using UnityEngine;

public class hydrolitoExedraScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Animator hydroAnimator;

    hydrolitoAudioManager hydroAudio;

    float timer = 0f;
    bool activeTimer = false;
    public bool startGreetingAnimation=false;
    private void Awake()
    {
        hydroAnimator=GetComponentInChildren<Animator>();
        hydroAudio = GetComponentInChildren<hydrolitoAudioManager>();
        
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (activeTimer==true)
        {
            timer += Time.deltaTime;
        }
        if (startGreetingAnimation)
        {
           Invoke("startAnimation",0.1f);
        }
    }
    void startTimer()
    {
        activeTimer = true;
    }
    void stopTimer() {  
        activeTimer = false; 
    }
    void startAnimation()
    {
        startTimer();
        hydroAnimator.SetBool("StartAnimation",true);
        hydroAudio.audioPlay("Saludo");
    }
}
