using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ArabicSupport;

[RequireComponent(typeof(Text))]
public class CreditsTypeWriter : MonoBehaviour
{
    [Header("Typewriter Settings")]
    public bool typeWriterEffect;
    public float typeCharTime = 0.05f;
    
    [Header("Alignment Settings")]
    public bool centerAlign;
    public bool middleAlign;
    
    public event Action OnTypingFinished;
    
    private Text _textUI;
    private string _fixedText;
    public int len;
    
    private StringBuilder _builder = new StringBuilder();
    private float _counter;
    private bool _canReset;

    void Awake()
    {
        _textUI = GetComponent<Text>();
        
        FixAndPrepareText(_textUI.text);
    }

    private void Start()
    {
        if (!typeWriterEffect)
        {
            _textUI.text = _fixedText;
        }
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
            yield break;

        _counter = 0;
        
        if (middleAlign)
            _textUI.alignment = centerAlign ? TextAnchor.MiddleCenter : TextAnchor.MiddleRight;
        else
            _textUI.alignment = centerAlign ? TextAnchor.UpperCenter : TextAnchor.UpperRight;
        
        _textUI.text = "";

        string[] lines = _fixedText.Split('\n');

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
                
                if (_canReset)
                {
                    _textUI.text = "";
                    _canReset = false;
                    yield return new WaitForSeconds(typeCharTime * 2f);
                    StartCoroutine(TypeTextCoroutine());
                    yield break;
                }

                _counter++;
            }

            yield return new WaitForSeconds(typeCharTime * 2f);
        }
        
        if(OnTypingFinished != null)
            OnTypingFinished.Invoke();
    }
}