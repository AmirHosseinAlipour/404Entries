using System;
using UnityEngine;

public class MusicPlayerTrigger : MonoBehaviour
{
    public RectTransform MusicPlayerPanel;
    private void OnTriggerEnter2D(Collider2D other)
    {
        UIAnimationManager.Instance.ShowWindow(MusicPlayerPanel , 0.5f);
    }

    public void BackToMainPanel()
    {
        UIAnimationManager.Instance.HideWindow(MusicPlayerPanel , 0.5f);
    }
}
