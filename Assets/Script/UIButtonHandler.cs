using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private HUDHandler hudHandler;
    [SerializeField] private string hoverMessage = "Presiona el gatillo";

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

        if (hudHandler != null)
        {
            hudHandler.ShowText(hoverMessage);
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
}
