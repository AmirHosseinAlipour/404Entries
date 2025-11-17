using UnityEngine;
using UnityEngine.UI;

public class ImageSetter : MonoBehaviour
{
    public Image targetImage;         // تصویری که می‌خوای اسپریت رو رویش ست کنی
    public Sprite Boy;
    private bool set = false;

    // Update is called once per frame
    void Update()
    {
        if (targetImage == null)
        {
            targetImage = GetComponentInChildren<Image>();
        }

        if (!set  && SelectionPlayer.instance.PlayerID == 1)
        {
            targetImage.sprite = Boy;
            set = true;
        }
        
    }
}
