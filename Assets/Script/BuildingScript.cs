using UnityEngine;

public class BuildingScript : MonoBehaviour
{
    public Vector3 playerPosition;
    public Vector3 characterPosition;
    private Vector3 _worldPosition;
    private Transform _player;
    private bool _isMovingPlayer = false;
    private Canvas _buildingCanvas;
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
    }
    public Vector3 GetToGoPosition()
    {
        return _worldPosition;
    }
    private void MovePlayerToPosition(Vector3 Position)
    {
        _player.position = Vector3.Slerp(_player.position, Position, 5f * Time.deltaTime);
        if(_player.position==Position)_isMovingPlayer=false;
    }
    
    public void StartMovingPlayer()
    {
        _isMovingPlayer = true;
    }

    private void OnDrawGizmos()
    {
        
        Gizmos.color = Color.pink;
        Vector3 newWorldPosition=transform.TransformPoint(playerPosition);
        Gizmos.DrawSphere(newWorldPosition, 1f);
        
        
    }
}
