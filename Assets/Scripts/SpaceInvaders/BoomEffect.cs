using UnityEngine;

public class BoomEffect : MonoBehaviour
{
    public float growSpeed = 3f;   
    public float maxScale = 1f;      
    public float fadeSpeed = 2f;     

    private SpriteRenderer sr;
    private Vector3 startScale;
    private bool isPlaying = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
        Color c = sr.color;
        c.a = 0;                 
        sr.color = c;
    }

    public void Play()
    {
        transform.localScale = startScale;
        Color c = sr.color;
        c.a = 1;
        sr.color = c;
        isPlaying = true;
    }
    void Update()
    {
        if (!isPlaying) return;
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * maxScale, Time.deltaTime * growSpeed);
        Color c = sr.color;
        c.a -= Time.deltaTime * fadeSpeed;
        sr.color = c;
        if (c.a <= 0.01f)
        {
            isPlaying = false;
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}