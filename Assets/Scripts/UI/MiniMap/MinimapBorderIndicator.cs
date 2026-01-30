using System;
using UnityEngine;
using UnityEngine.UI;

public class MinimapBorderIndicator : MonoBehaviour
{
    public Transform targetWorldObject;
    public Transform miniMapCenter;
    public float miniMapRadius;
    public float minimapScaleFactor = 10f;
    
    public Camera minimapCamera;
    public RectTransform minimapBackground; 

    private float _minimapScaleFactor;
    private float _minimapRadius;

    private RectTransform _rectTransform;
    private Image _image;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
        
        _minimapRadius = Mathf.Min(minimapBackground.rect.width, minimapBackground.rect.height) / 2f;

        float uiWidth = minimapBackground.rect.width;
        float worldWidth = minimapCamera.orthographicSize * 2f * minimapCamera.aspect;
        _minimapScaleFactor = uiWidth / worldWidth;
    }

    private void LateUpdate()
    {
        Vector3 centerWorldPos = miniMapCenter.position;
        Vector3 targetWorldPos = targetWorldObject.position;
        
        Vector2 worldDifference = new Vector2(
            targetWorldPos.x - centerWorldPos.x,
            targetWorldPos.y - centerWorldPos.y
        );
        
        Vector2 minimapLocalPos = worldDifference * _minimapScaleFactor;

        if (minimapLocalPos.sqrMagnitude > _minimapRadius * _minimapRadius)
        {
            Vector2 direction = minimapLocalPos.normalized;
            _rectTransform.anchoredPosition = direction * _minimapRadius;
            if (_image != null) _image.enabled = true;
        }
        else
        {
            _rectTransform.anchoredPosition = minimapLocalPos;
            if (_image != null) _image.enabled = true; 
        }
    }
}