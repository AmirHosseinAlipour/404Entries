using UnityEngine;

public static class UIUtils
{
    public static void SetAlpha(GameObject obj, float alpha)
    {
        if (obj == null) return;

        CanvasGroup group = obj.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = obj.AddComponent<CanvasGroup>();
        }
        group.alpha = alpha;
        group.blocksRaycasts = alpha > 0.01f;
        group.interactable = alpha > 0.01f;
    }
}
