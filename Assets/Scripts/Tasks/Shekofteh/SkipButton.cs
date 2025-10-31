using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Random = UnityEngine.Random;

public class SkipButton : MonoBehaviour
{
    // UI text that counts the touches
    public TextMeshProUGUI _counter;
    
    // 2 different skip button
    public Sprite fastForwardImage;
    public Sprite falseForwardImage;

    public Image buttonImage;
    
    public CircleTimerAnimation background;
    
    [Header("Teleport Settings")]
    public int[] counts;
    public float[] teleportIntervals;
    
    [Range(0f, 0.5f)]
    public float edgePadding = 0.3f;

    private int _count;
    public bool canGoToNextLevel;
        
    private int _currentGoal;
    private int _currentLevel = 0;
    private float _currentTeleportInterval;
    
    private RectTransform rectTransform;
    private RectTransform parentRectTransform;
    private Coroutine teleportCoroutine;
    private Button button;

    private bool _canTouch = true;
    private Vector2 lastPosition;
    private bool isFirstTeleport = true;
    
    // state for 2 different skip buttons
    private int _value;

    [HideInInspector] public bool _firstLoop = true;
    
    [HideInInspector] public System.Action OnLevelGoalReached;
    
    [HideInInspector] public System.Action OnMissionComplete;
    
    [HideInInspector]
    public bool isCompleted;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();

        _currentGoal = counts[0];
        _currentTeleportInterval = teleportIntervals[0];
        
        UIUtils.SetAlpha(buttonImage.gameObject, 0);
        UIUtils.SetAlpha(background.gameObject, 0);
        
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
            button.onClick.AddListener(ChangeCounter);
        }

        buttonImage.sprite = fastForwardImage;
        _value = 1;
    }

    private void Start()
    {
        background.animationDuration = _currentTeleportInterval;
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

        _firstLoop = true;
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
    
    private void ChangeCounter()
    {
        if (_canTouch)
        {
            _canTouch = false;
            _count += _value;
            if (_count <= 0) _count = 0;
            _counter.text = _count + " / " + _currentGoal;
            
            CheckForLevelCompletion();
        }
    }

    private void CheckForLevelCompletion()
    {
        if (_count >= _currentGoal && !canGoToNextLevel)
        {
            canGoToNextLevel = true;
            OnLevelGoalReached?.Invoke();
        }
    }
    
    private IEnumerator RandomTeleportRoutine()
    {
        while (true)
        {
            // 1. Teleport and Start Animation
            TeleportButton();
            background.StartCountdown();
            yield return new WaitForSeconds(_currentTeleportInterval);
        }
    }

    private void TeleportButton()
    {
        if (parentRectTransform == null) return;

        _canTouch = true;

        // Get the actual visible area of the parent panel
        float parentWidth = parentRectTransform.rect.width;
        float parentHeight = parentRectTransform.rect.height;

        // Get the button size (ac_counting for scale)
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

        if (_firstLoop)
        {
            _firstLoop = false;
            UIUtils.SetAlpha(buttonImage.gameObject, 1);
            UIUtils.SetAlpha(background.gameObject, 1);
        }

        ChangeButton();
    }

    private void ChangeButton()
    {
        float chance = Random.value;

        if (chance < 0.75f)
        {
            // 75% chance to use fastForwardImage
            buttonImage.sprite = fastForwardImage;
            _value = 1;
        }
        else
        {
            // 25% chance to use falseForwardImage
            buttonImage.sprite = falseForwardImage;
            _value = -1;
        }
    }


    // Reset the counter
    public void ResetCounter()
    {
        _count = 0;
        if (_counter != null)
            _counter.text = _count + " / " + _currentGoal;
        isFirstTeleport = true;
    }

    public void NextLevel()
    {
        _currentLevel++;
        
        // End logic
        if (_currentLevel >= counts.Length)
        {
            isCompleted = true;
            OnMissionComplete?.Invoke();
            StopTeleportMovement();
            return;
        }

        _currentGoal = counts[_currentLevel];
        _currentTeleportInterval = teleportIntervals[_currentLevel];
        background.animationDuration = _currentTeleportInterval;
        ResetCounter();
        canGoToNextLevel = false;
    }
    
    // Resets the entire component to its initial state (Level 0)
    public void ResetState()
    {
        // Stop any active movement
        StopTeleportMovement();
        background.StopCountdown();

        // Reset level and completion status
        _currentLevel = 0;
        isCompleted = false;
        canGoToNextLevel = false;

        // Set stats for the first level (Level 0)
        if (counts.Length > 0)
            _currentGoal = counts[0];

        if (teleportIntervals.Length > 0)
        {
            _currentTeleportInterval = teleportIntervals[0];
            background.animationDuration = _currentTeleportInterval;
        }

        // Reset the counter UI and state
        ResetCounter(); // This sets _count = 0 and updates the text

        // --- NEW CODE ---
        // Force the counter text to be active and visible.
        // This fixes the "no text" problem.
        if (_counter != null)
        {
            _counter.gameObject.SetActive(true);
        }
        // --- END NEW CODE ---

        // Hide the button visuals (they get shown on first teleport)
        UIUtils.SetAlpha(buttonImage.gameObject, 0);
        UIUtils.SetAlpha(background.gameObject, 0);

        // Reset state flags
        _firstLoop = true;
        isFirstTeleport = true;
    }
}