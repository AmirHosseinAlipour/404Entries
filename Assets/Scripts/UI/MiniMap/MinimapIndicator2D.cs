using System.Collections.Generic;
using UnityEngine;

public class MinimapIndicator2D : MonoBehaviour
{
    public Transform playerTransform;   
    public RectTransform holderRect;    
    public GameObject indicatorPrefab;  
    
    [Header("Settings")]
    public float detectionRadius = 20f; 
    public float uiRadius = 100f;       

    private static List<Target> targets = new List<Target>();
    private Dictionary<Target, RectTransform> indicators = new Dictionary<Target, RectTransform>();

    private void Update()
    {
        if (playerTransform == null || holderRect == null) return;

        // 1. Update existing indicators or create new ones
        foreach (Target target in targets)
        {
            if (target == null || target.transform == playerTransform) continue;

            if (!indicators.ContainsKey(target))
            {
                GameObject ind = Instantiate(indicatorPrefab, holderRect);
                indicators.Add(target, ind.GetComponent<RectTransform>());
            }

            UpdateUIIndicator(target, indicators[target]);
        }

        // 2. CLEANUP: Remove indicators that no longer have a target in the list
        List<Target> keysToRemove = new List<Target>();

        foreach (var pair in indicators)
        {
            // If the target was removed from the list or destroyed
            if (pair.Key == null || !targets.Contains(pair.Key))
            {
                keysToRemove.Add(pair.Key);
            }
        }

        foreach (Target key in keysToRemove)
        {
            if (indicators[key] != null)
            {
                Destroy(indicators[key].gameObject);
            }
            indicators.Remove(key);
        }
    }

    private void UpdateUIIndicator(Target target, RectTransform indicator)
    {
        Vector2 worldDirection = (Vector2)target.transform.position - (Vector2)playerTransform.position;
        float worldDistance = worldDirection.magnitude;

        float worldToUiRatio = uiRadius / detectionRadius;
        float uiDistance = worldDistance * worldToUiRatio;

        float finalDistance = Mathf.Min(uiDistance, uiRadius);

        Vector2 dirNormalized = worldDirection.normalized;
        indicator.anchoredPosition = dirNormalized * finalDistance;

        float angle = Mathf.Atan2(dirNormalized.y, dirNormalized.x) * Mathf.Rad2Deg;
        indicator.localRotation = Quaternion.Euler(0, 0, angle - 90f);
        
        if (!indicator.gameObject.activeSelf) indicator.gameObject.SetActive(true);
    }

    public static void AddTarget(Target t) 
    {
        if (!targets.Contains(t)) targets.Add(t);
    }

    public static void RemoveTarget(Target t) 
    {
        if (targets.Contains(t)) targets.Remove(t);
    }
}