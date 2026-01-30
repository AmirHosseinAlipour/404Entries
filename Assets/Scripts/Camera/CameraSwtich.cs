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
        voidBackground = voidParent.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sp in voidBackground)
        {
            Color c = sp.color;
            c.a = 0;
            sp.color = c;
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
            
            roomCamera.Priority = isEnteringRoom ? 19 : 0;
            
            CameraMaskSwitcher.Instance.roomVcam = roomCamera;
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        float startAlpha = voidBackground[0].color.a;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / fadeDuration);
            foreach (SpriteRenderer sp in voidBackground)
            {
                Color c = sp.color;
                c.a = alpha;
                sp.color = c;
            }
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        float startAlpha = voidBackground[0].color.a;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
            foreach (SpriteRenderer sp in voidBackground)
            {
                Color c = sp.color;
                c.a = alpha;
                sp.color = c;
            }
            yield return null;
        }
    }
}