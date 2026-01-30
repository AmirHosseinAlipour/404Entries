using UnityEngine;
using UnityEngine.Video;
using System;

public class VideoController : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    private Action onCompleteAction; // برای ذخیره‌ی تابعی که بعد از اتمام ویدیو باید اجرا بشه

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    public void PlayVideo(Action onComplete = null)
    {
        onCompleteAction = onComplete;
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Play();
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        videoPlayer.loopPointReached -= OnVideoEnd; // جلوگیری از دوباره صدا خوردن
        onCompleteAction?.Invoke(); // اجرای تابع نهایی
    }
}   