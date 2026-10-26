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
    private MusicChange musicChange;
    private bool isProcessingAnswer = false;

    protected override void Start()
    {
        base.Start();
        musicChange = GetComponent<MusicChange>();
        if (ButtonCanvas != null)
        {
            ButtonCanvas.gameObject.SetActive(false);
        }
        if (textPanel != null)
        {
            textPanel.gameObject.SetActive(false);
        }
        SetOptionButtonsInteractable(false);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && !isMinigameActive && !IsAnyMinigameActive)
        {
            if (acceptRect != null)
            {
                acceptRect.gameObject.SetActive(true);
                UIUtils.SetAlpha(acceptRect.gameObject, 1f);
            }
            if (textPanel != null)
            {
                textPanel.gameObject.SetActive(false);
            }
            if (ButtonCanvas != null)
            {
                ButtonCanvas.gameObject.SetActive(false);
            }
            currentPhase = 0;
            isProcessingAnswer = false;
        }
        base.OnTriggerEnter2D(other);
    }

    public void StartGame()
    {
        StartMinigame();
        currentPhase = 0;
        isProcessingAnswer = false;

        if (StartPanel != null)
        {
            StartPanel.gameObject.SetActive(true);
            UIUtils.SetAlpha(StartPanel.gameObject, 1f);
        }

        if (acceptRect != null)
        {
            acceptRect.gameObject.SetActive(false);
        }

        if (textPanel != null)
        {
            textPanel.gameObject.SetActive(true);
            UIUtils.SetAlpha(textPanel.gameObject, 1f);
        }

        if (ButtonCanvas != null)
        {
            ButtonCanvas.gameObject.SetActive(false);
        }

        SetOptionButtonsInteractable(false);

        if (musicChange != null)
        {
            musicChange.ToggleMusic();
            musicChange.PlayThemeMusic();
        }

        StartPhase();
    }

    private void StartPhase()
    {
        if (phaseTexts == null || currentPhase >= phaseTexts.Length)
        {
            HandleEnding();
            return;
        }

        isProcessingAnswer = false;

        if (ButtonCanvas != null)
        {
            ButtonCanvas.gameObject.SetActive(false);
        }

        SetOptionButtonsInteractable(false);

        if (textPanel != null)
        {
            textPanel.gameObject.SetActive(true);
            UIUtils.SetAlpha(textPanel.gameObject, 1f);
        }

        if (typewriter != null)
        {
            typewriter.OnTypingFinished -= OnLectureFinished;
            typewriter.OnTypingFinished += OnLectureFinished;
            typewriter.SetText(phaseTexts[currentPhase]);
            var dfr = typewriter.GetComponent<DynamicFontResizer>();
            if (dfr != null)
            {
                dfr.AdjustFontSize();
            }
            if (!typewriter.typeWriterEffect)
            {
                OnLectureFinished();
            }
        }
        else
        {
            if (dialogueText != null)
            {
                dialogueText.text = phaseTexts[currentPhase];
            }
            OnLectureFinished();
        }
    }

    private void OnLectureFinished()
    {
        if (typewriter != null)
        {
            typewriter.OnTypingFinished -= OnLectureFinished;
        }
        ShowOptions();
    }

    private void ShowOptions()
    {
        if (ButtonCanvas != null)
        {
            var panelImg = ButtonCanvas.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.raycastTarget = false;
            }
            UIAnimationManager.Instance.ShowWindow(ButtonCanvas, 0.3f);
            var cg = ButtonCanvas.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.blocksRaycasts = true;
                cg.interactable = true;
            }
        }

        if (phaseOptions != null && currentPhase >= 0 && currentPhase < phaseOptions.Length && phaseOptions[currentPhase].options != null)
        {
            string[] currentOpts = phaseOptions[currentPhase].options;
            if (optionButtons != null)
            {
                for (int i = 0; i < optionButtons.Length; i++)
                {
                    if (optionButtons[i] == null) continue;
                    int index = i;
                    optionButtons[i].onClick.RemoveAllListeners();
                    optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
                    optionButtons[i].interactable = true;

                    var btnGraphic = optionButtons[i].targetGraphic != null ? optionButtons[i].targetGraphic : optionButtons[i].GetComponent<Graphic>();
                    if (btnGraphic != null)
                    {
                        btnGraphic.raycastTarget = true;
                    }

                    Graphic[] childGraphics = optionButtons[i].GetComponentsInChildren<Graphic>(true);
                    for (int g = 0; g < childGraphics.Length; g++)
                    {
                        if (childGraphics[g] != null && childGraphics[g].gameObject != optionButtons[i].gameObject)
                        {
                            childGraphics[g].raycastTarget = false;
                        }
                    }

                    if (i < currentOpts.Length)
                    {
                        string optText = currentOpts[i];
                        var ftw = optionButtons[i].GetComponentInChildren<FarsiTypewriter>(true);
                        if (ftw != null)
                        {
                            ftw.allowClickToSkip = false;
                            ftw.typeWriterEffect = false;
                            ftw.SetText(optText);
                        }
                        else
                        {
                            var txt = optionButtons[i].GetComponentInChildren<Text>(true);
                            if (txt != null)
                            {
                                txt.text = optText;
                            }
                        }

                        var dfr = optionButtons[i].GetComponentInChildren<DynamicFontResizer>(true);
                        if (dfr != null)
                        {
                            dfr.AdjustFontSize();
                        }
                    }
                }
            }
        }
    }

    private void SetOptionButtonsInteractable(bool state)
    {
        if (optionButtons == null) return;
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] != null)
            {
                optionButtons[i].interactable = state;
            }
        }
    }

    private void OnOptionSelected(int index)
    {
        if (isProcessingAnswer) return;
        isProcessingAnswer = true;

        SetOptionButtonsInteractable(false);

        if (ButtonCanvas != null)
        {
            UIAnimationManager.Instance.HideWindow(ButtonCanvas, 0.2f);
        }

        bool isCorrect = (correctOptionIndex != null && currentPhase >= 0 && currentPhase < correctOptionIndex.Length && index == correctOptionIndex[currentPhase]);

        if (!isCorrect)
        {
            StartCoroutine(HandleLooseRoutine());
        }
        else
        {
            currentPhase++;
            StartCoroutine(TransitionToNextPhase());
        }
    }

    private IEnumerator TransitionToNextPhase()
    {
        yield return new WaitForSeconds(0.25f);
        StartPhase();
    }

    private IEnumerator HandleLooseRoutine()
    {
        yield return new WaitForSeconds(0.25f);
        HandleLoose();
    }

    private void HandleLoose()
    {
        if (typewriter != null)
        {
            typewriter.OnTypingFinished -= OnLectureFinished;
            typewriter.StopTyping();
        }

        if (ButtonCanvas != null)
        {
            ButtonCanvas.gameObject.SetActive(false);
        }

        if (textPanel != null)
        {
            UIAnimationManager.Instance.HideWindow(textPanel, 0.2f);
        }

        if (StartPanel != null)
        {
            UIAnimationManager.Instance.HideWindow(StartPanel, 0.3f);
        }

        if (acceptRect != null)
        {
            acceptRect.gameObject.SetActive(true);
        }

        FailMission();
    }

    private void HandleEnding()
    {
        if (typewriter != null)
        {
            typewriter.OnTypingFinished -= OnLectureFinished;
            typewriter.StopTyping();
        }

        if (ButtonCanvas != null)
        {
            ButtonCanvas.gameObject.SetActive(false);
        }

        if (textPanel != null)
        {
            UIAnimationManager.Instance.HideWindow(textPanel, 0.2f);
        }

        if (StartPanel != null)
        {
            UIAnimationManager.Instance.HideWindow(StartPanel, 0.3f);
        }

        if (acceptRect != null)
        {
            acceptRect.gameObject.SetActive(true);
        }

        CompleteMission();
    }

    private void OnDestroy()
    {
        if (typewriter != null)
        {
            typewriter.OnTypingFinished -= OnLectureFinished;
        }
    }
}