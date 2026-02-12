using System;
using UnityEngine;
using UnityEngine.UI;

public class FarsiFix : MonoBehaviour
{
    public FarsiTypewriter _ft;

    private void OnEnable()
    {
        _ft.SetText(GetComponent<Text>().text);
        _ft.StartTyping();
    }
}
