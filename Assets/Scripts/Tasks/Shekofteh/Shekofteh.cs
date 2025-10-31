using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shekofteh : BaseProfessors
{
    [Header("Character")]
    public RectTransform UICharacterRectTransform;
    public float UICharacterMoveSpeed = 800f;
    
    [Header("Positions")]
    public RectTransform endPos1;
    public RectTransform endPos2;

    [Header("Mission")]
    public GameObject textPanel;
    public SkipButton SkipButton;
    public CircleTimerAnimation buttonBackground;
    public SpeechTypeWriterShekofteh mission;
    public float nextLevelWaitDuration;
    
    private bool _isNextLevelRunning = false;

    [Header("Others")]
    // To make it visible after movement
    public GameObject counter;
    
    private Animator _UIAnimator;

    private SoundPlayer _soundPlayer;

    protected override void Awake()
    {
        base.Awake();

        _soundPlayer = GetComponent<SoundPlayer>();
    }
    
    protected override void Start()
    {
        base.Start();
        
        _UIAnimator = UICharacterRectTransform.GetComponent<Animator>();
        
        SkipButton.OnLevelGoalReached += HandleLevelGoalReached;
        
        SkipButton.OnMissionComplete += HandleEnding;
    }

    private void HandleLevelGoalReached()
    {
        if (!_isNextLevelRunning)
        {
            StartCoroutine(NextLevel());
        }
    }

    private void HandleEnding()
    {
        TaskManager.Instance.CompleteTask(TaskOrderNumber);
        
        RectTransform parent = StartPanel.transform.parent.GetComponent<RectTransform>();
        textPanel.GetComponent<SpeechTypeWriterShekofteh>().StopTyping();
        SkipButton.gameObject.SetActive(false);
        parent.gameObject.SetActive(false);
        UIAnimationManager.Instance.HideWindow(parent, 0.5f);
        UIAnimationManager.Instance.ShowDialogueWindow(firstWinDialogue, 0.5f, firstWinDialogueFtw);
    }

    public void StartMission()
    {
        StartCoroutine(ExecuteSequence());
    }
    
    private IEnumerator ExecuteSequence()
    {
        yield return StartCoroutine(MoveObject(endPos1.position));
        yield return StartCoroutine(MoveObject(endPos2.position));
        counter.SetActive(true);

        yield return new WaitForSeconds(1f);

        textPanel.transform.parent.gameObject.SetActive(true);
        textPanel.GetComponent<SpeechTypeWriterShekofteh>().StartTyping();
        
        SkipButton.gameObject.SetActive(true);
        buttonBackground.gameObject.SetActive(true);
        
        UIUtils.SetAlpha(SkipButton.buttonImage.gameObject, 0);
        UIUtils.SetAlpha(buttonBackground.gameObject, 0);
        
        SkipButton.StartTeleportMovement();
    }

    private IEnumerator MoveObject(Vector3 targetWorldPosition)
    {
        Vector3 direction = (targetWorldPosition - UICharacterRectTransform.position).normalized;

        if (_UIAnimator != null)
        {
            _UIAnimator.SetFloat("MoveX", direction.x);
            _UIAnimator.SetFloat("MoveY", direction.y);
            _UIAnimator.SetFloat("MoveMagnitude", 1f);
        }
        
        while (Vector3.Distance(UICharacterRectTransform.position, targetWorldPosition) > 1f)
        {
            UICharacterRectTransform.position = Vector3.MoveTowards(
                UICharacterRectTransform.position,
                targetWorldPosition,
                UICharacterMoveSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (_UIAnimator != null)
        {
            _UIAnimator.SetFloat("MoveMagnitude", 0f);
        }

        UICharacterRectTransform.position = targetWorldPosition;
    }

    private IEnumerator NextLevel()
    {
        _isNextLevelRunning = true;

        float originalSpeed = mission.typeCharTime;
        mission.typeCharTime /= 10;

        UIUtils.SetAlpha(SkipButton.gameObject, 0f);
        UIUtils.SetAlpha(buttonBackground.gameObject, 0f);
        
        _soundPlayer.Play("Skip");
        yield return new WaitForSecondsRealtime(nextLevelWaitDuration);
        mission.typeCharTime = originalSpeed;
        UIUtils.SetAlpha(SkipButton.gameObject, 1f);
        UIUtils.SetAlpha(buttonBackground.gameObject, 1f);

        SkipButton.NextLevel();
        _isNextLevelRunning = false;
    }
}