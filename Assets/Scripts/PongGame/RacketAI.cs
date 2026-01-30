using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacketAI : Racket
{
    public Transform ball_Transform;
    protected override void Movement()
    {
        
        float distance = Mathf.Abs(ball_Transform.position.y - transform.position.y);
        if (distance > 3)
        {
            if (ball_Transform.position.y > transform.position.y)
            {
                GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, 1) * moveSpeed;
            }

            if (ball_Transform.position.y < transform.position.y)
            {
                GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, -1) * moveSpeed;
            }
        }

    }

}
