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
            PrepareLtrFixedText(targetText);
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
                _textUI.resizeTextForBestFit = false;
            }

            if (_textUI.lineSpacing <= 0.05f)
            {
                _textUI.lineSpacing = 1.15f;
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

        // Support explicit newlines while trimming excess whitespace
        string normalized = targetText.Replace("\r\n", "\n").Replace('\r', '\n');
        string[] rawParagraphs = normalized.Split('\n');
        List<string> cleanParagraphs = new List<string>();
        foreach (string p in rawParagraphs)
        {
            cleanParagraphs.Add(p.Trim());
        }

        float containerWidth = GetContainerWidth(_textUI);
        float containerHeight = _textUI != null && _textUI.rectTransform != null ? _textUI.rectTransform.rect.height : 0f;

        float maxWidth = containerWidth;
        int baseSz = _baseFontSize > 0 ? _baseFontSize : (_textUI != null ? _textUI.fontSize : 36);
        if (baseSz > 40) baseSz = 36;
        if (baseSz < 26) baseSz = 32;
        if (_textUI != null) _textUI.fontSize = baseSz;
        int minLimit = Mathf.Max(12, baseSz - 12);

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

    private void PrepareLtrFixedText(string targetText)
    {
        if (string.IsNullOrEmpty(targetText))
        {
            _fixedText = "";
            return;
        }

        string normalized = targetText.Replace("\r\n", "\n").Replace('\r', '\n');
        string[] rawParagraphs = normalized.Split('\n');
        List<string> cleanParagraphs = new List<string>();
        foreach (string p in rawParagraphs)
        {
            cleanParagraphs.Add(p.Trim());
        }

        float containerWidth = GetContainerWidth(_textUI);
        float containerHeight = _textUI != null && _textUI.rectTransform != null ? _textUI.rectTransform.rect.height : 0f;

        float maxWidth = containerWidth;
        int baseSz = _baseFontSize > 0 ? _baseFontSize : (_textUI != null ? _textUI.fontSize : 36);
        if (baseSz > 40) baseSz = 36;
        if (baseSz < 26) baseSz = 32;
        if (_textUI != null) _textUI.fontSize = baseSz;
        int minLimit = Mathf.Max(12, baseSz - 12);

        while (true)
        {
            List<string> cleanLines = new List<string>();

            foreach (string para in cleanParagraphs)
            {
                if (string.IsNullOrWhiteSpace(para))
                {
                    cleanLines.Add("");
                    continue;
                }

                List<string> wrapped = WrapParagraph(para, maxWidth, _textUI);
                foreach (string w in wrapped)
                {
                    cleanLines.Add(w);
                }
            }

            string candidate = string.Join("\n", cleanLines);

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
        float safeMaxWidth = Mathf.Max(60f, maxWidth - 15f);

        // If entire paragraph fits safely on one line, return single line
        if (MeasureWidth(paragraph, textUI) <= safeMaxWidth)
        {
            result.Add(paragraph);
            return result;
        }

        List<string> lines = new List<string>();
        List<string> currentWords = new List<string>();

        // Persian dangling conjunctions / prepositions that should not dangle at the end of a line
        HashSet<string> danglingSet = new HashSet<string> { "که", "تا", "از", "به", "با", "در", "را", "یا", "و" };

        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            if (currentWords.Count == 0)
            {
                currentWords.Add(word);
                continue;
            }

            string candidate = string.Join(" ", currentWords) + " " + word;
            float candidateWidth = MeasureWidth(candidate, textUI);

            if (candidateWidth <= safeMaxWidth)
            {
                currentWords.Add(word);

                // Smart break at natural clause/sentence endings (. ? ! : ،)
                char lastChar = word[word.Length - 1];
                bool isMajorPunctuation = (lastChar == '.' || lastChar == '!' || lastChar == '؟' || lastChar == '?' || lastChar == ':');
                int remWords = words.Length - 1 - i;

                if (isMajorPunctuation && remWords >= 2 && (candidateWidth >= safeMaxWidth * 0.35f || currentWords.Count >= 3))
                {
                    lines.Add(string.Join(" ", currentWords));
                    currentWords.Clear();
                }
            }
            else
            {
                // Line overflow: before breaking, check if the last word is a dangling Persian conjunction
                if (currentWords.Count >= 3 && danglingSet.Contains(currentWords[currentWords.Count - 1]))
                {
                    string dangling = currentWords[currentWords.Count - 1];
                    currentWords.RemoveAt(currentWords.Count - 1);
                    lines.Add(string.Join(" ", currentWords));
                    currentWords.Clear();
                    currentWords.Add(dangling);
                    currentWords.Add(word);
                }
                else
                {
                    lines.Add(string.Join(" ", currentWords));
                    currentWords.Clear();
                    currentWords.Add(word);
                }
            }
        }

        if (currentWords.Count > 0)
        {
            lines.Add(string.Join(" ", currentWords));
        }

        // Post-processing: Multi-word Widow / Orphan prevention & Line balancing
        if (lines.Count >= 2)
        {
            string lastLine = lines[lines.Count - 1];
            List<string> lastLineWords = new List<string>(lastLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            if (lastLineWords.Count <= 2)
            {
                string prevLine = lines[lines.Count - 2];
                List<string> prevLineWords = new List<string>(prevLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

                while (lastLineWords.Count < 3 && prevLineWords.Count > 3)
                {
                    string moved = prevLineWords[prevLineWords.Count - 1];
                    prevLineWords.RemoveAt(prevLineWords.Count - 1);
                    lastLineWords.Insert(0, moved);

                    string testLast = string.Join(" ", lastLineWords);
                    string testPrev = string.Join(" ", prevLineWords);

                    if (MeasureWidth(testLast, textUI) <= safeMaxWidth && MeasureWidth(testPrev, textUI) <= safeMaxWidth)
                    {
                        lines[lines.Count - 2] = testPrev;
                        lines[lines.Count - 1] = testLast;
                    }
                    else
                    {
                        // Rollback
                        lastLineWords.RemoveAt(0);
                        prevLineWords.Add(moved);
                        break;
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
            string textToMeasure = LocalizationManager.IsRTL ? ArabicFixer.Fix(rawText, true, true) : rawText;
            TextGenerationSettings settings = textUI.GetGenerationSettings(Vector2.zero);
            float ppu = textUI.pixelsPerUnit > 0.001f ? textUI.pixelsPerUnit : 1f;
            float measured = textUI.cachedTextGeneratorForLayout.GetPreferredWidth(textToMeasure, settings) / ppu;
            if (measured > 1f) return measured;
        }
        catch
        {
        }
        return rawText.Length * (textUI.fontSize * 0.52f);
    }

    public static float GetContainerWidth(Text textUI)
    {
        if (textUI == null || textUI.rectTransform == null) return 1060f;

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
                    return prt.rect.width - 80f;
                }
                if (prt.sizeDelta.x > 20f)
                {
                    return prt.sizeDelta.x - 80f;
                }
            }
            curr = curr.parent;
        }

        return 1060f; // Robust fallback for standard dialogue box (1140 - 80)
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

        if (!string.IsNullOrEmpty(_rawOriginalText))
        {
            FixAndPrepareText(_rawOriginalText);
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