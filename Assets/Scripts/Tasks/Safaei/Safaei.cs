using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

[System.Serializable]
public class PhaseOptions
{
    [TextArea(3, 10)]
    public string[] options = new string[3];
}
public class Safaei : BaseProfessors
{
    public Text dialogueText;
    public RectTransform textPanel;
    public Button[] optionButtons;
    public FarsiTypewriter typewriter;
    public RectTransform ButtonCanvas;
    [TextArea(3, 10)] public string[] phaseTexts;
    public PhaseOptions[] phaseOptions;

    public int[] correctOptionIndex;

    private int currentPhase = 0;
    private bool isTyping = false;
    private float baseSpeed = 0.05f;
    private float currentSpeed;

    void Start()
    {
        base.Start();
        currentSpeed = baseSpeed;
        HideAllOptions();
    }

    public void StartGame()
    {
        currentPhase = 0;
        textPanel.gameObject.SetActive(true);
        StartCoroutine(StartPhase());
    }

    IEnumerator StartPhase()
    {
        if (currentPhase >= phaseTexts.Length)
        {
            TaskManager.Instance.CompleteTask(TaskOrderNumber);
    
            HideAllOptions();
            RectTransform parent = StartPanel.transform.parent.GetComponent<RectTransform>();
            parent.gameObject.SetActive(false);
            UIAnimationManager.Instance.HideWindow(parent, 0.5f);
            UIAnimationManager.Instance.ShowDialogueWindow(firstWinDialogue, 0.5f, firstWinDialogueFtw);

            yield break;
        }

        HideAllOptions();
        typewriter.SetText(phaseTexts[currentPhase]);
        typewriter.GetComponent<DynamicFontResizer>().AdjustFontSize();
        ShowOptions();
    }

    void HideAllOptions()
    {
       UIAnimationManager.Instance.HideWindow(ButtonCanvas, 0.5f);
    }

    void ShowOptions()
    {
        UIAnimationManager.Instance.ShowWindow(ButtonCanvas, 0.5f);
        
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
            optionButtons[i].GetComponentInChildren<FarsiTypewriter>().SetText(phaseOptions[currentPhase].options[i]);
            optionButtons[i].GetComponentInChildren<DynamicFontResizer>().AdjustFontSize();
        }
    }

    void OnOptionSelected(int index)
    {
        HideAllOptions();
        bool isCorrect = (index == correctOptionIndex[currentPhase]);
        if (!isCorrect)
        {
            currentPhase = -1;
            HandleLoose();
            return;
        }

        currentPhase++;
        StartCoroutine(StartPhase());
    }

    private void HandleLoose()
    {
        UIAnimationManager.Instance.HideWindow(StartPanel , 0.5f);
        UIAnimationManager.Instance.ShowWindow(acceptRect, 0.01f);
        UIAnimationManager.Instance.HideWindow(textPanel, 0.01f);
        UIAnimationManager.Instance.ShowDialogueWindow(firstFailDialogue, 0.5f, firstFailDialogueFtw);
    }
}