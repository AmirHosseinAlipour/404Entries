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
    public float typeCharTime = 0.035f;
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
        if (Mathf.Approximately(typeCharTime, 0.05f))
        {
            typeCharTime = 0.035f;
        }

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
        }

        if (LocalizationManager.IsRTL)
        {
            PrepareRtlFixedText(targetText);
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

    private void PrepareRtlFixedText(string targetText)
    {
        if (string.IsNullOrEmpty(targetText))
        {
            _fixedText = "";
            return;
        }

        // Normalize awkward single newlines into spaces so text flows naturally across the dialogue box width
        // Preserves double newlines (paragraphs) if any
        string normalized = targetText.Replace("\r\n", "\n").Replace('\r', '\n');
        string[] rawParagraphs = normalized.Split(new[] { "\n\n" }, StringSplitOptions.None);
        List<string> cleanParagraphs = new List<string>();
        foreach (string p in rawParagraphs)
        {
            cleanParagraphs.Add(p.Replace('\n', ' ').Trim());
        }

        float containerWidth = GetContainerWidth(_textUI);
        float containerHeight = _textUI != null && _textUI.rectTransform != null ? _textUI.rectTransform.rect.height : 0f;

        float maxWidth = containerWidth;
        int baseSz = _baseFontSize > 0 ? _baseFontSize : (_textUI != null ? _textUI.fontSize : 24);
        if (baseSz > 48) baseSz = 46;
        if (_textUI != null) _textUI.fontSize = baseSz;
        int minLimit = Mathf.Max(12, baseSz - 16);

        while (true)
        {
            List<string> fixedLines = new List<string>();

            foreach (string para in cleanParagraphs)
            {
                if (string.IsNullOrWhiteSpace(para))
                {
                    fixedLines.Add("");
                    continue;
                }

                List<string> wrapped = WrapParagraph(para, maxWidth, _textUI);
                foreach (string w in wrapped)
                {
                    fixedLines.Add(ArabicFixer.Fix(w, true, true));
                }
            }

            string candidate = string.Join("\n", fixedLines);

            if (_textUI != null && containerHeight > 10f && _textUI.fontSize > minLimit)
            {
                _textUI.text = candidate;
                if (_textUI.preferredHeight > containerHeight)
                {
                    _textUI.fontSize -= 2;
                    continue;
                }
            }

            _fixedText = candidate;
            break;
        }
    }

    public static string FixRTL(string rawText, Text textUI)
    {
        if (string.IsNullOrEmpty(rawText)) return "";
        if (textUI == null) return ArabicFixer.Fix(rawText, true, true);

        string normalized = rawText.Replace("\r\n", "\n").Replace('\r', '\n');
        string[] rawParagraphs = normalized.Split(new[] { "\n\n" }, StringSplitOptions.None);
        List<string> cleanParagraphs = new List<string>();
        foreach (string p in rawParagraphs)
        {
            cleanParagraphs.Add(p.Replace('\n', ' ').Trim());
        }

        float containerWidth = GetContainerWidth(textUI);
        float maxWidth = containerWidth;
        List<string> finalFixedLines = new List<string>();

        foreach (string para in cleanParagraphs)
        {
            if (string.IsNullOrWhiteSpace(para))
            {
                finalFixedLines.Add("");
                continue;
            }

            List<string> wrapped = WrapParagraph(para, maxWidth, textUI);
            foreach (string w in wrapped)
            {
                finalFixedLines.Add(ArabicFixer.Fix(w, true, true));
            }
        }

        return string.Join("\n", finalFixedLines);
    }

    public static List<string> WrapParagraph(string paragraph, float maxWidth, Text textUI)
    {
        List<string> result = new List<string>();
        if (string.IsNullOrEmpty(paragraph)) return result;

        string[] words = paragraph.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return result;

        // Effective width inside dialogue box with safe inner breathing room
        float safeMaxWidth = Mathf.Max(60f, maxWidth - 20f);

        // If entire paragraph fits safely on one line, return single line
        if (MeasureWidth(paragraph, textUI) <= safeMaxWidth)
        {
            result.Add(paragraph);
            return result;
        }

        List<string> lines = new List<string>();
        StringBuilder currentLine = new StringBuilder();

        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            if (currentLine.Length == 0)
            {
                currentLine.Append(word);
                continue;
            }

            string candidate = currentLine.ToString() + " " + word;
            float candidateWidth = MeasureWidth(candidate, textUI);

            if (candidateWidth <= safeMaxWidth)
            {
                currentLine.Append(" ").Append(word);

                // Smart break at natural clause/sentence endings (. ? ! : ،)
                // If line is >= 55% full and ends with punctuation, and at least 2 words remain:
                char lastChar = word[word.Length - 1];
                bool isPunctuation = (lastChar == '.' || lastChar == '!' || lastChar == '؟' || lastChar == '?' || lastChar == ':' || lastChar == '،' || lastChar == ',');
                if (isPunctuation && candidateWidth >= safeMaxWidth * 0.55f && (words.Length - 1 - i) >= 2)
                {
                    lines.Add(currentLine.ToString().Trim());
                    currentLine.Clear();
                }
            }
            else
            {
                lines.Add(currentLine.ToString().Trim());
                currentLine.Clear();
                currentLine.Append(word);
            }
        }

        if (currentLine.Length > 0)
        {
            lines.Add(currentLine.ToString().Trim());
        }

        // Post-processing: Widow / Orphan prevention!
        // If the last line has only 1 word, and the previous line has at least 3 words:
        if (lines.Count >= 2)
        {
            string lastLine = lines[lines.Count - 1];
            string[] lastLineWords = lastLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (lastLineWords.Length == 1)
            {
                string prevLine = lines[lines.Count - 2];
                string[] prevLineWords = prevLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (prevLineWords.Length >= 3)
                {
                    string movedWord = prevLineWords[prevLineWords.Length - 1];
                    string newPrevLine = string.Join(" ", prevLineWords, 0, prevLineWords.Length - 1);
                    string newLastLine = movedWord + " " + lastLine;

                    if (MeasureWidth(newLastLine, textUI) <= safeMaxWidth)
                    {
                        lines[lines.Count - 2] = newPrevLine;
                        lines[lines.Count - 1] = newLastLine;
                    }
                }
            }
        }

        return lines;
    }

    public static float MeasureWidth(string rawText, Text textUI)
    {
        if (textUI == null || textUI.font == null || string.IsNullOrEmpty(rawText)) return 0f;
        try
        {
            string fixedText = ArabicFixer.Fix(rawText, true, true);
            TextGenerationSettings settings = textUI.GetGenerationSettings(Vector2.zero);
            float ppu = textUI.pixelsPerUnit > 0.001f ? textUI.pixelsPerUnit : 1f;
            float measured = textUI.cachedTextGeneratorForLayout.GetPreferredWidth(fixedText, settings) / ppu;
            if (measured > 1f) return measured;
        }
        catch
        {
        }
        return rawText.Length * (textUI.fontSize * 0.52f);
    }

    public static float GetContainerWidth(Text textUI)
    {
        if (textUI == null || textUI.rectTransform == null) return 1020f;

        // Force canvas layout update if not yet evaluated
        if (textUI.rectTransform.rect.width <= 1f)
        {
            Canvas.ForceUpdateCanvases();
        }

        float w = textUI.rectTransform.rect.width;
        if (w > 20f) return w;

        if (textUI.rectTransform.sizeDelta.x > 20f)
            return textUI.rectTransform.sizeDelta.x;

        // Traverse ancestors to find container width
        Transform curr = textUI.transform.parent;
        while (curr != null)
        {
            RectTransform prt = curr as RectTransform;
            if (prt != null)
            {
                if (prt.rect.width > 20f)
                {
                    return prt.rect.width - 120f;
                }
                if (prt.sizeDelta.x > 20f)
                {
                    return prt.sizeDelta.x - 120f;
                }
            }
            curr = curr.parent;
        }

        return 1020f; // Robust fallback for standard dialogue box (1140 - 120)
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

        if (LocalizationManager.IsRTL && !string.IsNullOrEmpty(_rawOriginalText))
        {
            PrepareRtlFixedText(LocalizationManager.Translate(_rawOriginalText));
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