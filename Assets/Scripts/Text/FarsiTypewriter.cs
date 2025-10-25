using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ArabicSupport;

[RequireComponent(typeof(Text))]
public class FarsiTypewriter : MonoBehaviour
{
    [Header("Typewriter Settings")]
    public bool typeWriterEffect;
    public float typeCharTime;
    public bool autoResetText;
    
    private Text _textUI;
    private string _rawText; // <-- ADDED: To store the original, unfixed text
    private string _finalProcessedText; // <-- RENAMED: from _fixedText
    public int len;
    
    private StringBuilder _builder = new StringBuilder(); // This will now store the RAW text
    private TextGenerationSettings _cachedSettings;
    private float _boxHeight;
    private float _counter;
    private bool _canReset;

    void Awake()
    {
        _textUI = GetComponent<Text>();
        
        FixAndPrepareText(_textUI.text);
    }

    private void Start()
    {
        _cachedSettings = _textUI.GetGenerationSettings(_textUI.rectTransform.rect.size);
        _boxHeight = _textUI.rectTransform.rect.height;
    }

    public void SetText(string newRawFarsiText)
    {
        StopAllCoroutines();
        
        FixAndPrepareText(newRawFarsiText);

        if (typeWriterEffect)
        {
            StartTyping();
        }
        else
        {
            _textUI.text = _finalProcessedText; // <-- CHANGED
        }
    }

    private void FixAndPrepareText(string rawText)
    {
        _rawText = rawText; // <-- ADDED: Store the original text
        
        string[] lines = rawText.Split('\n');
        
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = ArabicFixer.Fix(lines[i], true, true);
        }
        
        _finalProcessedText = string.Join("\n", lines); // <-- RENAMED
        
        _textUI.text = "";

        len = _rawText.Length; // <-- CHANGED: Length of the raw text
        
        _builder.Clear(); // Builder will store raw text
        _counter = 0;
    }


    public void StartTyping()
    {
        StopAllCoroutines();
        
        _builder.Clear(); // Clear the raw text builder
        
        StartCoroutine(TypeTextCoroutine());
    }
    
    public void StopTyping()
    {
        StopAllCoroutines();
    }

    public void ResetText()
    {
        _canReset = true;
    }

    private IEnumerator TypeTextCoroutine()
    {
        if (string.IsNullOrEmpty(_rawText)) // <-- CHANGED: Check raw text
        {
            yield break;
        }
        
        _counter = 0;
        _builder.Clear(); // <-- Ensure raw text builder is clear
        
        // <-- CHANGED: Iterate over the original RAW text
        foreach (char c in _rawText) 
        {
            _builder.Append(c); // <-- Build the RAW string
            
            // --- This is the new, critical part ---
            // We must re-fix the *current* raw text at every step
            
            string currentRawText = _builder.ToString();
            string[] currentLines = currentRawText.Split('\n');
            string[] fixedLines = new string[currentLines.Length];
            
            for (int i = 0; i < currentLines.Length; i++)
            {
                // Fix each line of the *current partial* text
                fixedLines[i] = ArabicFixer.Fix(currentLines[i], true, true);
            }
            
            // Set the UI text to the newly fixed, partial string
            _textUI.text = string.Join("\n", fixedLines);
            // --- End of new part ---
            
            
            _counter++;

            if (autoResetText)
            {
                _canReset = true;
            }
            
            // This overflow logic should now work fine
            if (_canReset && _counter % 5 == 0 
                         && CheckVerticalOverflow(_cachedSettings, _boxHeight))
            {
                _textUI.text = "";
                _builder.Clear(); // Clears the raw text builder
                _counter = 0;
                _canReset = false;
            }
            
            yield return new WaitForSeconds(typeCharTime);
        }
    }

    private bool CheckVerticalOverflow(TextGenerationSettings settings, float boxHeight)
    {
        float preferredHeight = _textUI.cachedTextGenerator.GetPreferredHeight(_textUI.text, settings);

        return preferredHeight > boxHeight;
    }
}