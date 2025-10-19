using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ArabicSupport;

public class FarsiTypewriter : MonoBehaviour
{
    public bool typeWriterEffect;
    public float typeCharTime;
    public bool autoResetText;
    
    // To fix RTL
    private Text _textUI;
    private string _fixedText;
    
    // To achieve text reset after vertical overflow
    private StringBuilder _builder = new StringBuilder();
    private TextGenerationSettings _cachedSettings;
    private float _boxHeight;
    private float _counter;
    private bool _canReset;

    void Awake()
    {
        _textUI = GetComponent<Text>();
        
        string originalFarsiText = _textUI.text;
        
        // 1. Split the original text into lines.
        string[] lines = originalFarsiText.Split('\n');
        
        // 2. Fix each line individually.
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = ArabicFixer.Fix(lines[i], true, true);
        }
        
        // 3. Join the fixed lines back together with newlines.
        _fixedText = string.Join("\n", lines);
        
        // Clear the UI text to prepare for typing.
        _textUI.text = "";

    }

    private void Start()
    {
        if (!typeWriterEffect)
        {
            _textUI.text = _fixedText;
        }
        
        _cachedSettings = _textUI.GetGenerationSettings(_textUI.rectTransform.rect.size);
        _boxHeight = _textUI.rectTransform.rect.height;
    }

    public void StartTyping()
    {
        // Stop any previous typing routines before starting a new one.
        StopAllCoroutines();
        
        // Clear the string builder
        _builder.Clear();
        
        // Start a new Type Writer Effect
        StartCoroutine(TypeTextCoroutine());
    }

    public void ResetText()
    {
        _canReset = true;
    }

    private IEnumerator TypeTextCoroutine()
    {
        if (string.IsNullOrEmpty(_fixedText))
        {
            yield break; // Exit if there is no text to type.
        }
        
        foreach (char c in _fixedText)
        {
            _builder.Append(c);
            _textUI.text = _builder.ToString();
            
            _counter++;

            if (autoResetText)
            {
                _canReset = true;
            }
            
            if (_canReset && _counter % 5 == 0 
                          && CheckVerticalOverflow(_cachedSettings, _boxHeight))
            {
                _textUI.text = "";
                _builder.Clear();
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