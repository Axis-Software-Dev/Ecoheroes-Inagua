using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections.Generic;

public class InteractorHUDController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] 
    private HUDHandler hudHandler;

    [Header("HUD Messages")]
    [SerializeField] 
    private string grabMessage = "Presiona el botón lateral para tomar";
    [SerializeField] 
    private string interactMessage = "Presiona el gatillo";

    [Header("One-Time Display")]
    [SerializeField] 
    private bool onlyShowOnce = true;

    private NearFarInteractor nearFarInteractor;
    private IXRHoverInteractable currentHoverTarget;
    private HashSet<GameObject> shownObjects = new HashSet<GameObject>();

    private void Awake()
    {
        nearFarInteractor = GetComponent<NearFarInteractor>();

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
        if (hudHandler == null) return;

        currentHoverTarget = args.interactableObject;
        GameObject targetObject = (currentHoverTarget as MonoBehaviour)?.gameObject;

        if (targetObject == null) return;

        if (onlyShowOnce && shownObjects.Contains(targetObject))
        {
            return;
        }

        string message = DetermineHUDMessage(currentHoverTarget);

        if (!string.IsNullOrEmpty(message))
        {
            hudHandler.ShowText(message);

            if (onlyShowOnce)
            {
                shownObjects.Add(targetObject);
            }
        }
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        if (hudHandler != null)
        {
            hudHandler.HideText();
        }

        currentHoverTarget = null;
    }

    private string DetermineHUDMessage(IXRHoverInteractable interactable)
    {
        if (interactable == null) return string.Empty;

        GameObject targetObject = (interactable as MonoBehaviour)?.gameObject;
        if (targetObject == null) return string.Empty;

        if (targetObject.GetComponent<XRGrabInteractable>() != null)
        {
            return grabMessage;
        }
        
        if (targetObject.GetComponent<XRSimpleInteractable>() != null)
        {
            return interactMessage;
        }

        return string.Empty;
    }

    public void ResetShownObjects()
    {
        shownObjects.Clear();
    }

    public void ResetObject(GameObject obj)
    {
        if (obj != null)
        {
            shownObjects.Remove(obj);
        }
    }
}
