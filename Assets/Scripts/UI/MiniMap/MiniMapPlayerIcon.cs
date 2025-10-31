using UnityEngine;

public class MinimapPlayerIcon : MonoBehaviour
{
    public float rotationSpeed = 10f;

    private PlayerController _playerController;
    private RectTransform _rectTransform;

    private void Start()
    {
        _playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        _rectTransform = GetComponent<RectTransform>();
        if (_playerController == null)
        {
            _playerController = FindObjectOfType<PlayerController>();
        }
    }

    private void LateUpdate()
    {
        if (_playerController == null) return;

        float playerAngle = _playerController.FacingAngle;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, playerAngle);

        _rectTransform.rotation = Quaternion.Slerp(
            _rectTransform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}