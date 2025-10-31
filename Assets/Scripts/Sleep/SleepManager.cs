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

    [Header("References")]
    public EyeLidController eyelidController; 

    private float elapsed = 0f;
    private int redClickCount = 0;
    private float lastInteractionTime;
    private float targetSleepProgress = 0f;

    [Header("LoseOption")]
    public Image PanelToChange;
    private bool IsLose = false;
    public Sprite Loosing_Sprite;
    private bool isGameStarted = false;

    void Start()
    {
        lastInteractionTime = Time.time;
        targetSleepProgress = sleepProgress;
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

        sleepProgress = Mathf.Lerp(sleepProgress, targetSleepProgress, Time.deltaTime * smoothSpeed);

        if (eyelidController != null)
            eyelidController.SetCloseAmount(sleepProgress);

        if (sleepProgress >= 0.99f || redClickCount >= redClicksToSleep)
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
            targetSleepProgress += 0.33f;
        }
        else
        {
            targetSleepProgress -= greenClickRecovery;
        }

        targetSleepProgress = Mathf.Clamp01(targetSleepProgress);
    }

    void OnSleep()
    {
        Debug.Log("on sleep");
        targetSleepProgress = 1f;
        sleepProgress = Mathf.Lerp(sleepProgress, 1f, Time.deltaTime * smoothSpeed);
        StartCoroutine(JustWait());
    }

    IEnumerator JustWait()
    {
        yield return new WaitForSeconds(1f);
        targetSleepProgress = 0;
        sleepProgress = Mathf.Lerp(sleepProgress, 0f, Time.deltaTime * smoothSpeed);
        PanelToChange.sprite = Loosing_Sprite;
        yield return new WaitForSeconds(0.5f);
        IsLose = true;
        UIAnimationManager.Instance.HideWindow(mainPanel, 0.5f);
    }

    void OnWin()
    {
        gameObject.SetActive(false);
        UIAnimationManager.Instance.HideWindow(mainPanel , 0.5f);
        TaskManager.Instance.CompleteTask(prof.TaskOrderNumber);
        UIAnimationManager.Instance.ShowDialogueWindow(prof.firstWinDialogue, 0.5f, prof.firstWinDialogueFtw);
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
    }
}
