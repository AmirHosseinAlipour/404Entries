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
    private MusicChange musicChange;

    protected override void Start()
    {
        base.Start();
        currentSpeed = baseSpeed;
        HideAllOptions();
        musicChange = GetComponent<MusicChange>();
    }

    public void StartGame()
    {
        StartMinigame();
        currentPhase = 0;
        if (textPanel != null) textPanel.gameObject.SetActive(true);
        if (musicChange != null)
        {
            musicChange.ToggleMusic();
            musicChange.PlayThemeMusic();
        }
        StartCoroutine(StartPhase());
    }

    IEnumerator StartPhase()
    {
        if (phaseTexts == null || currentPhase >= phaseTexts.Length)
        {
            CompleteMission();
            HideAllOptions();
            if (StartPanel != null && StartPanel.transform.parent != null)
            {
                RectTransform parent = StartPanel.transform.parent.GetComponent<RectTransform>();
                if (parent != null)
                {
                    parent.gameObject.SetActive(false);
                    UIAnimationManager.Instance.HideWindow(parent, 0.5f);
                }
            }
            yield break;
        }

        HideAllOptions();
        if (typewriter != null)
        {
            typewriter.SetText(phaseTexts[currentPhase]);
            var dfr = typewriter.GetComponent<DynamicFontResizer>();
            if (dfr != null) dfr.AdjustFontSize();
        }
        ShowOptions();
    }

    void HideAllOptions()
    {
        if (ButtonCanvas != null)
            UIAnimationManager.Instance.HideWindow(ButtonCanvas, 0.5f);
    }

    void ShowOptions()
    {
        if (ButtonCanvas != null)
            UIAnimationManager.Instance.ShowWindow(ButtonCanvas, 0.5f);
        
        if (optionButtons == null) return;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] == null) continue;
            int index = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
            
            if (phaseOptions != null && currentPhase >= 0 && currentPhase < phaseOptions.Length && phaseOptions[currentPhase].options != null && i < phaseOptions[currentPhase].options.Length)
            {
                var ftw = optionButtons[i].GetComponentInChildren<FarsiTypewriter>();
                if (ftw != null) ftw.SetText(phaseOptions[currentPhase].options[i]);
                var dfr = optionButtons[i].GetComponentInChildren<DynamicFontResizer>();
                if (dfr != null) dfr.AdjustFontSize();
            }
        }
    }

    void OnOptionSelected(int index)
    {
        HideAllOptions();
        bool isCorrect = (correctOptionIndex != null && currentPhase >= 0 && currentPhase < correctOptionIndex.Length && index == correctOptionIndex[currentPhase]);
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
        if (StartPanel != null) UIAnimationManager.Instance.HideWindow(StartPanel, 0.5f);
        if (acceptRect != null) UIAnimationManager.Instance.ShowWindow(acceptRect, 0.01f);
        if (textPanel != null) UIAnimationManager.Instance.HideWindow(textPanel, 0.01f);
        FailMission();
    }
}