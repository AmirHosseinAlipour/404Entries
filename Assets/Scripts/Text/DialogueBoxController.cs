using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBoxController : MonoBehaviour
{
    public static DialogueBoxController Instance { get; private set; }

    [Header("UI Elements")]
    public RectTransform dialoguePanel;
    public Image speakerAvatarImage;
    public Text speakerNameText;
    public Text dialogueText;
    public FarsiTypewriter typewriter;
    public Button nextButton;

    private List<DialogueLine> _currentLines;
    private int _currentIndex = 0;
    private Action _onCompleteCallback;
    private bool _isActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextClicked);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.gameObject.SetActive(false);
        }

        if (dialogueText != null)
        {
            dialogueText.color = Color.white;
            Shadow s = dialogueText.GetComponent<Shadow>();
            if (s == null)
            {
                s = dialogueText.gameObject.AddComponent<Shadow>();
            }
            s.effectColor = new Color(0f, 0f, 0f, 0.85f);
            s.effectDistance = new Vector2(1.5f, -1.5f);
        }
    }

    private void OnEnable()
    {
        InputReader.OnSubmitPressed += HandleSubmit;
    }

    private void OnDisable()
    {
        InputReader.OnSubmitPressed -= HandleSubmit;
    }

    private void HandleSubmit()
    {
        if (_isActive)
        {
            OnNextClicked();
        }
    }

    public void StartDialogue(DialogueData data, Action onComplete = null)
    {
        if (data == null || data.lines == null || data.lines.Count == 0) return;
        StartDialogue(data.lines, onComplete);
    }

    public void StartDialogue(List<DialogueLine> lines, Action onComplete = null)
    {
        if (lines == null || lines.Count == 0) return;

        _currentLines = lines;
        _currentIndex = 0;
        _onCompleteCallback = onComplete;
        _isActive = true;

        PlayerUIModeHelper.PlayerEnterUIMode();

        if (dialoguePanel != null)
        {
            UIAnimationManager.Instance.ShowWindow(dialoguePanel, 0.25f);
        }

        DisplayLine(_currentIndex);
    }

    private void DisplayLine(int index)
    {
        if (_currentLines == null || index < 0 || index >= _currentLines.Count) return;

        DialogueLine line = _currentLines[index];
        if (line == null) return;

        if (speakerNameText != null)
        {
            speakerNameText.text = LocalizationManager.Translate(line.speakerName);
        }

        if (speakerAvatarImage != null)
        {
            if (line.speakerAvatar != null)
            {
                speakerAvatarImage.gameObject.SetActive(true);
                speakerAvatarImage.sprite = line.speakerAvatar;
            }
            else
            {
                speakerAvatarImage.gameObject.SetActive(false);
            }
        }

        if (typewriter != null)
        {
            typewriter.SetText(line.text);
            typewriter.StartTyping();
        }
        else if (dialogueText != null)
        {
            dialogueText.text = LocalizationManager.Translate(line.text);
        }
    }

    public void OnNextClicked()
    {
        if (!_isActive) return;

        if (typewriter != null && typewriter.isTyping)
        {
            typewriter.SkipTyping();
            return;
        }

        _currentIndex++;
        if (_currentLines != null && _currentIndex < _currentLines.Count)
        {
            DisplayLine(_currentIndex);
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        _isActive = false;

        if (dialoguePanel != null)
        {
            UIAnimationManager.Instance.HideWindow(dialoguePanel, 0.2f);
        }

        PlayerUIModeHelper.PlayerExitUIMode();

        Action callback = _onCompleteCallback;
        _onCompleteCallback = null;
        callback?.Invoke();
    }
}
