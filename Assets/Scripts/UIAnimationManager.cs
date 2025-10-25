using UnityEngine;
using DG.Tweening;

public class UIAnimationManager : MonoBehaviour
{
    public static UIAnimationManager Instance { get; private set; }

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
    public void ShowWindow(RectTransform window, float showWindowDuration)
    {
        if (window == null) return;
    
        window.gameObject.SetActive(true);
        window.DOKill();
    
        window.DOScale(1f, showWindowDuration).From(0f).SetEase(Ease.OutBack).SetUpdate(true);
    }
    
    public void ShowDialogueWindow(RectTransform window, float showWindowDuration, FarsiTypewriter fw)
    {
        if (window == null) return;

        window.gameObject.SetActive(true);
        window.DOKill();

        window.DOScale(1f, showWindowDuration)
            .From(0f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                fw.StartTyping();
            });
    }
    
    public void HideWindow(RectTransform window, float hidWindowDuration)
    {
        if (window == null) return;
    
        window.DOKill();
        window.DOScale(0f, hidWindowDuration).SetEase(Ease.InQuad).OnComplete(() =>
        {
            window.gameObject.SetActive(false);
        }).SetUpdate(true);
    }
}