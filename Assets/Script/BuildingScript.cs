using UnityEngine;

public class BuildingScript : MonoBehaviour
{
    public GameObject[] EcoHeroes;
    public Vector3 playerPosition;
    
    public Vector3 characterPosition;
    private Vector3 _worldPosition;
    private Transform _player;
    private bool _isMovingPlayer = false;
    private bool isLookingAtPlayer = true;
    private Canvas _buildingCanvas;
    public string textToShow;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        _buildingCanvas=GetComponentInChildren<Canvas>();
        _worldPosition =transform.TransformPoint(playerPosition);
       
    }

    void Start()
    {
        _player=GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        _buildingCanvas
            .transform.LookAt(Camera.main.transform);
        if (_isMovingPlayer)
        {
            MovePlayerToPosition(_worldPosition);
        }
        LookAtTargetCharacter(_player, GameManager.Instance.unSelectedHeroe);
    }
    public Vector3 GetToGoPosition()
    {
        return _worldPosition;
    }
    private void SetUnselectedCharacter()
    {

        GameObject speakingCharacter=GameManager.Instance.unSelectedHeroe;


        speakingCharacter.layer = LayerMask.NameToLayer("Default");
        speakingCharacter.GetComponentInChildren<SkinnedMeshRenderer>().gameObject.layer 
            = LayerMask.NameToLayer("Default");
        speakingCharacter.GetComponent<CharacterSpeaking>().ShowDialogText(textToShow);
        speakingCharacter.GetComponent<Animator>().Rebind();
        speakingCharacter.GetComponent<Animator>().Update(0f); ;
        speakingCharacter.GetComponent<Animator>().enabled = true;
        speakingCharacter.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        speakingCharacter.transform.position=GetCharacterPosition();
        speakingCharacter.SetActive(true);
        if(GameManager.Instance.externalTool!=null)GameManager.Instance.externalTool.enabled=true;
        Invoke("StartPresenting", 1f);
        
    }
    public Vector3 GetCharacterPosition()
    {
        return transform.TransformPoint(characterPosition);
    }
    private void MovePlayerToPosition(Vector3 Position)
    {
        _player.position = Vector3.Slerp(_player.position, Position, 5f * Time.deltaTime);
        if(_player.position==Position)_isMovingPlayer=false;
    }
    
    private void StartPresenting()
    {
        GameObject speakingCharacter = GameManager.Instance.unSelectedHeroe;
        speakingCharacter.GetComponent<Animator>().SetBool("isPresenting",true);
    }
    public void StartMovingPlayer()
    {
        _isMovingPlayer = true;
        SetUnselectedCharacter();
    }

    private void OnDrawGizmos()
    {
        
        Gizmos.color = Color.pink;
        Vector3 newWorldPosition=transform.TransformPoint(playerPosition);
        Gizmos.DrawSphere(newWorldPosition,1f);

    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 newCharacterPosition=transform.TransformPoint(characterPosition);
        Gizmos.DrawSphere(newCharacterPosition,1f);
    }
    private void LookAtTargetCharacter(Transform target,GameObject ObjectRotate)
    {
        Vector3 lookDir = target.position - ObjectRotate.transform.position;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            ObjectRotate.transform.rotation = Quaternion.Slerp(ObjectRotate.transform.rotation, targetRot, Time.deltaTime);
        }


    }
}
