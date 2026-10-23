using System;
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

    private System.Collections.Generic.Dictionary<Emotion, Sprite> _emotionMap;

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
            InitializeMap();
        }
    }

    private void InitializeMap()
    {
        if (_emotionMap != null) return;
        _emotionMap = new System.Collections.Generic.Dictionary<Emotion, Sprite>(21)
        {
            { Emotion.scary, scary },
            { Emotion.clown, clown },
            { Emotion.happy1, happy1 },
            { Emotion.happy2, happy2 },
            { Emotion.happy3, happy3 },
            { Emotion.raisedEyebrow, raisedEyebrow },
            { Emotion.heartKiss, heartKiss },
            { Emotion.dahanKaj, dahanKaj },
            { Emotion.blushLaugh, blushLaugh },
            { Emotion.laughter, laughter },
            { Emotion.blush, blush },
            { Emotion.skeleton, skeleton },
            { Emotion.sleepy, sleepy },
            { Emotion.steamyNose, steamyNose },
            { Emotion.heartEye, heartEye },
            { Emotion.lovely, lovely },
            { Emotion.happyKajEyebrow, happyKajEyebrow },
            { Emotion.coolSunGlass, coolSunGlass },
            { Emotion.smirk, smirk },
            { Emotion.sus, sus },
            { Emotion.wink, wink }
        };
    }

    public Sprite SetChosenSprite(Emotion emotion)
    {
        if (_emotionMap == null)
        {
            InitializeMap();
        }

        if (_emotionMap.TryGetValue(emotion, out Sprite chosenSprite))
        {
            return chosenSprite;
        }

        return null;
    }
}
