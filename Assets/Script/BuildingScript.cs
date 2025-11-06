using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class BuildingScript : MonoBehaviour
{
    public GameObject[] EcoHeroes;
    public Vector3 playerPosition;
    public TeleportEffect playerTeleportEffect;
    public Vector3 characterPosition;
    public float timeTostay = 15f;
    private Vector3 _worldPosition;
    private Transform _player;
    private Transform _mainCamera;
    private bool _isMovingPlayer = false;
    private TeleportationProvider _teleportationProvider;
    public string textToShow;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        if (playerTeleportEffect == null) playerTeleportEffect=
                GameObject.Find("TP Player").GetComponent<TeleportEffect>();
        _worldPosition = transform.TransformPoint(playerPosition);
        _teleportationProvider = 
            GameObject.Find("Teleportation").GetComponent<TeleportationProvider>();

    }

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _mainCamera = Camera.main != null ? Camera.main.transform : GameObject.FindWithTag("MainCamera")?.transform;

    }

    // Update is called once per frame
    void Update()
    {

        
        LookAtTargetCharacter(_mainCamera, GameManager.Instance.unSelectedHeroe);
    }
    public Vector3 GetToGoPosition()
    {
        return _worldPosition;
    }
    private void SetUnselectedCharacter()
    {

        GameObject speakingCharacter = GameManager.Instance.unSelectedHeroe;


        speakingCharacter.layer = LayerMask.NameToLayer("Default");
        speakingCharacter.GetComponentInChildren<SkinnedMeshRenderer>().gameObject.layer
            = LayerMask.NameToLayer("Default");
        speakingCharacter.GetComponent<CharacterSpeaking>().ShowDialogText(textToShow);
        speakingCharacter.GetComponent<Animator>().Rebind();
        speakingCharacter.GetComponent<Animator>().Update(0f); ;
        speakingCharacter.GetComponent<Animator>().enabled = true;
        speakingCharacter.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        speakingCharacter.transform.position = GetCharacterPosition();
        speakingCharacter.SetActive(true);
        if (GameManager.Instance.externalTool != null) GameManager.Instance.externalTool.enabled = true;
        Invoke("StartPresenting", 1f);

    }
    public Vector3 GetCharacterPosition()
    {
        return transform.TransformPoint(characterPosition);
    }
    private IEnumerator MovePlayerToPosition(Vector3 Position)
    {
        int randAudioNum= Random.Range(1, 3);
        string audioName = "TP" + randAudioNum.ToString();
        GameManager.Instance.PlayAudio(audioName);
        playerTeleportEffect.originalPosition = _player.position;
        playerTeleportEffect.PlayCurtainEffect();
        
        yield return new WaitForSeconds(0.7f);
        playerTeleportEffect.originalPosition = _worldPosition;
        if (_teleportationProvider == null)
        {
            Debug.LogError("TeleportationProvider no encontrado!");
            yield break;
        }

        TeleportRequest request = new TeleportRequest
        {
            destinationPosition = _worldPosition,
            matchOrientation = MatchOrientation.None
        };

        bool queued = _teleportationProvider.QueueTeleportRequest(request);
        
        if (queued)
            Debug.Log("Teleport en cola!");
        else
            Debug.LogWarning("No se pudo encolar teleporte");
        
    }


    private void StartPresenting()
    {
        GameObject speakingCharacter = GameManager.Instance.unSelectedHeroe;
        speakingCharacter.GetComponent<Animator>().SetBool("isPresenting", true);
    }
    public void StartMovingPlayer()
    {
        StartCoroutine(MovePlayerToPosition(_worldPosition));
        SetUnselectedCharacter();
        SetButtons();

    }

    private void SetButtons()
    {
        GameManager.Instance.StartCoroutine(GameManager.Instance.SetButtons(false, 0f));
        GameManager.Instance.StartCoroutine(GameManager.Instance.SetButtons(true, timeTostay));

    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.pink;
        Vector3 newWorldPosition = transform.TransformPoint(playerPosition);
        Gizmos.DrawSphere(newWorldPosition, 1f);

    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 newCharacterPosition = transform.TransformPoint(characterPosition);
        Gizmos.DrawSphere(newCharacterPosition, 1f);
    }
    private void LookAtTargetCharacter(Transform target, GameObject ObjectRotate)
    {
        Vector3 lookDir = target.position - ObjectRotate.transform.position;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            ObjectRotate.transform.rotation = Quaternion.Slerp(ObjectRotate.transform.rotation, targetRot, Time.deltaTime);
        }


    }
}
