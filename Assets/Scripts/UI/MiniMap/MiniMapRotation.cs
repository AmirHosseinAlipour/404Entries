using System.Collections;
using UnityEngine;

public class MinimapRotation : MonoBehaviour
{
    [Header("Settings")]
    public float transitionDuration = 0.25f;

    private PlayerController _playerController;
    private RectTransform _mapRectTransform;
    private Coroutine _rotationCoroutine;
    private float _currentTargetAngle;

    void Start()
    {
        _playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        _mapRectTransform = GetComponent<RectTransform>();
        
        _currentTargetAngle = -_playerController.FacingAngle;
        _mapRectTransform.rotation = Quaternion.Euler(0, 0, _currentTargetAngle);
    }

    void LateUpdate()
    {
        if (_playerController == null) return;

        float newTargetAngle = _playerController.FacingAngle;

        if (!Mathf.Approximately(newTargetAngle, _currentTargetAngle))
        {
            if (_rotationCoroutine != null)
            {
                StopCoroutine(_rotationCoroutine);
            }
            
            _rotationCoroutine = StartCoroutine(RotateMap(newTargetAngle));
            
            _currentTargetAngle = newTargetAngle;
        }
    }

    private IEnumerator RotateMap(float targetAngle)
    {
        float timeElapsed = 0f;
        
        float startAngle = _mapRectTransform.eulerAngles.z;

        while (timeElapsed < transitionDuration)
        {
            timeElapsed += Time.deltaTime;

            float t = timeElapsed / transitionDuration;

            float smoothedAngle = Mathf.LerpAngle(startAngle, targetAngle, t);

            _mapRectTransform.rotation = Quaternion.Euler(0, 0, smoothedAngle);

            yield return null;
        }

        _mapRectTransform.rotation = Quaternion.Euler(0, 0, targetAngle);
    }
}