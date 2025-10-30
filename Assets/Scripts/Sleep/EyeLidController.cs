using UnityEngine;

public class EyeLidController : MonoBehaviour
{
    [Header("Lids")]
    public RectTransform topLid;
    public RectTransform bottomLid;

    [Header("Settings")]
    [Range(0f, 1f)] public float closeAmount = 0f;
    public Canvas canvas;

    private float canvasHeight;
    private float halfHeight;

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
        halfHeight = canvasHeight / 2f;
    }

    void Update()
    {
        if (topLid == null || bottomLid == null) return;
        float moveDistance = halfHeight * closeAmount;
        topLid.anchoredPosition = new Vector2(0, -moveDistance + halfHeight );
        bottomLid.anchoredPosition = new Vector2(0, moveDistance - halfHeight );
    }

    public void SetCloseAmount(float amount)
    {
        closeAmount = Mathf.Clamp01(amount);
    }
}