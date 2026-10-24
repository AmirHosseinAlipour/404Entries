using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SkipButton : MonoBehaviour
{
    public TextMeshProUGUI _counter;
    
    public Sprite fastForwardImage;
    public Sprite falseForwardImage;
    public Image buttonImage;
    public CircleTimerAnimation background;
    
    [Header("Teleport Settings")]
    public int[] counts;
    public float[] teleportIntervals;
    
    [Range(0f, 0.5f)]
    public float edgePadding = 0.2f;

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
    
    private int _value;

    [HideInInspector] public bool _firstLoop = true;
    [HideInInspector] public System.Action OnLevelGoalReached;
    [HideInInspector] public System.Action OnMissionComplete;
    [HideInInspector] public bool isCompleted;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();

        if (counts == null || counts.Length == 0 || counts[0] > 5)
        {
            counts = new int[] { 4, 4, 3 };
        }

        if (teleportIntervals == null || teleportIntervals.Length == 0)
        {
            teleportIntervals = new float[] { 1.2f, 1.0f, 0.8f };
        }

        _currentGoal = counts[0];
        _currentTeleportInterval = teleportIntervals[0];
        
        if (buttonImage != null) UIUtils.SetAlpha(buttonImage.gameObject, 0);
        if (background != null) UIUtils.SetAlpha(background.gameObject, 0);
        
        if (rectTransform.parent != null)
        {
            parentRectTransform = rectTransform.parent.GetComponent<RectTransform>();
        }
        
        if (rectTransform == null || parentRectTransform == null)
        {
            return;
        }

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(128f, 128f);
        
        if (button != null)
        {
            button.onClick.AddListener(ChangeCounter);
        }

        if (buttonImage != null) buttonImage.sprite = fastForwardImage;
        _value = 1;
    }

    private void Start()
    {
        if (background != null) background.animationDuration = _currentTeleportInterval;
    }

    private void OnDisable()
    {
        StopTeleportMovement();
    }
    
    public void StartTeleportMovement()
    {
        gameObject.SetActive(true);
        if (buttonImage != null) UIUtils.SetAlpha(buttonImage.gameObject, 1f);
        if (background != null)
        {
            background.gameObject.SetActive(true);
            UIUtils.SetAlpha(background.gameObject, 1f);
        }

        if (teleportCoroutine != null)
        {
            StopCoroutine(teleportCoroutine);
        }

        _firstLoop = true;
        TeleportButton();
        if (gameObject.activeInHierarchy)
        {
            teleportCoroutine = StartCoroutine(RandomTeleportRoutine());
        }
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
        if (_canTouch && !canGoToNextLevel && !isCompleted)
        {
            _canTouch = false;
            _count += _value;
            if (_count <= 0) _count = 0;
            if (_counter != null)
            {
                _counter.text = _count + " / " + _currentGoal;
            }
            
            CheckForLevelCompletion();

            if (!canGoToNextLevel && !isCompleted)
            {
                if (teleportCoroutine != null)
                {
                    StopCoroutine(teleportCoroutine);
                }
                TeleportButton();
                if (gameObject.activeInHierarchy)
                {
                    teleportCoroutine = StartCoroutine(RandomTeleportRoutine());
                }
            }
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
            yield return new WaitForSeconds(_currentTeleportInterval);
            TeleportButton();
        }
    }

    private void TeleportButton()
    {
        if (parentRectTransform == null)
        {
            if (rectTransform != null && rectTransform.parent != null)
                parentRectTransform = rectTransform.parent.GetComponent<RectTransform>();
        }

        _canTouch = true;

        float parentWidth = parentRectTransform != null ? parentRectTransform.rect.width : 1536f;
        if (parentWidth < 300f) parentWidth = 1536f;
        float parentHeight = parentRectTransform != null ? parentRectTransform.rect.height : 864f;
        if (parentHeight < 200f) parentHeight = 864f;

        float buttonWidth = 128f * Mathf.Abs(rectTransform.localScale.x);
        float buttonHeight = 128f * Mathf.Abs(rectTransform.localScale.y);

        float minX = buttonWidth * 0.5f;
        float maxX = parentWidth - buttonWidth * 0.5f;
        
        float minY = buttonHeight * 0.5f;
        float maxY = parentHeight - buttonHeight * 0.5f;

        float paddingX = (maxX - minX) * edgePadding;
        float paddingY = (maxY - minY) * edgePadding;

        minX += paddingX;
        maxX -= paddingX;
        minY += paddingY;
        maxY -= paddingY;

        if (minX > maxX) minX = maxX = parentWidth * 0.5f;
        if (minY > maxY) minY = maxY = parentHeight * 0.5f;

        float minTeleportDistance = 200f;
        int maxAttempts = 20;
        
        Vector2 newPosition;
        int currentAttempt = 0;
        
        do
        {
            float randomX = UnityEngine.Random.Range(minX, maxX);
            float randomY = UnityEngine.Random.Range(minY, maxY);
            newPosition = new Vector2(randomX, randomY);
            currentAttempt++;
        } 
        while (!isFirstTeleport && Vector2.Distance(newPosition, lastPosition) < minTeleportDistance && currentAttempt < maxAttempts);

        lastPosition = newPosition;
        isFirstTeleport = false;

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(128f, 128f);
        rectTransform.anchoredPosition = new Vector2(newPosition.x - parentWidth * 0.5f, newPosition.y - parentHeight * 0.5f);

        UIUtils.SetAlpha(gameObject, 1f);
        if (buttonImage != null) UIUtils.SetAlpha(buttonImage.gameObject, 1f);
        if (background != null)
        {
            background.gameObject.SetActive(true);
            UIUtils.SetAlpha(background.gameObject, 1f);
            background.animationDuration = _currentTeleportInterval;
            background.StartCountdown();
        }

        ChangeButton();
    }

    private void ChangeButton()
    {
        float chance = UnityEngine.Random.value;

        if (chance < 0.75f)
        {
            if (buttonImage != null) buttonImage.sprite = fastForwardImage;
            _value = 1;
        }
        else
        {
            if (buttonImage != null) buttonImage.sprite = falseForwardImage;
            _value = -1;
        }
    }

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
        
        if (_currentLevel >= counts.Length)
        {
            isCompleted = true;
            OnMissionComplete?.Invoke();
            StopTeleportMovement();
            return;
        }

        _currentGoal = counts[_currentLevel];
        _currentTeleportInterval = (_currentLevel < teleportIntervals.Length) ? teleportIntervals[_currentLevel] : 1f;
        if (background != null) background.animationDuration = _currentTeleportInterval;
        ResetCounter();
        canGoToNextLevel = false;
    }
    
    public void ResetState()
    {
        StopTeleportMovement();
        if (background != null) background.StopCountdown();

        _currentLevel = 0;
        isCompleted = false;
        canGoToNextLevel = false;
        _canTouch = true;

        if (counts == null || counts.Length == 0 || counts[0] > 5)
        {
            counts = new int[] { 4, 4, 3 };
        }

        if (teleportIntervals == null || teleportIntervals.Length == 0)
        {
            teleportIntervals = new float[] { 1.2f, 1.0f, 0.8f };
        }

        _currentGoal = counts[0];
        _currentTeleportInterval = teleportIntervals[0];
        if (background != null) background.animationDuration = _currentTeleportInterval;

        ResetCounter();

        if (_counter != null)
        {
            _counter.gameObject.SetActive(true);
        }

        UIUtils.SetAlpha(gameObject, 1f);
        if (buttonImage != null) UIUtils.SetAlpha(buttonImage.gameObject, 1f);
        if (background != null)
        {
            background.gameObject.SetActive(true);
            UIUtils.SetAlpha(background.gameObject, 1f);
        }

        _firstLoop = true;
        isFirstTeleport = true;
    }
}