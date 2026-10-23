using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A smooth, floating/dynamic on-screen joystick.
/// Spawns right where the player clicks or touches within the touch zone,
/// follows finger/mouse smoothly, and sends normalized input to PlayerController.
/// </summary>
public class DynamicFloatingJoystick : MonoBehaviour
{
    [Header("Visual Components")]
    public RectTransform backgroundRect;
    public RectTransform handleRect;
    public CanvasGroup canvasGroup;

    [Header("Joystick Settings")]
    [Tooltip("Maximum distance handle can move from background center (in pixels).")]
    public float movementRange = 75f;

    [Tooltip("Smoothing speed for handle movement.")]
    public float handleSmoothness = 30f;

    [Tooltip("If true, the joystick background will slide to follow the finger when dragged beyond movement range.")]
    public bool dynamicFollow = true;

    [Tooltip("When resting/untouched, this is the resting alpha.")]
    public float restingAlpha = 0.6f;

    [Tooltip("When touched/dragging, this is the active alpha.")]
    public float activeAlpha = 1.0f;

    [Header("Touch Zone Area")]
    [Tooltip("Fraction of screen width for the touch zone (0 to 1). 0.65 covers the left 65% of the screen.")]
    [Range(0.2f, 1f)]
    public float touchZoneWidthFraction = 0.65f;

    // Runtime state
    private RectTransform _canvasRect;
    private Vector2 _defaultBackgroundPosition;
    private Vector2 _targetHandlePosition;
    private Vector2 _currentInput;
    private bool _isDragging = false;
    private int _activePointerId = -999;
    private PlayerController _player;
    private GameObject _touchZoneObj;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoAttachToJoystickCanvas()
    {
        GameObject joystickCanvas = PlatformHelper.GetJoystickCanvas();
        if (joystickCanvas != null)
        {
            if (joystickCanvas.GetComponent<DynamicFloatingJoystick>() == null)
            {
                joystickCanvas.AddComponent<DynamicFloatingJoystick>();
            }
        }
    }

    private void Awake()
    {
        _canvasRect = GetComponent<RectTransform>();
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        FindReferences();
        CreateTouchZone();
    }

    private Vector3 _defaultWorldPosition;

    private void Start()
    {
        _player = Object.FindFirstObjectByType<PlayerController>();

        if (backgroundRect != null)
        {
            _defaultBackgroundPosition = backgroundRect.anchoredPosition;
            _defaultWorldPosition = backgroundRect.position;
        }

        // Disable legacy OnScreenStick if present so it doesn't conflict with dynamic positioning
        if (handleRect != null)
        {
            var legacyStick = handleRect.GetComponent<UnityEngine.InputSystem.OnScreen.OnScreenStick>();
            if (legacyStick != null)
            {
                legacyStick.enabled = false;
            }
        }

        SetRestingVisuals();
    }

    private void FindReferences()
    {
        if (backgroundRect == null)
        {
            var bgObj = GameObject.FindWithTag("JoyStick");
            if (bgObj != null)
            {
                backgroundRect = bgObj.GetComponent<RectTransform>();
            }
            else
            {
                Transform bg = transform.Find("Background");
                if (bg != null) backgroundRect = bg.GetComponent<RectTransform>();
            }
        }

        if (handleRect == null && backgroundRect != null)
        {
            Transform h = backgroundRect.Find("Handle");
            if (h != null)
            {
                handleRect = h.GetComponent<RectTransform>();
            }
            else if (backgroundRect.childCount > 0)
            {
                handleRect = backgroundRect.GetChild(0).GetComponent<RectTransform>();
            }
        }
    }

    private void CreateTouchZone()
    {
        if (_touchZoneObj != null) return;

        Transform existing = transform.Find("DynamicJoystickTouchZone");
        if (existing != null)
        {
            _touchZoneObj = existing.gameObject;
            AttachReceiver(_touchZoneObj);
            if (backgroundRect != null) AttachReceiver(backgroundRect.gameObject);
            if (handleRect != null) AttachReceiver(handleRect.gameObject);
            return;
        }

        // Create invisible touch zone panel covering the left section of the screen
        _touchZoneObj = new GameObject("DynamicJoystickTouchZone", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        _touchZoneObj.transform.SetParent(transform, false);
        _touchZoneObj.transform.SetAsFirstSibling(); // Behind Background

        RectTransform tzRect = _touchZoneObj.GetComponent<RectTransform>();
        tzRect.anchorMin = new Vector2(0f, 0f);
        tzRect.anchorMax = new Vector2(touchZoneWidthFraction, 1f);
        tzRect.offsetMin = Vector2.zero;
        tzRect.offsetMax = Vector2.zero;

        Image img = _touchZoneObj.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0); // transparent
        img.raycastTarget = true;

        // Attach event receivers
        AttachReceiver(_touchZoneObj);
        if (backgroundRect != null) AttachReceiver(backgroundRect.gameObject);
        if (handleRect != null) AttachReceiver(handleRect.gameObject);
    }

