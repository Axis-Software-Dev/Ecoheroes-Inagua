using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CreditsXRSetup : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private bool autoSetupOnStart = true;

    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupCreditsXRInteraction();
        }
    }

    public void SetupCreditsXRInteraction()
    {
        GameObject isoA = GameObject.Find("Iso A");
        if (isoA == null)
        {
            Debug.LogWarning("Iso A not found - Credits XR Setup skipped");
            return;
        }

        Transform creditsCanvas = isoA.transform.Find("Credits Canvas");
        if (creditsCanvas == null)
        {
            Debug.LogWarning("Credits Canvas not found - Credits XR Setup skipped");
            return;
        }

        Transform creditsButton = creditsCanvas.Find("Credits Button");
        if (creditsButton == null)
        {
            Debug.LogWarning("Credits Button not found - Credits XR Setup skipped");
            return;
        }

        XRSimpleInteractable existingInteractable = creditsButton.GetComponent<XRSimpleInteractable>();
        if (existingInteractable != null)
        {
            Debug.Log("Credits Button already has XR Interactable component");
            ConnectToController(creditsCanvas, existingInteractable);
            return;
        }

        BoxCollider existingCollider = creditsButton.GetComponent<BoxCollider>();
        if (existingCollider == null)
        {
            BoxCollider buttonCollider = creditsButton.gameObject.AddComponent<BoxCollider>();
            buttonCollider.size = new Vector3(200f, 60f, 1f);
            Debug.Log("Added BoxCollider to Credits Button");
        }

        XRSimpleInteractable xrInteractable = creditsButton.gameObject.AddComponent<XRSimpleInteractable>();
        Debug.Log("Added XRSimpleInteractable to Credits Button");

        ConnectToController(creditsCanvas, xrInteractable);
        
        Debug.Log("Credits XR Setup completed successfully!");
    }

    private void ConnectToController(Transform creditsCanvas, XRSimpleInteractable xrInteractable)
    {
        CreditsController creditsController = creditsCanvas.GetComponent<CreditsController>();
        if (creditsController != null)
        {
            creditsController.xrInteractable = xrInteractable;
            
            xrInteractable.activated.RemoveAllListeners();
            xrInteractable.activated.AddListener(creditsController.OnXRActivated);
            
            Debug.Log("Connected XR Interactable activated event to Credits Controller");
        }
        else
        {
            Debug.LogWarning("CreditsController not found on Credits Canvas");
        }
    }
}
