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
    private MusicChange _musicChange;
    protected override void Awake()
    {
        base.Awake();

        if (UICharacterRectTransform != null)
            _characterStartPosition = UICharacterRectTransform.position;
        _soundPlayer = GetComponent<SoundPlayer>();
        
        if (mission != null)
            mission.OnTypingFinished += MissionFailed;
        _musicChange = GetComponent<MusicChange>();
    }
    
    protected override void Start()
    {
        base.Start();
        
        if (UICharacterRectTransform != null)
            _UIAnimator = UICharacterRectTransform.GetComponent<Animator>();
        
        if (SkipButton != null)
        {
            SkipButton.OnLevelGoalReached += HandleLevelGoalReached;
            SkipButton.OnMissionComplete += HandleEnding;
        }
    }

    private void OnDestroy()
    {
        if (mission != null)
            mission.OnTypingFinished -= MissionFailed;

        if (SkipButton != null)
        {
            SkipButton.OnLevelGoalReached -= HandleLevelGoalReached;
            SkipButton.OnMissionComplete -= HandleEnding;
        }
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
        if (TaskManager.Instance != null) TaskManager.Instance.CompleteTask(TaskOrderNumber);
        
        if (StartPanel != null && StartPanel.transform.parent != null)
        {
            RectTransform parent = StartPanel.transform.parent.GetComponent<RectTransform>();
            if (parent != null)
            {
                parent.gameObject.SetActive(false);
                UIAnimationManager.Instance.HideWindow(parent, 0.5f);
            }
        }

        if (textPanel != null)
        {
            var st = textPanel.GetComponent<SpeechTypeWriterShekofteh>();
            if (st != null) st.StopTyping();
        }

        if (SkipButton != null) SkipButton.gameObject.SetActive(false);
        if (firstWinDialogue != null) UIAnimationManager.Instance.ShowDialogueWindow(firstWinDialogue, 0.5f, firstWinDialogueFtw);
        if (_musicChange != null)
        {
            _musicChange.ToggleMusic();
            _musicChange.PlayMainMusic();
        }
    }

    public void StartMission()
    {
        StartCoroutine(ExecuteSequence());
    }
    
    private IEnumerator ExecuteSequence()
    {
        counter.SetActive(true);

        yield return new WaitForSeconds(1f);
        _musicChange.ToggleMusic();
        _musicChange.PlayThemeMusic();
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
        if (mission != null) mission.StopTyping();

        // Stop the video player if it's running
        if (videoPlayer != null) videoPlayer.Stop();
        
        if (textPanel != null && textPanel.transform.parent != null)
            textPanel.transform.parent.gameObject.SetActive(false);
        if (SkipButton != null) SkipButton.gameObject.SetActive(false);
        if (buttonBackground != null) buttonBackground.gameObject.SetActive(false);
        if (counter != null) counter.SetActive(false);

        // Hide the video renderer (e.g., by setting alpha to 0)
        if (videoRnderer != null)
        {
            RawImage r = videoRnderer.GetComponent<RawImage>();
            if (r != null)
            {
                Color c = r.color;
                c.a = 0f;
                r.color = c;
            }
        }
        
        if (UICharacterRectTransform != null)
            UICharacterRectTransform.position = _characterStartPosition;
        _isNextLevelRunning = false;

        HandleLoose();
    }
    
    private void HandleLoose()
    {
        if (StartPanel != null) UIAnimationManager.Instance.HideWindow(StartPanel , 0.5f);
        if (acceptRect != null) UIAnimationManager.Instance.ShowWindow(acceptRect, 0.01f);
        if (firstFailDialogue != null) UIAnimationManager.Instance.ShowDialogueWindow(firstFailDialogue, 0.5f, firstFailDialogueFtw);
        if (_musicChange != null)
        {
            _musicChange.ToggleMusic();
            _musicChange.PlayMainMusic();
        }
    }
}