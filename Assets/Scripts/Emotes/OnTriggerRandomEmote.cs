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

    private void RandomEmoteSelector()
    {
        Array allEmotions = System.Enum.GetValues(typeof(Emotion));
        int randomIndex = UnityEngine.Random.Range(0, allEmotions.Length);
        Emotion randomEmotion = (Emotion)allEmotions.GetValue(randomIndex);
        Sprite chosenSprite = EmoteSystem.Instance.SetChosenSprite(randomEmotion);
        if (chosenSprite != null)
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
