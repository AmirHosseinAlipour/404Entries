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

        // NOTE: Generic windows (elevator buttons, menus, popups) MUST NOT call CenterDialogueLayout!
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

        CenterDialogueLayout(window);

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

    public void CenterDialogueLayout(RectTransform window)
    {
        if (window == null) return;

        string winName = window.gameObject.name.ToLower();

        // Safety Guard: NEVER modify non-dialogue UI like Elevator, Menus, or Tasks
        if (winName.Contains("elevator") || winName.Contains("butoon") || winName.Contains("task") || winName.Contains("pause") || winName.Contains("setting"))
        {
            return;
        }

        // 1. Center the window container horizontally if full-width or bottom anchored
        if (window.anchorMin.x <= 0.05f && window.anchorMax.x >= 0.95f)
        {
            window.pivot = new Vector2(0.5f, window.pivot.y);
            window.anchoredPosition = new Vector2(0f, window.anchoredPosition.y);
        }

        // Determine window-level speaker side
        bool? windowIsRight = null;
        if (winName.Contains("person2") || winName.Contains("player") || winName.Contains("girl"))
        {
            windowIsRight = true;
        }
        else if (winName.Contains("person1") || winName.Contains("prof") || winName.Contains("npc") || winName.Contains("teacher"))
        {
            windowIsRight = false;
        }

        // 2. Find and center the Dialogue Box and position portraits safely inward
        for (int i = 0; i < window.childCount; i++)
        {
            Transform child = window.GetChild(i);
            if (child == null) continue;
            RectTransform childRt = child as RectTransform;
            if (childRt == null) continue;

            string childName = child.gameObject.name.ToLower();

            // Handle Text / Dialogue Box Container
            if (childName == "text" || child.GetComponent<FarsiTypewriter>() != null || childName.Contains("dialogue") || childName.Contains("textpanel"))
            {
                childRt.anchorMin = new Vector2(0.5f, 0f);
                childRt.anchorMax = new Vector2(0.5f, 0f);
                childRt.pivot = new Vector2(0.5f, 0f);
                childRt.anchoredPosition = new Vector2(0f, childRt.anchoredPosition.y > 0 ? childRt.anchoredPosition.y : 20f);
                childRt.sizeDelta = new Vector2(1140f, 350f);
            }
            // Handle Portrait Image
            else if (childName == "image" || childName.Contains("portrait") || childName.Contains("person") || childName.Contains("avatar"))
            {
                bool isRight = false;
                if (windowIsRight.HasValue)
                {
                    isRight = windowIsRight.Value;
                }
                else
                {
                    var img = child.GetComponent<UnityEngine.UI.Image>();
                    if (img != null && img.sprite != null)
                    {
                        string spName = img.sprite.name.ToLower();
                        if (spName.Contains("girl") || spName.Contains("player"))
                        {
                            isRight = true;
                        }
                        else if (spName.Contains("prof") || spName.Contains("shekofteh") || spName.Contains("aliakbari") || spName.Contains("background removed") || spName.Contains("gemini"))
                        {
                            isRight = false;
                        }
                        else
                        {
                            isRight = childRt.anchorMin.x > 0.5f || childRt.anchoredPosition.x < -10f;
                        }
                    }
                    else
                    {
                        isRight = childRt.anchorMin.x > 0.5f || childRt.anchoredPosition.x < -10f;
                    }
                }

                // Standardized margins: Dialogue box is centered at 1140px width (x: 390 to 1530).
                // Left margin is [0, 390] with center at 195px.
                // Right margin is [1530, 1920] with center at 1725px (-195px from right anchor).
                // With pivot (0.5, 0), flipping localScale.x in-place keeps portraits centered safely inside margin!
                if (isRight)
                {
                    childRt.anchorMin = new Vector2(1f, 0f);
                    childRt.anchorMax = new Vector2(1f, 0f);
                    childRt.pivot = new Vector2(0.5f, 0f);
                    childRt.anchoredPosition = new Vector2(-195f, 0f);
                    float sx = Mathf.Abs(childRt.localScale.x);
                    if (sx < 0.25f || sx > 0.55f) sx = 0.36f;
                    float sy = Mathf.Abs(childRt.localScale.y);
                    if (sy < 0.25f || sy > 0.55f) sy = 0.36f;
                    childRt.localScale = new Vector3(-sx, sy, 1f); // Flipped horizontally to face left towards dialogue box
                }
                else
                {
                    childRt.anchorMin = new Vector2(0f, 0f);
                    childRt.anchorMax = new Vector2(0f, 0f);
                    childRt.pivot = new Vector2(0.5f, 0f);
                    childRt.anchoredPosition = new Vector2(195f, 0f);
                    float sx = Mathf.Abs(childRt.localScale.x);
                    if (sx < 0.25f || sx > 0.55f) sx = 0.36f;
                    float sy = Mathf.Abs(childRt.localScale.y);
                    if (sy < 0.25f || sy > 0.55f) sy = 0.36f;
                    childRt.localScale = new Vector3(sx, sy, 1f); // Facing right towards dialogue box
                }
            }
        }
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
