using System;
using UnityEditor;
using UnityEngine;

public enum Emotion
{
    scary,
    clown,
    happy1,
    happy2,
    happy3,
    raisedEyebrow,
    heartKiss,
    dahanKaj,
    blushLaugh,
    laughter,
    blush,
    skeleton,
    sleepy,
    steamyNose,
    heartEye,
    lovely,
    happyKajEyebrow,
    coolSunGlass,
    smirk,
    sus,
    wink
}

public class EmoteSystem : MonoBehaviour
{
    public static EmoteSystem Instance { get; private set; }
    
    [Header("Emote Sprites")] 
    public Sprite scary;
    public Sprite clown;
    public Sprite happy1;
    public Sprite happy2;
    public Sprite happy3;
    public Sprite raisedEyebrow;
    public Sprite heartKiss;
    public Sprite dahanKaj;
    public Sprite blushLaugh;
    public Sprite laughter;
    public Sprite blush;
    public Sprite skeleton;
    public Sprite sleepy;
    public Sprite steamyNose;
    public Sprite heartEye;
    public Sprite lovely;
    public Sprite happyKajEyebrow;
    public Sprite coolSunGlass;
    public Sprite smirk;
    public Sprite sus;
    public Sprite wink;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public Sprite SetChosenSprite(Emotion emotion)
    {
        Sprite chosenSprite;

        switch (emotion)
        {
            case Emotion.scary:
                chosenSprite = EmoteSystem.Instance.scary;
                break;
            case Emotion.clown:
                chosenSprite = EmoteSystem.Instance.clown;
                break;
            case Emotion.happy1:
                chosenSprite = EmoteSystem.Instance.happy1;
                break;
            case Emotion.happy2:
                chosenSprite = EmoteSystem.Instance.happy2;
                break;
            case Emotion.happy3:
                chosenSprite = EmoteSystem.Instance.happy3;
                break;
            case Emotion.raisedEyebrow:
                chosenSprite = EmoteSystem.Instance.raisedEyebrow;
                break;
            case Emotion.heartKiss:
                chosenSprite = EmoteSystem.Instance.heartKiss;
                break;
            case Emotion.dahanKaj:
                chosenSprite = EmoteSystem.Instance.dahanKaj;
                break;
            case Emotion.blushLaugh:
                chosenSprite = EmoteSystem.Instance.blushLaugh;
                break;
            case Emotion.laughter:
                chosenSprite = EmoteSystem.Instance.laughter;
                break;
            case Emotion.blush:
                chosenSprite = EmoteSystem.Instance.blush;
                break;
            case Emotion.skeleton:
                chosenSprite = EmoteSystem.Instance.skeleton;
                break;
            case Emotion.sleepy:
                chosenSprite = EmoteSystem.Instance.sleepy;
                break;
            case Emotion.steamyNose:
                chosenSprite = EmoteSystem.Instance.steamyNose;
                break;
            case Emotion.heartEye:
                chosenSprite = EmoteSystem.Instance.heartEye;
                break;
            case Emotion.lovely:
                chosenSprite = EmoteSystem.Instance.lovely;
                break;
            case Emotion.happyKajEyebrow:
                chosenSprite = EmoteSystem.Instance.happyKajEyebrow;
                break;
            case Emotion.coolSunGlass:
                chosenSprite = EmoteSystem.Instance.coolSunGlass;
                break;
            case Emotion.smirk:
                chosenSprite = EmoteSystem.Instance.smirk;
                break;
            case Emotion.sus:
                chosenSprite = EmoteSystem.Instance.sus;
                break;
            case Emotion.wink:
                chosenSprite = EmoteSystem.Instance.wink;
                break;
            default:
                chosenSprite = null;
                break;
        }

        return chosenSprite;
    }
}
