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
    private string _lastTargetText = "";

    void Awake()
    {
        textComponent = GetComponent<Text>();
        rectTransform = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        AdjustFontSize();
    }

    void OnRectTransformDimensionsChange()
    {
        AdjustFontSize();
    }
    
    public void SetTextAndResize(string newText)
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<Text>();
            rectTransform = GetComponent<RectTransform>();
        }

        _lastTargetText = newText;
        if (textComponent != null)
        {
            textComponent.text = newText;
        }
        AdjustFontSize();
    }

    public void AdjustFontSize()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<Text>();
            rectTransform = GetComponent<RectTransform>();
        }
        if (textComponent == null || rectTransform == null) return;

        if (textComponent.horizontalOverflow != HorizontalWrapMode.Overflow)
        {
            textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        string textToMeasure = textComponent.text;
        if (string.IsNullOrEmpty(textToMeasure))
        {
            if (!string.IsNullOrEmpty(_lastTargetText))
            {
                textToMeasure = _lastTargetText;
            }
            else
            {
                var ftw = GetComponent<FarsiTypewriter>();
                if (ftw != null && !string.IsNullOrEmpty(ftw.GetFixedText()))
                {
                    textToMeasure = ftw.GetFixedText();
                }
            }
        }

        if (string.IsNullOrEmpty(textToMeasure)) return;

        string originalText = textComponent.text;
        textComponent.text = textToMeasure;

        float containerWidth = rectTransform.rect.width;
        if (containerWidth <= 0)
        {
            textComponent.text = originalText;
            return;
        }

        for (int i = maxFontSize; i >= minFontSize; i--)
        {
            textComponent.fontSize = i;
            Canvas.ForceUpdateCanvases();

            if (textComponent.preferredWidth <= containerWidth)
            {
                textComponent.text = originalText;
                return;
            }
        }
        
        textComponent.text = originalText;
    }
}