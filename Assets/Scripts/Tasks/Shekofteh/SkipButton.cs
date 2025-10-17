using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Random = UnityEngine.Random;

public class SkipButton : MonoBehaviour
{
    // UI text that counts the touches
    public TextMeshProUGUI counter;
    public CircleTimerAnimation background;
    
    [Header("Teleport Settings")]
    public float teleportInterval = 0.5f;
    
    [Range(0f, 0.5f)]
    public float edgePadding = 0.3f;

    private int count;
    private RectTransform rectTransform;
    private RectTransform parentRectTransform;
    private Coroutine teleportCoroutine;
    private Button button;

    private bool _canTouch = true;
    private Vector2 lastPosition;
    private bool isFirstTeleport = true;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        
        // Get the parent's RectTransform
        if (rectTransform.parent != null)
        {
            parentRectTransform = rectTransform.parent.GetComponent<RectTransform>();
        }
        
        if (rectTransform == null || parentRectTransform == null)
        {
            return;
        }

        // Set anchors to stretch within parent for better positioning
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        // Subscribe to the button click event
        if (button != null)
        {
            button.onClick.AddListener(IncreaseCountAndLog);
        }
    }

    private void Start()
    {
        background.animationDuration = teleportInterval;
    }

    private void OnDisable()
    {
        StopTeleportMovement();
    }
    
    public void StartTeleportMovement()
    {
        if (teleportCoroutine != null)
        {
            StopCoroutine(teleportCoroutine);
        }
        
        teleportCoroutine = StartCoroutine(RandomTeleportRoutine());
    }
    
    public void StopTeleportMovement()
    {
        if (teleportCoroutine != null)
        {
            StopCoroutine(teleportCoroutine);
            teleportCoroutine = null;
        }
    }
    
    public void IncreaseCountAndLog()
    {
        if (_canTouch)
        {
            _canTouch = false;
            count++;
            counter.text = count + " / 3";
        }
    }
    
    private IEnumerator RandomTeleportRoutine()
    {
        while (true)
        {
            // 1. Teleport and Start Animation
            TeleportButton();
            background.StartCountdown();
            yield return new WaitForSeconds(teleportInterval);
        }
    }

    private void TeleportButton()
    {
        if (parentRectTransform == null) return;

        _canTouch = true;

        // Get the actual visible area of the parent panel
        float parentWidth = parentRectTransform.rect.width;
        float parentHeight = parentRectTransform.rect.height;

        // Get the button size (accounting for scale)
        float buttonWidth = rectTransform.rect.width * rectTransform.localScale.x;
        float buttonHeight = rectTransform.rect.height * rectTransform.localScale.y;

        // Calculate the available area for teleportation
        float minX = buttonWidth * 0.5f;
        float maxX = parentWidth - buttonWidth * 0.5f;
        
        float minY = buttonHeight * 0.5f;
        float maxY = parentHeight - buttonHeight * 0.5f;

        // Apply edge padding
        float paddingX = (maxX - minX) * edgePadding;
        float paddingY = (maxY - minY) * edgePadding;

        minX += paddingX;
        maxX -= paddingX;
        minY += paddingY;
        maxY -= paddingY;

        // Ensure we have valid ranges
        if (minX > maxX)
        {
            minX = maxX = parentWidth * 0.5f;
        }
        if (minY > maxY)
        {
            minY = maxY = parentHeight * 0.5f;
        }

        const float minTeleportDistance = 300f;
        const int maxAttempts = 30;
        
        Vector2 newPosition;
        int currentAttempt = 0;
        
        do
        {
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);
            newPosition = new Vector2(randomX, randomY);
            currentAttempt++;
        } 
        while (!isFirstTeleport && Vector2.Distance(newPosition, lastPosition) < minTeleportDistance && currentAttempt < maxAttempts);

        lastPosition = newPosition;
        isFirstTeleport = false;

        // Set the position using offsetMin/offsetMax or anchoredPosition
        // Reset the anchors temporarily to set the position
        Vector2 originalMin = rectTransform.anchorMin;
        Vector2 originalMax = rectTransform.anchorMax;
        
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(newPosition.x - parentWidth * 0.5f, newPosition.y - parentHeight * 0.5f);
    }

    // Reset the counter
    public void ResetCounter()
    {
        count = 0;
        if (counter != null)
            counter.text = count + " / 3";
        isFirstTeleport = true;
    }
}