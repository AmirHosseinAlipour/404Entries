using UnityEngine;

public class PongManager : MonoBehaviour
{
    [Header("Fade Settings")]
    public GameObject targetObject;
    public float fadeSpeed = 2f;

    private Renderer[] renderers;
    private bool isFadingIn = false;
    private bool isFadingOut = false;

    void Awake()
    {
        if (targetObject != null)
        {
            // همه‌ی Rendererهای فرزند رو می‌گیریم (برای Mesh یا Sprite)
            renderers = targetObject.GetComponentsInChildren<Renderer>();
            targetObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isFadingIn)
        {
            bool done = FadeTo(1f);
            if (done)
                isFadingIn = false;
        }
        else if (isFadingOut)
        {
            bool done = FadeTo(0f);
            if (done)
            {
                isFadingOut = false;
                targetObject.SetActive(false);
            }
        }
    }

    public void FadeInAndActivate()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(true);
            SetAlpha(0f);
            isFadingIn = true;
        }
    }

    public void FadeOutAndDeactivate()
    {
        if (targetObject != null)
        {
            isFadingOut = true;
        }
    }

    private bool FadeTo(float targetAlpha)
    {
        bool allDone = true;
        foreach (var rend in renderers)
        {
            Color c = rend.material.color;
            float newAlpha = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
            if (!Mathf.Approximately(newAlpha, targetAlpha))
                allDone = false;
            c.a = newAlpha;
            rend.material.color = c;
        }
        return allDone;
    }

    private void SetAlpha(float alpha)
    {
        foreach (var rend in renderers)
        {
            Color c = rend.material.color;
            c.a = alpha;
            rend.material.color = c;
        }
    }
}