using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

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

    public VideoPlayer videoPlayer;
    public GameObject videoRnderer;
    
    private Animator _UIAnimator;
    private Vector3 _characterStartPosition;
    
    private SoundPlayer _soundPlayer;

    protected override void Awake()
    {
        base.Awake();

        _characterStartPosition = UICharacterRectTransform.position;
        _soundPlayer = GetComponent<SoundPlayer>();
        
        mission.OnTypingFinished += MissionFailed;
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

    private IEnumerator NextLevel()
    {
        _isNextLevelRunning = true;

        float originalSpeed = mission.typeCharTime;
        mission.typeCharTime /= 10;

        UIUtils.SetAlpha(SkipButton.gameObject, 0f);
        UIUtils.SetAlpha(buttonBackground.gameObject, 0f);
        
        _soundPlayer.Play("Skip");

        RawImage r = videoRnderer.GetComponent<RawImage>();
        Color c = r.color;
        c.a = 0.5f;
        r.color = c;
        videoPlayer.Play();
        yield return new WaitForSecondsRealtime(nextLevelWaitDuration);
        c.a = 0;
        r.color = c;
        mission.typeCharTime = originalSpeed;
        UIUtils.SetAlpha(SkipButton.gameObject, 1f);
        UIUtils.SetAlpha(buttonBackground.gameObject, 1f);

        SkipButton.NextLevel();
        _isNextLevelRunning = false;
    }

    private void MissionFailed()
    {
        StopAllCoroutines();
    
        // Also stop the typewriter, just in case (though it just finished)
        mission.StopTyping();

        // Stop the video player if it's running
        videoPlayer.Stop();
        
        
        textPanel.transform.parent.gameObject.SetActive(false);
        SkipButton.gameObject.SetActive(false);
        buttonBackground.gameObject.SetActive(false);
        counter.SetActive(false);

        // Hide the video renderer (e.g., by setting alpha to 0)
        RawImage r = videoRnderer.GetComponent<RawImage>();
        Color c = r.color;
        c.a = 0f;
        r.color = c;
        
        UICharacterRectTransform.position = _characterStartPosition;
        _isNextLevelRunning = false;

        HandleLoose();
    }
    
    private void HandleLoose()
    {
        UIAnimationManager.Instance.HideWindow(StartPanel , 0.5f);
        UIAnimationManager.Instance.ShowWindow(acceptRect, 0.01f);
        UIAnimationManager.Instance.ShowDialogueWindow(firstFailDialogue, 0.5f, firstFailDialogueFtw);
    }
}