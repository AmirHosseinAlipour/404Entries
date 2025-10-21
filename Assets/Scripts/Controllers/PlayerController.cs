using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    // Public Fields
    public float speed;
    
    // Private Fields
    private Vector2 _lastMove;
    private bool _facingLeft = true;

    
    // Player components
    private Rigidbody2D _rb;
    private Vector2 _movementInput;
    private Animator _animator;
    
    // Skip Inputs with this bool
    public bool isUIActive = false;

    private void Start()
    {
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _animator = gameObject.GetComponent<Animator>();
    }

    private void Update()
    {
        ProcessMovement();
        
        Animate();
        
        // Flip the character
        if (_movementInput.x < 0 && !_facingLeft || _movementInput.x > 0 && _facingLeft)
        {
            Flip();
        }
        
        // Enters the mini game (missions)
        if (isUIActive) UIMode();
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _movementInput * speed;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // If player was in UI mini games inputs should be skipped
        if (isUIActive) return;
        
        // Normalize inputs to prevent faster diagonal movement
        _movementInput = context.ReadValue<Vector2>().normalized;
    }

    private void ProcessMovement()
    {
        float moveX = _movementInput.x;
        float moveY = _movementInput.y;

        if (moveX != 0 || moveY != 0)
        {
            _lastMove = _movementInput;
        }
    }

    private void Animate()
    {
        // Set animator parameters
        _animator.SetFloat("MoveX", _movementInput.x);
        _animator.SetFloat("MoveY", _movementInput.y);
        _animator.SetFloat("MoveMagnitude", _movementInput.magnitude);
        _animator.SetFloat("LastMoveX", _lastMove.x);
        _animator.SetFloat("LastMoveY", _lastMove.y);
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
    }
    public bool IsMoving()
    {
        return _movementInput.magnitude > 0.1f;
    }
    public virtual void Respawn(Vector3 position)
    {
        transform.position = position;
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }
    }
}
