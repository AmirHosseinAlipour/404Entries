using UnityEngine;

public class HumanRacket : Racket
{
    protected override void Movement()
    {
        float moveAxesValue = 0f;

        // کنترل با کیبورد (برای تست در PC)
        if (Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow))
            moveAxesValue = 1f;
        else if (Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow))
            moveAxesValue = -1f;

        // کنترل با تاچ برای موبایل
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // نصف بالایی صفحه → بالا
            if (touch.position.y > Screen.height / 2)
                moveAxesValue = 1f;
            // نصف پایینی صفحه → پایین
            else
                moveAxesValue = -1f;
        }

        rb.linearVelocity = new Vector2(0f, moveAxesValue * moveSpeed);
    }
}