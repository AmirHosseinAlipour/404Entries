using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ArabicSupport;

[RequireComponent(typeof(Text))]
public class OldFarsiTypewriter : MonoBehaviour
{
    [Header("Typewriter Settings")]
    public bool typeWriterEffect;
    public float typeCharTime;
    public bool autoResetText;
    
    private Text _textUI;
    private string _fixedText;
    public int len;
    
    private StringBuilder _builder = new StringBuilder();
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
        if (typeWriterEffect)
        {
            StartTyping();
        }
        else
        {
            _textUI.text = _fixedText;
        }
        
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
            _textUI.text = _fixedText;
        }
    }

    private void FixAndPrepareText(string rawText)
    {
        string[] lines = rawText.Split('\n');
        
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = ArabicFixer.Fix(lines[i], true, true);
        }
        
        _fixedText = string.Join("\n", lines);
        
        _textUI.text = "";

        len = _fixedText.Length;
        
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
        if (string.IsNullOrEmpty(_fixedText))
        {
            yield break;
        }
        
        _counter = 0; 
        
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