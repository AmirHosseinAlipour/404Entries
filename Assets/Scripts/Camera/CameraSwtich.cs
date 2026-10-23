using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public CinemachineCamera roomCamera;
    public GameObject voidParent;
    private SpriteRenderer[] voidBackground;
    public float fadeDuration;
    public bool isEnteringRoom;
    private Coroutine currentFadeCoroutine;

    private void Start()
    {
        if (voidParent != null)
        {
            voidBackground = voidParent.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sp in voidBackground)
            {
                if (sp != null)
                {
                    Color c = sp.color;
                    c.a = 0;
                    sp.color = c;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
            }

            if (isEnteringRoom)
            {
                currentFadeCoroutine = StartCoroutine(FadeIn());
            }
            else
            {
                currentFadeCoroutine = StartCoroutine(FadeOut());
            }
            
            if (roomCamera != null)
            {
                roomCamera.Priority = isEnteringRoom ? 19 : 0;
            }
            
            if (CameraMaskSwitcher.Instance != null)
            {
                CameraMaskSwitcher.Instance.roomVcam = roomCamera;
            }
        }
    }

    private IEnumerator FadeIn()
    {
        if (voidBackground == null || voidBackground.Length == 0) yield break;

        float elapsedTime = 0f;
        float startAlpha = (voidBackground[0] != null) ? voidBackground[0].color.a : 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / fadeDuration);
            foreach (SpriteRenderer sp in voidBackground)
            {
                if (sp != null)
                {
                    Color c = sp.color;
                    c.a = alpha;
                    sp.color = c;
                }
            }
            yield return null;
        }

        foreach (SpriteRenderer sp in voidBackground)
        {
            if (sp != null)
            {
                Color c = sp.color;
                c.a = 1f;
                sp.color = c;
            }
        }
    }

    private IEnumerator FadeOut()
    {
        if (voidBackground == null || voidBackground.Length == 0) yield break;

        float elapsedTime = 0f;
        float startAlpha = (voidBackground[0] != null) ? voidBackground[0].color.a : 1f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
            foreach (SpriteRenderer sp in voidBackground)
            {
                if (sp != null)
                {
                    Color c = sp.color;
                    c.a = alpha;
                    sp.color = c;
                }
            }
            yield return null;
        }

        foreach (SpriteRenderer sp in voidBackground)
        {
            if (sp != null)
            {
                Color c = sp.color;
                c.a = 0f;
                sp.color = c;
            }
        }
    }
}