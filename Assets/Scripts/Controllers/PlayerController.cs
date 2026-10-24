using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    // Public Fields
    public float speed = 5f;
    public float sprintMultiplier = 1.4f;
    [HideInInspector] public bool isSprinting = false;
    
    // Private Fields
    private Vector2 _lastMove;
    private bool _facingLeft = true;
    
    // Player components
    private Rigidbody2D _rb;
    private Vector2 _movementInput;
    private Animator _animator;
    
    // Skip Inputs with this bool
    [HideInInspector] public bool isUIActive = false;
    
    // Logic for minimap
    [HideInInspector] public float FacingAngle { get; private set; }

    [Header("Platform & Mobile Controls")]
    [Tooltip("If true, shows the virtual mobile joystick even when testing inside the PC/Unity Editor.")]
    public bool forceShowMobileJoystick = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        if (_rb != null)
        {
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    private void Start()
    {
        if (forceShowMobileJoystick)
        {
            PlatformHelper.SetForceShowJoystick(true);
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            PlatformHelper.SetForceShowJoystick(forceShowMobileJoystick);
        }
    }

    [Header("Movement Polish & Smoothing")]
    [Tooltip("How smoothly character accelerates to top speed.")]
    public float acceleration = 35f;
    [Tooltip("How smoothly character decelerates to a stop.")]
    public float deceleration = 40f;
    private Vector2 _joystickInput = Vector2.zero;

    private void Update()
    {
        // Enters the mini game (missions) / dialogues - freeze immediately
        if (isUIActive)
        {
            UIMode();
            return;
        }

        Vector2 activeInput = GetActiveInput();
        ProcessMovement(activeInput);
        
        Animate(activeInput);
        
        // Flip the character
        if (activeInput.x < -0.01f && !_facingLeft || activeInput.x > 0.01f && _facingLeft)
        {
            Flip();
        }
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;

        if (isUIActive)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 activeInput = GetActiveInput();
        bool shiftHeld = (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed);
        float currentSpeed = (isSprinting || shiftHeld) ? (speed * sprintMultiplier) : speed;
        Vector2 targetVelocity = activeInput * currentSpeed;

        float rate = (targetVelocity.sqrMagnitude > 0.001f) ? acceleration : deceleration;
        _rb.linearVelocity = Vector2.MoveTowards(_rb.linearVelocity, targetVelocity, rate * Time.fixedDeltaTime);
    }

    public Vector2 GetActiveInput()
    {
        if (isUIActive) return Vector2.zero;
        return _joystickInput.sqrMagnitude > 0.001f ? _joystickInput : _movementInput;
    }

    public void SetJoystickInput(Vector2 input)
    {
        if (isUIActive)
        {
            _joystickInput = Vector2.zero;
            return;
        }
        _joystickInput = Vector2.ClampMagnitude(input, 1f);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // If player was in UI mini games inputs should be skipped
        if (isUIActive) return;
        
        // Normalize inputs to prevent faster diagonal movement
        _movementInput = context.ReadValue<Vector2>().normalized;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
            isSprinting = true;
        else if (context.canceled)
            isSprinting = false;
    }

    private void ProcessMovement(Vector2 input)
    {
        if (input.sqrMagnitude > 0.001f)
        {
            _lastMove = input;
        }
        
        // For mini map
        FacingAngle = Mathf.Atan2(_lastMove.y, _lastMove.x) * Mathf.Rad2Deg - 90f;
    }

    private void Animate(Vector2 input)
    {
        if (_animator == null) return;

        // Set animator parameters
        _animator.SetFloat("MoveX", input.x);
        _animator.SetFloat("MoveY", input.y);
        _animator.SetFloat("MoveMagnitude", input.magnitude);
        _animator.SetFloat("LastMoveX", _lastMove.x);
        _animator.SetFloat("LastMoveY", _lastMove.y);

        // Dynamically scale animation playback speed when sprinting to perfectly match footstep pace
        bool shiftHeld = (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed);
        bool isMoving = input.sqrMagnitude > 0.01f;
        bool isSprintingNow = (isSprinting || shiftHeld) && isMoving;
        _animator.speed = isSprintingNow ? sprintMultiplier : 1f;
    }

    private void Flip()
    {
        Vector3 scale = gameObject.transform.localScale;
        scale.x *= -1;
        gameObject.transform.localScale = scale;
        _facingLeft = !_facingLeft;
    }

    private void UIMode()
    {
        _movementInput = Vector2.zero;
        _joystickInput = Vector2.zero;
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
        }
        if (_animator != null)
        {
            _animator.SetFloat("MoveX", 0f);
            _animator.SetFloat("MoveY", 0f);
            _animator.SetFloat("MoveMagnitude", 0f);
            _animator.speed = 1f;
        }
    }
    public bool IsMoving()
    {
        return GetActiveInput().magnitude > 0.1f;
    }
    public virtual void Respawn(Vector3 position)
    {
        transform.position = position;
        if (_rb != null)
        {
            _rb.position = new Vector2(position.x, position.y);
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }
    }

    public void SetIdleDirection(Vector2 idleDirection)
    {
        _lastMove = idleDirection.normalized; 
    }
}