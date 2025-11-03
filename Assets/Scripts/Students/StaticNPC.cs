using System;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class StaticNPC : MonoBehaviour
{
    public enum Direction
    {
        Front,
        Back,
        Left,
        Right
    }

    public Direction idleDirection;
    
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        
        Vector2 direction = Vector2.down;

        switch (idleDirection)
        {
            case Direction.Front:
                direction = Vector2.up;
                break;
            case Direction.Left:
                direction = Vector2.left;
                break;
            case Direction.Right:
                direction = Vector2.right;
                break;
        }
        
        _animator.SetFloat("LastMoveX", direction.x);
        _animator.SetFloat("LastMoveY", direction.y);
    }
}
