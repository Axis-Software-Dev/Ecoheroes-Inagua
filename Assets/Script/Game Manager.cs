using Fluvio;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public FluvioController fluvio;
    public CalorInfernalScript calorInfernal;
    public int minijuegosCompletados = 0;
    [SerializeField]
    public int POINTS_TO_WIN = 3;
    public GameObject[] Heroes;
    public GameObject unSelectedHeroe;
    public Button[] buttons;
    public MeshRenderer externalTool;
    persistanceData data;
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
        StartCoroutine(SetButtons(true,61f));
    }
    public void AddMinigamePoint()
    {
        minijuegosCompletados++;
    }
    private IEnumerator SetButtons(bool State,float Time)
    {

        yield return new WaitForSeconds(Time);
        foreach (Button button in buttons)
        {
          button.interactable = State;
        }
        
    }
}