    private void AttachReceiver(GameObject target)
    {
        if (target == null) return;
        var receiver = target.GetComponent<JoystickEventForwarder>();
        if (receiver == null)
        {
            receiver = target.AddComponent<JoystickEventForwarder>();
        }
        receiver.joystick = this;
    }

    private void Update()
    {
        if (_player == null)
        {
            _player = Object.FindFirstObjectByType<PlayerController>();
        }

        if (_player != null && _player.isUIActive && _isDragging)
        {
            ResetJoystick();
            return;
        }

        // Smooth handle movement
        if (handleRect != null)
        {
            handleRect.anchoredPosition = Vector2.Lerp(
                handleRect.anchoredPosition, 
                _targetHandlePosition, 
                handleSmoothness * Time.unscaledDeltaTime
            );
        }

        // Return background towards default position smoothly when not dragging
        if (!_isDragging && backgroundRect != null)
        {
            backgroundRect.position = Vector3.Lerp(
                backgroundRect.position, 
                _defaultWorldPosition, 
                8f * Time.unscaledDeltaTime
            );
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isDragging) return;
        if (_player != null && _player.isUIActive) return;

        _activePointerId = eventData.pointerId;
        _isDragging = true;

        // Position background at the exact pointer / touch position in world/screen space
        if (backgroundRect != null)
        {
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                backgroundRect, 
                eventData.position, 
                eventData.pressEventCamera, 
                out Vector3 worldPos))
            {
                backgroundRect.position = worldPos;
            }
        }

        _targetHandlePosition = Vector2.zero;
        if (handleRect != null) handleRect.anchoredPosition = Vector2.zero;

        // Fade in to full active alpha
        if (canvasGroup != null)
        {
            canvasGroup.alpha = activeAlpha;
        }

        ProcessPointerPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging || eventData.pointerId != _activePointerId) return;
        ProcessPointerPosition(eventData);
    }

    private void ProcessPointerPosition(PointerEventData eventData)
    {
        if (backgroundRect == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            backgroundRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            float distance = localPoint.magnitude;
            Vector2 direction = (distance > 0.001f) ? localPoint.normalized : Vector2.zero;

            if (distance > movementRange)
            {
                _targetHandlePosition = direction * movementRange;

                // Dynamic follow: gently slide the background so finger doesn't slide off
                if (dynamicFollow)
                {
                    Vector2 overshoot = localPoint - _targetHandlePosition;
                    backgroundRect.anchoredPosition += overshoot;
                }
            }
            else
            {
                _targetHandlePosition = localPoint;
            }

            // Calculate normalized input vector
            _currentInput = _targetHandlePosition / movementRange;

            if (_player != null)
            {
                _player.SetJoystickInput(_currentInput);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != _activePointerId) return;
        ResetJoystick();
    }

    public void ResetJoystick()
    {
        _isDragging = false;
        _activePointerId = -999;
        _targetHandlePosition = Vector2.zero;
        _currentInput = Vector2.zero;

        if (_player != null)
        {
            _player.SetJoystickInput(Vector2.zero);
        }

        SetRestingVisuals();
    }

    private void SetRestingVisuals()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = restingAlpha;
        }
    }

    private void OnDisable()
    {
        ResetJoystick();
    }
}

/// <summary>
/// Helper forwarder that passes UI pointer events from the touch zone, background, or handle
/// up to the DynamicFloatingJoystick.
/// </summary>
public class JoystickEventForwarder : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [HideInInspector] public DynamicFloatingJoystick joystick;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (joystick != null) joystick.OnPointerDown(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (joystick != null) joystick.OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (joystick != null) joystick.OnPointerUp(eventData);
    }
}
