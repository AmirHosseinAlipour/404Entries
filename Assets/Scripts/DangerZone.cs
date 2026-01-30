using UnityEngine;

public class DangerZone : MonoBehaviour
{
    public CanvasGroup dangerPanel; 
    public float fadeSpeed = 2f;    

    private bool playerInside = false;
    private bool fadingIn = true;

    void Update()
    {
        if (playerInside && dangerPanel != null)
        {
            if (fadingIn)
            {
                dangerPanel.alpha += Time.deltaTime * fadeSpeed;
                if (dangerPanel.alpha >= 1f)
                    fadingIn = false;
            }
            else
            {
                dangerPanel.alpha -= Time.deltaTime * fadeSpeed;
                if (dangerPanel.alpha <= 0f)
                    fadingIn = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            dangerPanel.alpha = 0f; 
        }
    }
}
