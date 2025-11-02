using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SleepManager : MonoBehaviour
{
    [Header("Professor")]
    public SleepProfessor prof;
    
    [Header("Bar Settings")]
    public RectTransform barRect;
    public RectTransform pointerRect;
    public float totalTime = 90f; 
    public int redClicksToSleep = 3;
    public RectTransform mainPanel;
    
    [Header("Sleep / Inactivity")]
    public float inactivityThreshold = 3f; 
    public float inactivitySleepRate = 0.02f;
    public float greenClickRecovery = 0.15f;
    public float sleepProgress = 0f;  
    public float sleepProgressToSleep = 1f;
    public float smoothSpeed = 3f;
    
    [Header("Speed Settings")]
    [Tooltip("How many times the speed will increase over the totalTime.")]
    public int numberOfSpeedIncreases = 6;
    [Tooltip("The multiplier applied to the speed (e.g., 1.26 for 26% faster).")]
    public float speedMultiplier = 1.26f;

    [Header("References")]
    public EyeLidController eyelidController;

    public PointerMover pointerMover;

    private float elapsed = 0f;
    private int redClickCount = 0;
    private float lastInteractionTime;
    private float targetSleepProgress = 0f;

    [Header("LoseOption")]
    public Image PanelToChange;
    private bool IsLose = false;
    public Sprite main_Sprite;
    public Sprite Loosing_Sprite;
    private bool isGameStarted = false;

    [Header("Covers")]
    public RectTransform upperCover;
    public RectTransform lowerCover;
    private float initialPointerSpeed;
    private float timePerSpeedIncrease;
    private int currentSpeedIncreaseCount;
    private MusicChange _musicChange;

    void Start()
    {
        lastInteractionTime = Time.time;
        targetSleepProgress = sleepProgress;
        
        if (pointerMover != null)
        {
            initialPointerSpeed = pointerMover.speed;
        }

        _musicChange = GetComponent<MusicChange>();

        //mainPanel = PanelToChange.GetComponent<RectTransform>();
        SetInitialPos();
    }

    void Update()
    {
        if (!isGameStarted || IsLose) return;

        elapsed += Time.deltaTime;
        float left = Mathf.Max(0f, totalTime - elapsed);

        if (Time.time - lastInteractionTime > inactivityThreshold)
        {
            targetSleepProgress += inactivitySleepRate * Time.deltaTime;
            targetSleepProgress = Mathf.Clamp01(targetSleepProgress);
        }

        if (numberOfSpeedIncreases > 0 && currentSpeedIncreaseCount < numberOfSpeedIncreases)
        {
            // Calculate the time when the *next* increase should happen
            float nextIncreaseTime = timePerSpeedIncrease * (currentSpeedIncreaseCount + 1);

            if (elapsed >= nextIncreaseTime)
            {
                // We've passed the checkpoint! Increase speed and update the counter.
                pointerMover.speed *= speedMultiplier;
                currentSpeedIncreaseCount++;
            }
        }

        sleepProgress = Mathf.Lerp(sleepProgress, targetSleepProgress, Time.deltaTime * smoothSpeed);

        if (eyelidController != null)
            eyelidController.SetCloseAmount(sleepProgress);

        if (sleepProgress >= 0.49f || redClickCount >= redClicksToSleep)
            OnSleep();

        if (left <= 0f && sleepProgress < sleepProgressToSleep && redClickCount < redClicksToSleep)
            OnWin();
    }

    public void HandleClick()
    {
        if (!isGameStarted || IsLose) return;

        lastInteractionTime = Time.time;
        float barHalf = barRect.rect.width;
        float px = pointerRect.localPosition.x;

        if (px < -(barHalf / 5) || px > (barHalf / 5))
        {
            redClickCount++;
            targetSleepProgress += 0.15f;
        }
        else
        {
            targetSleepProgress -= greenClickRecovery;
        }

        targetSleepProgress = Mathf.Clamp01(targetSleepProgress);
    }

    void OnSleep()
    {
        barRect.gameObject.SetActive(false);
        Debug.Log("on sleep");
        targetSleepProgress = 0.5f;
        sleepProgress = Mathf.Lerp(sleepProgress, 0.5f, Time.deltaTime * smoothSpeed);
        StartCoroutine(JustWait());
    }

    IEnumerator JustWait()
    {
        yield return new WaitForSeconds(1f);
        targetSleepProgress = 0;
        sleepProgress = Mathf.Lerp(sleepProgress, 0f, Time.deltaTime * 5 * smoothSpeed);
        PanelToChange.sprite = Loosing_Sprite;
        yield return new WaitForSeconds(0.5f);
        IsLose = true;
        UIAnimationManager.Instance.HideWindow(mainPanel, 0.5f);

        HandleLoose();
    }

    void OnWin()
    {
        gameObject.SetActive(false);
        UIAnimationManager.Instance.HideWindow(mainPanel , 0.5f);
        TaskManager.Instance.CompleteTask(prof.TaskOrderNumber);
        UIAnimationManager.Instance.ShowDialogueWindow(prof.firstWinDialogue, 0.5f, prof.firstWinDialogueFtw);
        _musicChange.ToggleMusic();
        _musicChange.PlayMainMusic();
        
    }
    public void StartGame()
    {
        isGameStarted = true;
        elapsed = 0f;
        redClickCount = 0;
        sleepProgress = 0f;
        targetSleepProgress = 0f;
        lastInteractionTime = Time.time;
        IsLose = false;
        barRect.gameObject.SetActive(true);
        if (PanelToChange != null && main_Sprite != null)
        {
            PanelToChange.sprite = main_Sprite;
        }
        
        if (pointerMover != null)
        {
            pointerMover.speed = initialPointerSpeed;
        }
        
        currentSpeedIncreaseCount = 0;

        if (numberOfSpeedIncreases > 0)
        {
            // Calculate the duration of each "step"
            timePerSpeedIncrease = totalTime / numberOfSpeedIncreases;
        }
        else
        {
            // Avoid division by zero; just set a huge time so it never triggers
            timePerSpeedIncrease = float.MaxValue;
        }
        _musicChange.ToggleMusic();
        _musicChange.PlayThemeMusic();
    }
    
    private void HandleLoose()
    {
       // UIAnimationManager.Instance.HideWindow(mainPanel , 0.5f);
        UIAnimationManager.Instance.ShowWindow(prof.acceptRect, 0.01f);
        UIAnimationManager.Instance.ShowDialogueWindow(prof.firstFailDialogue, 0.5f, prof.firstFailDialogueFtw);
        _musicChange.ToggleMusic();
        _musicChange.PlayMainMusic();
    }

    private void SetInitialPos()
    {
        float halfHeight = mainPanel.rect.height;

        // Move upper cover UP
        Vector2 upperPos = upperCover.anchoredPosition;
        upperPos.y = halfHeight;
        upperCover.anchoredPosition = upperPos;

        // Move lower cover DOWN
        Vector2 lowerPos = lowerCover.anchoredPosition;
        lowerPos.y = -halfHeight;
        lowerCover.anchoredPosition = lowerPos;

    }
}
