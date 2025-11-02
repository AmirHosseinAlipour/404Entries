using UnityEngine;

public class EyeLidController : MonoBehaviour
{
    [Header("Lids")]
    public RectTransform topLid;
    public RectTransform bottomLid;

    [Header("Settings")]
    [Range(0f, 0.5f)] public float closeAmount = 0f;
    public Canvas canvas;

    private float canvasHeight;
    private float Height;

    void Start()
    {
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("No Canvas assigned or found!");
            enabled = false;
            return;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasHeight = canvasRect.rect.height;
        Height = canvasHeight;
    }

    void Update()
    {
        if (topLid == null || bottomLid == null) return;
        float moveDistance = Height * closeAmount;
        topLid.anchoredPosition = new Vector2(0, -moveDistance + Height);
        bottomLid.anchoredPosition = new Vector2(0, moveDistance - Height);
    }

    public void SetCloseAmount(float amount)
    {
        closeAmount = Mathf.Clamp01(amount);
    }
}