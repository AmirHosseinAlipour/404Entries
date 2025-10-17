using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TypeWriterEffect : MonoBehaviour
{
    public IEnumerator PlayText(TMP_Text uiText, string fullText, float delay)
    {
        uiText.text = "";
        foreach (char c in fullText)
        {
            uiText.text += c;
            yield return new WaitForSeconds(delay);
        }
    }
}