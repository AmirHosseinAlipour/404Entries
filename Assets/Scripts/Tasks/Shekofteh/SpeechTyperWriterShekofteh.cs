using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArabicSupport;

[RequireComponent(typeof(Text))]
public class SpeechTypeWriterShekofteh : MonoBehaviour
{
    [Header("Typewriter Settings")]
    public bool typeWriterEffect = true;
    public float typeCharTime = 0.05f;

    [Header("Paging")]
    public bool newPageOnBottom = true;

    [Header("Paging Tuning")]
    [Tooltip("چند پیکسل از پایین خالی بماند، قبل از اینکه صفحه جدید شود. مقدار بیشتر = زودتر صفحه پاک می‌شود.")]
    public float pageClearPaddingPx = 150;

    public event Action OnTypingFinished;

    private Text _textUI;
    private string _originalText = "";
    private Coroutine _typingCo;

    private TextGenerationSettings _settings;
    private float _maxWidthPx;
    private float _boxHeightPx;

    // Build RAW text (before ArabicFixer)
    private readonly List<string> _lines = new List<string>();
    private string _currentLine = "";
    private string _currentWord = "";
    private bool _spaceBeforeWord = false;

    private void Awake()
    {
        _textUI = GetComponent<Text>();

        // IMPORTANT: disable Unity internal wrapping to avoid reflow
        _textUI.horizontalOverflow = HorizontalWrapMode.Overflow;
        _textUI.verticalOverflow = VerticalWrapMode.Overflow;

        _originalText = _textUI.text ?? "";
        _textUI.text = "";
    }

    private void Start()
    {
        _settings = _textUI.GetGenerationSettings(_textUI.rectTransform.rect.size);

        float scale = 1f;
        if (_textUI.canvas != null) scale = _textUI.canvas.scaleFactor;

        _maxWidthPx = _textUI.rectTransform.rect.width * scale;
        _boxHeightPx = _textUI.rectTransform.rect.height * scale;

        if (!typeWriterEffect)
        {
            string rawWrapped = ManualWrapWholeText(_originalText);
            _textUI.text = FixLineByLine(rawWrapped);
        }
        else
        {
            StartTyping();
        }
    }

    public void SetText(string newRawFarsiText)
    {
        _originalText = newRawFarsiText ?? "";
        StartTyping();
    }

    public void StartTyping()
    {
        StopTyping();

        _textUI.text = "";
        _lines.Clear();
        _currentLine = "";
        _currentWord = "";
        _spaceBeforeWord = false;

        _typingCo = StartCoroutine(TypeTextCoroutine());
    }

    public void StopTyping()
    {
        if (_typingCo != null)
        {
            StopCoroutine(_typingCo);
            _typingCo = null;
        }
    }

    private IEnumerator TypeTextCoroutine()
    {
        if (string.IsNullOrEmpty(_originalText))
            yield break;

        for (int i = 0; i < _originalText.Length; i++)
        {
            char c = _originalText[i];

            if (c == '\n')
            {
                FlushWordIntoLine();
                PushLineWithPaging();
                _spaceBeforeWord = false;
            }
            else if (c == ' ' || c == '\t')
            {
                FlushWordIntoLine();
                _spaceBeforeWord = true;
            }
            else
            {
                _currentWord += c;

                // If word is too long to fit even on an empty line, break it safely
                if (WordAloneExceedsWidth(_currentWord) && _currentLine.Length == 0)
                {
                    BreakVeryLongWordCharByCharWithPaging();
                }
            }

            RenderNow();
            yield return new WaitForSeconds(typeCharTime);
        }

        // finish remaining word
        FlushWordIntoLine();
        RenderNow();

        _typingCo = null;
        OnTypingFinished?.Invoke();
    }

    // -----------------------------
    // Word-wrap + Paging logic
    // -----------------------------

    private void FlushWordIntoLine()
    {
        if (string.IsNullOrEmpty(_currentWord))
            return;

        string prefixSpace = (_spaceBeforeWord && _currentLine.Length > 0) ? " " : "";
        string candidateLine = _currentLine + prefixSpace + _currentWord;

        // If the word doesn't fit in current line, move to next line
        if (LineExceedsWidth(candidateLine))
        {
            if (_currentLine.Length > 0)
            {
                // Before pushing the current line, handle paging
                PushLineWithPaging();

                // Start new line with the word (no leading space)
                _currentLine = _currentWord;
            }
            else
            {
                // Line is empty but word still doesn't fit => break long word
                BreakVeryLongWordCharByCharWithPaging();
                _currentWord = "";
                _spaceBeforeWord = false;
                return;
            }
        }
        else
        {
            _currentLine = candidateLine;
        }

        _currentWord = "";
        _spaceBeforeWord = false;
    }

