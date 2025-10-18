using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArabicSupport;

public class FarsiTypewriter : MonoBehaviour
{
    public float TypeTextTotalTime;
    public bool TypeWriterEffect;
    
    private Text _textUI;
    private string _fixedText;

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
        if (!TypeWriterEffect)
        {
            _textUI.text = _fixedText;
        }
    }

    public void StartTyping()
    {
        // Stop any previous typing routines before starting a new one.
        StopAllCoroutines();
        StartCoroutine(TypeTextCoroutine());
    }

    private IEnumerator TypeTextCoroutine()
    {
        if (string.IsNullOrEmpty(_fixedText))
        {
            yield break; // Exit if there is no text to type.
        }

        // Calculate the time to wait for each character to make a typewriter effect.
        float time = TypeTextTotalTime / _fixedText.Length;
        
        foreach (char c in _fixedText)
        {
            _textUI.text += c;
            yield return new WaitForSeconds(time);
        }
    }
}