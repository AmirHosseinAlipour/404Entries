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
    /// <summary>
    /// Fires when user clicks AFTER typing has already finished (use to advance dialogue).
    /// </summary>
    public event Action OnClickWhenComplete;
    
    public bool isTyping { get; private set; }
    /// <summary>
    /// True after typing finishes, reset when new text starts.
    /// </summary>
    public bool isComplete { get; private set; }
    
    private int _startFrame = -1;
    
    private Text _textUI;
    private string _rawOriginalText = "";
    private string _fixedText = "";
    public int len;
    
    private StringBuilder _builder = new StringBuilder();
    private TextGenerationSettings _cachedSettings;
    private float _boxHeight;
    private float _counter;
    private bool _canReset;

    void Awake()
    {
        if (_textUI == null) _textUI = GetComponent<Text>();
        if (_textUI != null)
        {
            _rawOriginalText = _textUI.text ?? "";
            FixAndPrepareText(_rawOriginalText);
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
        if (string.IsNullOrEmpty(_rawOriginalText)) return;

        bool wasTyping = isTyping;
        FixAndPrepareText(_rawOriginalText);

        if (wasTyping && typeWriterEffect)
        {
            StartTyping();
        }
        else
        {
            ApplyCompletedText();
        }
    }

    private void Start()
    {
        if (_textUI == null) _textUI = GetComponent<Text>();

        if (!typeWriterEffect)
        {
            ApplyCompletedText();
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

        if (typeWriterEffect)
        {
            StartTyping();
        }
        else
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
            // Persian / RTL: Fix Arabic letters and order
            string[] lines = targetText.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = ArabicFixer.Fix(lines[i], true, true);
            }
            _fixedText = string.Join("\n", lines);
        }
        else
        {
            // English / LTR: Direct string without ArabicFixer
            _fixedText = targetText;
        }

        ApplyAlignment();
        if (_textUI != null) _textUI.text = "";

        len = _fixedText != null ? _fixedText.Length : 0;
        _builder.Clear();
        _counter = 0;
        isComplete = false;
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
        if (Time.frameCount <= _startFrame) return;
        
        bool clicked = false;
        // New Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            clicked = true;
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            clicked = true;
        else if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            clicked = true;
        
        // Fallback: Old Input Manager (project uses Both input handlers)
        if (!clicked && Input.GetMouseButtonDown(0))
            clicked = true;

        if (!clicked) return;

        Debug.Log($"[FarsiTypewriter] Click detected on '{gameObject.name}' | isTyping={isTyping} | isComplete={isComplete} | _startFrame={_startFrame} | frame={Time.frameCount}");

        if (isTyping)
        {
            // Click while typing → skip to complete text
            SkipTyping();
        }
        else if (isComplete)
        {
            // Click after text is fully shown → advance to next dialogue
            if (OnClickWhenComplete != null)
            {
                OnClickWhenComplete.Invoke();
            }
            else
            {
                // Auto-find a Button in parent hierarchy and click it
                var btn = GetComponentInParent<Button>();
                if (btn != null && btn.interactable)
                {
                    btn.onClick.Invoke();
                }
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Handled in Update() for consistency
    }

    public void StartTyping()
    {
        StopAllCoroutines();
        
        _builder.Clear();
        isTyping = true;
        isComplete = false;
        _startFrame = Time.frameCount;
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
            // RTL Typewriter effect (Persian)
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
            // LTR Typewriter effect (English)
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