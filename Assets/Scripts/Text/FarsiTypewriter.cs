using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using ArabicSupport;

[RequireComponent(typeof(Text))]
public class FarsiTypewriter : MonoBehaviour, IPointerClickHandler
{
    [Header("Typewriter Settings")]
    public bool typeWriterEffect;
    public float typeCharTime = 0.05f;
    public bool autoResetText;
    public bool allowClickToSkip = true;
    
    [Header("Alignment Settings")]
    public bool centerAlign;
    public bool middleAlign;
    
    public event Action OnTypingFinished;
    public event Action OnClickWhenComplete;
    
    public bool isTyping { get; private set; }
    public bool isComplete { get; private set; }
    
    private int _startFrame = -1;
    private int _baseFontSize = -1;
    
    private Text _textUI;
    private string _rawOriginalText = "";
    private string _fixedText = "";
    public int len;

    public string GetFixedText()
    {
        return _fixedText;
    }
    
    private StringBuilder _builder = new StringBuilder();
    private TextGenerationSettings _cachedSettings;
    private float _boxHeight;
    private float _counter;
    private bool _canReset;

    public void EnsureTextCaptured()
    {
        if (_textUI == null) _textUI = GetComponent<Text>();
        if (string.IsNullOrEmpty(_rawOriginalText))
        {
            if (_textUI != null && !string.IsNullOrEmpty(_textUI.text))
            {
                _rawOriginalText = _textUI.text;
                FixAndPrepareText(_rawOriginalText);
            }
        }
        else if (string.IsNullOrEmpty(_fixedText))
        {
            FixAndPrepareText(_rawOriginalText);
        }
    }

    public void PrepareForTyping()
    {
        EnsureTextCaptured();
        StopAllCoroutines();
        if (_textUI == null) _textUI = GetComponent<Text>();
        typeWriterEffect = true;
        isTyping = false;
        isComplete = false;
        _builder.Clear();
        _counter = 0;
        if (_textUI != null)
        {
            _textUI.text = "";
        }
    }

    void Awake()
    {
        EnsureTextCaptured();

        // Only disable raycastTarget on direct button text children, never override typeWriterEffect!
        if (transform.parent != null && transform.parent.GetComponent<Button>() != null && !transform.parent.name.ToLower().Contains("person"))
        {
            if (_textUI != null)
            {
                _textUI.raycastTarget = false;
            }
        }

        if (_textUI != null)
        {
            if (!typeWriterEffect)
            {
                ApplyCompletedText();
            }
            else
            {
                _textUI.text = "";
            }
        }
    }

    private void OnEnable()
    {
        EnsureTextCaptured();
        LocalizationManager.OnLanguageChanged += HandleLanguageChanged;
        if (!typeWriterEffect)
        {
            ApplyCompletedText();
        }
        else
        {
            if (!isTyping && !isComplete)
            {
                if (_textUI == null) _textUI = GetComponent<Text>();
                if (_textUI != null)
                {
                    _textUI.text = "";
                }
            }
        }
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= HandleLanguageChanged;
        if (typeWriterEffect)
        {
            isTyping = false;
            isComplete = false;
            StopAllCoroutines();
            if (_textUI != null)
            {
                _textUI.text = "";
            }
        }
    }

    private void HandleLanguageChanged(GameLanguage newLang)
    {
        if (string.IsNullOrEmpty(_rawOriginalText)) return;

        bool wasTyping = isTyping;
        FixAndPrepareText(_rawOriginalText);

        if (wasTyping && typeWriterEffect)
        {
            StartTyping();
        }
        else if (!typeWriterEffect || isComplete)
        {
            ApplyCompletedText();
        }
        else
        {
            if (_textUI != null) _textUI.text = "";
        }
    }

    private void Start()
    {
        EnsureTextCaptured();
        if (_textUI == null) _textUI = GetComponent<Text>();
        if (GetComponentInParent<Button>() != null && _textUI != null)
        {
            _textUI.raycastTarget = false;
        }

        if (!typeWriterEffect)
        {
            ApplyCompletedText();
        }
        else
        {
            if (!isTyping && !isComplete)
            {
                if (_textUI != null) _textUI.text = "";
            }
        }
        
        if (_textUI != null)
        {
            _cachedSettings = _textUI.GetGenerationSettings(_textUI.rectTransform.rect.size);
            _boxHeight = _textUI.rectTransform.rect.height;
        }
    }

    public void SetText(string newRawFarsiText)
    {
        StopAllCoroutines();
        if (_textUI == null) _textUI = GetComponent<Text>();
        _rawOriginalText = newRawFarsiText ?? "";
        FixAndPrepareText(_rawOriginalText);

        isTyping = false;
        isComplete = false;
        if (typeWriterEffect && _textUI != null)
        {
            _textUI.text = "";
        }

        if (typeWriterEffect && gameObject.activeInHierarchy)
        {
            StartTyping();
        }
        else if (!typeWriterEffect)
        {
            ApplyCompletedText();
        }
    }

