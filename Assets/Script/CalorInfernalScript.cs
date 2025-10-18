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
    public Vector3 startPosition=Vector3.zero;
    private Vector3 positionToGo;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        positionToGo = startPosition;
        /*
        pipeSection= new PipeBehavior[GameObject.FindGameObjectsWithTag("Minijuego").Length];
        for (int i=0; i < GameObject.FindGameObjectsWithTag("Minijuego").Length; i++) 
        {
         pipeSection[i] = GameObject.FindGameObjectsWithTag("Minijuego")[i].GetComponent<PipeBehavior>();
        }*/



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
        if (!isLookingAtPlayer) LookAtTarget(pipeSection[randObj].transform);
        if (isMoving)MoveToPipe(positionToGo);
       
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Minijuego"))
        {
            Debug.Log("Entered to: "+other.name);
            isLookingAtPlayer = false;
            objectInteract(randObj);

        }
    }
    IEnumerator selectObjectiveRoutine()
    {
        yield return new WaitForSeconds(10f);

        while (isGameStarted) {
            SelectObjective();
            yield return new WaitForSeconds(20f);
        }

    }
    private void LookAtTarget(Transform target)
    {
        Vector3 lookDir = target.position - transform.position;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            if (pipeSection[randObj].gameObject.name=="Wheel")
            {

                Debug.Log("Rotating for wheel");
                targetRot = Quaternion.Euler(targetRot.eulerAngles.x, (targetRot.eulerAngles.y + 90f), (targetRot.eulerAngles.z));
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1 * Time.deltaTime);
        }

    }
    private void MoveToPipe(Vector3 position)
    {
        transform.position = Vector3.Slerp(transform.position, position, moveSpeed * Time.deltaTime);
    }


    private void SelectObjective()
    {
        Debug.Log("Selecting new objective");
        randObj = Random.Range(0, pipeSection.Length);
        positionToGo = pipeSection[randObj].getInfernalPosition();
        isMoving = true;

    }
    private void objectInteract(int objectSelected)
    {
        Debug.Log("Interacting with " + pipeSection[objectSelected].getSectionType());
        StartAnimationInteraction(pipeSection[objectSelected].getSectionType());
        isInteracting = true;
        isLookingAtPlayer = false;
        
        Invoke("stopMoving", 5f);

    }
    private void stopMoving()
    {
        calorInfAnimator.SetTrigger("Idle");
        positionToGo = startPosition;
        isLookingAtPlayer = true;
        isInteracting = false;
    }
    private void StartAnimationInteraction(string objectType)
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
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(startPosition, 0.2f);
    }
}
