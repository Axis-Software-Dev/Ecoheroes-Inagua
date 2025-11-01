using UnityEngine;
using TMPro;

public class TextScroller : MonoBehaviour
{
    public enum ScrollMode
    {
        Scrolling,
        Fixed
    }

    [Header("Scroll Settings")]
    [SerializeField] private ScrollMode scrollMode = ScrollMode.Scrolling;
    [SerializeField] private float scrollSpeed = 50f;
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool loop = false;

    private RectTransform textRectTransform;
    private RectTransform canvasRectTransform;
    private TextMeshProUGUI textComponent;
    private float startY;
    private float textHeight;
    private bool isScrolling;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        textRectTransform = GetComponent<RectTransform>();

        if (transform.parent != null)
        {
            canvasRectTransform = transform.parent.GetComponent<RectTransform>();
        }

        if (textComponent == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on " + gameObject.name);
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        CalculateTextHeight();
        startY = textRectTransform.anchoredPosition.y;

        if (scrollMode == ScrollMode.Fixed)
        {
            StopScrolling();
            textRectTransform.anchoredPosition = new Vector2(0, startY);
        }
        else if (autoStart)
        {
            StartScrolling();
        }
    }

    private void Update()
    {
        if (scrollMode == ScrollMode.Fixed || !isScrolling) return;

        Vector2 currentPos = textRectTransform.anchoredPosition;
        currentPos.y += scrollSpeed * Time.deltaTime;
        textRectTransform.anchoredPosition = currentPos;

        float canvasHeight = canvasRectTransform != null ? canvasRectTransform.sizeDelta.y : 0;
        float endY = canvasHeight / 2f + textHeight;

        if (currentPos.y >= endY)
        {
            if (loop)
            {
                ResetScroll();
            }
            else
            {
                StopScrolling();
            }
        }
    }

    private void CalculateTextHeight()
    {
        textComponent.ForceMeshUpdate();
        textHeight = textComponent.preferredHeight;
    }

    public void StartScrolling()
    {
        if (scrollMode == ScrollMode.Scrolling)
            isScrolling = true;
    }

    public void StopScrolling()
    {
        isScrolling = false;
    }

    public void ResetScroll()
    {
        if (canvasRectTransform != null)
        {
            float canvasHeight = canvasRectTransform.sizeDelta.y;
            textRectTransform.anchoredPosition = new Vector2(0, -(canvasHeight / 2f + textHeight / 2f));
        }
        else
        {
            textRectTransform.anchoredPosition = new Vector2(0, startY);
        }
    }

    public void RestartScroll()
    {
        ResetScroll();
        StartScrolling();
    }

    public void SetScrollMode(ScrollMode mode)
    {
        scrollMode = mode;

        if (scrollMode == ScrollMode.Fixed)
        {
            StopScrolling();
            textRectTransform.anchoredPosition = new Vector2(0, startY);
        }
        else if (autoStart)
        {
            RestartScroll();
        }
    }

    public ScrollMode GetScrollMode() => scrollMode;
}
