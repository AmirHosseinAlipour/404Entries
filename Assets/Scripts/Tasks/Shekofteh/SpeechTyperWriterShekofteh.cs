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
    public float pageClearPaddingPx = 150;

    public event Action OnTypingFinished;

    private Text _textUI;
    private string _originalText = "";
    private Coroutine _typingCo;

    private TextGenerationSettings _settings;
    private float _maxWidthPx;
    private float _boxHeightPx;

    private readonly List<string> _lines = new List<string>();
    private string _currentLine = "";
    private string _currentWord = "";
    private bool _spaceBeforeWord = false;

    private void Awake()
    {
        _textUI = GetComponent<Text>();
        if (_textUI != null)
        {
            _textUI.horizontalOverflow = HorizontalWrapMode.Overflow;
            _textUI.verticalOverflow = VerticalWrapMode.Overflow;

            if (string.IsNullOrEmpty(_originalText) && !string.IsNullOrEmpty(_textUI.text))
            {
                _originalText = _textUI.text;
            }
            _textUI.text = "";
        }
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += HandleLanguageChanged;
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= HandleLanguageChanged;
    }

    private void HandleLanguageChanged(GameLanguage newLang)
    {
        if (_typingCo != null)
        {
            StartTyping();
        }
        else
        {
            ApplyAlignment();
        }
    }

    private void ApplyAlignment()
    {
        if (_textUI != null)
        {
            _textUI.alignment = LocalizationManager.IsRTL ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
        }
    }

    private void Start()
    {
        UpdateSettings();
        ApplyAlignment();

        if (!typeWriterEffect)
        {
            string rawWrapped = ManualWrapWholeText(LocalizationManager.Translate(_originalText));
            _textUI.text = FixLineByLine(rawWrapped);
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
        UpdateSettings();
        ApplyAlignment();

        if (string.IsNullOrEmpty(_originalText))
        {
            _originalText = "بسم الله الرحمن الرحیم،\nضمن تبریک برای قبولی داخل دانشگاه بهشتی، این دانشگاه یکی از بهترین دانشگاه های سراسر کشور است که به تازگی از رتبه 500 به رتبه 447 صعود کرده، نکته ای که باید بهتون بگم: اینه که شما باید به چارت توجه کنید، خروج از چارت، خط قرمز ماست، شما باید هر طور شده در ۸ ترم دانشگاه رو تموم کنین شاید بپرسین چطوری؟ باید بگم ممنونم شما چطورین. اگر ۷ ترمه تموم کنین که چه بهتر. البته بعضی از دانشجویان هم هستن که میرن کار های متفرقه انجام می دن یا سر کار میرن ما این افراد رو کارشون رو تایید نمی کنیم و بعضا این افراد اصلا میرن سر کار و ۹ ترمه دانشگاه رو تموم میکنن باز هم میگم این افراد مورد تایید ما نیستند و ما تاییدشون نمی کنیم\n موضوع دیگه که باید بهش بپردازیم نمره هست. ببینید دانشگاه روی نمره خیلی حساسه و نمره براش اهمیت داره، سعی کنین نمرتون رو بالا نگه دارید این بعدا در دانشگاه و حتی کار کردن خیلی قراره تاثیر بذاره حالا درسته که هیچ جا به معدل دانشگاه نگاه دیگه نمی کنن و حتی برای اپلای هم مقاله داشتن مهم تر از نمره هست ولی خب تمام اینها دلیل نمیشه که شما نمرتون رو پایین نگه دارید.\nنکته دیگه که باید بهش بپردازیم در مورد زوج و فرد بودن هست ببینید دانشگاه برای آسانی مدیریت ورودی، شما رو به ۲ دسته تقسیم کردند: یک دسته گروه زوج و یک دسته گروه فرد. شما باید بدونید که توی کدوم دسته هستید و طبق اون عمل کنید. حالا اینکه چطور بفهمید کدوم هستید، به شماره دانشجوییتون بستگی داره. اگر رقم آخر شماره دانشجوییتون زوج باشه، شما در گروه زوج قرار میگیرید و اگر فرد باشه در گروه فرد. این موضوع در انتخاب واحد و زمان بندی کلاس ها خیلی مهمه و باید حتما بهش دقت داشته باشید.\nموضوع بعدی در مورد خوابگاه هست. دانشجویانی که از شهرستان میان باید حتما در مهلت مقرر برای خوابگاه ثبت نام کنند. خوابگاه های دانشگاه امکانات مناسبی دارند ولی ظرفیت محدوده، پس اولویت با کسانی هست که زودتر اقدام کنند و مدارکشون کامل باشه.\nدر نهایت امیدوارم دوران تحصیلی موفقی داشته باشید و بتونید از امکانات دانشگاه به بهترین نحو استفاده کنید.";
        }

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

    private void UpdateSettings()
    {
        if (_textUI == null) _textUI = GetComponent<Text>();
        if (_textUI == null) return;

        _textUI.horizontalOverflow = HorizontalWrapMode.Overflow;
        _textUI.verticalOverflow = VerticalWrapMode.Overflow;

        Vector2 size = _textUI.rectTransform.rect.size;
        if (size.x < 50f || size.y < 50f)
        {
            RectTransform parentRect = _textUI.rectTransform.parent as RectTransform;
            if (parentRect != null && parentRect.rect.width > 50f)
            {
                size = parentRect.rect.size;
            }
            else
            {
                size = new Vector2(1000f, 600f);
            }
        }

        _settings = _textUI.GetGenerationSettings(size);
        _maxWidthPx = size.x;
        _boxHeightPx = size.y;

        if (_textUI.lineSpacing <= 0.05f)
        {
            _textUI.lineSpacing = 1f;
        }
    }

    private IEnumerator TypeTextCoroutine()
    {
        string textToType = LocalizationManager.Translate(_originalText);
        if (!LocalizationManager.IsRTL)
        {
            int origLines = _originalText.Split('\n').Length;
            if (origLines > 1 && !textToType.Contains("\n"))
            {
                textToType = BalanceTextLines(textToType, origLines);
            }
        }
        if (string.IsNullOrEmpty(textToType))
            yield break;

        for (int i = 0; i < textToType.Length; i++)
        {
            char c = textToType[i];

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
                {
                    BreakVeryLongWordCharByCharWithPaging();
                }
            }

            RenderNow();
            yield return new WaitForSeconds(typeCharTime);
        }

        FlushWordIntoLine();
        RenderNow();

        _typingCo = null;
        OnTypingFinished?.Invoke();
    }

    private void FlushWordIntoLine()
    {
        if (string.IsNullOrEmpty(_currentWord))
            return;

        string prefixSpace = (_spaceBeforeWord && _currentLine.Length > 0) ? " " : "";
        string candidateLine = _currentLine + prefixSpace + _currentWord;

        if (LineExceedsWidth(candidateLine))
        {
            if (_currentLine.Length > 0)
            {
                PushLineWithPaging();
                _currentLine = _currentWord;
            }
            else
            {
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

    private void PushLineWithPaging()
    {
        if (!newPageOnBottom)
        {
            _lines.Add(_currentLine);
            _currentLine = "";
            return;
        }

        if (WouldOverflowIfAddLine(_currentLine))
        {
            _lines.Clear();
        }

        _lines.Add(_currentLine);
        _currentLine = "";
    }

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

    private bool LineExceedsWidth(string rawLine)
    {
        string fixedLine = LocalizationManager.IsRTL ? ArabicFixer.Fix(rawLine, true, true) : rawLine;
        float w = _textUI.cachedTextGeneratorForLayout.GetPreferredWidth(fixedLine, _settings);
        return w > (_maxWidthPx - 4f);
    }

    private bool WordAloneExceedsWidth(string rawWord)
    {
        string fixedWord = LocalizationManager.IsRTL ? ArabicFixer.Fix(rawWord, true, true) : rawWord;
        float w = _textUI.cachedTextGeneratorForLayout.GetPreferredWidth(fixedWord, _settings);
        return w > (_maxWidthPx - 4f);
    }

    private bool WouldOverflowIfAddLine(string newLineRaw)
    {
        string raw;

        if (_lines.Count == 0)
            raw = newLineRaw;
        else
            raw = string.Join("\n", _lines) + "\n" + newLineRaw;

        string fixedText = FixLineByLine(raw);
        float h = _textUI.cachedTextGeneratorForLayout.GetPreferredHeight(fixedText, _settings);
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
        if (!LocalizationManager.IsRTL) return rawText ?? "";
        string[] lines = (rawText ?? "").Split('\n');
        for (int i = 0; i < lines.Length; i++)
            lines[i] = ArabicFixer.Fix(lines[i], true, true);

        return string.Join("\n", lines);
    }

    private string ManualWrapWholeText(string rawText)
    {
        if (string.IsNullOrEmpty(rawText)) return "";

        List<string> resultLines = new List<string>();
        string currentL = "";
        string currentW = "";
        bool spaceBefore = false;

        for (int i = 0; i < rawText.Length; i++)
        {
            char c = rawText[i];

            if (c == '\n')
            {
                if (!string.IsNullOrEmpty(currentW))
                {
                    string pref = (spaceBefore && currentL.Length > 0) ? " " : "";
                    currentL += pref + currentW;
                    currentW = "";
                }
                resultLines.Add(currentL);
                currentL = "";
                spaceBefore = false;
            }
            else if (c == ' ' || c == '\t')
            {
                if (!string.IsNullOrEmpty(currentW))
                {
                    string pref = (spaceBefore && currentL.Length > 0) ? " " : "";
                    string cand = currentL + pref + currentW;
                    if (LineExceedsWidth(cand))
                    {
                        if (currentL.Length > 0)
                        {
                            resultLines.Add(currentL);
                            currentL = currentW;
                        }
                        else
                        {
                            resultLines.Add(currentW);
                            currentL = "";
                        }
                    }
                    else
                    {
                        currentL = cand;
                    }
                    currentW = "";
                }
                spaceBefore = true;
            }
            else
            {
                currentW += c;
            }
        }

        if (!string.IsNullOrEmpty(currentW))
        {
            string pref = (spaceBefore && currentL.Length > 0) ? " " : "";
            string cand = currentL + pref + currentW;
            if (LineExceedsWidth(cand) && currentL.Length > 0)
            {
                resultLines.Add(currentL);
                currentL = currentW;
            }
            else
            {
                currentL = cand;
            }
        }

        if (!string.IsNullOrEmpty(currentL))
        {
            resultLines.Add(currentL);
        }

        return string.Join("\n", resultLines);
    }

    private string BalanceTextLines(string text, int targetLineCount)
    {
        if (string.IsNullOrEmpty(text) || targetLineCount <= 1) return text;
        string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= targetLineCount) return text;

        int totalChars = text.Length;
        int targetCharsPerLine = Mathf.Max(1, totalChars / targetLineCount);

        List<string> resultLines = new List<string>();
        System.Text.StringBuilder currentLine = new System.Text.StringBuilder();

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
}
