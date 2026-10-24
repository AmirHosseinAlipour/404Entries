using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CircleTimerAnimation : MonoBehaviour
{
    [HideInInspector] public float animationDuration;
    private Image _fillImage;

    private void Awake()
    {
        if (_fillImage == null)
        {
            _fillImage = GetComponent<Image>();
        }
    }

    public void StartCountdown()
    {
        StopAllCoroutines();
        if (_fillImage == null)
        {
            _fillImage = GetComponent<Image>();
        }
        StartCoroutine(AnimateCircleFill());
    }
    
    public void StopCountdown()
    {
        StopAllCoroutines();
        if (_fillImage == null)
        {
            _fillImage = GetComponent<Image>();
        }
        if (_fillImage != null)
        {
            _fillImage.fillAmount = 0f; 
        }
    }

    private IEnumerator AnimateCircleFill()
    {
        if (_fillImage == null)
        {
            _fillImage = GetComponent<Image>();
        }
        if (_fillImage == null) yield break;

        float dur = animationDuration > 0.05f ? animationDuration : 1f;
        float elapsedTime = 0f;
        _fillImage.fillAmount = 1f; 

        while (elapsedTime < dur)
        {
            elapsedTime += Time.deltaTime;
            _fillImage.fillAmount = 1f - (elapsedTime / dur); 
            yield return null;
        }

        _fillImage.fillAmount = 0f;
    }
}