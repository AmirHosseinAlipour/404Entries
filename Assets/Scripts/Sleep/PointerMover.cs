using UnityEngine;

public class PointerMover : MonoBehaviour
{
    public RectTransform barRect; 
    public float speed = 300f;    
    private float halfWidth;

    void Start()
    {
        if (barRect == null) Debug.LogError("Bar Rect not assigned!");
        halfWidth = barRect.rect.width / 2f;
    }

    void Update()
    {
        float range = halfWidth;
        float x = Mathf.PingPong(Time.time * speed, range * 2f) - range;
        Vector3 lp = transform.localPosition;
        lp.x = x;
        transform.localPosition = lp;
    }
}