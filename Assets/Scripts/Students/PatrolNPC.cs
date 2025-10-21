using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Transactions;
using UnityEngine;
using Random = UnityEngine.Random;

public class PatrolNPC : MonoBehaviour
{
    public List<Transform> wayPoints;
    public float moveSpeed;

    private int _currentIndex = 0;
    private bool _isReturning = false;

    // Private movement fields
    private Vector2 _lastMove;
    private bool _facingLeft;
    
    // Stop logic
    [Header("Stopping Behavior")]
    [Range(0, 1)]
    public float stopChance = 0.2f;
    public float minStopTime;
    public float maxStopTime = 3f;
    private float _chosenStopTime;
    private bool _isStopping;
    private float _stopTimer;
    
    // Private character fields
    private Vector2 _movementInput;
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (_isStopping)
        {
            _stopTimer += Time.fixedDeltaTime;
            if (_stopTimer >= _chosenStopTime)
            {
                _isStopping = false;
                _stopTimer = 0;
            }
        }
        
        if (!_isStopping) ProcessMovement();
        
        Animate();
        
        // Flip the character
        if (_movementInput.x < 0 && _facingLeft || _movementInput.x > 0 && !_facingLeft)
        {
            Flip();
        }
    }

    private void ProcessMovement()
    {
        // Skip if the there is no way to go
        if (wayPoints.Count == 0) return;

        // Move a little bit in each frame
        Transform target = wayPoints[_currentIndex];
        transform.position = Vector2.MoveTowards(transform.position, target.position
            , moveSpeed * Time.fixedDeltaTime);
        
        Vector2 direction = (target.position - transform.position).normalized;
        _movementInput = direction;

        if (direction.magnitude > 0.01f)
        {
            _lastMove = direction;
        }

        if (Vector2.Distance(transform.position, target.position) < 0.01f)
        {
            if (Random.value < stopChance)
            {
                _isStopping = true;
                _chosenStopTime = Random.Range(minStopTime, maxStopTime);
                _stopTimer = 0;
                return;
            }
            
            if (!_isReturning)
            {
                _currentIndex++;
                if (_currentIndex >= wayPoints.Count)
                {
                    _currentIndex = wayPoints.Count - 2;
                    _isReturning = true;
                }
            }
            else
            {
                _currentIndex--;
                if (_currentIndex < 0)
                {
                    _currentIndex = 1;
                    _isReturning = false;
                }
            }
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
}
