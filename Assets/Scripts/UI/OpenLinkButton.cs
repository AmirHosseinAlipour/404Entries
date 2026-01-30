using System;
using UnityEngine;

public class OpenLinkButton : MonoBehaviour
{
    [SerializeField] private string Eclipse_Url = "https://example.com";
    [SerializeField] private string Blade_Url = "https://example.com";
    public RectTransform GameLab; 
    public void OpenLinkEclipse()
    {
        Application.OpenURL(Eclipse_Url);
    }

    public void OpenLinkBlade()
    {
        Application.OpenURL(Blade_Url);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            UIAnimationManager.Instance.ShowWindow(GameLab , 0.5f);
        }
    }

    public void CloseGameLab()
    {
        UIAnimationManager.Instance.HideWindow(GameLab , 0.5f);
    }
}