    private void FixAndPrepareText(string rawText)
    {
        if (_textUI == null) _textUI = GetComponent<Text>();
        if (rawText == null) rawText = "";
        _rawOriginalText = rawText;

        string targetText = LocalizationManager.Translate(rawText);

        if (LocalizationManager.IsRTL)
        {
            string[] lines = targetText.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = ArabicFixer.Fix(lines[i], true, true);
            }
            _fixedText = string.Join("\n", lines);
        }
        else
        {
            int originalLineCount = rawText.Split('\n').Length;
            if (originalLineCount > 1 && !targetText.Contains("\n"))
            {
                _fixedText = BalanceTextLines(targetText, originalLineCount);
            }
            else
            {
                _fixedText = targetText;
            }
        }

        if (_textUI != null)
        {
            if (_baseFontSize <= 0)
            {
                _baseFontSize = _textUI.fontSize;
            }
            else
            {
                _textUI.fontSize = _baseFontSize;
            }

            var dfr = GetComponent<DynamicFontResizer>();
            if (dfr != null)
            {
                _textUI.horizontalOverflow = HorizontalWrapMode.Overflow;
                _textUI.resizeTextForBestFit = false;
                dfr.SetTextAndResize(_fixedText);
            }
            else
            {
                _textUI.horizontalOverflow = HorizontalWrapMode.Wrap;

                // Auto-fit guard: ensure text never spills out of the dialogue box
                if (_textUI.rectTransform != null && !string.IsNullOrEmpty(_fixedText))
                {
                    float containerHeight = _textUI.rectTransform.rect.height;
                    if (containerHeight > 10f)
                    {
                        _textUI.text = _fixedText;
                        int minLimit = Mathf.Max(10, _baseFontSize - 16);
                        while (_textUI.preferredHeight > containerHeight && _textUI.fontSize > minLimit)
                        {
                            _textUI.fontSize -= 2;
                        }
                    }
                }

                if (!LocalizationManager.IsRTL)
                {
                    _textUI.resizeTextForBestFit = true;
                    _textUI.resizeTextMinSize = Mathf.Max(14, (int)(_textUI.fontSize * 0.45f));
                    _textUI.resizeTextMaxSize = _textUI.fontSize;
                }
            }

            if (_textUI.lineSpacing <= 0.05f)
            {
                _textUI.lineSpacing = 1f;
            }
            if (GetComponentInParent<Button>() != null)
            {
                _textUI.raycastTarget = false;
            }
        }

        ApplyAlignment();
        if (_textUI != null) _textUI.text = "";

