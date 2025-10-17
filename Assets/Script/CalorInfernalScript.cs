using UnityEngine;

public class CalorInfernalScript : MonoBehaviour
{

    Animator calorInfAnimator;
    public GameObject[] pipeSection;
    Transform _playerTransform;
    public bool isLookingAtPlayer = false;
    public bool isMoving = false;
    public float moveSpeed = 1f;
    int randObj = 0;
    Vector3 lastPosition=Vector3.zero;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        calorInfAnimator = GetComponent<Animator>();
        calorInfAnimator?.Play("CalorInfernalAnim");
        _playerTransform = Camera.main != null ? Camera.main.transform : GameObject.FindWithTag("MainCamera")?.transform;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isLookingAtPlayer) LookAtTarget(_playerTransform);
        if(isMoving)MoveToPipe(randObj);
        if (transform.position == pipeSection[randObj].transform.position&&lastPosition!= pipeSection[randObj].transform.position) { 
            Debug.Log("Llegue al objetivo");
            
            calorInfAnimator.SetTrigger("Punch");
        }
        lastPosition= transform.position;
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
        transform.position = Vector3.MoveTowards(transform.position, pipeSection[pipeNumber].transform.position,moveSpeed*Time.deltaTime);
    }


    private void SelectObjective()
    {
        randObj= Random.Range(0, pipeSection.Length);
        
       ;
    }
}
