using UnityEngine;
using UnityEngine.InputSystem;
public class actionListener : MonoBehaviour
{
    public InputActionReference actionReference;
    private MeshRenderer meshRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        actionReference.action.performed += ctx => OnActionTriggered();
        actionReference.action.canceled += ctx => OnActionUnTriggered();
        

    }

    public void OnActionTriggered()
    {
        meshRenderer.enabled = false;
    }
    public void OnActionUnTriggered()
    {
        meshRenderer.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
