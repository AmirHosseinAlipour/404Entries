using UnityEngine;

// This attribute lets the script run in the editor
[ExecuteInEditMode]
public class SetTransformByPercentage : MonoBehaviour
{
    private RectTransform panelRectTransform;
    private RectTransform parentRectTransform;

    [Header("Position")]
    [Tooltip("The X position as a percentage of the parent's width (0 to 100).")]
    [Range(0f, 100f)]
    public float X_Percent = 50f;

    [Tooltip("The Y position as a percentage of the parent's height (0 to 100).")]
    [Range(0f, 100f)]
    public float Y_Percent = 50f;

    [Header("Size & Scale")]
    [Tooltip("The new size as a percentage of the parent's *smaller* dimension (0 to 100).")]
    [Range(0f, 100f)]
    public float Size_Percent = 10f;

    [Tooltip("The design-time size of this panel (in pixels) that equals a scale of 1. All children should be designed relative to this size.")]
    public float Base_Size = 100f; // IMPORTANT: Set this in the Inspector!

    void Start()
    {
        Initialize();
        UpdateTransform();
    }
    
    void Initialize()
    {
        panelRectTransform = GetComponent<RectTransform>();
        if (panelRectTransform.parent != null)
        {
            parentRectTransform = panelRectTransform.parent.GetComponent<RectTransform>();
        }
    }

    // Call this function whenever you want to update the position and size
    public void UpdateTransform()
    {
        if (parentRectTransform == null)
        {
            Initialize();
            if (parentRectTransform == null)
            {
                Debug.LogError("This component needs to be a child of a RectTransform to work.");
                return;
            }
        }

        // --- 1. Set Position (Your Anchor-Moving Method) ---
        float xNormalized = X_Percent / 100f;
        float yNormalized = Y_Percent / 100f;

        // Set both anchors to the same percentage point
        panelRectTransform.anchorMin = new Vector2(xNormalized, yNormalized);
        panelRectTransform.anchorMax = new Vector2(xNormalized, yNormalized);

        // Set the position to (0,0) relative to the new anchor point
        panelRectTransform.anchoredPosition = Vector2.zero;

        // --- 2. Set Scale (to resize children) ---
        
        // Get Parent Dimensions
        float parentWidth = parentRectTransform.rect.width;
        float parentHeight = parentRectTransform.rect.height;
        float minParentDimension = Mathf.Min(parentWidth, parentHeight);
        
        // Calculate the target size in pixels
        float targetSize = (Size_Percent / 100f) * minParentDimension;
        
        // Failsafe to prevent divide-by-zero
        if (Base_Size <= 0)
        {
            if (Application.isPlaying) // Only show error when playing
            {
                Debug.LogError("Base_Size is set to 0. Cannot calculate scale. Please set a Base_Size > 0 in the Inspector.");
            }
            Base_Size = 100f; // Use a temporary failsafe
        }
        
        // Calculate the scale factor
        // (e.g., targetSize = 50, Base_Size = 100 -> scale = 0.5)
        float scaleFactor = targetSize / Base_Size;

        // Set the panel's local scale. 
        // This scales the panel AND all its children proportionally.
        panelRectTransform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
    }

    // This is called in the Editor whenever you change a value
    private void OnValidate()
    {
        if (panelRectTransform == null || parentRectTransform == null)
        {
            Initialize();
        }
        
        // Only update if we have the necessary components
        if (parentRectTransform != null)
        {
            UpdateTransform();
        }
    }
}