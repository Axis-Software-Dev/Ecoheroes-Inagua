using Fluvio;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public FluvioController fluvio;
    public CalorInfernalScript calorInfernal;
    public int minijuegosCompletados = 0;
    [SerializeField]
    public int POINTS_TO_WIN = 3;


    private int lastPointScore=0;
    private void Awake()
    {
        minijuegosCompletados = (0 - calorInfernal.pipeSection.Length);
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
      

    }
    
    void Update()
    {
        if (minijuegosCompletados == POINTS_TO_WIN && lastPointScore == POINTS_TO_WIN-1) EndGame();
        lastPointScore = minijuegosCompletados;
    }


    public void StartFluvioAnimation()
    {
        fluvio.StartAnimationOnFlag = true;

    }
    public void EndGame()
    {
        calorInfernal.EndGame();
        Invoke("StartFluvioAnimation",5f);
    }
    public void AddMinigamePoint()
    {
        minijuegosCompletados++;
    }
}
