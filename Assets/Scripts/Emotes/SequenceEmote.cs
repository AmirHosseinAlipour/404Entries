using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SequenceEmote : MonoBehaviour
{
    public List<RectTransform> emotes;
    public float changeInterval;

    private float _timer = 0f;
    private int _index = 0;
    private int _len;

    private void Awake()
    {
        _len = emotes.Count;
    }

    private void FixedUpdate()
    {
        if (_timer >= changeInterval)
        {
            _timer = 0f;

            HideEmote();
                
            _index++;

            if (_index >= _len)
            {
                _index = 0;
            }
            
            ShowEmote();
        }

        _timer += Time.fixedDeltaTime;
    }

    private void ShowEmote()
    {
        UIAnimationManager.Instance.ShowWindow(emotes[_index], 0.3f);
    }

    private void HideEmote()
    {
        UIAnimationManager.Instance.HideWindow(emotes[_index], 0.3f);
    }
}
