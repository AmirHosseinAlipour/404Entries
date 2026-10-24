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
        EnsureFillImage();
    }

    private void EnsureFillImage()
    {
        if (_fillImage == null)
        {
            _fillImage = GetComponent<Image>();
            if (_fillImage == null)
            {
                _fillImage = GetComponentInChildren<Image>();
            }
        }

        if (_fillImage != null)
        {
            _fillImage.type = Image.Type.Filled;
            _fillImage.fillMethod = Image.FillMethod.Radial360;
        }
    }

    public void StartCountdown()
    {
        StopAllCoroutines();
        EnsureFillImage();
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(AnimateCircleFill());
        }
    }
    
    public void StopCountdown()
    {
        StopAllCoroutines();
        EnsureFillImage();
        if (_fillImage != null)
        {
            _fillImage.fillAmount = 0f; 
        }
    }

    private IEnumerator AnimateCircleFill()
    {
        EnsureFillImage();
        if (_fillImage == null) yield break;

        float dur = animationDuration > 0.05f ? animationDuration : 1f;
        float elapsedTime = 0f;
        _fillImage.fillAmount = 1f; 

        while (elapsedTime < dur)
        {
            elapsedTime += Time.unscaledDeltaTime;
            _fillImage.fillAmount = Mathf.Clamp01(1f - (elapsedTime / dur)); 
            yield return null;
        }

        _fillImage.fillAmount = 0f;
    }
}