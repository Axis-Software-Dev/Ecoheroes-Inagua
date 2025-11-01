using UnityEngine;
using TMPro;

public class ControllerHintBlinker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI targetText; // TMP text source
    [SerializeField] private SpriteRenderer lateralSprite; // "lateral" button sprite
    [SerializeField] private SpriteRenderer gatilloSprite; // "gatillo" button sprite

    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 2f; // Blink frequency (Hz)

    private bool blinkLateral;
    private bool blinkGatillo;

    private void Reset()
    {
        // Auto-assign TMP and sprites if possible
        targetText = GetComponentInChildren<TextMeshProUGUI>();
        var sprites = GetComponentsInChildren<SpriteRenderer>();
        if (sprites.Length >= 2)
        {
            lateralSprite = sprites[0];
            gatilloSprite = sprites[1];
        }
    }

    private void Update()
    {
        if (targetText == null || lateralSprite == null || gatilloSprite == null)
            return;

        string textValue = targetText.text.ToLower();

        // Detect if words appear in text
        bool hasLateral = textValue.Contains("lateral");
        bool hasGatillo = textValue.Contains("gatillo");

        // Update blink state only if something changed
        if (hasLateral != blinkLateral || hasGatillo != blinkGatillo)
        {
            blinkLateral = hasLateral;
            blinkGatillo = hasGatillo;

            // Enable/disable sprites based on detected words
            lateralSprite.enabled = blinkLateral;
            gatilloSprite.enabled = blinkGatillo;

            // Reset alpha to visible
            if (blinkLateral) SetAlpha(lateralSprite, 1f);
            if (blinkGatillo) SetAlpha(gatilloSprite, 1f);
        }

        // Blink any active sprites
        float blink = (Mathf.Sin(Time.time * blinkSpeed) + 1f) * 0.5f; // 0→1→0 alpha
        if (blinkLateral)
            SetAlpha(lateralSprite, blink);
        if (blinkGatillo)
            SetAlpha(gatilloSprite, blink);
    }

    private void SetAlpha(SpriteRenderer sprite, float a)
    {
        if (sprite == null) return;
        Color c = sprite.color;
        c.a = a;
        sprite.color = c;
    }
}
