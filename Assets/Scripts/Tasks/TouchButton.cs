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
    
    private void Start()
    {
        // Auto-get references if not assigned in Inspector
        if (_button == null)
            _button = GetComponent<Button>();
            
        if (_buttonRect == null)
            _buttonRect = GetComponent<RectTransform>();
        
        // If _button is still null, try getting it from the same object
        if (_button == null)
            _button = GetComponentInChildren<Button>();
    }
    
    private void OnEnable()
    {
        // Safety check - ensure we have required components
        if (_button == null || touchPressAction == null || touchPositionAction == null)
        {
            Debug.LogWarning("Touch_button missing required references!", this);
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
            // Optional: Add visual feedback
            if (_button.animator != null)
                _button.animator.SetTrigger("Pressed");
        }
    }
    
    private void OnTouchEnded(InputAction.CallbackContext context)
    {
        // Optional: Reset _button state
    }
    
    private bool IsTouchOver_button(Vector2 screenPosition)
    {
        if (_buttonRect == null)
            return false;
            
        return RectTransformUtility.RectangleContainsScreenPoint(_buttonRect, screenPosition);
    }
}