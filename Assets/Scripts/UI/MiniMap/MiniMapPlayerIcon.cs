using UnityEngine;

public class MinimapPlayerIcon : MonoBehaviour
{
    public float rotationSpeed = 10f;

    private PlayerController _playerController;
    private Transform _transform;

    private void Start()
    {
        _playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        _transform = GetComponent<Transform>();
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

        _transform.rotation = Quaternion.Slerp(
            _transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}