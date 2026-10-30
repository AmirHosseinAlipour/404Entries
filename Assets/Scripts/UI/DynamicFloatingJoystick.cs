using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DynamicFloatingJoystick : MonoBehaviour
{
    [Header("Visual Components")]
    public RectTransform backgroundRect;
    public RectTransform handleRect;
    public CanvasGroup canvasGroup;

    [Header("Size & Scaling")]
    [Range(0.2f, 2f)]
    public float joystickScale = 0.8f;

    [Range(0.15f, 0.8f)]
    public float handleScaleRatio = 0.38f;

    public float baseBackgroundDiameter = 180f;

    public float movementRange = 65f;
    public float handleSmoothness = 30f;

    [Header("Elasticity & Resistance (Famous Games Feel)")]
    public bool enableElasticResistance = true;

    [Range(0.05f, 0.5f)]
    public float elasticTension = 0.22f;

    public bool dynamicFollow = true;

    public bool anchorOnTouch = false;

    [Range(0.05f, 0.5f)]
    public float followResistance = 0.18f;

    [Range(1.1f, 2.5f)]
    public float followDeadzoneMultiplier = 1.35f;

    public float maxFollowDistanceFromOrigin = 300f;

    [Header("Sprint Settings")]
    [Range(0.5f, 1f)]
    public float sprintThreshold = 0.75f;

    [Header("Visual Feedback")]
    public float restingAlpha = 0.5f;
    public float activeAlpha = 1.0f;

    [Header("Touch Zone Area")]
    [Range(0.2f, 1f)]
    public float touchZoneWidthFraction = 0.65f;

    private RectTransform _canvasRect;
    private Canvas _canvas;
    private Vector3 _defaultLocalPosition;
    private Vector3 _touchStartLocalPos;
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
        FindCanvasAndReferences();
        CreateTouchZone();
        ApplyVisualSizes();
    }

    private void Start()
    {
        _player = Object.FindFirstObjectByType<PlayerController>();

        if (handleRect != null)
        {
            var legacyStick = handleRect.GetComponent<UnityEngine.InputSystem.OnScreen.OnScreenStick>();
            if (legacyStick != null)
            {
                Destroy(legacyStick);
            }
        }

        if (backgroundRect != null)
        {
            var percentageSetter = backgroundRect.GetComponent<SetTransformByPercentage>();
            if (percentageSetter != null)
            {
                percentageSetter.UpdateTransform();
            }

            _defaultLocalPosition = backgroundRect.localPosition;
            _touchStartLocalPos = _defaultLocalPosition;
        }

        ApplyVisualSizes();
        SetRestingVisuals();
    }

    private void OnValidate()
    {
        FindCanvasAndReferences();
        ApplyVisualSizes();
    }

    private void FindCanvasAndReferences()
    {
        _canvas = GetComponentInParent<Canvas>();
        if (_canvas == null) _canvas = GetComponent<Canvas>();
        if (_canvas != null)
        {
            _canvasRect = _canvas.GetComponent<RectTransform>();
            if (_canvas.sortingOrder < 5)
            {
                _canvas.sortingOrder = 5;
            }
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null && _canvas != null)
            {
                canvasGroup = _canvas.gameObject.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = _canvas.gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (backgroundRect == null)
        {
            if (CompareTag("JoyStick"))
            {
                backgroundRect = GetComponent<RectTransform>();
            }
            else
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

    public void ApplyVisualSizes()
    {
        if (backgroundRect != null)
        {
            backgroundRect.pivot = new Vector2(0.5f, 0.5f);
            backgroundRect.sizeDelta = new Vector2(baseBackgroundDiameter, baseBackgroundDiameter);

            if (joystickScale > 0.05f)
            {
                backgroundRect.localScale = new Vector3(joystickScale, joystickScale, 1f);
            }

            if (handleRect != null)
            {
                handleRect.pivot = new Vector2(0.5f, 0.5f);
                handleRect.anchorMin = new Vector2(0.5f, 0.5f);
                handleRect.anchorMax = new Vector2(0.5f, 0.5f);

                float handleDiameter = baseBackgroundDiameter * handleScaleRatio;
                handleRect.sizeDelta = new Vector2(handleDiameter, handleDiameter);
                handleRect.localScale = Vector3.one;
            }

            if (movementRange <= 0.01f || Mathf.Approximately(movementRange, 75f))
            {
                movementRange = (baseBackgroundDiameter * 0.5f) * (1f - handleScaleRatio * 0.5f);
            }
        }
    }

    private void CreateTouchZone()
    {
        if (_touchZoneObj != null) return;
        Transform parentTransform = _canvasRect != null ? _canvasRect : transform;

        Transform existing = parentTransform.Find("DynamicJoystickTouchZone");
        if (existing != null)
        {
            _touchZoneObj = existing.gameObject;
            AttachReceiver(_touchZoneObj);
            if (backgroundRect != null) AttachReceiver(backgroundRect.gameObject);
            if (handleRect != null) AttachReceiver(handleRect.gameObject);
            return;
        }

        _touchZoneObj = new GameObject("DynamicJoystickTouchZone", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        _touchZoneObj.transform.SetParent(parentTransform, false);
        _touchZoneObj.transform.SetAsFirstSibling();

        RectTransform tzRect = _touchZoneObj.GetComponent<RectTransform>();
        tzRect.anchorMin = new Vector2(0f, 0f);
        tzRect.anchorMax = new Vector2(touchZoneWidthFraction, 1f);
        tzRect.offsetMin = Vector2.zero;
        tzRect.offsetMax = Vector2.zero;

        Image img = _touchZoneObj.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0);
        img.raycastTarget = true;

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
        bool f2Pressed = false;
        if (Keyboard.current != null && Keyboard.current.f2Key.wasPressedThisFrame) f2Pressed = true;
        if (Input.GetKeyDown(KeyCode.F2)) f2Pressed = true;
        if (f2Pressed)
        {
            PlatformHelper.SetForceShowJoystick(!PlatformHelper.forceShowMobileControls);
        }

        if (_player == null)
        {
            _player = Object.FindFirstObjectByType<PlayerController>();
        }

        bool isUiBlocked = (_player != null && _player.isUIActive);
        if (isUiBlocked)
        {
            if (_isDragging)
            {
                ResetJoystick();
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            if (_touchZoneObj != null && _touchZoneObj.activeSelf)
            {
                _touchZoneObj.SetActive(false);
            }

            return;
        }
        else
        {
            if (_touchZoneObj != null && !_touchZoneObj.activeSelf)
            {
                _touchZoneObj.SetActive(true);
            }

            if (canvasGroup != null && !canvasGroup.blocksRaycasts)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                if (!_isDragging)
                {
                    canvasGroup.alpha = restingAlpha;
                }
            }
        }

        if (_isDragging)
        {
            bool hasPointer = false;
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
                hasPointer = true;
            else if (Input.GetMouseButton(0))
                hasPointer = true;
            else if (Input.touchCount > 0)
                hasPointer = true;
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                hasPointer = true;

            if (!hasPointer)
            {
                ResetJoystick();
                return;
            }
        }

        if (handleRect != null)
        {
            handleRect.anchoredPosition = Vector2.Lerp(
                handleRect.anchoredPosition,
                _targetHandlePosition,
                handleSmoothness * Time.unscaledDeltaTime
            );
        }

        if (!_isDragging && backgroundRect != null)
        {
            backgroundRect.localPosition = Vector3.Lerp(
                backgroundRect.localPosition,
                _defaultLocalPosition,
                8f * Time.unscaledDeltaTime
            );
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isDragging) return;
        if (_player != null && _player.isUIActive) return;

        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            GameObject hit = eventData.pointerCurrentRaycast.gameObject;
            if (hit != _touchZoneObj && hit != gameObject &&
                (backgroundRect == null || hit != backgroundRect.gameObject) &&
                (handleRect == null || hit != handleRect.gameObject))
            {
                if (hit.GetComponentInParent<Button>() != null ||
                    hit.GetComponentInParent<InputField>() != null ||
                    hit.GetComponentInParent<ScrollRect>() != null ||
                    hit.GetComponentInParent<Slider>() != null ||
                    hit.GetComponentInParent<Toggle>() != null)
                {
                    return;
                }
            }
        }

        _activePointerId = eventData.pointerId;
        _isDragging = true;

        if (backgroundRect != null)
        {
            RectTransform parentRect = backgroundRect.parent as RectTransform;
            if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPointInParent))
            {
                backgroundRect.localPosition = new Vector3(localPointInParent.x, localPointInParent.y, backgroundRect.localPosition.z);
                _touchStartLocalPos = backgroundRect.localPosition;
            }
        }

        _targetHandlePosition = Vector2.zero;
        if (handleRect != null)
        {
            handleRect.anchoredPosition = Vector2.zero;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = activeAlpha;
        }

        _currentInput = Vector2.zero;
        if (_player != null)
        {
            _player.SetJoystickInput(Vector2.zero);
            _player.isSprinting = false;
        }
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
            Vector2 direction = (distance > 0.001f) ? (localPoint / distance) : Vector2.zero;
            float currentRange = movementRange > 0.01f ? movementRange : 65f;

            if (distance <= currentRange)
            {
                _targetHandlePosition = localPoint;
            }
            else
            {
                float extra = distance - currentRange;
                if (enableElasticResistance)
                {
                    float elasticOffset = Mathf.Log(1f + extra / currentRange) * (currentRange * elasticTension);
                    _targetHandlePosition = direction * (currentRange + elasticOffset);
                }
                else
                {
                    _targetHandlePosition = direction * currentRange;
                }

                if (dynamicFollow && !anchorOnTouch)
                {
                    float followThreshold = currentRange * followDeadzoneMultiplier;
                    if (distance > followThreshold)
                    {
                        float excess = distance - followThreshold;
                        Vector2 followStep = direction * (excess * followResistance);
                        Vector3 candidatePos = backgroundRect.localPosition + (Vector3)(followStep * backgroundRect.localScale.x);

                        if (maxFollowDistanceFromOrigin > 0.01f)
                        {
                            Vector3 fromOrigin = candidatePos - _touchStartLocalPos;
                            if (fromOrigin.magnitude > maxFollowDistanceFromOrigin)
                            {
                                candidatePos = _touchStartLocalPos + fromOrigin.normalized * maxFollowDistanceFromOrigin;
                            }
                        }

                        backgroundRect.localPosition = candidatePos;
                    }
                }
            }

            float normalizedMag = Mathf.Clamp01(distance / currentRange);
            _currentInput = direction * normalizedMag;

            if (_player != null)
            {
                _player.SetJoystickInput(_currentInput);
                _player.isSprinting = (normalizedMag >= sprintThreshold);
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
            _player.isSprinting = false;
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
