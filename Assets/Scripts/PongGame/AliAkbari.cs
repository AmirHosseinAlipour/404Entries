using UnityEngine;

public class AliAkbari : MonoBehaviour
{
    public PongManager fadeActivator;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().isUIActive = true; 
            fadeActivator.FadeInAndActivate();
        }
    }
}
