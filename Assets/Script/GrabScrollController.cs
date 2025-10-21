using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GrabScrollController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private TextScroller textScroller;

    [Header("Settings")]
    [SerializeField] private bool hideOnRelease = true;
    [SerializeField] private bool resetScrollOnRelease = true;

    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable == null)
        {
            Debug.LogError("XRGrabInteractable component not found on " + gameObject.name);
            enabled = false;
            return;
        }

        if (targetCanvas == null)
        {
            targetCanvas = GetComponentInChildren<Canvas>(true);
            if (targetCanvas == null)
            {
                Debug.LogError("Canvas not found as child of " + gameObject.name);
                enabled = false;
                return;
            }
        }

        if (textScroller == null)
        {
            textScroller = GetComponentInChildren<TextScroller>(true);
            if (textScroller == null)
            {
                Debug.LogError("TextScroller not found as child of " + gameObject.name);
                enabled = false;
                return;
            }
        }

        if (targetCanvas != null)
        {
            targetCanvas.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    private void OnGrabbed(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        if (targetCanvas != null)
        {
            targetCanvas.gameObject.SetActive(true);
        }

        if (textScroller != null)
        {
            textScroller.RestartScroll();
        }
    }

    private void OnReleased(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
    {
        if (resetScrollOnRelease && textScroller != null)
        {
            textScroller.ResetScroll();
        }

        if (hideOnRelease && targetCanvas != null)
        {
            targetCanvas.gameObject.SetActive(false);
        }
    }
}
