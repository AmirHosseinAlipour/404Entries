using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class DynamicFontResizer : MonoBehaviour
{
    [Tooltip("The smallest font size the text can be.")]
    public int minFontSize = 10;
    
    [Tooltip("The largest font size the text can be.")]
    public int maxFontSize = 40;

    private Text textComponent;
    private RectTransform rectTransform;

    void Awake()
    {
        textComponent = GetComponent<Text>();
        rectTransform = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        // Adjust font size when the component is enabled
        AdjustFontSize();
    }

    void OnRectTransformDimensionsChange()
    {
        // Adjust font size if the container's size changes
        AdjustFontSize();
    }

    // ---
    // You MUST call this function manually from your other scripts
    // whenever you change the text content.
    //
    // Example:
    // myTextComponent.text = "New dynamic text";
    // myTextComponent.GetComponent<DynamicFontResizer>().AdjustFontSize();
    // ---
    public void AdjustFontSize()
    {
        if (textComponent == null) return;

        // CRITICAL: Must be set to Overflow for preferredWidth to be accurate.
        if (textComponent.horizontalOverflow != HorizontalWrapMode.Overflow)
        {
            textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        float containerWidth = rectTransform.rect.width;

        // Start from the max size and work down
        for (int i = maxFontSize; i >= minFontSize; i--)
        {
            // Set the font size
            textComponent.fontSize = i;

            // Force the canvas to update to get the correct preferredWidth
            Canvas.ForceUpdateCanvases();

            // Check if the text now fits
            if (textComponent.preferredWidth <= containerWidth)
            {
                // It fits, so we are done
                return;
            }
        }
        
        // If the loop completes, it means even at minFontSize,
        // it doesn't fit. It will just stay at minFontSize.
    }
}