        len = _fixedText != null ? _fixedText.Length : 0;
        _builder.Clear();
        _counter = 0;
        isComplete = false;
    }

    private string BalanceTextLines(string text, int targetLineCount)
    {
        if (string.IsNullOrEmpty(text) || targetLineCount <= 1) return text;
        string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= targetLineCount) return text;

        int totalChars = text.Length;
        int targetCharsPerLine = Mathf.Max(1, totalChars / targetLineCount);

        List<string> resultLines = new List<string>();
        StringBuilder currentLine = new StringBuilder();

        for (int i = 0; i < words.Length; i++)
        {
            if (currentLine.Length > 0 && resultLines.Count < targetLineCount - 1)
            {
                if (currentLine.Length + 1 + words[i].Length > targetCharsPerLine)
                {
                    resultLines.Add(currentLine.ToString());
                    currentLine.Clear();
                }
            }

            if (currentLine.Length > 0) currentLine.Append(" ");
            currentLine.Append(words[i]);
        }

        if (currentLine.Length > 0)
        {
            resultLines.Add(currentLine.ToString());
        }

        return string.Join("\n", resultLines);
    }

    private void ApplyAlignment()
    {
        if (_textUI == null) return;

        if (LocalizationManager.IsRTL)
        {
            if (middleAlign)
                _textUI.alignment = centerAlign ? TextAnchor.MiddleCenter : TextAnchor.MiddleRight;
            else
                _textUI.alignment = centerAlign ? TextAnchor.UpperCenter : TextAnchor.UpperRight;
        }
        else
        {
            if (middleAlign)
                _textUI.alignment = centerAlign ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
            else
                _textUI.alignment = centerAlign ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
        }
    }

    private void ApplyCompletedText()
    {
        if (_textUI != null && !string.IsNullOrEmpty(_fixedText))
        {
            ApplyAlignment();
            _textUI.text = _fixedText;
        }
    }

    private void Update()
    {
        if (!allowClickToSkip) return;
        if (GetComponentInParent<Button>() != null) return;
        if (CompareTag("CurrentTaskButton") || gameObject.name == "CurrentTaskText") return;
        if (Time.frameCount <= _startFrame) return;
        
        bool clicked = false;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            clicked = true;
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            clicked = true;
        else if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            clicked = true;
        
        if (!clicked && Input.GetMouseButtonDown(0))
            clicked = true;

        if (!clicked) return;

        if (isTyping)
        {
            SkipTyping();
        }
        else if (isComplete)
        {
            if (OnClickWhenComplete != null)
            {
                OnClickWhenComplete.Invoke();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (allowClickToSkip && isTyping)
        {
            SkipTyping();
            return;
        }

        Button btn = GetComponentInParent<Button>();
        if (btn != null)
        {
            if (btn.interactable)
            {
                ExecuteEvents.ExecuteHierarchy(btn.gameObject, eventData, ExecuteEvents.pointerClickHandler);
            }
            return;
        }

        if (allowClickToSkip && isComplete && OnClickWhenComplete != null)
        {
            OnClickWhenComplete.Invoke();
        }
    }

    public void StartTyping()
    {
        EnsureTextCaptured();
        StopAllCoroutines();
        if (_textUI == null) _textUI = GetComponent<Text>();
        
        typeWriterEffect = true;
        _builder.Clear();
        isTyping = true;
        isComplete = false;
        _startFrame = Time.frameCount;

        if (_textUI != null && _textUI.rectTransform != null && !string.IsNullOrEmpty(_fixedText))
        {
            float containerHeight = _textUI.rectTransform.rect.height;
            if (containerHeight > 10f)
            {
                int baseSz = _baseFontSize > 0 ? _baseFontSize : _textUI.fontSize;
                _textUI.fontSize = baseSz;
                _textUI.text = _fixedText;
                int minLimit = Mathf.Max(10, baseSz - 16);
                while (_textUI.preferredHeight > containerHeight && _textUI.fontSize > minLimit)
                {
                    _textUI.fontSize -= 2;
                }
            }
        }

        ApplyAlignment();
        if (_textUI != null) _textUI.text = "";

        StartCoroutine(TypeTextCoroutine());
    }
    
    public void StopTyping()
    {
        StopAllCoroutines();
        isTyping = false;
    }

    public void SkipTyping()
    {
        if (!isTyping) return;
        StopAllCoroutines();
        isTyping = false;
        isComplete = true;
        
        ApplyCompletedText();

        OnTypingFinished?.Invoke();
    }

    public void ResetText()
    {
        _canReset = true;
    }

    private IEnumerator TypeTextCoroutine()
    {
        if (string.IsNullOrEmpty(_fixedText))
        {
            if (!string.IsNullOrEmpty(_rawOriginalText))
            {
                FixAndPrepareText(_rawOriginalText);
            }
            else if (_textUI != null && !string.IsNullOrEmpty(_textUI.text))
            {
                _rawOriginalText = _textUI.text;
                FixAndPrepareText(_rawOriginalText);
            }
        }

        if (string.IsNullOrEmpty(_fixedText))
        {
            isTyping = false;
            isComplete = true;
            yield break;
        }

        isTyping = true;
        isComplete = false;
        _counter = 0;
        
        ApplyAlignment();
        _textUI.text = "";

        string[] lines = _fixedText.Split('\n');

        if (LocalizationManager.IsRTL)
        {
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string currentLine = lines[lineIndex];
                StringBuilder lineBuilder = new StringBuilder();

                for (int i = currentLine.Length - 1; i >= 0; i--)
                {
                    yield return new WaitForSeconds(typeCharTime);
                    
                    lineBuilder.Insert(0, currentLine[i]);
                    string typedSoFar = "";
                    for (int j = 0; j < lineIndex; j++)
                        typedSoFar += lines[j] + "\n";

                    typedSoFar += lineBuilder.ToString();
                    _textUI.text = typedSoFar;
                    _counter++;
                }

                yield return new WaitForSeconds(typeCharTime * 2f);
            }
        }
        else
        {
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string currentLine = lines[lineIndex];
                StringBuilder lineBuilder = new StringBuilder();

                for (int i = 0; i < currentLine.Length; i++)
                {
                    yield return new WaitForSeconds(typeCharTime);

                    lineBuilder.Append(currentLine[i]);
                    string typedSoFar = "";
                    for (int j = 0; j < lineIndex; j++)
                        typedSoFar += lines[j] + "\n";

                    typedSoFar += lineBuilder.ToString();
                    _textUI.text = typedSoFar;
                    _counter++;
                }

                yield return new WaitForSeconds(typeCharTime * 2f);
            }
        }
        
        isTyping = false;
        isComplete = true;
        OnTypingFinished?.Invoke();
    }

    private bool CheckVerticalOverflow(TextGenerationSettings settings, float boxHeight)
    {
        float preferredHeight = _textUI.cachedTextGenerator.GetPreferredHeight(_textUI.text, settings);
        return preferredHeight > boxHeight;
    }
}