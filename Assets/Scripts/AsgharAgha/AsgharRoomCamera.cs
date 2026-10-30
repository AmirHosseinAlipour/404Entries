using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class AsgharRoomCamera : MonoBehaviour
{
    public static AsgharRoomCamera Instance { get; private set; }

    [Header("Player Camera Zoom Settings")]
    [Tooltip("Normal orthographic size of the player camera outside this class")]
    public float normalOrthoSize = 5f;

    [Tooltip("Zoomed-out orthographic size in Dr. Esghaei's class (allows player to see more)")]
    public float zoomedOrthoSize = 7.2f;

    [Tooltip("Smooth transition duration in seconds")]
    public float zoomDuration = 1.2f;

    // Backward-compatibility properties if referenced elsewhere
    public CinemachineCamera roomCamera => _playerCam;
    public GameObject voidParent;

    private CinemachineCamera _playerCam;
    private float _originalOrthoSize = -1f;
    private Coroutine _zoomCoroutine;
    private bool _isInRoom = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        EnsurePlayerCamera();
        DisableOldRoomCameraAndVoid();
        SetupDoorTriggers();
    }

    private void Start()
    {
        EnsurePlayerCamera();
        DisableOldRoomCameraAndVoid();
        SetupDoorTriggers();
    }

    private void OnDestroy()
    {
        if (_playerCam != null && _isInRoom && _originalOrthoSize > 0f)
        {
            _playerCam.Lens.OrthographicSize = _originalOrthoSize;
        }
    }

    private void EnsurePlayerCamera()
    {
        if (_playerCam != null) return;

        // 1. Try finding CinemachineCamera attached to Player GameObject
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            _playerCam = player.GetComponentInChildren<CinemachineCamera>(true);
        }

        // 2. Search scene for player follow camera by name
        if (_playerCam == null)
        {
            CinemachineCamera[] allCams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var cam in allCams)
            {
                if (cam != null && cam.gameObject.name.Contains("FollowPlayer"))
                {
                    _playerCam = cam;
                    break;
                }
            }
        }

        // 3. Fallback search by tracking target
        if (_playerCam == null)
        {
            CinemachineCamera[] allCams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var cam in allCams)
            {
                if (cam != null && cam.Target.TrackingTarget != null && cam.Target.TrackingTarget.CompareTag("Player"))
                {
                    _playerCam = cam;
                    break;
                }
            }
        }

        if (_playerCam != null && _originalOrthoSize <= 0f)
        {
            _originalOrthoSize = _playerCam.Lens.OrthographicSize;
            if (normalOrthoSize <= 0f)
            {
                normalOrthoSize = _originalOrthoSize;
            }
        }
    }

    private void DisableOldRoomCameraAndVoid()
    {
        // 1. Disable VCam_Room (1) so it never takes over the screen
        CinemachineCamera[] allCams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in allCams)
        {
            if (c != null && c.gameObject.name == "VCam_Room (1)")
            {
                c.Priority = 0;
                c.gameObject.SetActive(false);
            }
        }

        // 2. Disable Void (12) so black solid blocks don't appear in the classroom
        Transform[] allTransforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in allTransforms)
        {
            if (t != null && t.gameObject.name == "Void (12)")
            {
                t.gameObject.SetActive(false);
            }
        }

        // 3. Remove any temporary Void_AsgharAgha
        Transform tempVoid = transform.Find("Void_AsgharAgha");
        if (tempVoid != null)
        {
            Destroy(tempVoid.gameObject);
        }

        // 4. Ensure CameraMaskSwitcher is reset to normal mask
        if (CameraMaskSwitcher.Instance != null)
        {
            if (CameraMaskSwitcher.Instance.roomVcam != null && CameraMaskSwitcher.Instance.roomVcam.gameObject.name.Contains("(1)"))
            {
                CameraMaskSwitcher.Instance.roomVcam = null;
            }
            if (CameraMaskSwitcher.Instance.mainCamera != null && CameraMaskSwitcher.Instance.normalMask != 0)
            {
                CameraMaskSwitcher.Instance.mainCamera.cullingMask = CameraMaskSwitcher.Instance.normalMask;
            }
        }
    }

    private void SetupDoorTriggers()
    {
        GameObject enterObj = GameObject.Find("Enter_Trigger_Room (1)");
        if (enterObj != null)
        {
            var cs = enterObj.GetComponent<CameraSwitch>();
            if (cs != null) cs.enabled = false;

            var fwd = enterObj.GetComponent<AsgharDoorTriggerForwarder>();
            if (fwd == null) fwd = enterObj.AddComponent<AsgharDoorTriggerForwarder>();
            fwd.isEnter = true;
        }

        GameObject exitObj = GameObject.Find("Exit_Trigger_Room (1)");
        if (exitObj != null)
        {
            var cs = exitObj.GetComponent<CameraSwitch>();
            if (cs != null) cs.enabled = false;

            var fwd = exitObj.GetComponent<AsgharDoorTriggerForwarder>();
            if (fwd == null) fwd = exitObj.AddComponent<AsgharDoorTriggerForwarder>();
            fwd.isEnter = false;
        }
    }

    public void EnterRoom()
    {
        if (_isInRoom) return;
        _isInRoom = true;

        EnsurePlayerCamera();
        DisableOldRoomCameraAndVoid();

        if (_zoomCoroutine != null) StopCoroutine(_zoomCoroutine);
        _zoomCoroutine = StartCoroutine(AnimateOrthoSize(zoomedOrthoSize, zoomDuration));
    }

    public void ExitRoom()
    {
        if (!_isInRoom) return;
        _isInRoom = false;

        EnsurePlayerCamera();

        if (_zoomCoroutine != null) StopCoroutine(_zoomCoroutine);
        _zoomCoroutine = StartCoroutine(AnimateOrthoSize(normalOrthoSize, zoomDuration));
    }

    private IEnumerator AnimateOrthoSize(float targetSize, float duration)
    {
        if (_playerCam == null) yield break;

        float startSize = _playerCam.Lens.OrthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            _playerCam.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, smoothT);
            yield return null;
        }

        _playerCam.Lens.OrthographicSize = targetSize;
        _zoomCoroutine = null;
    }
}

public class AsgharDoorTriggerForwarder : MonoBehaviour
{
    public bool isEnter;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && other.CompareTag("Player") && AsgharRoomCamera.Instance != null)
        {
            if (isEnter)
            {
                AsgharRoomCamera.Instance.EnterRoom();
            }
            else
            {
                AsgharRoomCamera.Instance.ExitRoom();
            }
        }
    }
}
