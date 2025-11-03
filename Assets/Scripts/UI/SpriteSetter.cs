using System;
using UnityEngine;
using UnityEngine.UI;

public class SpriteSetter : MonoBehaviour
{
    public Image targetImage;         // تصویری که می‌خوای اسپریت رو رویش ست کنی
    public Sprite[] sprites;          // آرایه‌ای از اسپریت‌ها (مثلاً دو تا اسپریت)

    private void Start()
    {
        targetImage = GetComponentInChildren<Image>();
    }

    public void SetSpriteByIndex(int index)
    {
        if (index >= 0 && index < sprites.Length)
        {
            targetImage.sprite = sprites[index];
        }
        else
        {
            Debug.LogWarning("Index خارج از محدوده است!");
        }
    }
}