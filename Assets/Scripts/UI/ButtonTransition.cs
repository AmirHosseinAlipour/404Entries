using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonTransition : MonoBehaviour
{
    [Header("Buttons to Move")]
    public RectTransform[] buttons;

    [Header("Canvas Reference")]
    [Tooltip("Drag your main Canvas object here. We need its width.")]
    public RectTransform canvasRect; 

    [Header("Movement Settings")]
    [Tooltip("Movement distance as a multiplier of screen width (e.g., 1.0 = 100%)")]
    public float[] moveDistanceMultiplier;
    public float initialSpace;
    public float moveDuration = 0.5f;
    public float delayBetween = 0.2f;

    private float screenWidth;

    private void Start()
    {
        if (canvasRect == null)
        {
            Debug.LogError("ButtonTransition: Canvas RectTransform is not assigned! Cannot calculate responsive movement.");
            return;
        }

        foreach (var b in buttons)
        {
            b.anchoredPosition += new Vector2(initialSpace, 0f);
        }

        screenWidth = canvasRect.rect.width;

        StartCoroutine(MoveButtonsRightToLeft());
    }

    private IEnumerator MoveButtonsRightToLeft()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i < moveDistanceMultiplier.Length)
            {
                float pixelDistance = moveDistanceMultiplier[i] * screenWidth + initialSpace;
                StartCoroutine(MoveButton(buttons[i], pixelDistance, moveDuration));
            }
            else
            {
                Debug.LogWarning($"ButtonTransition: Not enough move distances defined for button {buttons[i].name}");
            }
            
            yield return new WaitForSeconds(delayBetween);
        }
    }

    private IEnumerator MoveButton(RectTransform button, float distanceInPixels, float duration)
    {
        Vector3 startPos = button.anchoredPosition;
        Vector3 endPos = startPos + Vector3.left * distanceInPixels;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            button.anchoredPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        button.anchoredPosition = endPos;
    }
}