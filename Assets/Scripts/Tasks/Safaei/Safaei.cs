using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using DG.Tweening;

public class OptionButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Vector3 originalScale = Vector3.one;
    private Button button;

    private void Awake()
    {
        originalScale = transform.localScale;
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && (!button.interactable || !button.enabled)) return;
        transform.DOKill();
        transform.DOScale(originalScale * 1.05f, 0.15f).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalScale, 0.15f).SetUpdate(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && (!button.interactable || !button.enabled)) return;
        transform.DOKill();
        transform.DOScale(originalScale * 0.95f, 0.1f).SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button != null && (!button.interactable || !button.enabled)) return;
        transform.DOKill();
        transform.DOScale(originalScale * 1.05f, 0.1f).SetUpdate(true);
    }

    private void OnDisable()
    {
        transform.DOKill();
        transform.localScale = originalScale;
    }
}

[System.Serializable]
public class PhaseOptions
{
    [TextArea(3, 10)]
    public string[] options = new string[3];
}

public class Safaei : BaseProfessors
{
    public override ProfessorUIType UIType => ProfessorUIType.ScreenSpaceUI;

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
        HideMinigameUI();
        ApplyModernVisualStyles();
    }

    public override void HideMinigameUI()
    {
        base.HideMinigameUI();
        if (ButtonCanvas != null) ButtonCanvas.gameObject.SetActive(false);
        if (textPanel != null) textPanel.gameObject.SetActive(false);
        SetOptionButtonsInteractable(false);
    }

    public override void AcceptChallenge()
    {
        StartGame();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && !isMinigameActive && !IsAnyMinigameActive)
        {
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
        ApplyModernVisualStyles();

        if (ButtonCanvas != null)
        {
            var panelImg = ButtonCanvas.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.raycastTarget = false;
            }
            UIAnimationManager.Instance.ShowWindow(ButtonCanvas, 0.25f);
            var cg = ButtonCanvas.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.blocksRaycasts = true;
                cg.interactable = true;
                cg.alpha = 1f;
            }

            if (optionButtons == null || optionButtons.Length == 0 || optionButtons[0] == null)
            {
                optionButtons = ButtonCanvas.GetComponentsInChildren<Button>(true);
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
                    Button btn = optionButtons[i];

                    btn.gameObject.SetActive(true);
                    btn.enabled = true;
                    btn.interactable = true;

                    Image img = btn.GetComponent<Image>();
                    if (img != null)
                    {
                        img.enabled = true;
                        img.raycastTarget = true;
                    }

                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnOptionSelected(index));

                    Graphic[] childGraphics = btn.GetComponentsInChildren<Graphic>(true);
                    for (int g = 0; g < childGraphics.Length; g++)
                    {
                        if (childGraphics[g] != null && childGraphics[g].gameObject != btn.gameObject)
                        {
                            childGraphics[g].raycastTarget = false;
                        }
                    }

                    if (i < currentOpts.Length)
                    {
                        string optText = currentOpts[i];
                        var ftw = btn.GetComponentInChildren<FarsiTypewriter>(true);
                        if (ftw != null)
                        {
                            ftw.allowClickToSkip = false;
                            ftw.typeWriterEffect = false;
                            ftw.SetText(optText);
                        }
                        else
                        {
                            var txt = btn.GetComponentInChildren<Text>(true);
                            if (txt != null)
                            {
                                txt.text = optText;
                            }
                        }

                        var dfr = btn.GetComponentInChildren<DynamicFontResizer>(true);
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

    public override void ApplyModernVisualStyles()
    {
        base.ApplyModernVisualStyles();

        Sprite cardPanel = Resources.Load<Sprite>("UI/dialogue_card_panel");
        Sprite btnNormal = Resources.Load<Sprite>("UI/dialogue_button_normal");
        Sprite btnHover = Resources.Load<Sprite>("UI/dialogue_button_hover");
        Sprite btnPressed = Resources.Load<Sprite>("UI/dialogue_button_pressed");

        if (textPanel != null && cardPanel != null)
        {
            Image panelImg = textPanel.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.sprite = cardPanel;
                panelImg.type = Image.Type.Sliced;
                panelImg.color = Color.white;
            }
        }

        if (dialogueText != null)
        {
            dialogueText.color = Color.white;
            Shadow shadow = dialogueText.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = dialogueText.gameObject.AddComponent<Shadow>();
            }
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
        }

        if (ButtonCanvas != null)
        {
            var panelImg = ButtonCanvas.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.raycastTarget = false;
            }

            if (optionButtons == null || optionButtons.Length == 0 || optionButtons[0] == null)
            {
                optionButtons = ButtonCanvas.GetComponentsInChildren<Button>(true);
            }
        }

        if (optionButtons != null)
        {
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] == null) continue;
                Button btn = optionButtons[i];
                btn.gameObject.SetActive(true);
                btn.enabled = true;
                btn.interactable = true;

                Image img = btn.GetComponent<Image>();
                if (img == null)
                {
                    img = btn.gameObject.AddComponent<Image>();
                }
                img.enabled = true;
                img.raycastTarget = true;
                img.material = null;
                img.color = Color.white;
                if (btnNormal != null)
                {
                    img.sprite = btnNormal;
                    img.type = Image.Type.Sliced;
                }

                MonoBehaviour[] monos = btn.GetComponents<MonoBehaviour>();
                for (int m = 0; m < monos.Length; m++)
                {
                    if (monos[m] != null && monos[m].GetType().Name.Contains("RoundedCorners"))
                    {
                        monos[m].enabled = false;
                    }
                }

                btn.targetGraphic = img;

                if (btnHover != null && btnPressed != null)
                {
                    btn.transition = Selectable.Transition.SpriteSwap;
                    SpriteState ss = btn.spriteState;
                    ss.highlightedSprite = btnHover;
                    ss.pressedSprite = btnPressed;
                    ss.selectedSprite = btnHover;
                    ss.disabledSprite = btnNormal != null ? btnNormal : (img != null ? img.sprite : null);
                    btn.spriteState = ss;
                }
                else
                {
                    btn.transition = Selectable.Transition.ColorTint;
                    ColorBlock cb = btn.colors;
                    cb.normalColor = Color.white;
                    cb.highlightedColor = new Color(0.85f, 0.95f, 1f, 1f);
                    cb.pressedColor = new Color(0.7f, 0.85f, 1f, 1f);
                    cb.selectedColor = cb.highlightedColor;
                    btn.colors = cb;
                }

                OptionButtonHover hover = btn.GetComponent<OptionButtonHover>();
                if (hover == null)
                {
                    hover = btn.gameObject.AddComponent<OptionButtonHover>();
                }

                Graphic[] childGraphics = btn.GetComponentsInChildren<Graphic>(true);
                for (int g = 0; g < childGraphics.Length; g++)
                {
                    if (childGraphics[g] != null && childGraphics[g].gameObject != btn.gameObject)
                    {
                        childGraphics[g].raycastTarget = false;
                    }
                }

                Text btnTxt = btn.GetComponentInChildren<Text>(true);
                if (btnTxt != null)
                {
                    btnTxt.color = Color.white;
                    Shadow shadow = btnTxt.GetComponent<Shadow>();
                    if (shadow == null)
                    {
                        shadow = btnTxt.gameObject.AddComponent<Shadow>();
                    }
                    shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
                    shadow.effectDistance = new Vector2(1.2f, -1.2f);
                }

                var dfr = btn.GetComponentInChildren<DynamicFontResizer>(true);
                if (dfr != null)
                {
                    dfr.minFontSize = 18;
                    dfr.maxFontSize = 26;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (typewriter != null)
        {
            typewriter.OnTypingFinished -= OnLectureFinished;
        }
    }
}