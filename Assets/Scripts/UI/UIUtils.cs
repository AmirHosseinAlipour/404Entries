using UnityEngine;

public static class UIUtils
{
    public static void SetAlpha(GameObject obj, float alpha)
    {
        CanvasGroup group = obj.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = obj.AddComponent<CanvasGroup>();
        }
        group.alpha = alpha;
    }
}