    /// <summary>
    /// Push currentLine into lines, but BEFORE that, check if adding this line would overflow height.
    /// If yes => clear page first, then add.
    /// </summary>
    private void PushLineWithPaging()
    {
        if (!newPageOnBottom)
        {
            _lines.Add(_currentLine);
            _currentLine = "";
            return;
        }

        // Simulate adding this line and see if it would exceed height
        if (WouldOverflowIfAddLine(_currentLine))
        {
            // New page
            _lines.Clear();
        }

        _lines.Add(_currentLine);
        _currentLine = "";
    }

    /// <summary>
    /// For extremely long words: break char-by-char, pushing lines with paging when needed.
    /// </summary>
    private void BreakVeryLongWordCharByCharWithPaging()
    {
        if (string.IsNullOrEmpty(_currentWord))
            return;

        string temp = "";

        for (int i = 0; i < _currentWord.Length; i++)
        {
            char ch = _currentWord[i];
            string cand = temp + ch;

            if (LineExceedsWidth(cand))
            {
                // before pushing temp, handle paging
                if (newPageOnBottom && WouldOverflowIfAddLine(temp))
                    _lines.Clear();

                _lines.Add(temp);
                temp = "" + ch;
            }
            else
            {
                temp = cand;
            }
        }

        _currentLine = temp;
        _currentWord = "";
        _spaceBeforeWord = false;
    }

    // -----------------------------
    // Measuring & Rendering
    // -----------------------------

    private bool LineExceedsWidth(string rawLine)
    {
        string fixedLine = ArabicFixer.Fix(rawLine, true, true);
        float w = _textUI.cachedTextGeneratorForLayout.GetPreferredWidth(fixedLine, _settings);
        return w > (_maxWidthPx - 4f);
    }

    private bool WordAloneExceedsWidth(string rawWord)
    {
        string fixedWord = ArabicFixer.Fix(rawWord, true, true);
        float w = _textUI.cachedTextGeneratorForLayout.GetPreferredWidth(fixedWord, _settings);
        return w > (_maxWidthPx - 4f);
    }

    /// <summary>
    /// Check if adding a new completed line would make the text exceed the box height.
    /// This is where the "clear before overflow" happens.
    /// </summary>
    private bool WouldOverflowIfAddLine(string newLineRaw)
    {
        string raw;

        if (_lines.Count == 0)
            raw = newLineRaw;
        else
            raw = string.Join("\n", _lines) + "\n" + newLineRaw;

        string fixedText = FixLineByLine(raw);

        float h = _textUI.cachedTextGeneratorForLayout.GetPreferredHeight(fixedText, _settings);

        // ✅ Key tuning line: increase padding => clear page sooner
        return h > (_boxHeightPx - pageClearPaddingPx);
    }

    private void RenderNow()
    {
        string linePlusWord;

        if (string.IsNullOrEmpty(_currentWord))
        {
            linePlusWord = _currentLine;
        }
        else
        {
            string prefixSpace = (_spaceBeforeWord && _currentLine.Length > 0) ? " " : "";
            linePlusWord = _currentLine + prefixSpace + _currentWord;

            // If while typing, the word would overflow width, jump to next line early (word-wrap)
            if (LineExceedsWidth(linePlusWord) && _currentLine.Length > 0)
            {
                PushLineWithPaging();
                linePlusWord = _currentWord;
            }
        }

        string raw;
        if (_lines.Count == 0) raw = linePlusWord;
        else raw = string.Join("\n", _lines) + "\n" + linePlusWord;

        _textUI.text = FixLineByLine(raw);
    }

    private string FixLineByLine(string rawText)
    {
        string[] lines = (rawText ?? "").Split('\n');
        for (int i = 0; i < lines.Length; i++)
            lines[i] = ArabicFixer.Fix(lines[i], true, true);

        return string.Join("\n", lines);
    }

    // Non-typewriter helper
    private string ManualWrapWholeText(string raw)
    {
        _lines.Clear();
        _currentLine = "";
        _currentWord = "";
        _spaceBeforeWord = false;

        for (int i = 0; i < (raw ?? "").Length; i++)
        {
            char c = raw[i];

            if (c == '\n')
            {
                FlushWordIntoLine();
                PushLineWithPaging();
                _spaceBeforeWord = false;
            }
            else if (c == ' ' || c == '\t')
            {
                FlushWordIntoLine();
                _spaceBeforeWord = true;
            }
            else
            {
                _currentWord += c;
                if (WordAloneExceedsWidth(_currentWord) && _currentLine.Length == 0)
                    BreakVeryLongWordCharByCharWithPaging();
            }
        }

        FlushWordIntoLine();

        string finalRaw;
        if (_lines.Count == 0) finalRaw = _currentLine;
        else finalRaw = string.Join("\n", _lines) + (_currentLine.Length > 0 ? "\n" + _currentLine : "");

        _lines.Clear();
        _currentLine = "";
        _currentWord = "";
        _spaceBeforeWord = false;

        return finalRaw;
    }
}
