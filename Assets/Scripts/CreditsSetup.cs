using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

public class CreditsSetup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform isoATransform;
    [SerializeField] private Transform terrainTransform;
    
    [Header("Positioning")]
    [SerializeField] private float buttonOffsetBelowIsoA = 0.5f;
    [SerializeField] private float creditsBoxHeight = 2f;
    [SerializeField] private float creditsBoxWidth = 2f;

    [Header("Credits Content")]
    [TextArea(10, 30)]
    [SerializeField] private string creditsContent = "ECOHÉROES\n\n\nDesarrollado por:\nTu Equipo\n\n\nDiseño:\nEquipo de Diseño\n\n\nProgramación:\nEquipo de Programación\n\n\nArte:\nEquipo de Arte\n\n\nMúsica y Sonido:\nEquipo de Audio\n\n\nAgradecimientos especiales:\nA todos los que hicieron posible este proyecto\n\n\nGracias por jugar!";

    private GameObject canvasObject;
    private GameObject buttonObject;
    private GameObject creditsObject;
    private CreditsController creditsController;

    private void Start()
    {
        if (FindObjectOfType<CreditsController>() == null)
        {
            CreateCreditsUI();
        }
    }

    public void CreateCreditsUI()
    {
        if (isoATransform == null)
        {
            isoATransform = GameObject.Find("Iso A")?.transform;
        }

        if (terrainTransform == null)
        {
            terrainTransform = GameObject.Find("Terrain")?.transform;
        }

        if (isoATransform == null)
        {
            Debug.LogError("Iso A object not found!");
            return;
        }

        CreateCanvas();
        CreateButton();
        CreateCreditsBox();
        SetupCreditsController();
    }

    private void CreateCanvas()
    {
        canvasObject = new GameObject("Credits Canvas");
        canvasObject.transform.SetParent(isoATransform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        canvasObject.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(creditsBoxWidth * 100f, creditsBoxHeight * 100f);
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        
        Vector3 buttonPosition = isoATransform.position;
        buttonPosition.y -= buttonOffsetBelowIsoA;
        canvasObject.transform.position = buttonPosition;
        canvasObject.transform.rotation = Quaternion.Euler(0, isoATransform.rotation.eulerAngles.y, 0);
    }

    private void CreateButton()
    {
        buttonObject = new GameObject("Credits Button");
        buttonObject.transform.SetParent(canvasObject.transform, false);

        RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(200f, 60f);
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = Vector2.zero;

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.9f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        
        ColorBlock colorBlock = button.colors;
        colorBlock.normalColor = new Color(0.2f, 0.6f, 0.9f, 1f);
        colorBlock.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f);
        colorBlock.pressedColor = new Color(0.15f, 0.5f, 0.8f, 1f);
        button.colors = colorBlock;

        CanvasGroup buttonCanvasGroup = buttonObject.AddComponent<CanvasGroup>();

        BoxCollider buttonCollider = buttonObject.AddComponent<BoxCollider>();
        buttonCollider.size = new Vector3(200f, 60f, 1f);

        XRSimpleInteractable xrInteractable = buttonObject.AddComponent<XRSimpleInteractable>();

        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = buttonTextObj.AddComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(190f, 50f);
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "CRÉDITOS";
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;
    }

    private void CreateCreditsBox()
    {
        creditsObject = new GameObject("Credits Box");
        creditsObject.transform.SetParent(canvasObject.transform, false);

        RectTransform creditsRect = creditsObject.AddComponent<RectTransform>();
        creditsRect.sizeDelta = new Vector2(creditsBoxWidth * 100f, creditsBoxHeight * 100f);
        creditsRect.anchorMin = new Vector2(0.5f, 0.5f);
        creditsRect.anchorMax = new Vector2(0.5f, 0.5f);
        creditsRect.anchoredPosition = Vector2.zero;

        Image creditsBackground = creditsObject.AddComponent<Image>();
        creditsBackground.color = new Color(0f, 0f, 0f, 0.8f);

        CanvasGroup creditsCanvasGroup = creditsObject.AddComponent<CanvasGroup>();
        creditsCanvasGroup.alpha = 0f;

        Mask mask = creditsObject.AddComponent<Mask>();
        mask.showMaskGraphic = true;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(creditsObject.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(creditsBoxWidth * 90f, 2000f);
        textRect.anchorMin = new Vector2(0.5f, 0f);
        textRect.anchorMax = new Vector2(0.5f, 0f);
        textRect.pivot = new Vector2(0.5f, 0f);
        textRect.anchoredPosition = new Vector2(0f, 0f);

        TextMeshProUGUI creditsText = textObj.AddComponent<TextMeshProUGUI>();
        creditsText.fontSize = 18;
        creditsText.color = Color.white;
        creditsText.alignment = TextAlignmentOptions.Center;
        creditsText.overflowMode = TextOverflowModes.Overflow;
        creditsText.enableWordWrapping = true;

        creditsObject.SetActive(false);
    }

    private void SetupCreditsController()
    {
        creditsController = canvasObject.AddComponent<CreditsController>();
        
        Button button = buttonObject.GetComponent<Button>();
        CanvasGroup buttonCanvasGroup = buttonObject.GetComponent<CanvasGroup>();
        TextMeshProUGUI creditsText = creditsObject.GetComponentInChildren<TextMeshProUGUI>();
        CanvasGroup creditsCanvasGroup = creditsObject.GetComponent<CanvasGroup>();
        XRSimpleInteractable xrInteractable = buttonObject.GetComponent<XRSimpleInteractable>();

        creditsController.SetReferences(button, creditsText, buttonCanvasGroup, creditsCanvasGroup, creditsContent, xrInteractable);

        Debug.Log("Credits UI created successfully below Iso A!");
    }
}
