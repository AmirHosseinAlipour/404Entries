using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapIndicator2D : MonoBehaviour
{
    public static int TargetFloorIndex = -1;
    public static bool IsDifferentFloor = false;

    public Transform playerTransform;
    public RectTransform holderRect;
    public GameObject indicatorPrefab;

    [Header("Sprites")]
    public Sprite targetSprite;
    public Sprite elevatorSprite;
    public Sprite ringSprite;
    public Sprite targetPinSprite;
    public Sprite elevatorPinSprite;
    public Sprite targetEdgeSprite;
    public Sprite elevatorEdgeSprite;

    [Header("Settings")]
    public float detectionRadius = 20f;
    public float uiRadius = 0f;
    public float smoothSpeed = 12f;

    private static List<Target> targets = new List<Target>();
    private RectTransform primaryIndicator;
    private ElevatorUI elevatorUI;
    private ElevatorTrigger[] elevatorTriggers;
    private float lastElevatorRefreshTime;

    private void Awake()
    {
        if (targetSprite == null) targetSprite = Resources.Load<Sprite>("Minimap/indicator_target");
        if (elevatorSprite == null) elevatorSprite = Resources.Load<Sprite>("Minimap/indicator_elevator");
        if (targetPinSprite == null) targetPinSprite = targetSprite;
        if (elevatorPinSprite == null) elevatorPinSprite = elevatorSprite;
        if (targetEdgeSprite == null) targetEdgeSprite = targetSprite;
        if (elevatorEdgeSprite == null) elevatorEdgeSprite = elevatorSprite;
    }

    private void OnDestroy()
    {
        targets.Clear();
        if (primaryIndicator != null)
        {
            Destroy(primaryIndicator.gameObject);
            primaryIndicator = null;
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }
        if (holderRect == null)
        {
            holderRect = GetComponent<RectTransform>();
        }
        if (playerTransform == null || holderRect == null) return;

        RefreshElevatorsIfNeeded();

        Transform activeTarget = GetActiveTargetTransform();
        if (activeTarget == null)
        {
            if (primaryIndicator != null && primaryIndicator.gameObject.activeSelf)
            {
                primaryIndicator.gameObject.SetActive(false);
            }
            IsDifferentFloor = false;
            TargetFloorIndex = -1;
            return;
        }

        Vector2 playerPos = playerTransform.position;
        Vector2 targetPos = activeTarget.position;

        int playerFloor = GetFloorIndex(playerPos);
        int targetFloor = GetFloorIndex(targetPos);
        TargetFloorIndex = targetFloor;
        IsDifferentFloor = (playerFloor != targetFloor);

        Vector2 destinationPos;
        if (IsDifferentFloor)
        {
            ElevatorTrigger nearestElevator = GetNearestElevator(playerPos);
            if (nearestElevator != null)
            {
                destinationPos = nearestElevator.transform.position;
            }
            else if (elevatorUI != null && elevatorUI.floorPositions != null && playerFloor >= 0 && playerFloor < elevatorUI.floorPositions.Length && elevatorUI.floorPositions[playerFloor] != null)
            {
                destinationPos = elevatorUI.floorPositions[playerFloor].transform.position;
            }
            else
            {
                destinationPos = targetPos;
            }
        }
        else
        {
            destinationPos = targetPos;
        }

        EnsureIndicatorInstance();
        if (primaryIndicator == null) return;

        Vector2 worldDiff = destinationPos - playerPos;
        float effectiveRadius = uiRadius;
        if (effectiveRadius <= 0f)
        {
            effectiveRadius = Mathf.Min(holderRect.rect.width, holderRect.rect.height) * 0.44f;
        }

        float worldToUiRatio = effectiveRadius / Mathf.Max(0.1f, detectionRadius);
        Vector2 localMapPos = worldDiff * worldToUiRatio;
        float localDist = localMapPos.magnitude;
        bool isEdge = (localDist > effectiveRadius);

        Vector2 targetAnchoredPos;
        Vector3 targetScale;

        if (!isEdge)
        {
            targetAnchoredPos = localMapPos;
            float pulse = 1f + Mathf.PingPong(Time.time * 2f, 0.1f);
            targetScale = new Vector3(pulse, pulse, 1f);
        }
        else
        {
            Vector2 dir = (worldDiff.sqrMagnitude > 0.001f) ? worldDiff.normalized : Vector2.up;
            targetAnchoredPos = dir * effectiveRadius;
            float edgePulse = 1f + Mathf.PingPong(Time.time * 3f, 0.12f);
            targetScale = new Vector3(edgePulse * 1.08f, edgePulse * 1.08f, 1f);
        }

        Image img = primaryIndicator.GetComponent<Image>();
        if (img != null)
        {
            Sprite chosenSprite = IsDifferentFloor 
                ? (isEdge ? (elevatorEdgeSprite != null ? elevatorEdgeSprite : elevatorSprite) : (elevatorPinSprite != null ? elevatorPinSprite : elevatorSprite))
                : (isEdge ? (targetEdgeSprite != null ? targetEdgeSprite : targetSprite) : (targetPinSprite != null ? targetPinSprite : targetSprite));

            if (chosenSprite != null && img.sprite != chosenSprite)
            {
                img.sprite = chosenSprite;
            }
            img.color = Color.white;
            img.raycastTarget = false;
        }

        if (!primaryIndicator.gameObject.activeSelf)
        {
            primaryIndicator.anchoredPosition = targetAnchoredPos;
            primaryIndicator.localRotation = Quaternion.identity;
            primaryIndicator.localScale = targetScale;
            primaryIndicator.gameObject.SetActive(true);
        }
        else
        {
            float t = Mathf.Clamp01(Time.deltaTime * Mathf.Max(1f, smoothSpeed));
            primaryIndicator.anchoredPosition = Vector2.Lerp(primaryIndicator.anchoredPosition, targetAnchoredPos, t);
            primaryIndicator.localScale = Vector3.Lerp(primaryIndicator.localScale, targetScale, t);
            primaryIndicator.localRotation = Quaternion.identity;
        }
    }

    private void EnsureIndicatorInstance()
    {
        if (primaryIndicator != null) return;
        if (indicatorPrefab == null || holderRect == null) return;

        GameObject ind = Instantiate(indicatorPrefab, holderRect);
        primaryIndicator = ind.GetComponent<RectTransform>();
        primaryIndicator.anchorMin = new Vector2(0.5f, 0.5f);
        primaryIndicator.anchorMax = new Vector2(0.5f, 0.5f);
        primaryIndicator.pivot = new Vector2(0.5f, 0.5f);
        primaryIndicator.sizeDelta = new Vector2(32f, 32f);
        primaryIndicator.gameObject.SetActive(false);
    }

    private Transform GetActiveTargetTransform()
    {
        if (TaskManager.Instance != null)
        {
            int currentTaskIdx = TaskManager.Instance.GetCurrentTaskIndex();
            if (currentTaskIdx >= 0)
            {
                if (GameManager.Instance != null)
                {
                    if (GameManager.Instance.taskTriggers != null &&
                        currentTaskIdx < GameManager.Instance.taskTriggers.Count &&
                        GameManager.Instance.taskTriggers[currentTaskIdx] != null)
                    {
                        return GameManager.Instance.taskTriggers[currentTaskIdx].transform;
                    }

                    if (currentTaskIdx % 2 == 0)
                    {
                        return GameManager.Instance.transform;
                    }
                }
            }
        }

        if (targets.Count > 0)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null && targets[i].gameObject.activeInHierarchy && targets[i].transform != playerTransform)
                {
                    return targets[i].transform;
                }
            }
        }

        return null;
    }

    private void RefreshElevatorsIfNeeded()
    {
        if (elevatorUI == null || elevatorTriggers == null || elevatorTriggers.Length == 0 || Time.time - lastElevatorRefreshTime > 2f)
        {
            lastElevatorRefreshTime = Time.time;
            if (elevatorUI == null) elevatorUI = Object.FindAnyObjectByType<ElevatorUI>();
            elevatorTriggers = Object.FindObjectsByType<ElevatorTrigger>(FindObjectsInactive.Include);
        }
    }

    private int GetFloorIndex(Vector2 pos)
    {
        if (elevatorUI != null && elevatorUI.floorPositions != null && elevatorUI.floorPositions.Length > 0)
        {
            int bestIndex = -1;
            float minDistance = float.MaxValue;
            for (int i = 0; i < elevatorUI.floorPositions.Length; i++)
            {
                if (elevatorUI.floorPositions[i] == null) continue;
                float d = Vector2.Distance(pos, elevatorUI.floorPositions[i].transform.position);
                if (d < minDistance)
                {
                    minDistance = d;
                    bestIndex = i;
                }
            }
            if (bestIndex >= 0) return bestIndex;
        }

        if (elevatorTriggers != null && elevatorTriggers.Length > 0)
        {
            int nearestIdx = 0;
            float minD = float.MaxValue;
            for (int i = 0; i < elevatorTriggers.Length; i++)
            {
                if (elevatorTriggers[i] == null) continue;
                float d = Vector2.Distance(pos, elevatorTriggers[i].transform.position);
                if (d < minD)
                {
                    minD = d;
                    nearestIdx = i;
                }
            }
            return nearestIdx;
        }

        return 0;
    }

    private ElevatorTrigger GetNearestElevator(Vector2 pos)
    {
        if (elevatorTriggers == null || elevatorTriggers.Length == 0) return null;
        ElevatorTrigger best = null;
        float minD = float.MaxValue;
        for (int i = 0; i < elevatorTriggers.Length; i++)
        {
            if (elevatorTriggers[i] == null) continue;
            float d = Vector2.Distance(pos, elevatorTriggers[i].transform.position);
            if (d < minD)
            {
                minD = d;
                best = elevatorTriggers[i];
            }
        }
        return best;
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