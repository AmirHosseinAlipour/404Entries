using System;
using UnityEngine;

public class OnTriggerSpecialEmote : MonoBehaviour
{
    public RectTransform normalEmote;
    public RectTransform triggerEmote;

    private void Start()
    {
        HideEmote(triggerEmote);
        ShowEmote(normalEmote);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HideEmote(normalEmote);
            ShowEmote(triggerEmote);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HideEmote(triggerEmote);
            ShowEmote(normalEmote);
        }
    }
    
    private void ShowEmote(RectTransform emote)
    {
        UIAnimationManager.Instance.ShowWindow(emote, 0.3f);
    }

    private void HideEmote(RectTransform emote)
    {
        UIAnimationManager.Instance.HideWindow(emote, 0.3f);
    }
}
