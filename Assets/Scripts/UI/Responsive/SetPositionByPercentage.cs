using UnityEngine;

[ExecuteInEditMode]
public class SetTransformByPercentage : MonoBehaviour
{
    private RectTransform panelRectTransform;
    private RectTransform parentRectTransform;

    [Header("Position")]
    [Range(0f, 100f)]
    public float X_Percent = 50f;

    [Range(0f, 100f)]
    public float Y_Percent = 50f;

    [Header("Size & Scale")]
    [Range(0f, 100f)]
    public float Size_Percent = 10f;

    public float Base_Size = 100f;

    private void OnEnable()
    {
        Initialize();
        UpdateTransform();
    }

    void Start()
    {
        Initialize();
        UpdateTransform();
    }
    
    void Initialize()
    {
        panelRectTransform = GetComponent<RectTransform>();
        if (panelRectTransform != null && panelRectTransform.parent != null)
        {
            parentRectTransform = panelRectTransform.parent.GetComponent<RectTransform>();
        }
    }

    public void UpdateTransform()
    {
        if (parentRectTransform == null)
        {
            Initialize();
            if (parentRectTransform == null)
            {
                return;
            }
        }

        if (panelRectTransform == null) return;

        if (CompareTag("JoyStick"))
        {
            panelRectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        float xNormalized = X_Percent / 100f;
        float yNormalized = Y_Percent / 100f;

        panelRectTransform.anchorMin = new Vector2(xNormalized, yNormalized);
        panelRectTransform.anchorMax = new Vector2(xNormalized, yNormalized);
        panelRectTransform.anchoredPosition = Vector2.zero;

        if (!CompareTag("JoyStick"))
        {
            float parentWidth = parentRectTransform.rect.width;
            float parentHeight = parentRectTransform.rect.height;
            float minParentDimension = Mathf.Min(parentWidth, parentHeight);
            
            float targetSize = (Size_Percent / 100f) * minParentDimension;
            
            if (Base_Size <= 0)
            {
                Base_Size = 100f;
            }
            
            float scaleFactor = targetSize / Base_Size;
            panelRectTransform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        }
    }

    private void OnValidate()
    {
        if (panelRectTransform == null || parentRectTransform == null)
        {
            Initialize();
        }
        
        if (parentRectTransform != null)
        {
            UpdateTransform();
        }
    }
}