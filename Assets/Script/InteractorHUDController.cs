using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections.Generic;

public class InteractorHUDController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HUDHandler hudHandler;

    [Header("HUD Messages")]
    [SerializeField] private string grabMessage = "Presiona el botón lateral para tomar";
    [SerializeField] private string interactMessage = "Presiona el gatillo";

    [Header("One-Time Display")]
    [SerializeField] private bool onlyShowOnce = true;

    private NearFarInteractor nearFarInteractor;
    private IXRHoverInteractable currentHoverTarget;
    private HashSet<GameObject> shownObjects = new HashSet<GameObject>();

    private void Awake()
    {
        nearFarInteractor = GetComponent<NearFarInteractor>();
        Debug.Log("nearFarInteractor found: " + nearFarInteractor.gameObject);

        if (hudHandler == null)
        {
            hudHandler = FindFirstObjectByType<HUDHandler>();
        }
    }

    private void OnEnable()
    {
        if (nearFarInteractor != null)
        {
            nearFarInteractor.hoverEntered.AddListener(OnHoverEntered);
            nearFarInteractor.hoverExited.AddListener(OnHoverExited);

            Debug.Log("Listeners added");
        }
    }

    private void OnDisable()
    {
        if (nearFarInteractor != null)
        {
            nearFarInteractor.hoverEntered.RemoveListener(OnHoverEntered);
            nearFarInteractor.hoverExited.RemoveListener(OnHoverExited);
        }
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        Debug.Log("Hover entered");

        if (hudHandler == null) return;

        currentHoverTarget = args.interactableObject;
        GameObject targetObject = (currentHoverTarget as MonoBehaviour)?.gameObject;

        if (targetObject == null) return;

        if (onlyShowOnce && shownObjects.Contains(targetObject))
        {
            Debug.Log("HUD already shown for " + targetObject.name + ", skipping");
            return;
        }

        string message = DetermineHUDMessage(currentHoverTarget);

        if (!string.IsNullOrEmpty(message))
        {
            hudHandler.ShowText(message);

            if (onlyShowOnce)
            {
                shownObjects.Add(targetObject);
                Debug.Log("HUD shown for " + targetObject.name + ", marked as shown");
            }
        }
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        Debug.Log("Hover exited");

        if (hudHandler == null) return;

        currentHoverTarget = null;
        hudHandler.HideText();
    }

    private string DetermineHUDMessage(IXRHoverInteractable interactable)
    {
        if (interactable == null)
        {
            Debug.LogWarning("Interactable is empty, cannot determine HUD message");
            return string.Empty;
        }

        GameObject targetObject = (interactable as MonoBehaviour)?.gameObject;
        if (targetObject == null) return string.Empty;

        if (targetObject.GetComponent<XRGrabInteractable>() != null)
        {
            return grabMessage;
        }
        else if (targetObject.GetComponent<XRSimpleInteractable>() != null)
        {
            return interactMessage;
        }

        return string.Empty;
    }

    public void ResetShownObjects()
    {
        shownObjects.Clear();
        Debug.Log("Cleared all shown objects");
    }

    public void ResetObject(GameObject obj)
    {
        if (shownObjects.Remove(obj))
        {
            Debug.Log("Reset shown status for " + obj.name);
        }
    }
}
