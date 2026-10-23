using System;
using UnityEngine;
using UnityEngine.UI;

public class OnTriggerRandomEmote : MonoBehaviour
{
    private RectTransform _rt;
    private Image _image;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
    }

    private void Start()
    {
        HideEmote();
    }

    private static readonly Emotion[] _cachedEmotions = (Emotion[])System.Enum.GetValues(typeof(Emotion));

    private void RandomEmoteSelector()
    {
        if (EmoteSystem.Instance == null || _cachedEmotions.Length == 0) return;
        int randomIndex = UnityEngine.Random.Range(0, _cachedEmotions.Length);
        Emotion randomEmotion = _cachedEmotions[randomIndex];
        Sprite chosenSprite = EmoteSystem.Instance.SetChosenSprite(randomEmotion);
        if (chosenSprite != null && _image != null)
        {
            _image.sprite = chosenSprite;
        }
    }

    public void ShowEmote()
    {
        RandomEmoteSelector();
        UIAnimationManager.Instance.ShowWindow(_rt, 0.3f);
    }
    
    public void HideEmote()
    {
        UIAnimationManager.Instance.HideWindow(_rt, 0.3f);
    }
}
