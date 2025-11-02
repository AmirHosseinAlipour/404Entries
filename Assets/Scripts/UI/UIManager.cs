using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button button1;
    public Button button2;
    [Header("Panels")]
    public RectTransform panel1;
    public RectTransform panel2;
    
    private bool panel1Active = false;
    private bool panel2Active = false;

    void Start()
    {
        
        button1.onClick.AddListener(OpenPanel1);
        button2.onClick.AddListener(OpenPanel2);
        DeactivateAllPanels();
    }
    public void OpenPanel1()
    {
        Deactive();
        panel1Active = true;
        UIAnimationManager.Instance.ShowWindow(panel1, 0.5f);
    }
    public void OpenPanel2()
    {
        Deactive();
        panel2Active = true;
        UIAnimationManager.Instance.ShowWindow(panel2, 0.5f);
    }
    public void DeactivateAllPanels()
    {
        if (panel1Active)
        {
            UIAnimationManager.Instance.HideWindow(panel1, 0.5f);
            panel1Active = false;
        }
        else if (panel2Active)
        {
            UIAnimationManager.Instance.HideWindow(panel2, 0.5f);
            panel2Active = false;
        }
    }

    private void Deactive()
    {
        UIAnimationManager.Instance.HideWindow(panel2, 0.5f);
        UIAnimationManager.Instance.HideWindow(panel1, 0.5f);
    }
    public void ExitGame()
    {
        Debug.Log("Game is quitting...");
        Application.Quit();
    }
}