using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 1. Add a requirement for a Selectable component (like a Button)
[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(Selectable))]
public class OutlineHoverEffect : MonoBehaviour, 
    IPointerEnterHandler, 
    IPointerExitHandler, 
    ISelectHandler,     // 2. Add ISelectHandler
    IDeselectHandler    // 3. Add IDeselectHandler
{
    private Outline buttonOutline;
    private Color originalColor;

    // 4. Use this flag to track the "selected" state
    private bool isSelected = false;

    void Awake()
    {
        buttonOutline = GetComponent<Outline>();
        if (buttonOutline != null)
        {
            // This stores your "deselected" color (which can have 0 alpha)
            originalColor = buttonOutline.effectColor;
        }
    }

    void OnEnable()
    {
        // Reset state when enabled
        isSelected = false;
        if (buttonOutline != null)
        {
            buttonOutline.effectColor = originalColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Always show white on hover
        if (buttonOutline != null)
        {
            buttonOutline.effectColor = Color.white;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // If we move the mouse off, *and we are not selected*, reset the color.
        if (buttonOutline != null && !isSelected)
        {
            buttonOutline.effectColor = originalColor;
        }
    }

    // 5. This is called when the button is CLICKED
    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        if (buttonOutline != null)
        {
            buttonOutline.effectColor = Color.white;
        }
    }

    // 6. This is called on the OLD button when a NEW button is clicked
    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        if (buttonOutline != null)
        {
            // This resets the color to its original state (e.g., 0 alpha)
            buttonOutline.effectColor = originalColor;
        }
    }
}