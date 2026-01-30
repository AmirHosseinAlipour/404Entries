using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ArabicSupport;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class NewFarsiTypewriter : MonoBehaviour
{
    [Header("Typewriter Settings")]
    public bool typeWriterEffect;
    public float typeCharTime = 0.05f;
    public bool autoResetText;
    
    private TextMeshProUGUI _textUI;
    private string _rawText;
    private string _finalProcessedText;
    public int len;
    
    private StringBuilder _builder = new StringBuilder();
    
    private float _boxHeight;
    private float _counter;
    private bool _canReset;

    void Awake()
    {
        _textUI = GetComponent<TextMeshProUGUI>();
        
        // --- KEEP THESE TMP SETTINGS OFF ---
        _textUI.isRightToLeftText = false; 
        _textUI.alignment = TextAlignmentOptions.TopRight; 
        // ---
        
        FixAndPrepareText(_textUI.text);
    }

    private void Start()
    {
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
            _textUI.text = _finalProcessedText;
        }
    }

    private void FixAndPrepareText(string rawText)
    {
        _rawText = rawText;
        
        string[] lines = rawText.Split('\n');
        
        // --- NEW LINE ADDED ---
        // Manually reverse the array to fight TMP's line reversal.
        Array.Reverse(lines);
        // ---

        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = ArabicFixer.Fix(lines[i], true, true);
        }
        _finalProcessedText = string.Join("\n", lines);


        _textUI.text = "";
        _builder.Clear();
        _counter = 0;
    }

    public void StartTyping()
    {
        StopAllCoroutines();
        _builder.Clear();
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
        if (string.IsNullOrEmpty(_rawText))
        {
            yield break;
        }
        
        _counter = 0;
        _builder.Clear();
        
        foreach (char c in _rawText) 
        {
            yield return new WaitForSeconds(typeCharTime);
            _builder.Append(c);
            
            string currentRawText = _builder.ToString();

            string[] currentLines = currentRawText.Split('\n');
            string[] fixedLines = new string[currentLines.Length];
            
            for (int i = 0; i < currentLines.Length; i++)
            {
                fixedLines[i] = ArabicFixer.Fix(currentLines[i], true, true);
            }
            
            // --- NEW LINE ADDED ---
            // Manually reverse the array to fight TMP's line reversal.
            Array.Reverse(fixedLines);
            // ---

            _textUI.text = string.Join("\n", fixedLines);
            
            _counter++;

            if (autoResetText)
            {
                _canReset = true;
            }
            
            if (_canReset && _counter % 5 == 0 && CheckVerticalOverflow(_boxHeight))
            {
                _textUI.text = "";
                _builder.Clear();
                _counter = 0;
                _canReset = false;
            }
        }
    }

    private bool CheckVerticalOverflow(float boxHeight)
    {
        _textUI.ForceMeshUpdate(); 
        float preferredHeight = _textUI.preferredHeight;
        return preferredHeight > boxHeight;
    }
}