using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIButtonHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] 
    private HUDHandler hudHandler;
    [SerializeField] 
    private string hoverMessage = "Presiona el gatillo";

    [Header("One-Time Display")]
    [SerializeField] 
    private bool onlyShowOnce = true;

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
        if (hudHandler == null) return;

        if (onlyShowOnce && shownButtons.Contains(gameObject))
        {
            return;
        }

        hudHandler.ShowText(hoverMessage);

        if (onlyShowOnce)
        {
            shownButtons.Add(gameObject);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hudHandler != null)
        {
            hudHandler.HideText();
        }
    }

    public static void ResetAllShownButtons()
    {
        shownButtons.Clear();
    }

    public static void ResetButton(GameObject button)
    {
        if (button != null)
        {
            shownButtons.Remove(button);
        }
    }
}
