using UnityEngine;
using System.Collections; // حتماً برای Coroutine اضافه شود

public class Test : MonoBehaviour
{
    void Start()
    {
        // اجرای Coroutine
        StartCoroutine(TriggerAfterDelay(5f));
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