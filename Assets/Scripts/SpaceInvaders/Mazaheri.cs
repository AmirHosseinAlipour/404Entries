using UnityEngine;
using System.Collections; // حتماً برای Coroutine اضافه شود

public class Mazaheri : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(TriggerAfterDelay(0.1f)); 
        }
        
    }

    // Coroutine برای تاخیر
    IEnumerator TriggerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // صبر کردن به مدت delay ثانیه

        // پیدا کردن SpaceInvadersManager و فراخوانی PlayerEnteredTrigger
        SpaceInvadersManager manager = FindObjectOfType<SpaceInvadersManager>();
        if (manager != null)
        {
            manager.PlayerEnteredTrigger();
        }
        else
        {
            Debug.LogWarning("SpaceInvadersManager پیدا نشد!");
        }
    }
}