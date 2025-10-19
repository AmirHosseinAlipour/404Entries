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
        _fillImage = gameObject.GetComponent<Image>();
    }

    public void StartCountdown()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateCircleFill());
    }

    private IEnumerator AnimateCircleFill()
    {
        float elapsedTime = 0f;
        _fillImage.fillAmount = 1f; 

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            _fillImage.fillAmount = 1f - (elapsedTime / animationDuration); 
            yield return null;
        }

        _fillImage.fillAmount = 0f;
    }
}