using System;
using UnityEngine;

public class enterdialouge : MonoBehaviour
{
    public RectTransform dialouge;
    public FarsiTypewriter dialougeFtw;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            UIAnimationManager.Instance.ShowDialogueWindow(dialouge, 0.5f,dialougeFtw);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            UIAnimationManager.Instance.HideWindow(dialouge, 0.5f);
        }
    }
}
