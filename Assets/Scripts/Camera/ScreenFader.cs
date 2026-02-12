using UnityEngine;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [SerializeField] private SpriteRenderer fadeSprite;
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine fadeCoroutine;

    // Track current fade state: 0 = fully transparent, 1 = fully black
    private float targetAlpha = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure initial state
        Color c = fadeSprite.color;
        c.a = 0f;
        fadeSprite.color = c;
    }

    public void FadeIn()
    {
        StartFade(0f); // transparent
    }

    public void FadeOut()
    {
        StartFade(1f); // black
    }

    private void StartFade(float toAlpha)
    {
        // Ignore if already fading to this alpha
        if (Mathf.Approximately(toAlpha, targetAlpha))
            return;

        targetAlpha = toAlpha;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeRoutine(toAlpha));
    }

    private IEnumerator FadeRoutine(float toAlpha)
    {
        float fromAlpha = fadeSprite.color.a;
        float time = 0f;

        while (!Mathf.Approximately(fadeSprite.color.a, toAlpha))
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);
            Color c = fadeSprite.color;
            c.a = Mathf.Lerp(fromAlpha, toAlpha, t);
            fadeSprite.color = c;
            yield return null;
        }

        Color final = fadeSprite.color;
        final.a = toAlpha;
        fadeSprite.color = final;
    }
}