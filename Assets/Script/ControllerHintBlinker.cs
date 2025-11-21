using UnityEngine;
using TMPro;

public class ControllerHintBlinker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] 
    private TextMeshProUGUI targetText;
    [SerializeField] 
    private SpriteRenderer lateralSprite;
    [SerializeField] 
    private SpriteRenderer gatilloSprite;

    [Header("Blink Settings")]
    [SerializeField] 
    private float blinkSpeed = 2f;

    private bool blinkLateral;
    private bool blinkGatillo;

    private const string LATERAL_KEYWORD = "lateral";
    private const string GATILLO_KEYWORD = "gatillo";
    private const float MIN_ALPHA = 0f;
    private const float MAX_ALPHA = 1f;
    private const float BLINK_OFFSET = 1f;
    private const float BLINK_MULTIPLIER = 0.5f;

    private void Reset()
    {
        targetText = GetComponentInChildren<TextMeshProUGUI>();
        
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        if (sprites.Length >= 2)
        {
            lateralSprite = sprites[0];
            gatilloSprite = sprites[1];
        }
    }

    private void Update()
    {
        if (targetText == null || lateralSprite == null || gatilloSprite == null)
        {
            return;
        }

        string textValue = targetText.text.ToLower();
        bool hasLateral = textValue.Contains(LATERAL_KEYWORD);
        bool hasGatillo = textValue.Contains(GATILLO_KEYWORD);

        if (hasLateral != blinkLateral || hasGatillo != blinkGatillo)
        {
            blinkLateral = hasLateral;
            blinkGatillo = hasGatillo;

            lateralSprite.enabled = blinkLateral;
            gatilloSprite.enabled = blinkGatillo;

            if (blinkLateral)
            {
                SetAlpha(lateralSprite, MAX_ALPHA);
            }
            
            if (blinkGatillo)
            {
                SetAlpha(gatilloSprite, MAX_ALPHA);
            }
        }

        float blink = (Mathf.Sin(Time.time * blinkSpeed) + BLINK_OFFSET) * BLINK_MULTIPLIER;
        
        if (blinkLateral)
        {
            SetAlpha(lateralSprite, blink);
        }
        
        if (blinkGatillo)
        {
            SetAlpha(gatilloSprite, blink);
        }
    }

    private void SetAlpha(SpriteRenderer sprite, float alpha)
    {
        if (sprite == null) return;

        Color c = sprite.color;
        c.a = Mathf.Clamp01(alpha);
        sprite.color = c;
    }
}
