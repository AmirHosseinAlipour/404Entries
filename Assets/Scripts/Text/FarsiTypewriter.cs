using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ArabicSupport;

public class FarsiTypewriter : MonoBehaviour
{
    public float TypeTextTotalTime;
    
    private Text _textUI;
    private string _fixedText;

    void Awake()
    {
        _textUI = GetComponent<Text>();
        
        string originalFarsiText = _textUI.text;
        _fixedText = ArabicFixer.Fix(originalFarsiText);
        _textUI.text = "";
    }
    
    public void StartTyping()
    {
        // Stop any previous typing routines before starting a new one.
        StopAllCoroutines();
        StartCoroutine(TypeTextCoroutine());
    }

    private IEnumerator TypeTextCoroutine()
    {
        // Calculate the time to wait for each character to make a type writer effect
        float time = TypeTextTotalTime / _fixedText.Length;
        
        foreach (char c in _fixedText)
        {
            _textUI.text += c;
            yield return new WaitForSeconds(time);
        }
    }
}