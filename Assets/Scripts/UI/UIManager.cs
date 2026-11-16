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

    private void Awake()
    {
        if (panel1 != null) panel1.gameObject.SetActive(false);
        if (panel2 != null) panel2.gameObject.SetActive(false);
        panel1Active = false;
        panel2Active = false;
    }

    void Start()
    {
        if (button1 != null) button1.onClick.AddListener(OpenPanel1);
        if (button2 != null) button2.onClick.AddListener(OpenPanel2);
        DeactivateAllPanels();
    }

    public void OpenPanel1()
    {
        DeactivateAllPanels();
        panel1Active = true;
        if (panel1 != null)
        {
            panel1.gameObject.SetActive(true);
            UIAnimationManager.Instance.ShowWindow(panel1, 0.4f);
        }
    }

    public void OpenPanel2()
    {
        DeactivateAllPanels();
        panel2Active = true;
        if (panel2 != null)
        {
            panel2.gameObject.SetActive(true);
            UIAnimationManager.Instance.ShowWindow(panel2, 0.4f);
        }
    }

    public void DeactivateAllPanels()
    {
        if (panel1 != null && panel1.gameObject.activeSelf)
        {
            UIAnimationManager.Instance.HideWindow(panel1, 0.4f);
        }
        if (panel2 != null && panel2.gameObject.activeSelf)
        {
            UIAnimationManager.Instance.HideWindow(panel2, 0.4f);
        }
        panel1Active = false;
        panel2Active = false;
    }

    private void Deactive()
    {
        DeactivateAllPanels();
    }
    public void ExitGame()
    {
        Debug.Log("Game is quitting...");
        Application.Quit();
    }
}