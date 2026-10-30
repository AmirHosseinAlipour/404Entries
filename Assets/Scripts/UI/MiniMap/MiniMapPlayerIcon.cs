using UnityEngine;

public class MinimapPlayerIcon : MonoBehaviour
{
    public float rotationSpeed = 10f;
    public float iconScale = 2.8f;

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

        ApplyIconScale();
    }

    private void ApplyIconScale()
    {
        Transform circle = transform.Find("Circle");
        if (circle == null) circle = transform.Find("Icon");
        if (circle != null)
        {
            circle.localScale = new Vector3(iconScale, iconScale, iconScale);
        }
        Transform light = transform.Find("Light");
        if (light != null)
        {
            light.localPosition = new Vector3(0f, 0.45f * iconScale, 0f);
            light.localScale = new Vector3(0.45f * iconScale, 0.45f * iconScale, 0.45f * iconScale);
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