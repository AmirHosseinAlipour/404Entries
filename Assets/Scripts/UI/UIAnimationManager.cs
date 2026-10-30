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

    public void ShowWindow(RectTransform window, float duration = 0.25f)
    {
        if (window == null) return;

        CanvasGroup cg = GetOrAddCanvasGroup(window);
        cg.DOKill();
        cg.alpha = 0f;
    
        window.gameObject.SetActive(true);
        window.localScale = Vector3.one;
        window.DOKill();

        float fadeDuration = Mathf.Min(duration, 0.25f);
        cg.DOFade(1f, fadeDuration).SetEase(Ease.Linear).SetUpdate(true);
    }
    
    public void ShowDialogueWindow(RectTransform window, float duration, FarsiTypewriter fw = null)
    {
        if (window == null) return;

        CanvasGroup cg = GetOrAddCanvasGroup(window);
        cg.DOKill();
        cg.alpha = 0f;

        window.gameObject.SetActive(true);
        window.localScale = Vector3.one;
        window.DOKill();

        FarsiTypewriter targetFw = fw;
        if (targetFw == null)
        {
            targetFw = window.GetComponentInChildren<FarsiTypewriter>(true);
        }

        if (targetFw != null)
        {
            if (!targetFw.gameObject.activeInHierarchy)
                targetFw.gameObject.SetActive(true);
            
            targetFw.StartTyping();
        }

        float fadeDuration = Mathf.Min(duration, 0.2f);
        cg.DOFade(1f, fadeDuration).SetEase(Ease.Linear).SetUpdate(true);
    }
    
    public void HideWindow(RectTransform window, float duration = 0.2f)
    {
        if (window == null) return;
    
        window.DOKill();
        window.localScale = Vector3.one;

        float fadeDuration = Mathf.Min(duration, 0.2f);
        CanvasGroup cg = GetOrAddCanvasGroup(window);
        cg.DOKill();
        cg.DOFade(0f, fadeDuration).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() =>
        {
            window.gameObject.SetActive(false);
            cg.alpha = 1f;
        });
    }

    private CanvasGroup GetOrAddCanvasGroup(RectTransform rect)
    {
        CanvasGroup cg = rect.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = rect.gameObject.AddComponent<CanvasGroup>();
        }
        return cg;
    }
}