using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFaderEnd : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("ScreenFader script needs a CanvasGroup component to work!");
        }
    }

    public void StartFade(float targetAlpha, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(Fade(targetAlpha, duration));
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            canvasGroup.alpha = newAlpha;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        canvasGroup.interactable = (targetAlpha == 1);
        canvasGroup.blocksRaycasts = (targetAlpha == 1);
    }

    public void FadeOut(float duration)
    {
        StartFade(1f, duration);
    }

    public void FadeIn(float duration)
    {
        StartFade(0f, duration);
    }
}