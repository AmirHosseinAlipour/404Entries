using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class TouchButton : MonoBehaviour
{
    private Button _button;
    private RectTransform _buttonRect;
    
    [Header("Input")]
    public InputActionReference touchPositionAction;
    public InputActionReference touchPressAction;

    private void Awake()
    {
        EnsureReferences();
    }
    
    private void Start()
    {
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        if (_button == null)
            _button = GetComponent<Button>();
            
        if (_buttonRect == null)
            _buttonRect = GetComponent<RectTransform>();
        
        if (_button == null)
            _button = GetComponentInChildren<Button>();
    }
    
    private void OnEnable()
    {
        EnsureReferences();
        if (_button == null || touchPressAction == null || touchPositionAction == null)
        {
            return;
        }
        
        touchPressAction.action.Enable();
        touchPositionAction.action.Enable();
        
        touchPressAction.action.started += OnTouchStarted;
        touchPressAction.action.canceled += OnTouchEnded;
    }
    
    private void OnDisable()
    {
        if (touchPressAction?.action != null)
        {
            touchPressAction.action.started -= OnTouchStarted;
            touchPressAction.action.canceled -= OnTouchEnded;
            
            touchPressAction.action.Disable();
        }
        
        touchPositionAction?.action?.Disable();
    }
    
    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        Vector2 touchPosition = touchPositionAction.action.ReadValue<Vector2>();
        
        if (IsTouchOver_button(touchPosition))
        {
            _button.onClick.Invoke();
            if (_button.animator != null)
                _button.animator.SetTrigger("Pressed");
        }
    }
    
    private void OnTouchEnded(InputAction.CallbackContext context)
    {
    }
    
    private bool IsTouchOver_button(Vector2 screenPosition)
    {
        if (_buttonRect == null)
            return false;
            
        return RectTransformUtility.RectangleContainsScreenPoint(_buttonRect, screenPosition);
    }
}