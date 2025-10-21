using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIButtonHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private HUDHandler hudHandler;
    [SerializeField] private string hoverMessage = "Presiona el gatillo";

    [Header("One-Time Display")]
    [SerializeField] private bool onlyShowOnce = true;

    private static HashSet<GameObject> shownButtons = new HashSet<GameObject>();

    private void Awake()
    {
        if (hudHandler == null)
        {
            hudHandler = FindFirstObjectByType<HUDHandler>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer entered UI: " + gameObject.name);

        if (hudHandler == null) return;

        if (onlyShowOnce && shownButtons.Contains(gameObject))
        {
            Debug.Log("HUD already shown for button " + gameObject.name + ", skipping");
            return;
        }

        hudHandler.ShowText(hoverMessage);

        if (onlyShowOnce)
        {
            shownButtons.Add(gameObject);
            Debug.Log("HUD shown for button " + gameObject.name + ", marked as shown");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer exited UI: " + gameObject.name);

        if (hudHandler != null)
        {
            hudHandler.HideText();
        }
    }

    public static void ResetAllShownButtons()
    {
        shownButtons.Clear();
        Debug.Log("Cleared all shown buttons");
    }

    public static void ResetButton(GameObject button)
    {
        if (shownButtons.Remove(button))
        {
            Debug.Log("Reset shown status for button " + button.name);
        }
    }
}
