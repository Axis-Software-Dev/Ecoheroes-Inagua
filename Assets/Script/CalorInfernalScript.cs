using System.Collections;
using UnityEngine;

public class CalorInfernalScript : MonoBehaviour
{

    Animator calorInfAnimator;
    public PipeBehavior[] pipeSection;
    Transform _playerTransform;
    public bool isLookingAtPlayer = false;
    public bool isMoving = false;
    public float moveSpeed = 1f;
    int randObj = 0;
    private bool isInteracting = false;
    public bool isGameStarted = false;
    Vector3 lastPosition=Vector3.zero;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        pipeSection= new PipeBehavior[GameObject.FindGameObjectsWithTag("Minijuego").Length];
        for (int i=0; i < GameObject.FindGameObjectsWithTag("Minijuego").Length; i++) 
        {
         pipeSection[i] = GameObject.FindGameObjectsWithTag("Minijuego")[i].GetComponent<PipeBehavior>();
        }
        
        
        
        calorInfAnimator = GetComponent<Animator>();
        calorInfAnimator?.Play("CalorInfernalAnim");
        _playerTransform = Camera.main != null ? Camera.main.transform : GameObject.FindWithTag("MainCamera")?.transform;
    }
    void Start()
    {
        StartCoroutine(selectObjectiveRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        
        if (isLookingAtPlayer) LookAtTarget(_playerTransform);
        if(isMoving)MoveToPipe(randObj);
       
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Minijuego"))
        {
            isLookingAtPlayer = false;
            objectInteract(randObj);

        }
    }
    IEnumerator selectObjectiveRoutine()
    {
        yield return new WaitForSeconds(10f);

        while (isGameStarted) {
            SelectObjective();
            yield return new WaitForSeconds(8f);
        }

    }
    private void LookAtTarget(Transform target)
    {
        Vector3 lookDir = _playerTransform.position - transform.position;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1 * Time.deltaTime);
        }

    }
    private void MoveToPipe(int pipeNumber)
    {
        transform.position = Vector3.Slerp(transform.position, pipeSection[pipeNumber].transform.position,moveSpeed*Time.deltaTime);
    }


    private void SelectObjective()
    {
        Debug.Log("Selecting new objective");
        randObj = Random.Range(0, pipeSection.Length);
        isMoving = true;


    }
    private void objectInteract(int objectSelected)
    {
        Debug.Log("Interacting with " + pipeSection[objectSelected].getSectionType());
        StartAnimation(pipeSection[objectSelected].getSectionType());
        isInteracting = true;
        Invoke("stopMoving", 10f);

    }
    private void stopMoving()
    {
        isMoving = false;
    }
    private void StartAnimation(string objectType)
    {
        switch (objectType)
        {
            case "cables":
                calorInfAnimator.SetTrigger("Punch");
                break;
            case "screw":
                calorInfAnimator.SetTrigger("Crank");
                break;
            case "wheel":
                calorInfAnimator.SetTrigger("Valve");
                break;
            default:
                Debug.Log("No se encontro el tipo de objeto");
                break;
        }
    }
}
