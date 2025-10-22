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

    persistanceData data;
    private int lastPointScore=0;
    private void Awake()
    {
        data = Resources.Load<persistanceData>("persistanceData");
        switch (data.getSelectedCharacter())
        {
            case "aguita":
                unSelectedHeroe=Heroes[0];
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
        SetButtons(false);
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
        SetButtons(true);
    }
    public void AddMinigamePoint()
    {
        minijuegosCompletados++;
    }
    private IEnumerator SetButtons(bool State)
    {
        yield return new WaitForSeconds(61f);
        foreach (Button button in buttons)
        {
                       button.interactable = State;
        }
    }
}
