using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonTransition : MonoBehaviour
{
    [Header("Buttons to Move")]
    public RectTransform[] buttons; 

    [Header("Movement Settings")]
    public float[] moveDistance ; 
    public float moveDuration = 0.5f; 
    public float delayBetween = 0.2f; 

    private void Start()
    {
        StartCoroutine(MoveButtonsRightToLeft());
    }

    private IEnumerator MoveButtonsRightToLeft()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            StartCoroutine(MoveButton(buttons[i], moveDistance[i], moveDuration));
            yield return new WaitForSeconds(delayBetween);
        }
    }

    private IEnumerator MoveButton(RectTransform button, float distance, float duration)
    {
        Vector3 startPos = button.anchoredPosition;
        Vector3 endPos = startPos + Vector3.left * distance;
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