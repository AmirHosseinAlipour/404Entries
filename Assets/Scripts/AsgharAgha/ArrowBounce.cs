using UnityEngine;

public class ArrowBounce : MonoBehaviour
{
    public float amplitude = 0.2f;  // میزان حرکت بالا و پایین
    public float speed = 2f;        // سرعت حرکت

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position; // موقعیت اولیه فلش
    }

    void Update()
    {
        // حرکت بالا و پایین با استفاده از سینوس
        float yOffset = Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = startPos + new Vector3(0, yOffset, 0);
    }
}