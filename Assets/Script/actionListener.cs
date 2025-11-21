using UnityEngine;
using UnityEngine.InputSystem;

public class actionListener : MonoBehaviour
{
    public InputActionReference actionReference;
    
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        if (actionReference != null && actionReference.action != null)
        {
            actionReference.action.performed += ctx => OnActionTriggered();
            actionReference.action.canceled += ctx => OnActionUnTriggered();
        }
    }

    public void OnActionTriggered()
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
    }

    public void OnActionUnTriggered()
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }
    }
}
