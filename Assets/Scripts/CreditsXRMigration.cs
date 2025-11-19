using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CreditsXRMigration : MonoBehaviour
{
    private void Start()
    {
        MigrateCreditsButton();
    }

    private void MigrateCreditsButton()
    {
        GameObject isoA = GameObject.Find("Iso A");
        if (isoA == null)
        {
            Debug.LogWarning("Iso A not found for Credits XR Migration");
            return;
        }

        Transform creditsCanvas = isoA.transform.Find("Credits Canvas");
        if (creditsCanvas == null)
        {
            Debug.LogWarning("Credits Canvas not found for Credits XR Migration");
            return;
        }

        Transform creditsButton = creditsCanvas.Find("Credits Button");
        if (creditsButton == null)
        {
            Debug.LogWarning("Credits Button not found for Credits XR Migration");
            return;
        }

        XRSimpleInteractable existingInteractable = creditsButton.GetComponent<XRSimpleInteractable>();
        if (existingInteractable != null)
        {
            Debug.Log("Credits Button already has XR components - migration skipped");
            Destroy(gameObject);
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

        CreditsController creditsController = creditsCanvas.GetComponent<CreditsController>();
        if (creditsController != null)
        {
            creditsController.xrInteractable = xrInteractable;
            xrInteractable.activated.AddListener(creditsController.OnXRActivated);
            
            Debug.Log("Connected XR Interactable to Credits Controller");
        }

        Debug.Log("Credits XR Migration completed successfully!");
        Destroy(gameObject);
    }
}
